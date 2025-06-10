using System;
using Core.Api;
using DG.Tweening;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

namespace Core
{
    public class FieldController : IService
    {
        private readonly Cell _reference;
        private readonly LevelGenerator _levelGenerator;
        private readonly GameObject _fieldParent;
        private readonly GameObject _fieldBg;
        private readonly Score _score = ServiceLocator.Get<Score>();
        private readonly Color _openColor = new(1, 1, 1, 1);
        private readonly Color _closeColor = new(.8f, .8f, .8f, .5f);

        private Cell _selected;
        private Cell _firstChange;
        private Cell _secondChange;
        private Cell[,,] _field;

        public event Action OnAnimationStateStarted;
        public event Action OnAnimationStateEnded;

        public FieldController(GameObject fieldParent, LevelGenerator levelGenerator, GameObject fieldBg = null)
        {
            _fieldParent = fieldParent;
            _fieldBg = fieldBg;
            _levelGenerator = levelGenerator;
        }

        public bool IsFieldEmpty => _levelGenerator.CellCount == 0;

        public void OnNewLevel()
        {
            StartLevel(true);
        }

        public void OnCellClicked(Cell cell)
        {
            if (_selected == null)
            {
                Select(cell);
                ServiceLocator.Get<ClickSoundController>().Play();
            }
            else if (_selected != null && _selected != cell && _levelGenerator.IsCellAvailable(cell))
            {
                if (_selected.Type == cell.Type)
                {
                    DeleteCells(_selected, cell);
                    ServiceLocator.Get<ClickSoundController>().Play();

                }

                Deselect();
            }
            else
            {
                Deselect();
            }
        }


        public void Deselect()
        {
            if (_selected != null)
                _selected.transform.localScale = Vector3.one;

            _selected = null;
        }

        public void SetFieldVisibility(bool isVisible)
        {
            if (_fieldBg) _fieldBg.SetActive(isVisible);

            if (_field == null) return;

            foreach (var cell in _field)
            {
                if (!cell) continue;

                cell.BackgroundRenderer.enabled = isVisible;
                cell.IconRenderer.enabled = isVisible;
                cell.GetComponent<Collider2D>().enabled = isVisible;
            }
        }

        public void ResetField() => StartLevel();

        public void UpdateCellsColor()
        {
            foreach (var cell in _field)
            {
                if (!cell || cell.Type == CellAtlas.CellType.None) continue;

                bool isAvailable = _levelGenerator.IsCellAvailable(cell);
                cell.IconRenderer.color = isAvailable ? _openColor : _closeColor;
                cell.BackgroundRenderer.color = isAvailable ? _openColor : _closeColor;
            }
        }


        private void Select(Cell cell)
        {
            if (_levelGenerator.IsCellAvailable(cell))
            {
                _selected = cell;
                _selected.transform.DOScale(Vector3.one * 1.2f, .1f);
            }
            else
            {
                Sequence seq = DOTween.Sequence();
                seq.Append(cell.transform.DOScale(Vector3.one * 1.2f, .1f));
                seq.Append(cell.transform.DOScale(Vector3.one, .1f));
                seq.Play();
            }
        }

        private void DeleteCells(Cell first, Cell second)
        {
            OnAnimationStateStarted?.Invoke();

            first.transform.DOScale(0f, 0.2f);
            second.transform.DOScale(0f, 0.2f).onComplete += () =>
            {
                _levelGenerator.DeleteCell(first);
                _levelGenerator.DeleteCell(second);
                UpdateCellsColor();
                _score.OnMatch();

                OnAnimationStateEnded?.Invoke();
            };
        }


        private void StartLevel(bool isFirst = false)
        {
            OnAnimationStateStarted?.Invoke();
            _levelGenerator.GenerateLevel();
            _field = _levelGenerator.Field;
            UpdateCellsColor();
            
            DOTween.SetTweensCapacity(300, 150);

            foreach (var cell in _field)
            {
                if (!cell) continue;

                cell.gameObject.SetActive(true);

                Sequence seq = DOTween.Sequence();
                if (isFirst)
                {
                    cell.transform.localScale = Vector3.zero;
                }

                seq.Append(cell.transform.DOScale(Vector3.zero, .5f));
                seq.AppendInterval(.25f);
                seq.Append(cell.transform.DOScale(Vector3.one, .5f));
                seq.Play();
                OnAnimationStateEnded?.Invoke();
            }
        }
    }
}