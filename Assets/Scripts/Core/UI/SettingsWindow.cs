using UnityEngine;
using UnityEngine.UI;

namespace Core.UI
{
    public class SettingsWindow : BaseWindow
    {
        [SerializeField] private Button _closeButton2;
        [SerializeField] private Button _soundButton;
        [SerializeField] private Button _musicButton;
        [SerializeField] private Button _musicOffButton;
        [SerializeField] private Button _musicOnButton;
        [SerializeField] private Button _soundOffButton;
        [SerializeField] private Button _soundOnButton;
        [SerializeField] private Slider _musicSlider;
        [SerializeField] private Slider _soundSlider;

        // [SerializeField] private Image _soundButtonImage;
        // [SerializeField] private Sprite _unmutedSprite;
        // [SerializeField] private Sprite _mutedSprite;

        private bool _isMusicMuted;
        private bool _isSoundMuted;
        private float _prevVolume;
        private BaseWindow _prev;

        private bool IsMuted => ServiceLocator.Get<SoundController>().Volume > 0;

        public void SetPrev(BaseWindow prev)
        {
            _prev = prev;
        }

        protected override void OnAwake()
        {
            _closeButton2?.onClick.AddListener(Close);
            _musicButton?.onClick.AddListener(SwitchMusic);
            _soundButton?.onClick.AddListener(SwitchSound);

            _musicOnButton?.onClick.AddListener(() => SwitchMusic(true));
            _musicOffButton?.onClick.AddListener(() => SwitchMusic(false));
            _soundOnButton?.onClick.AddListener(() => SwitchSound(true));
            _soundOffButton?.onClick.AddListener(() => SwitchSound(false));

            _musicSlider?.onValueChanged.AddListener(SetMusicVolume);
            if (_musicSlider != null) _musicSlider.value = ServiceLocator.Get<SoundController>().Volume;

            _soundSlider?.onValueChanged.AddListener(SetSoundVolume);
            if (_soundSlider != null) _soundSlider.value = ServiceLocator.Get<ClickSoundController>().Volume;
            
            // _prevVolume = _musicSlider.value;
            // _musicButton?.onClick.AddListener(SwitchMusic);
            // _soundButton?.onClick.AddListener(SwitchSound);
        }

        protected override void Close()
        {
            base.Close();

            _prev.gameObject.SetActive(true);
            if (_prev is GameWindow gameWindow)
            {
                gameWindow.Unpause();
            }
        }

        private void SetMusicVolume(float value)
        {
            ServiceLocator.Get<SoundController>().Volume = value;
        }

        private void SetSoundVolume(float value)
        {
            ServiceLocator.Get<ClickSoundController>().Volume = value;
        }
        
        private void SwitchMusic(bool isOn)
        {
            _isMusicMuted = isOn;
            _musicOnButton?.gameObject.SetActive(!isOn);
            _musicOffButton?.gameObject.SetActive(isOn);
            SwitchMusic();
        }
        
        private void SwitchSound(bool isOn)
        {
            _isSoundMuted = isOn;
            _soundOnButton?.gameObject.SetActive(!isOn);
            _soundOffButton?.gameObject.SetActive(isOn);
            SwitchSound();
        }

        private void SwitchSound()
        {
            _isSoundMuted = !_isSoundMuted;

            ServiceLocator.Get<ClickSoundController>().Volume = _isSoundMuted ? 0 : 1;

            // _soundMutedSprite.gameObject.SetActive(_isSoundMuted);
            // _soundButton.image.sprite = _isSoundMuted ? _mutedSprite : _unmutedSprite;
        }

        private void SwitchMusic()
        {
            _isMusicMuted = !_isMusicMuted;

            ServiceLocator.Get<SoundController>().Volume = _isMusicMuted ? 0 : 1;

            // _musicMutedSprite.gameObject.SetActive(_isMusicMuted);
            // _musicButton.image.sprite = _isMusicMuted ? _mutedSprite : _unmutedSprite;
        }
    }
}