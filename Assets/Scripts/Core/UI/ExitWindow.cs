using UnityEngine;
using UnityEngine.UI;

namespace Core.UI
{
    public class ExitWindow : BaseWindow
    {
        [SerializeField] private Button _backButton;
        [SerializeField] private Button _backButton2;

        private BaseWindow _prevWindow;

        public void SetPrevWindow(BaseWindow window)
        {
            _prevWindow = window;
        }

        protected override void OnAwake()
        {
            _backButton?.onClick.AddListener(Back);
            _backButton2?.onClick.AddListener(Back);
        }

        protected override void Close()
        {
            base.Close();

            Application.Quit();
        }

        private void Back()
        {
            _prevWindow?.gameObject.SetActive(true);

            base.Close();
        }
    }
}