using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Core.UI
{
    public class MainMenuWindow : BaseWindow
    {
        [SerializeField] private Button _playButton;
        [SerializeField] private Button _settingsButton;

        protected override void OnAwake()
        {
            _playButton?.onClick.AddListener(OpenLevelMenu);
            _settingsButton?.onClick.AddListener(OpenSettings);
        }

        protected override void Close()
        {
            var exitWindow = ServiceLocator.Get<InterfaceDispatcher>().Open<ExitWindow>();
            exitWindow.SetPrevWindow(this);
            
            CloseWindow();
        }

        private void OpenSettings()
        {
            var settings = ServiceLocator.Get<InterfaceDispatcher>().Open<SettingsWindow>();
            settings.SetPrev(this);

            CloseWindow();
        }

        private void OpenLevelMenu()
        {
            ServiceLocator.Get<InterfaceDispatcher>().Open<LevelMenuWindow>();

            CloseWindow();
        }
        
        private void OpenLevel()
        {
            ServiceLocator.Get<LevelLoader>().LoadLevel(0);
            GameWindow gameWindow = ServiceLocator.Get<InterfaceDispatcher>().Open<GameWindow>();
            gameWindow.LevelText = 1;
            gameWindow.Unpause();

            CloseWindow();
        }

        private void CloseWindow() => gameObject.SetActive(false);
    }
}