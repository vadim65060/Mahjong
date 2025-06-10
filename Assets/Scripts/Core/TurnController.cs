using System;
using System.Collections.Generic;
using System.Linq;
using Core.Api;
using Core.PlayerInput;
using Core.UI;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Core
{
    public class TurnController : IService
    {
        private readonly InputHandler _input;
        private readonly FieldController _field;
        private readonly AutoPlayController _autoPlay;
        private bool _playerControl;

        public TurnController()
        {
            _input = ServiceLocator.Get<InputHandler>();
            _field = ServiceLocator.Get<FieldController>();
            _autoPlay = ServiceLocator.Get<AutoPlayController>();

            _input.OnCellClicked += Select;
            _input.OnDeselected += Deselect;

            _playerControl = true;

            _field.OnAnimationStateStarted += OnFieldAnimStart;
            _field.OnAnimationStateEnded += OnFieldAnimEnd;

            _autoPlay.OnAutoPlayStarted += OnFieldAnimStart;
            _autoPlay.OnAutoPlayEnded += OnFieldAnimEnd;
        }

        public void OnTurn()
        {
            _playerControl = true;
        }

        private void OnFieldAnimEnd()
        {
            OnTurn();
        }

        private void OnFieldAnimStart()
        {
            _playerControl = false;
        }

        private void Deselect()
        {
            if (!_playerControl)
                return;

            _field.Deselect();
        }

        private void Select(Cell clicked)
        {
            if (!_playerControl)
                return;

            _field.OnCellClicked(clicked);
        }
    }

    public class CellPool : IService
    {
        private const int InitialAmount = 5;

        private readonly Cell _reference;
        private readonly Transform _parent;
        private readonly List<Cell> _pool = new();

        public CellPool(Cell reference, Transform parent = null)
        {
            _reference = reference;
            _parent = parent;
            for (int i = 0; i < InitialAmount; i++)
            {
                Cell instance = Object.Instantiate(reference, parent);
                instance.gameObject.SetActive(false);

                _pool.Add(instance);
            }
        }

        public void Push(Cell cell)
        {
            cell.Type = CellAtlas.CellType.None;
            cell.transform.localScale = Vector3.one;
            cell.transform.rotation = Quaternion.identity;
            cell.gameObject.SetActive(false);
            _pool.Add(cell);
        }

        public Cell Pop()
        {
            if (_pool.Count == 0)
                return Object.Instantiate(_reference, _parent);

            Cell instance = _pool[0];
            _pool.RemoveAt(0);
            instance.gameObject.SetActive(true);

            return instance;
        }
    }

    public class LevelLoader : IService
    {
        private readonly FieldController _field = ServiceLocator.Get<FieldController>();
        private readonly Timer _timer = ServiceLocator.Get<Timer>();
        private readonly Score _score = ServiceLocator.Get<Score>();
        private readonly UpdateProcessor _processor = ServiceLocator.Get<UpdateProcessor>();
        private GameEndListener _listener;

        public int CurrentLevelIndex { get; private set; }


        public event Action OnLoad;

        public void LoadLevel(int index)
        {
            CurrentLevelIndex = index;

            _timer.Start();
            _processor.SetPauseState(isPaused: false);
            _field.SetFieldVisibility(isVisible: true);
            _field.OnNewLevel();
            _timer.Reset();
            _score.Reset();
            _listener.Reset();

            OnLoad?.Invoke();
        }

        public void SetListener(GameEndListener listener)
        {
            _listener = listener;
        }
    }

    public class Score : IService
    {
        private const float MatchExtraCellLinearModifier = 1.1f;
        private const int DefaultMatchValue = 100;

        private int _score;

        public int Current => _score;

        public event Action<int> OnValueChanged;

        public void OnMatch()
        {
            int extraCells = 0;

            _score += DefaultMatchValue +
                      Mathf.RoundToInt(extraCells * (DefaultMatchValue * MatchExtraCellLinearModifier));

            OnValueChanged?.Invoke(_score);
        }

        public void Reset()
        {
            _score = 0;
            OnValueChanged?.Invoke(_score);
        }
    }

    public class Timer : IUpdateListener
    {
        private float _timeElapsed;
        private bool _isStarted;

        public int Current => Mathf.RoundToInt(_timeElapsed);

        void IUpdateListener.OnUpdate()
        {
            if (!_isStarted)
                return;

            _timeElapsed += Time.deltaTime;
        }

        public void Start() => _isStarted = false; // TIMER OFF!!!
        public void Stop() => _isStarted = false;
        public void Reset() => _timeElapsed = 0;
    }

    public class InterfaceDispatcher : IService
    {
        private readonly List<BaseWindow> _windows;
        private readonly Canvas _canvas;
        private readonly Dictionary<Type, BaseWindow> _onScene = new();

        public InterfaceDispatcher(List<BaseWindow> windows, Canvas canvas)
        {
            _windows = windows;
            _canvas = canvas;
        }

        public T Open<T>() where T : BaseWindow
        {
            if (!_onScene.ContainsKey(typeof(T)))
                _onScene[typeof(T)] = Object.Instantiate(_windows.First(window => window.GetType() == typeof(T)),
                    _canvas.transform);

            _onScene[typeof(T)].gameObject.SetActive(true);

            return _onScene[typeof(T)] as T;
        }

        public T Get<T>() where T : BaseWindow
        {
            if (!_onScene.ContainsKey(typeof(T)))
                _onScene[typeof(T)] = Object.Instantiate(_windows.First(window => window.GetType() == typeof(T)),
                    _canvas.transform);

            return _onScene[typeof(T)] as T;
        }
    }

    public class GameEndListener : IUpdateListener
    {
        public const int TimeConstraintInSeconds = 90;

        private readonly LevelScoreConstraints _constraints;
        private readonly Timer _timer;
        private readonly LevelLoader _levelLoader;
        private readonly FieldController _field;
        private bool _isInvoked;

        public event Action OnGameEnd;

        public GameEndListener(LevelLoader levelLoader)
        {
            ServiceLocator.Get<Score>().OnValueChanged += CheckGameEnd;
            _timer = ServiceLocator.Get<Timer>();
            _constraints = ServiceLocator.Get<LevelScoreConstraints>();
            _field = ServiceLocator.Get<FieldController>();
            _levelLoader = levelLoader;
        }

        private void CheckGameEnd(int _)
        {
            if (_isInvoked || !_field.IsFieldEmpty) return;

            _isInvoked = true;
            OnGameEnd?.Invoke();
            ServiceLocator.Get<Timer>().Stop();
        }

        void IUpdateListener.OnUpdate()
        {
            if (!_isInvoked && _timer.Current > TimeConstraintInSeconds)
            {
                _isInvoked = true;
                OnGameEnd?.Invoke();
                ServiceLocator.Get<Timer>().Stop();
            }
        }

        public void Reset() => _isInvoked = false;
    }

    public class WinResolver : IService
    {
        private readonly GameEndListener _listener;
        private readonly LevelScoreConstraints _constraints;
        private readonly LevelLoader _level;
        private readonly Score _score;

        public enum GameResult
        {
            Win,
            Lose
        }

        public event Action<GameResult> OnGameResult;

        public WinResolver()
        {
            _listener = ServiceLocator.Get<GameEndListener>();
            _constraints = ServiceLocator.Get<LevelScoreConstraints>();
            _level = ServiceLocator.Get<LevelLoader>();
            _score = ServiceLocator.Get<Score>();

            _listener.OnGameEnd += GetResult;
        }

        private void GetResult()
        {
            if (!ServiceLocator.Get<FieldController>().IsFieldEmpty)
            {
                OnGameResult?.Invoke(GameResult.Lose);

                return;
            }

            OnGameResult?.Invoke(GameResult.Win);
        }
    }

    public class GameController : IService
    {
        private readonly LevelScoreConstraints _constraints;
        private readonly LevelLoader _loader;
        private readonly WinResolver _resolver;
        private readonly InterfaceDispatcher _ui;
        private readonly Score _score;
        private readonly bool[] _openedLevels;

        public bool[] OpenedLevels => _openedLevels;
        public event Action OnNewLevelOpened;

        public GameController()
        {
            _constraints = ServiceLocator.Get<LevelScoreConstraints>();
            _loader = ServiceLocator.Get<LevelLoader>();
            _resolver = ServiceLocator.Get<WinResolver>();
            _ui = ServiceLocator.Get<InterfaceDispatcher>();
            _score = ServiceLocator.Get<Score>();
            _openedLevels = new bool[_constraints._map.Count];
            _openedLevels[0] = true;

            _resolver.OnGameResult += TryOpenLevel;
        }

        private void TryOpenLevel(WinResolver.GameResult result)
        {
            ServiceLocator.Get<FieldController>().SetFieldVisibility(false);
            EndGameWindow endWindow = _ui.Open<EndGameWindow>();
            endWindow.SetPrev(_ui.Open<GameWindow>());
            endWindow.SetResult(result);

            if (result == WinResolver.GameResult.Win)
            {
                if (_loader.CurrentLevelIndex + 1 >= _constraints._map.Count)
                {
                    endWindow.ButtonText = "To level menu";
                    endWindow.Init(WinResolver.GameResult.Win, true);
                }
                else
                {
                    endWindow.Init(WinResolver.GameResult.Win, false);
                    _openedLevels[_loader.CurrentLevelIndex + 1] = true;
                    OnNewLevelOpened?.Invoke();
                }

                endWindow.Score = _score.Current.ToString();
                return;
            }

            endWindow.Init(WinResolver.GameResult.Lose, false);
        }
    }

    public class SoundController : IService
    {
        protected readonly AudioSource Source;
        private readonly float _startVolume;

        public float Volume
        {
            get => Source.volume / _startVolume;
            set => Source.volume = value * _startVolume;
        }

        public SoundController(AudioSource source)
        {
            Source = source;
            _startVolume = Source.volume;
        }
    }

    public class ClickSoundController : SoundController
    {
        public ClickSoundController(AudioSource source) : base(source)
        {
        }

        public void Play()
        {
            Source.Play();
        }
    }
}