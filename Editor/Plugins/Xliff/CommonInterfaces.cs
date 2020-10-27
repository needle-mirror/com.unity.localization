using System.IO;

namespace UnityEditor.Localization.Plugins.XLIFF.Common
{
    /// <summary>
    /// The root element of an XLIFF document is <xliff>.
    /// It contains a collection of <file> elements. Typically, each <file> element contains a set of <unit> elements that contain
    /// the text to be translated in the <source> child of one or more <segment> elements.
    /// Translations are stored in the <target> child of each <segment> element.
    /// </summary>
    public interface IXliffDocument
    {
        string Version { get; }

        string SourceLanguage { get; set; }

        string TargetLanguage { get; set; }

        int FileCount { get; }
        IFile GetFile(int index);
        void AddFile(IFile f);
        void RemoveFile(IFile f);
        IFile AddNewFile();

        void Serialize(Stream stream);
    }

    public enum NoteType
    {
        General,
        Source,
        Target
    }

    /// <summary>
    /// Readable comments and annotations.
    /// </summary>
    public interface INote
    {
        NoteType AppliesTo { get; set; }
        string NoteText { get; set; }
    }

    /// <summary>
    /// Holds a collection of <see cref="INote"/>.
    /// </summary>
    public interface INoteCollection
    {
        int NoteCount { get; }
        INote GetNote(int index);
        void AddNote(INote note);
        void RemoveNote(INote note);
        INote AddNewNote();
    }

    /// <summary>
    /// Container for localization material extracted from an entire single document, or another high level
    /// self contained logical node in a content structure that cannot be described in the terms of documents.
    /// </summary>
    public interface IFile : IGroupCollection, ITranslationUnitCollection, INoteCollection
    {
        /// <summary>
        /// The Id the original document.
        /// </summary>
        string Id { get; set; }

        /// <summary>
        /// The location of the original document from which the content of the enclosing elements are extracted.
        /// </summary>
        string Original { get; set; }
    }

    /// <summary>
    /// Static container for a dynamic structure of elements holding the extracted translatable source text, aligned with the Translated text.
    /// </summary>
    public interface ITranslationUnit : INoteCollection
    {
        string Id { get; set; }
        string Name { get; set; }
        string Source { get; set; }
        string Target { get; set; }
    }

    /// <summary>
    /// Holds a collection of <see cref="ITranslationUnit"/>.
    /// </summary>
    public interface ITranslationUnitCollection
    {
        int TranslationUnitCount { get; }
        ITranslationUnit GetTranslationUnit(int index);
        void AddTranslationUnit(ITranslationUnit tu);
        void RemoveTranslationUnit(ITranslationUnit tu);
        ITranslationUnit AddNewTranslationUnit();
    }

    /// <summary>
    /// Provides a way to organize units into a structured hierarchy.
    /// </summary>
    public interface IGroup : IGroupCollection, INoteCollection, ITranslationUnitCollection
    {
        string Id { get; set; }
        string Name { get; set; }
    }

    /// <summary>
    /// Holds a collection of <see cref="IGroup"/>.
    /// </summary>
    public interface IGroupCollection
    {
        int GroupCount { get; }
        IGroup GetGroup(int index);
        void AddGroup(IGroup grp);
        void RemoveGroup(IGroup grp);
        IGroup AddNewGroup();
    }
}
