using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;

#region localized-text-font

public class LocalizedTextWithFont : MonoBehaviour
{
    [Serializable]
    public class LocalizedFont : LocalizedAsset<Font> {}

    public LocalizedString localizedString;
    public LocalizedFont localizedFont;

    public Text uiText;

    void OnEnable()
    {
        localizedString.StringChanged += UpdateText;
        localizedFont.AssetChanged += FontChanged;
    }

    void OnDisable()
    {
        localizedString.StringChanged -= UpdateText;
        localizedFont.AssetChanged -= FontChanged;
    }

    void FontChanged(Font f)
    {
        uiText.font = f;
    }

    void UpdateText(string s)
    {
        uiText.text = s;
    }
}
#endregion

#region localized-sprite

public class LocalizedSpriteExample : MonoBehaviour
{
    public LocalizedSprite localizedSprite;

    public Image image;

    void OnEnable()
    {
        LocalizationSettings.SelectedLocaleChanged += SelectedLocaleChanged;
        StartCoroutine(LoadAssetCoroutine());
    }

    void OnDisable()
    {
        LocalizationSettings.SelectedLocaleChanged -= SelectedLocaleChanged;
    }

    void SelectedLocaleChanged(Locale obj)
    {
        StartCoroutine(LoadAssetCoroutine());
    }

    IEnumerator LoadAssetCoroutine()
    {
        var operation = localizedSprite.LoadAssetAsync();
        yield return operation;
        image.sprite = operation.Result;
    }
}
#endregion

#region localized-prefab

public class LocalizedPrefabExample : MonoBehaviour
{
    [Serializable]
    public class LocalizedPrefab : LocalizedAsset<GameObject> {}

    public LocalizedPrefab localizedPrefab;

    GameObject currentInstance;

    void OnEnable()
    {
        localizedPrefab.AssetChanged += UpdatePrefab;
    }

    void OnDisable()
    {
        localizedPrefab.AssetChanged -= UpdatePrefab;
    }

    void UpdatePrefab(GameObject value)
    {
        if (currentInstance != null)
            Destroy(currentInstance);

        currentInstance = Instantiate(value);
    }
}
#endregion
