using System;
using System.Collections.Generic;
using Core.Api;
using UnityEngine;

namespace Core
{
    [CreateAssetMenu(fileName = "LevelConstraints", menuName = "Game/LevelConstaints")]
    public class LevelScoreConstraints : ScriptableObject, IService
    {

        [Serializable]
        public struct LevelSettings
        {
            public int _levelId;
            public Vector2Int _fieldSize;
            public int _layersCount; 
        }

        public List<LevelSettings> _map;
    }
}