using System;
using System.Collections;
using System.Collections.Generic;
using Core.Api;
using DG.Tweening;
using UnityEngine;

namespace Core
{
    public class AutoPlayController : IService
    {
        private readonly LevelGenerator _levelGenerator;
        private readonly FieldController _fieldController = ServiceLocator.Get<FieldController>();
        private readonly Score _score = ServiceLocator.Get<Score>();
        private readonly float _moveDelay;

        private bool _isRunning;

        public event Action OnAutoPlayStarted;
        public event Action OnAutoPlayEnded;

        public AutoPlayController(LevelGenerator levelGenerator, float moveDelay = 0.5f)
        {
            _levelGenerator = levelGenerator;
            _moveDelay = moveDelay;
        }

        public void ToggleAutoPlay()
        {
            if (_isRunning)
            {
                OnAutoPlayEnded?.Invoke();
                StopAutoPlay();
            }
            else
            {
                OnAutoPlayStarted?.Invoke();
                StartAutoPlay();
            }
        }

        private void StartAutoPlay()
        {
            if (_isRunning || _levelGenerator.CellCount == 0) return;

            _isRunning = true;
            CoroutineStarter.RunCoroutine(AutoPlayRoutine());
        }

        private void StopAutoPlay()
        {
            _isRunning = false;
        }

        private IEnumerator AutoPlayRoutine()
        {
            while (_isRunning && _levelGenerator.CellCount > 0)
            {
                var availablePair = FindAvailablePair();
                if (availablePair == null)
                {
                    yield return new WaitForSeconds(_moveDelay);
                    continue;
                }

                // Анимация выделения
                availablePair.Value.Item1.transform.DOScale(1.2f, _moveDelay / 2);
                availablePair.Value.Item2.transform.DOScale(1.2f, _moveDelay / 2);
                yield return new WaitForSeconds(_moveDelay);

                // Анимация удаления
                availablePair.Value.Item1.transform.DOScale(0f, _moveDelay / 2);
                availablePair.Value.Item2.transform.DOScale(0f, _moveDelay / 2);
                yield return new WaitForSeconds(_moveDelay);

                _levelGenerator.DeleteCell(availablePair.Value.Item1);
                _levelGenerator.DeleteCell(availablePair.Value.Item2);
                _score.OnMatch();
                _fieldController.UpdateCellsColor();

                yield return new WaitForSeconds(_moveDelay);
            }

            _isRunning = false;
        }

        private (Cell, Cell)? FindAvailablePair()
        {
            var availableCells = new List<Cell>();

            foreach (var cell in _levelGenerator.AllCells)
            {
                if (_levelGenerator.IsCellAvailable(cell))
                {
                    availableCells.Add(cell);
                }
            }

            int maxLayer = -1;
            (Cell, Cell)? pair = null;
            
            for (int i = 0; i < availableCells.Count; i++)
            {
                for (int j = i + 1; j < availableCells.Count; j++)
                {
                    if (availableCells[i].Type == availableCells[j].Type &&
                        availableCells[i].Layer + availableCells[j].Layer > maxLayer)
                    {
                        maxLayer = availableCells[i].Layer + availableCells[j].Layer;
                        pair = (availableCells[i], availableCells[j]);
                    }
                }
            }

            return pair;
        }
    }

    // Вспомогательный класс для запуска корутин из не-MonoBehaviour класса
    public class CoroutineStarter : MonoBehaviour
    {
        private static CoroutineStarter _instance;

        public static Coroutine RunCoroutine(IEnumerator coroutine)
        {
            if (_instance == null)
            {
                _instance = new GameObject("CoroutineStarter").AddComponent<CoroutineStarter>();
                DontDestroyOnLoad(_instance.gameObject);
            }

            return _instance.StartCoroutine(coroutine);
        }

        public static void StopRunningCoroutine(Coroutine coroutine)
        {
            if (_instance != null && coroutine != null)
            {
                _instance.StopCoroutine(coroutine);
            }
        }
    }
}