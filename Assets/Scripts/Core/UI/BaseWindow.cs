using UnityEngine;
using UnityEngine.UI;

namespace Core.UI
{
    public abstract class BaseWindow : MonoBehaviour
    {
        [SerializeField] private Button _closeButton;

        private void Awake()
        {
            _closeButton?.onClick.AddListener(Close);

            OnAwake();
        }

        private void Start() => OnStart();

        private void Update() => OnUpdate();

        private void OnEnable() => OnEnableWindow();

        private void OnDestroy() => OnDestroyWindow();

        protected virtual void OnEnableWindow() { }

        protected virtual void OnStart() { }

        protected virtual void OnAwake() { }

        protected virtual void OnUpdate() { }

        protected virtual void OnDestroyWindow() { }

        protected virtual void Close() => gameObject.SetActive(false);
        
        public void PlayClickSound() => ServiceLocator.Get<ClickSoundController>().Play();
    }
}