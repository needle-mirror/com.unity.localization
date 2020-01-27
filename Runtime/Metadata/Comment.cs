#if UNITY_EDITOR
using System;
using System.Globalization;

namespace UnityEngine.Localization.Metadata
{
    /// <summary>
    /// Used to add metadata comments.
    /// Comments can be any type of information but are particularly useful for providing context to translators.
    /// </summary>
    [Metadata]
    [Serializable]
    public class Comment : IMetadata, ISerializationCallbackReceiver
    {
        [SerializeField]
        string m_TimeStamp;

        [SerializeField]
        [TextArea(1, int.MaxValue)]
        string m_CommentText = "Comment Text";

        /// <summary>
        /// A time stamp to indicate when the comment was created.
        /// </summary>
        public DateTime TimeStamp { get; set; } = DateTime.Now;

        /// <summary>
        /// The comment text.
        /// </summary>
        public string CommentText
        {
            get => m_CommentText;
            set => m_CommentText = value;
        }

        public void OnAfterDeserialize()
        {
            if (DateTime.TryParse(m_TimeStamp, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime time))
                TimeStamp = time;
        }

        public void OnBeforeSerialize()
        {
            m_TimeStamp = TimeStamp.ToString("HH:mm:ss.ffffff");
        }
    }
}
#endif
