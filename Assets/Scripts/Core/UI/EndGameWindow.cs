using System.Collections;
using System.Collections.Generic;
using Core.PlayerInput;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Core.UI
{
    public class EndGameWindow : BaseWindow
    {
        [SerializeField] private Button _menuButton;
        [SerializeField] private Button _nextLevelButton;
        [SerializeField] private TMP_Text _label;
        [SerializeField] private TMP_Text _scoreText;
        [SerializeField] private TMP_Text _buttonText;
        [SerializeField] private List<GameObject> _activeOnWin;
        [SerializeField] private List<GameObject> _activeOnLose;

        private WinResolver.GameResult _result;
        private BaseWindow _prev;
        private bool _isLastLevel;

        public string Label
        {
            get => _label.text;
            set => _label.text = value;
        }

        public void SetResult(WinResolver.GameResult res)
        {
            if (res == WinResolver.GameResult.Win)
                SetWin();
            else
                SetLose();

            foreach (var o in _activeOnWin)
            {
                o.SetActive(res == WinResolver.GameResult.Win);
            }

            foreach (var o in _activeOnLose)
            {
                o.SetActive(res == WinResolver.GameResult.Lose);
            }
        }

        public string Score
        {
            get => _scoreText.text;
            set
            {
                if (_scoreText) _scoreText.text = $"{value}";
            }
        }

        public string ButtonText
        {
            get => _buttonText.text;
            set => _buttonText.text = value;
        }

        public void Init(WinResolver.GameResult res, bool isLastLevel)
        {
            _result = res;
            _isLastLevel = isLastLevel;
            StartCoroutine(HideFieldRoutine());
        }

        public void SetPrev(BaseWindow prev)
        {
            _prev = prev;
        }

        protected override void OnAwake()
        {
            _menuButton?.onClick.AddListener(OpenMenu);
            _nextLevelButton?.onClick.AddListener(OpenLevel);
        }

        protected override void OnEnableWindow()
        {
            StartCoroutine(HideFieldRoutine());
        }

        private IEnumerator HideFieldRoutine()
        {
            yield return null;
            ServiceLocator.Get<FieldController>().SetFieldVisibility(false);
        }

        private void OpenMenu()
        {
            ServiceLocator.Get<InterfaceDispatcher>().Open<LevelMenuWindow>();
            Close();
        }

        private void SetLose()
        {
            Label = "LOSE";
            ButtonText = "Repeat";
        }

        private void SetWin()
        {
            Label = "WINNER";
            ButtonText = "Next Level";
        }

        private void OnEnable()
        {
            ServiceLocator.Get<InputHandler>().Disable();
            StartCoroutine(ClosePrefRoutine());
        }

        private void OnDisable()
        {
            ServiceLocator.Get<InputHandler>().Enable();
        }

        private IEnumerator ClosePrefRoutine()
        {
            yield return null;

            _prev.gameObject.SetActive(false);
        }

        private void OpenLevel()
        {
            var loader = ServiceLocator.Get<LevelLoader>();

            if (_result == WinResolver.GameResult.Lose)
            {
                loader.LoadLevel(loader.CurrentLevelIndex);
                _prev.gameObject.SetActive(true);
                Close();
                // ServiceLocator.Get<Score>().Reset();
                // ServiceLocator.Get<Timer>().Reset();
            }
            else
            {
                if (_isLastLevel)
                {
                    ServiceLocator.Get<InterfaceDispatcher>().Open<LevelMenuWindow>();
                    Close();
                    return;
                }

                loader.LoadLevel(loader.CurrentLevelIndex + 1);
                if (_prev is GameWindow gameWindow)
                {
                    gameWindow.LevelText = loader.CurrentLevelIndex + 1;
                }

                _prev.gameObject.SetActive(true);
                Close();
            }
        }
    }
}