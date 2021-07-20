using System;

namespace UnityEditor.Localization.Plugins.Google
{
    public partial class SheetsServiceProvider
    {
        [Obsolete("SetOAuthCrendtials is deprecated, use SetOAuthCredentials instead. (UnityUpgrade) -> SetOAuthCredentials(*)")]
        public void SetOAuthCrendtials(string credentialsJson) => SetOAuthCredentials(credentialsJson);

        [Obsolete("SetOAuthCrendtials is deprecated, use SetOAuthCredentials instead. (UnityUpgrade) -> SetOAuthCredentials(*)")]
        public void SetOAuthCrendtials(string clientId, string clientSecret) => SetOAuthCredentials(clientId, clientSecret);

        [Obsolete("AuthoizeOAuth is deprecated, use AuthorizeOAuth instead. (UnityUpgrade) -> AuthorizeOAuth(*)")]
        public void AuthoizeOAuth() => AuthorizeOAuth();
    }
}
