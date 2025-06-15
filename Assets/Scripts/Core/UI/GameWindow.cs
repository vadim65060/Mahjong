using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Core.UI
{
    public class GameWindow : BaseWindow
    {
        [SerializeField] private Button _settingsButton;
        [SerializeField] private Button _restartButton;
        [SerializeField] private Button _autoPlayButton;
        [SerializeField] private TMP_Text _scoreText;
        [SerializeField] private TMP_Text _timeText;
        [SerializeField] private TMP_Text _levelText;

        private Score _score;
        private FieldController _field;
        private AutoPlayController _autoPlay;
        private Timer _timer;
        private LevelScoreConstraints _constraints;
        private LevelLoader _level;
        private bool _isPaused;
        private int _stage;

        public bool IsPaused
        {
            get => _isPaused;
            private set
            {
                ServiceLocator.Get<UpdateProcessor>().SetPauseState(isPaused: value);
                _isPaused = value;
            }
        }

        public int LevelText
        {
            set
            {
                if (!_levelText)
                    return;

                switch (value)
                {
                    case 1:
                        _levelText.text = "Easy";
                        break;
                    case 2:
                        _levelText.text = "Middle";
                        break;
                    case 3:
                        _levelText.text = "Hard";
                        break;
                }
            }
        }

        public float Time
        {
            set
            {
                if (!_timeText) return;

                int minutes = (int)value / 60;
                int seconds = Mathf.Clamp((int)value % 60, 0, 59);

                if (seconds < 10)
                    _timeText.text = $"{minutes}:0{seconds}";
                else
                    _timeText.text = $"{minutes}:{seconds}";
            }
        }

        public int Score
        {
            set
            {
                // int neededScore = _constraints.Map.First(pair => pair.LevelId.Equals(_level.CurrentLevelIndex)).Score;
                if (_scoreText) _scoreText.text = $"{value}";
            }
        }

        public void Unpause()
        {
            IsPaused = false;
            ServiceLocator.Get<FieldController>().SetFieldVisibility(true);
        }

        protected override void OnAwake()
        {
            base.OnAwake();
            _score = ServiceLocator.Get<Score>();
            _timer = ServiceLocator.Get<Timer>();
            _field = ServiceLocator.Get<FieldController>();
            _autoPlay = ServiceLocator.Get<AutoPlayController>();
            _score.OnValueChanged += SetScore;
            _constraints = ServiceLocator.Get<LevelScoreConstraints>();
            _level = ServiceLocator.Get<LevelLoader>();
            Score = 0;
            _settingsButton?.onClick.AddListener(OpenSettings);
            _restartButton?.onClick.AddListener(Restart);
            _autoPlayButton?.onClick.AddListener(_autoPlay.ToggleAutoPlay);
        }

        protected override void OnUpdate()
        {
            base.OnUpdate();

            Time = _timer.Current;
        }

        protected override void Close()
        {
            IsPaused = true;

            ServiceLocator.Get<InterfaceDispatcher>().Open<LevelMenuWindow>();
            ServiceLocator.Get<FieldController>().SetFieldVisibility(false);

            gameObject.SetActive(false);
        }
        
        private void Restart()
        {
            _autoPlay.AbortAutoPlay();
            _score.Reset();
            _field.ResetField();
        }

        private void Pause()
        {
            IsPaused = true;

            ServiceLocator.Get<InterfaceDispatcher>().Open<PauseWindow>();
            ServiceLocator.Get<FieldController>().SetFieldVisibility(false);

            gameObject.SetActive(false);
        }

        private void OpenSettings()
        {
            IsPaused = true;

            ServiceLocator.Get<InterfaceDispatcher>().Open<SettingsWindow>();
            ServiceLocator.Get<InterfaceDispatcher>().Get<SettingsWindow>().SetPrev(this);
            ServiceLocator.Get<FieldController>().SetFieldVisibility(false);

            gameObject.SetActive(false);
        }

        private void SetScore(int val)
        {
            Score = val;
        }
    }
}