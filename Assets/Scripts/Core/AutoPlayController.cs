using System;
using System.Collections.Generic;
using Core.Api;
using DG.Tweening;

namespace Core
{
    public class AutoPlayController : IService
    {
        public event Action OnAutoPlayStarted;
        public event Action OnAutoPlayEnded;

        private readonly LevelGenerator _levelGenerator;
        private readonly Score _score = ServiceLocator.Get<Score>();
        private readonly FieldController _fieldController = ServiceLocator.Get<FieldController>();
        private readonly float _moveDelay;

        private bool _isRunning;
        private Sequence _autoPlaySequence;

        public AutoPlayController(LevelGenerator levelGenerator, float moveDelay = 0.5f)
        {
            _levelGenerator = levelGenerator;
            _moveDelay = moveDelay;
        }

        public void ToggleAutoPlay()
        {
            if (_isRunning)
            {
                StopAutoPlay();
            }
            else
            {
                StartAutoPlay();
            }
        }

        public void AbortAutoPlay()
        {
            OnAutoPlayEnded?.Invoke();
            _isRunning = false;
            _autoPlaySequence.Kill();
            _autoPlaySequence = null;
        }

        private void StopAutoPlay()
        {
            if (!_isRunning) return;

            OnAutoPlayEnded?.Invoke();
            _isRunning = false;
            _autoPlaySequence = null;
        }

        private void StartAutoPlay()
        {
            if (_isRunning || _levelGenerator.CellCount == 0) return;

            OnAutoPlayStarted?.Invoke();
            _isRunning = true;
            _autoPlaySequence = DOTween.Sequence();
            RunAutoPlayStep();
        }

        private void RunAutoPlayStep()
        {
            if (!_isRunning || _levelGenerator.CellCount == 0)
            {
                _isRunning = false;
                return;
            }

            var availablePair = FindAvailablePair();
            if (availablePair == null)
            {
                _isRunning = false;
                return;
            }

            var (cell1, cell2) = availablePair.Value;

            _autoPlaySequence = DOTween.Sequence()
                .Join(cell1.transform.DOScale(1.2f, _moveDelay / 2))
                .Join(cell2.transform.DOScale(1.2f, _moveDelay / 2))
                .AppendInterval(_moveDelay)
                .Join(cell1.transform.DOScale(0f, _moveDelay / 2))
                .Join(cell2.transform.DOScale(0f, _moveDelay / 2))
                .AppendCallback(() =>
                {
                    _levelGenerator.DeleteCell(cell1);
                    _levelGenerator.DeleteCell(cell2);
                    _score.OnMatch();
                    _fieldController.UpdateCellsColor();
                })
                .AppendInterval(_moveDelay)

                // Следующий шаг
                .OnComplete(RunAutoPlayStep);
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
}