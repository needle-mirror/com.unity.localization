using System;
using UnityEngine.ResourceManagement;

namespace UnityEngine.Localization
{
    [Serializable]
    public class LocalizedReference
    {
        [SerializeField]
        string m_TableName;

        [SerializeField]
        string m_Key;

        public string TableName
        {
            get { return m_TableName; }
            set { m_TableName = value; }
        }

        public string Key
        {
            get { return m_Key; }
            set { m_Key = value; }
        }

        public override string ToString()
        {
            return "[" + m_TableName + "]" + m_Key;
        }
    }

    [Serializable]
    public class LocalizedAssetReference : LocalizedReference
    {
        public virtual Type AssetType
        {
            get { return null; }
        }

        // <summary>
        // Load the referenced asset as type TObject.
        // </summary>
        // <returns>The load operation.</returns>
        public IAsyncOperation<TObject> LoadAsset<TObject>() where TObject : Object
        {
            return LocalizationSettings.AssetDatabase.GetLocalizedAsset<TObject>(TableName, Key);
        }
    }

    public class LocalizedAssetReferenceT<TObject> : LocalizedAssetReference where TObject : Object
    {
        public override Type AssetType
        {
            get { return typeof(TObject); }
        }

        // <summary>
        // Load the referenced asset as type TObject.
        // </summary>
        // <returns>The load operation.</returns>
        public IAsyncOperation<TObject> LoadAsset()
        {
            return LoadAsset<TObject>();
        }
    }

    [Serializable]
    public class LocalizedAssetReferenceGameObject : LocalizedAssetReferenceT<GameObject> { }
    [Serializable]
    public class LocalizedAssetReferenceTexture2D : LocalizedAssetReferenceT<Texture2D> { }
    [Serializable]
    public class LocalizedAssetReferenceSprite : LocalizedAssetReferenceT<Sprite> { }

}