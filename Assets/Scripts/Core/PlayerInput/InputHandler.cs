using System;
using System.Collections.Generic;
using Core.Api;
using UnityEngine;

namespace Core.PlayerInput
{
    public class InputHandler : IUpdateListener
    {
        private bool _isDisabled;
        public event Action<Cell> OnCellClicked;
        public event Action OnDeselected;

        void IUpdateListener.OnUpdate()
        {
            if (_isDisabled)
                return;

            ListenInput();
        }

        public void Disable()
        {
            _isDisabled = true;
        }

        public void Enable()
        {
            _isDisabled = false;
        }

        private void ListenInput()
        {
            if (Input.GetMouseButtonDown(0))
            {
                Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                RaycastHit2D[] hits = Physics2D.RaycastAll(mousePos, Vector2.zero);

                if (hits.Length > 0)
                {
                    Cell topCell = null;
                    int highestOrder = int.MinValue;

                    foreach (var hit in hits)
                    {
                        if (hit.collider.TryGetComponent<Cell>(out Cell cell))
                        {
                            int currentOrder = cell.IconRenderer.sortingOrder;
                            if (currentOrder > highestOrder)
                            {
                                highestOrder = currentOrder;
                                topCell = cell;
                            }
                        }
                    }

                    if (topCell)
                    {
                        OnCellClicked?.Invoke(topCell);
                        return;
                    }
                }

                OnDeselected?.Invoke();
            }
        }
    }
}