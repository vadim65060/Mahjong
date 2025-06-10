using UnityEngine;

namespace Utils
{
    public static class VectorExtensions
    {
        public static Vector2 WithX(this Vector2 a, float val) => new Vector2(val, a.y);
        public static Vector2 WithY(this Vector2 a, float val) => new Vector2(a.x, val);
        public static Vector2 WithX(this Vector2 a, int val) => new Vector2(val, a.y);
        public static Vector2 WithY(this Vector2 a, int val) => new Vector2(a.x, val);
    }
}