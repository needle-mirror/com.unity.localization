#if UNITY_EDITOR
using System;

namespace UnityEngine.Localization.Metadata
{
    /// <summary>
    /// Used to add metadata comments.
    /// Comments can be any type of information but are particularly useful for providing context to translators.
    /// </summary>
    [Metadata]
    [Serializable]
    public class Comment : IMetadata
    {
        [SerializeField]
        string m_TimeStamp;

        [SerializeField]
        [TextArea(1, int.MaxValue)]
        string m_CommentText = "Comment Text";

        /// <summary>
        /// Creates a new Comment with the current date and time for <see cref="TimeStamp"/>.
        /// </summary>
        public Comment()
        {
            m_TimeStamp = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
        }

        /// <summary>
        /// A time stamp to indicate when the comment was created.
        /// </summary>
        public string TimeStamp
        {
            get => m_TimeStamp;
            set => m_TimeStamp = value;
        }

        /// <summary>
        /// The comment text.
        /// </summary>
        public string CommentText
        {
            get => m_CommentText;
            set => m_CommentText = value;
        }
    }
}
#endif