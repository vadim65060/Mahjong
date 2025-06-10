using UnityEngine;

namespace Core
{
    public class Cell : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer _backgroundRenderer;
        [SerializeField] private SpriteRenderer _iconRenderer;

        private CellAtlas.CellType _type;

        public CellAtlas.CellType Type
        {
            get => _type;
            set
            {
                var atlas = ServiceLocator.Get<CellAtlas>();
                _iconRenderer.sprite = atlas.Atlas[(int)value].Sprite;
                _type = value;
            }
        }

        public Vector2Int Position { get; set; }
        public int Layer { get; set; }

        public SpriteRenderer BackgroundRenderer => _backgroundRenderer;
        public SpriteRenderer IconRenderer => _iconRenderer;
    }
}