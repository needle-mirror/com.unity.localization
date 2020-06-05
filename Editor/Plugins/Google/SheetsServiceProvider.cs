using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Util.Store;
using UnityEngine;

namespace UnityEditor.Localization.Plugins.Google
{
    /// <summary>
    /// See https://cloud.google.com/docs/authentication
    /// </summary>
    public enum AuthenticationType
    {
        None,

        /// <summary>
        /// Accessing private data.
        /// </summary>
        OAuth,

        /// <summary>
        /// Accessing public data anonymously.
        /// </summary>
        APIKey
    }

    /// <summary>
    /// Configuration for connecting to a Google Sheet.
    /// </summary>
    public interface IGoogleSheetsService
    {
        /// <summary>
        /// The Google Sheet service that will be created using the Authorization API.
        /// </summary>
        SheetsService Service { get; }
    }

    /// <summary>
    /// Configuration for connecting to a Google Sheet.
    /// Includes the Authorization properties and general sheet properties such as default sheet styles etc.
    /// </summary>
    [CreateAssetMenu(fileName = "Google Sheets Service", menuName = "Localization/Google Sheets Service")]
    [HelpURL("https://developers.google.com/sheets/api/guides/authorizing#AboutAuthorization")]
    public class SheetsServiceProvider : ScriptableObject, IGoogleSheetsService
    {
        [SerializeField]
        string m_ApiKey;

        [SerializeField]
        string m_ClientId;

        [SerializeField]
        string m_ClientSecret;

        [SerializeField]
        AuthenticationType m_AuthenticationType;

        [SerializeField]
        string m_ApplicationName;

        [SerializeField]
        NewSheetProperties m_NewSheetProperties = new NewSheetProperties();

        SheetsService m_SheetsService;

        // The Google API access application we are requesting.
        readonly static string[] k_Scopes = { SheetsService.Scope.Spreadsheets };

        /// <summary>
        /// Used to make sure the access and refresh tokens persist. Uses a FileDataStore by default with "Library/Google/{name}" as the path.
        /// </summary>
        public IDataStore DataStore { get; set; }

        /// <summary>
        /// The Google Sheet service that will be created using the Authorization API.
        /// </summary>
        public virtual SheetsService Service
        {
            get
            {
                if (m_SheetsService == null)
                    m_SheetsService = Connect();
                return m_SheetsService;
            }
        }

        /// <summary>
        /// The authorization methodology to use.
        /// See <see href="https://developers.google.com/sheets/api/guides/authorizing"/>
        /// </summary>
        public AuthenticationType Authentication => m_AuthenticationType;

        /// <summary>
        /// The API Key to use when using <see cref="AuthenticationType.APIKey"/> authentication.
        /// </summary>
        public string ApiKey => m_ApiKey;

        /// <summary>
        /// <para>Client Id when using OAuth authentication.</para>
        /// See also <seealso cref="SetOAuthCredentials"/>
        /// </summary>
        public string ClientId => m_ClientId;

        /// <summary>
        /// <para>Client secret when using OAuth authentication.</para>
        /// See also <seealso cref="SetOAuthCredentials"/>
        /// </summary>
        public string ClientSecret => m_ClientSecret;

        /// <summary>
        /// The name of the application that will be sent when connecting.
        /// </summary>
        public string ApplicationName
        {
            get => m_ApplicationName;
            set => m_ApplicationName = value;
        }

        /// <summary>
        /// Properties to use when creating a new Google Spreadsheet sheet.
        /// </summary>
        public NewSheetProperties NewSheetProperties
        {
            get => m_NewSheetProperties;
            set => m_NewSheetProperties = value;
        }

        public void OnEnable()
        {
            if (string.IsNullOrEmpty(ApplicationName))
                ApplicationName = PlayerSettings.productName;
        }

        /// <summary>
        /// Set the API Key. An API key can only be used for reading from a public Google Spreadsheet.
        /// </summary>
        /// <param name="apiKey"></param>
        public void SetApiKey(string apiKey)
        {
            m_ApiKey = apiKey;
            m_AuthenticationType = AuthenticationType.APIKey;
        }

        /// <summary>
        /// Enable OAuth 2.0 authentication and extract the <see cref="ClientId"/> and <see cref="ClientSecret"/> from the supplied json.
        /// </summary>
        /// <param name="credentialsJson"></param>
        public void SetOAuthCrendtials(string credentialsJson)
        {
            var secrets = LoadSecrets(credentialsJson);
            m_ClientId = secrets.ClientId;
            m_ClientSecret = secrets.ClientSecret;
            m_AuthenticationType = AuthenticationType.OAuth;
        }

        /// <summary>
        /// Enable OAuth 2.0 authentication with the provided client Id and client secret.
        /// </summary>
        /// <param name="clientId"></param>
        /// <param name="clientSecret"></param>
        public void SetOAuthCrendtials(string clientId, string clientSecret)
        {
            m_ClientId = clientId;
            m_ClientSecret = clientSecret;
            m_AuthenticationType = AuthenticationType.OAuth;
        }

        SheetsService Connect()
        {
            if (Authentication == AuthenticationType.None)
                throw new Exception("No connection credentials. You must provide either OAuth2.0 credentials or an Api Key.");

            if (Authentication == AuthenticationType.OAuth)
                return ConnectWithOAuth2();
            else
                return ConnectWithApiKey();
        }

        /// <summary>
        /// When calling an API that does not access private user data, you can use a simple API key.
        /// This key is by Google to authenticate your application for accounting purposes.
        /// If you do need to access private user data, you must use OAuth 2.0.
        /// </summary>
        /// <param name="apiKey"></param>
        SheetsService ConnectWithApiKey()
        {
            SheetsService sheetsService = new SheetsService(new BaseClientService.Initializer
            {
                ApiKey = m_ApiKey,
                ApplicationName = ApplicationName
            });
            return sheetsService;
        }

        /// <summary>
        /// Call to preauthorize when using OAuth authorization. This will cause a browser to open a Google authorization
        /// page after which the token will be stored in <see cref="IDataStore"/> so that this does not need to be done each time.
        /// If this is not called then the first time <see cref="Service"/> is called it will be performed then.
        /// </summary>
        /// <returns></returns>
        public UserCredential AuthoizeOAuth()
        {
            if (string.IsNullOrEmpty(ClientSecret))
                throw new Exception($"{nameof(ClientSecret)} is empty");

            if (string.IsNullOrEmpty(ClientId))
                throw new Exception($"{nameof(ClientId)} is empty");

            try
            {
                // Prevents Unity locking up if the user canceled the auth request.
                var cts = new CancellationTokenSource();

                // We create a separate area for each so that multiple providers dont clash.
                var dataStore = DataStore ?? new FileDataStore($"Library/Google/{name}", true);

                var secrets = new ClientSecrets { ClientId = m_ClientId, ClientSecret = m_ClientSecret };
                var connectTask = GoogleWebAuthorizationBroker.AuthorizeAsync(secrets, k_Scopes, "user", cts.Token, dataStore);

                while (!connectTask.IsCompleted)
                {
                    if (EditorUtility.DisplayCancelableProgressBar("Authorizing with Google", string.Empty, 0))
                    {
                        cts.Cancel();
                    }
                }

                if (connectTask.Status == TaskStatus.Faulted)
                {
                    throw new Exception($"Failed to connect to Google Sheets.\n{connectTask.Exception}");
                }
                return connectTask.Result;
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
        }

        /// <summary>
        /// When calling an API that will access private user data, O Auth 2.0 credentials must be used.
        /// </summary>
        /// <param name="credentials"></param>
        SheetsService ConnectWithOAuth2()
        {
            var userCredentials = AuthoizeOAuth();
            SheetsService sheetsService = new SheetsService(new BaseClientService.Initializer
            {
                HttpClientInitializer = userCredentials,
                ApplicationName = ApplicationName,
            });
            return sheetsService;
        }

        internal static ClientSecrets LoadSecrets(string credentials)
        {
            if (string.IsNullOrEmpty(credentials))
                throw new ArgumentException(nameof(credentials));

            using (var stream = new MemoryStream(System.Text.Encoding.ASCII.GetBytes(credentials)))
            {
                var gcs = GoogleClientSecrets.Load(stream);
                return gcs.Secrets;
            }
        }
    }
}
