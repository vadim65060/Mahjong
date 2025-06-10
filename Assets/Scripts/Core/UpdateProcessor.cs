using System.Collections.Generic;
using Core.Api;
using UnityEngine;

namespace Core
{
    public class UpdateProcessor : MonoBehaviour, IService
    {
        private readonly LinkedList<IUpdateListener> _updateListeners = new LinkedList<IUpdateListener>();
        private readonly LinkedList<IGamePauseListener> _pauseListeners = new LinkedList<IGamePauseListener>();
        private readonly LinkedList<IGameResumeListener> _resumeListenersListeners = new LinkedList<IGameResumeListener>();
        private bool _isPaused;

        public readonly struct Binding
        {
            private readonly LinkedList<IUpdateListener> _update;
            private readonly LinkedList<IGamePauseListener> _pause;
            private readonly LinkedList<IGameResumeListener> _resume;
            private readonly IGameListener _bindTarget;

            public Binding(LinkedList<IUpdateListener> update, LinkedList<IGamePauseListener> pause, LinkedList<IGameResumeListener> resume, 
                IGameListener bindTarget)
            {
                _update = update;
                _pause = pause;
                _resume = resume;
                _bindTarget = bindTarget;
            }

            public void AsUpdateListener()
            {
                IUpdateListener casted = _bindTarget as IUpdateListener;

                if (casted == null)
                    return;

                _update.AddLast(casted);
            }

            public void AsPauseListener()
            {
                IGamePauseListener casted = _bindTarget as IGamePauseListener;

                if (casted == null)
                    return;

                _pause.AddLast(casted);
            }

            public void AsResumeListener()
            {
                IGameResumeListener casted = _bindTarget as IGameResumeListener;

                if (casted == null)
                    return;

                _resume.AddLast(casted);
            }

            public void AsAll()
            {
                AsUpdateListener();
                AsPauseListener();
                AsResumeListener();
            }
        }

        public Binding Bind(IGameListener listener) => new Binding(_updateListeners, _pauseListeners, _resumeListenersListeners, bindTarget: listener);

        public void SetPauseState(bool isPaused)
        {
            _isPaused = isPaused;

            if (_isPaused)
            {
                foreach (IGamePauseListener listener in _pauseListeners)
                {
                    listener.OnPause();
                }
            }
            else
            {
                foreach (IGameResumeListener listener in _resumeListenersListeners)
                {
                    listener.OnResume();
                }
            }
        }

        private void Update()
        {
            if (_isPaused)
                return;

            foreach (IUpdateListener listener in _updateListeners)
            {
                listener.OnUpdate();
            }
        }
    }
}