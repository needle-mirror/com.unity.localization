using System;

namespace UnityEngine.Localization.Tables
{
    /// <summary>
    /// It is possible to reference a table via either the table name of the table name guid.
    /// The TableReference provides a flexible way to reference via either of these methods and also includes editor functionality.
    /// </summary>
    [Serializable]
    public struct TableReference : ISerializationCallbackReceiver, IEquatable<TableReference>
    {
        /// <summary>
        /// The type of reference.
        /// </summary>
        public enum Type
        {
            /// <summary>
            /// No table is referenced.
            /// </summary>
            Empty,

            /// <summary>
            /// A table is referenced by its table name guid.
            /// </summary>
            Guid,

            /// <summary>
            /// A table is referenced by its name.
            /// </summary>
            Name
        }

        [SerializeField]
        string m_TableName;

        bool m_Valid;

        const string k_GuidTag = "GUID:";

        /// <summary>
        /// The type of reference.
        /// </summary>
        public Type ReferenceType { get; private set; }

        /// <summary>
        /// The table name guid when <see cref="ReferenceType"/> is <see cref="Type.Guid"/>.
        /// </summary>
        public Guid TableNameGuid { get; set; }

        /// <summary>
        /// The table name when <see cref="ReferenceType"/> is <see cref="Type.String"/>.
        /// </summary>
        public string TableName { get => m_TableName; private set => m_TableName = value; }

        /// <summary>
        /// Convert a table name into a <see cref="TableReference"/>.
        /// </summary>
        /// <param name="tableName">The name of the table.</param>
        public static implicit operator TableReference(string tableName)
        {
            return new TableReference() { TableName = tableName, ReferenceType = Type.Name };
        }

        /// <summary>
        /// Convert a table name guid into a <see cref="TableReference"/>.
        /// </summary>
        /// <param name="tableNameGuid">The table name guid.</param>
        public static implicit operator TableReference(Guid tableNameGuid)
        {
            return new TableReference() { TableNameGuid = tableNameGuid, ReferenceType = Type.Guid };
        }

        /// <summary>
        /// Returns <see cref="TableName"/>.
        /// </summary>
        /// <param name="tableReference"></param>
        /// <returns></returns>
        public static implicit operator string(TableReference tableReference)
        {
            return tableReference.TableName;
        }

        /// <summary>
        /// Returns <see cref="TableNameGuid"/>.
        /// </summary>
        /// <param name="tableReference"></param>
        /// <returns></returns>
        public static implicit operator Guid(TableReference tableReference)
        {
            return tableReference.TableNameGuid;
        }

        internal void Validate()
        {
            if (m_Valid)
                return;

            switch (ReferenceType)
            {
                case Type.Empty:
                    throw new ArgumentException("Empty Table Reference. Must contain a Guid or Table Name");

                case Type.Guid:
                    if (TableNameGuid == Guid.Empty)
                        throw new ArgumentException("Must use a valid Table Name Guid, can not be Empty.");
                    break;
                case Type.Name:
                    if (string.IsNullOrWhiteSpace(TableName))
                        throw new ArgumentException($"Table name can not be null or empty.");
                    break;
            }
            m_Valid = true;
        }

        internal string GetSerializedString()
        {
            if (ReferenceType == Type.Guid)
                return $"{k_GuidTag}{StringFromGuid(TableNameGuid)}";
            return TableName;
        }

        /// <summary>
        /// Returns a string representation.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            switch (ReferenceType)
            {
                case Type.Guid:
                    return $"{nameof(TableReference)}({TableNameGuid})";
                case Type.Name:
                    return $"{nameof(TableReference)}({TableName})";
            }
            return $"{nameof(TableReference)}(Empty)";
        }

        /// <summary>
        /// Compare 2 TableReferences.
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool Equals(TableReference other)
        {
            if (ReferenceType != other.ReferenceType)
                return false;

            if (ReferenceType == Type.Guid)
            {
                return TableNameGuid == other.TableNameGuid;
            }
            else if (ReferenceType == Type.Name)
            {
                return TableName == other.TableName;
            }
            return true;
        }

        /// <summary>
        /// Parse a string that contains uses the <see cref="k_GuidTag"/> tag.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        internal static Guid GuidFromString(string value)
        {
            return Guid.Parse(value.Substring(k_GuidTag.Length, value.Length - k_GuidTag.Length));
        }

        /// <summary>
        /// Returns a string version of the GUID which works with Addressables, it uses the "N" format(32 digits).
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        internal static string StringFromGuid(Guid value)
        {
            return value.ToString("N");
        }

        /// <summary>
        /// Converts a string into a a <see cref="TableReference"/>.
        /// </summary>
        /// <param name="value">The string to convert. The string can either be a table name or a GUID identified by prepending the <see cref="k_GuidTag"/> tag.</param>
        /// <returns></returns>
        internal static TableReference TableReferenceFromString(string value)
        {
            if (IsGuid(value))
                return GuidFromString(value);
            return value;
        }

        /// <summary>
        /// Is the string identified as a <see cref="Guid"/> string.
        /// Strings that start with <see cref="k_GuidTag"/> are considered a Guid.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        internal static bool IsGuid(string value)
        {
            return value.StartsWith(k_GuidTag);
        }

        /// <summary>
        /// Converts the reference into a serializable string.
        /// </summary>
        public void OnBeforeSerialize()
        {
            m_TableName = GetSerializedString();
        }

        /// <summary>
        /// Converts the serializable string into the correct reference type.
        /// </summary>
        public void OnAfterDeserialize()
        {
            if (string.IsNullOrEmpty(m_TableName))
            {
                ReferenceType = Type.Empty;
            }
            else if (IsGuid(m_TableName))
            {
                TableNameGuid = GuidFromString(m_TableName);
                ReferenceType = Type.Guid;
            }
            else
            {
                ReferenceType = Type.Name;
            }
        }
    }

    /// <summary>
    /// Allows for referencing a table entry via key or key id.
    /// </summary>
    [Serializable]
    public struct TableEntryReference : ISerializationCallbackReceiver, IEquatable<TableEntryReference>
    {
        /// <summary>
        /// The type of reference.
        /// </summary>
        public enum Type
        {
            /// <summary>
            /// No table entry is referenced.
            /// </summary>
            Empty,

            /// <summary>
            /// The key name is referenced.
            /// </summary>
            Name,

            /// <summary>
            /// The Key Id is referenced
            /// </summary>
            Id
        }

        [SerializeField]
        uint m_KeyId;

        [SerializeField]
        string m_Key;

        bool m_Valid;

        /// <summary>
        /// The type of reference.
        /// </summary>
        public Type ReferenceType { get; private set; }

        /// <summary>
        /// The Key Id when <see cref="ReferenceType"/> is <see cref="Type.Id"/>.
        /// </summary>
        public uint KeyId { get => m_KeyId; private set => m_KeyId = value; }

        /// <summary>
        /// The key name when <see cref="ReferenceType"/> is <see cref="Type.Name"/>.
        /// </summary>
        public string Key { get => m_Key; private set => m_Key = value; }

        /// <summary>
        /// Converts a string name into a reference.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static implicit operator TableEntryReference(string key)
        {
            return new TableEntryReference() { Key = key, ReferenceType = Type.Name };
        }

        /// <summary>
        /// Converts a key id into a reference.
        /// </summary>
        /// <param name="keyId"></param>
        /// <returns></returns>
        public static implicit operator TableEntryReference(uint keyId)
        {
            return new TableEntryReference() { KeyId = keyId, ReferenceType = Type.Id };
        }

        /// <summary>
        /// Returns <see cref="Key"/>.
        /// </summary>
        /// <param name="tableEntryReference"></param>
        /// <returns></returns>
        public static implicit operator string(TableEntryReference tableEntryReference)
        {
            return tableEntryReference.Key;
        }

        /// <summary>
        /// Returns <see cref="KeyId"/>
        /// </summary>
        /// <param name="tableEntryReference"></param>
        /// <returns></returns>
        public static implicit operator uint(TableEntryReference tableEntryReference)
        {
            return tableEntryReference.KeyId;
        }

        internal void Validate()
        {
            if (m_Valid)
                return;

            switch (ReferenceType)
            {
                case Type.Empty:
                    throw new ArgumentException("Empty Table Entry Reference. Must contain a Name or Key Id");

                case Type.Name:
                    if (string.IsNullOrWhiteSpace(Key))
                        throw new ArgumentException("Must use a valid Key, can not be null or Empty.");
                    break;
                case Type.Id:
                    if (KeyId == SharedTableData.EmptyId)
                        throw new ArgumentException("Key Id can not be empty.");
                    break;
            }
            m_Valid = true;
        }

        /// <summary>
        /// Returns the key name.
        /// If <see cref="ReferenceType"/> is <see cref="Type.Name"/> then <see cref="Key"/> will be returned.
        /// If <see cref="ReferenceType"/> is <see cref="Type.Id"/> then <paramref name="sharedData"/> will be used to extract the name.
        /// </summary>
        /// <param name="sharedData">The <see cref="SharedTableData"/> to use if the key name is not stored in the reference.</param>
        /// <returns></returns>
        public string ResolveKeyName(SharedTableData sharedData)
        {
            if (ReferenceType == Type.Name)
                return Key;
            else if (ReferenceType == Type.Id)
                return sharedData != null ? sharedData.GetKey(KeyId) : $"Key Id {KeyId}";
            return "None";
        }

        /// <summary>
        /// Returns a string representation.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            switch (ReferenceType)
            {
                case Type.Name:
                    return $"{nameof(TableEntryReference)}({Key})";
                case Type.Id:
                    return $"{nameof(TableEntryReference)}({KeyId})";
            }
            return $"{nameof(TableEntryReference)}(Empty)";
        }

        public bool Equals(TableEntryReference other)
        {
            if (ReferenceType != other.ReferenceType)
                return false;

            if (ReferenceType == Type.Name)
            {
                return Key == other.Key;
            }
            else if(ReferenceType == Type.Id)
            {
                return KeyId == other.KeyId;
            }
            return true;
        }

        /// <summary>
        /// Does nothing but is required for <see cref="OnAfterDeserialize"/>.
        /// </summary>
        public void OnBeforeSerialize()
        {
        }

        /// <summary>
        /// Determines the <see cref="ReferenceType"/>.
        /// </summary>
        public void OnAfterDeserialize()
        {
            if (KeyId != SharedTableData.EmptyId)
                ReferenceType = Type.Id;
            else if (string.IsNullOrEmpty(m_Key))
                ReferenceType = Type.Empty;
            else
                ReferenceType = Type.Name;
        }
    }
}
