using System;
using System.Collections.Generic;
using UnityEngine;

namespace Core
{
    public class LevelGenerator : MonoBehaviour
    {
        public Cell[,,] Field { get; private set; }

        [SerializeField] private GameObject _cellPrefab;
        [SerializeField] private Transform _cellsParent;
        [SerializeField] private Vector2 _cellSize = new(1, 1);
        [SerializeField] private Vector3 _cellOffset = new(.1f, .1f, .1f);

        private readonly int _typeCount = Enum.GetValues(typeof(CellAtlas.CellType)).Length - 1;
        private readonly List<Cell> _allCells = new();
        private readonly System.Random _random = new();

        private Vector2Int _fieldSize = new(10, 10);
        private int _layersCount = 5;
        private CellPool _cellPool;
        private LevelScoreConstraints _constraints;
        private List<LevelScoreConstraints.LevelSettings> _levelsSettings;

        public IReadOnlyList<Cell> AllCells => _allCells;
        public int CellCount => _allCells.Count;

        public void GenerateLevel()
        {
            _cellPool ??= ServiceLocator.Get<CellPool>();
            _levelsSettings ??= ServiceLocator.Get<LevelScoreConstraints>()._map;
            
            int levelId = ServiceLocator.Get<LevelLoader>().CurrentLevelIndex;
            _fieldSize = _levelsSettings[levelId]._fieldSize;
            _layersCount = _levelsSettings[levelId]._layersCount;

            ClearLevel();
            InitializeGrid();
        }

        public void DeleteCell(Cell cell)
        {
            _allCells.Remove(cell);
            _cellPool.Push(cell);
        }

        public bool IsCellAvailable(Cell cell)
        {
            for (int l = cell.Layer + 1; l < _layersCount; l++)
            {
                if (Field[cell.Position.x, cell.Position.y, l] &&
                    Field[cell.Position.x, cell.Position.y, l].gameObject.activeSelf)
                    return false;
            }
            int x = cell.Position.x;
            int y = cell.Position.y;
            int layer = cell.Layer;
            
            bool leftFree = x == 0 || Field[x - 1, y, layer] == null || !Field[x - 1, y, layer].gameObject.activeSelf;
            bool rightFree = x == _fieldSize.x - 1 || Field[x + 1, y, layer] == null ||
                             !Field[x + 1, y, layer].gameObject.activeSelf;

            return leftFree || rightFree;
        }

        private void InitializeGrid()
        {
            Field = new Cell[_fieldSize.x, _fieldSize.y, _layersCount];
            
            for (int layer = 0; layer < _layersCount; layer++)
            {
                CreateGuaranteedPairsLayer(layer);
            }
        }

        private void CreateGuaranteedPairsLayer(int layer)
        {
            List<Vector2Int> availablePositions = new List<Vector2Int>();
            for (int x = 0; x < _fieldSize.x; x++)
            {
                for (int y = 0; y < _fieldSize.y; y++)
                {
                    availablePositions.Add(new Vector2Int(x, y));
                }
            }
            
            Shuffle(availablePositions);
            int size = availablePositions.Count;

            for (int i = 0; i < size / 2; i++)
            {
                Vector2Int pos1 = availablePositions[0];
                availablePositions.RemoveAt(0);
                Vector2Int pos2 = availablePositions[0];
                availablePositions.RemoveAt(0);

                CellAtlas.CellType type = (CellAtlas.CellType)(i % _typeCount);
                CreateCell(pos1.x, pos1.y, layer, type);
                CreateCell(pos2.x, pos2.y, layer, type);
            }
        }

        private void CreateCell(int i, int j, int layer, CellAtlas.CellType type)
        {
            Cell cell = _cellPool.Pop();

            cell.Position = new Vector2Int(i, j);
            cell.Layer = layer;
            cell.Type = type;
            cell.transform.position = CalculateWorldPos(i, j, layer);
            cell.IconRenderer.sortingOrder = layer * 2 + 5;
            cell.BackgroundRenderer.sortingOrder = layer * 2 + 4;

            Field[i, j, layer] = cell;
            _allCells.Add(cell);
        }

        private Vector3 CalculateWorldPos(int i, int j, int layer)
        {
            float x = i * (_cellSize.x + _cellOffset.x) - (_fieldSize.x - 1) / 2f * (_cellSize.x + _cellOffset.x);
            float y = j * (_cellSize.y + _cellOffset.y) - _fieldSize.y / 2f * (_cellSize.y + _cellOffset.y) -
                      layer * _cellOffset.z;
            return new(x, y, layer);
        }

        private void Shuffle<T>(List<T> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = _random.Next(n + 1);
                (list[k], list[n]) = (list[n], list[k]);
            }
        }

        private void ClearLevel()
        {
            foreach (var cell in _allCells)
            {
                if (cell != null) _cellPool.Push(cell);
            }

            _allCells.Clear();
        }
    }
}