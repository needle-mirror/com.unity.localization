using System;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace UnityEngine.Localization
{
    [Serializable]
    public class LocalizedReference
    {
        [SerializeField]
        string m_TableName;

        [SerializeField]
        string m_Key;

        [SerializeField]
        uint m_KeyId;

        public string TableName
        {
            get => m_TableName;
            set => m_TableName = value;
        }

        public uint KeyId
        {
            get => m_KeyId;
            set => m_KeyId = value;
        }

        public string Key
        {
            get => m_Key;
            set => m_Key = value;
        }

        public override string ToString() => "[" + m_TableName + "]" + (string.IsNullOrEmpty(m_Key) ? m_KeyId.ToString() : m_Key);
    }

    [Serializable]
    public class LocalizedAssetReference : LocalizedReference
    {
        public virtual Type AssetType => null;

        // <summary>
        // Load the referenced asset as type TObject.
        // </summary>
        // <returns>The load operation.</returns>
        public AsyncOperationHandle<TObject> LoadAssetAsync<TObject>() where TObject : Object
        {
            if (KeyId == KeyDatabase.EmptyId)
                return LocalizationSettings.AssetDatabase.GetLocalizedAssetAsync<TObject>(TableName, Key);
            return LocalizationSettings.AssetDatabase.GetLocalizedAssetAsync<TObject>(TableName, KeyId);
        }
    }

    public class LocalizedAssetReferenceT<TObject> : LocalizedAssetReference where TObject : Object
    {
        public override Type AssetType => typeof(TObject);

        // <summary>
        // Load the referenced asset as type TObject.
        // </summary>
        // <returns>The load operation.</returns>
        public AsyncOperationHandle<TObject> LoadAssetAsync()
        {
            return LoadAssetAsync<TObject>();
        }
    }

    [Serializable]
    public class LocalizedAssetReferenceGameObject : LocalizedAssetReferenceT<GameObject> { }
    [Serializable]
    public class LocalizedAssetReferenceTexture2D : LocalizedAssetReferenceT<Texture2D> { }
    [Serializable]
    public class LocalizedAssetReferenceSprite : LocalizedAssetReferenceT<Sprite> { }

}