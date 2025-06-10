using System.Collections.Generic;
using Core.PlayerInput;
using Core.UI;
using UnityEngine;

namespace Core
{
    [DefaultExecutionOrder(-10000)]
    public class Entry : MonoBehaviour
    {
        [SerializeField] private List<BaseWindow> _ui;
        [SerializeField] private Canvas _canvas;
        [SerializeField] private GameObject _fieldParent;
        [SerializeField] private GameObject _fieldBg;
        [SerializeField] private Cell _cellReference;
        [SerializeField] private CellAtlas _cellAtlas;
        [SerializeField] private LevelScoreConstraints _levelConstraints;
        [SerializeField] private AudioSource _sound;
        [SerializeField] private AudioSource _clickSound;
        [SerializeField] private LevelGenerator _levelGenerator;

        private UpdateProcessor _updateProcessor;

        private void Awake()
        {
            _updateProcessor = GetComponent<UpdateProcessor>();

            InstallBindings();
        }

        private void Start()
        {
            ServiceLocator.Get<InterfaceDispatcher>().Open<MainMenuWindow>();
            // ServiceLocator.Get<InterfaceDispatcher>().Open<PrivacyDialogWindow>();
            ServiceLocator.Get<FieldController>().SetFieldVisibility(false);
        }

        private void InstallBindings()
        {
            ServiceLocator.Bind(_cellAtlas);
            ServiceLocator.Bind(_levelConstraints);
            ServiceLocator.Bind(_updateProcessor);
            ServiceLocator.Bind(new CellPool(_cellReference, _fieldParent.transform));
            ServiceLocator.Bind(new Score());
            ServiceLocator.Bind(new FieldController(_fieldParent, _levelGenerator, _fieldBg));
            ServiceLocator.Bind(new AutoPlayController(_levelGenerator));
            ServiceLocator.Bind(new InputHandler());
            ServiceLocator.Bind(new TurnController());
            ServiceLocator.Bind(new Timer());

            var levelLoader = new LevelLoader();
            ServiceLocator.Bind(levelLoader);
            ServiceLocator.Bind(new GameEndListener(levelLoader));
            levelLoader.SetListener(ServiceLocator.Get<GameEndListener>());

            ServiceLocator.Bind(new InterfaceDispatcher(_ui, _canvas));
            ServiceLocator.Bind(new WinResolver());
            ServiceLocator.Bind(new GameController());
            ServiceLocator.Bind(new SoundController(_sound));
            ServiceLocator.Bind(new ClickSoundController(_clickSound));

            _updateProcessor.Bind(ServiceLocator.Get<Timer>()).AsUpdateListener();
            _updateProcessor.Bind(ServiceLocator.Get<InputHandler>()).AsUpdateListener();
            _updateProcessor.Bind(ServiceLocator.Get<GameEndListener>()).AsUpdateListener();
        }
    }
}