
namespace UnityEngine.Localization.Components
{
    public abstract class LocalizationBehaviour : MonoBehaviour
    {
        protected virtual void OnEnable()
        {
            if (LocalizationSettings.HasSettings)
            {
                LocalizationSettings.SelectedLocaleChanged += OnLocaleChanged;
                LocalizationSettings.InitializationOperation.Value.Completed += (o) => OnLocaleChanged(LocalizationSettings.SelectedLocale);
            }
        }

        protected virtual void OnDisable()
        {
            if (LocalizationSettings.HasSettings)
            {
                LocalizationSettings.SelectedLocaleChanged -= OnLocaleChanged;
            }
        }

        public virtual void ForceUpdate()
        {
            if (Application.isPlaying)
            {
                OnLocaleChanged(LocalizationSettings.SelectedLocale);
            }
        }

        protected abstract void OnLocaleChanged(Locale newLocale);
    }
}