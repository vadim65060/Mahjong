using UnityEngine;
using UnityEngine.UI;

namespace Core.UI
{
    public class PauseWindow : BaseWindow
    {
        [SerializeField] private Button _restartButton;
        [SerializeField] private Button _soundButton;
        [SerializeField] private Image _soundButtonImage;
        [SerializeField] private Button _menuButton;
        [SerializeField] private Sprite _mutedSprite;
        [SerializeField] private Sprite _unmutedSprite;

        private bool _isMuted;

        protected override void OnAwake()
        {
            _restartButton?.onClick.AddListener(Restart);
            _soundButton?.onClick.AddListener(SwitchSound);
            _menuButton?.onClick.AddListener(OpenMenu);
        }

        protected override void Close()
        {
            base.Close();

            CloseAndReturnToGame();
        }

        private void CloseAndReturnToGame()
        {
            gameObject.SetActive(false);
            
            GameWindow gameWindow = ServiceLocator.Get<InterfaceDispatcher>().Open<GameWindow>();
            gameWindow.Unpause();
        }

        private void SwitchSound()
        {
            _isMuted = !_isMuted;

            ServiceLocator.Get<SoundController>().Volume = _isMuted ? 0 : 1;

            _soundButtonImage.sprite = _isMuted ? _unmutedSprite : _mutedSprite;
        }

        private void OpenMenu()
        {
            ServiceLocator.Get<InterfaceDispatcher>().Open<MainMenuWindow>();
            ServiceLocator.Get<FieldController>().SetFieldVisibility(false);

            gameObject.SetActive(false);
        }

        private void Restart()
        {
            ServiceLocator.Get<FieldController>().ResetField();
            ServiceLocator.Get<Timer>().Reset();
            ServiceLocator.Get<Score>().Reset();

            CloseAndReturnToGame();
        }
    }
}