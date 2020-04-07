using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Core
{
    public class SpriteSheetInfo : MonoBehaviour
    {
        [System.Serializable]
        private struct SpriteSheet
        {
            public string name;
            public Sprite texture;
            public int numRows;
            public int numCols;
            public SpriteSheetAnimation[] animations;
        }

        [System.Serializable]
        public struct SpriteSheetAnimation
        {
            public string name;
            public Vector2Int startCell;
            public Vector2Int endCell;
        }

        [SerializeField] private SpriteSheet[] _spriteSheets;

        public enum Animations
        {
            SPARTAN_IDLE,
            SAPRTAN_WALKING,
            SPARTAN_ATTACK,
        }

        public enum Direction
        {
            DOWN,
            DOWN_RIGHT,
            RIGHT,
            UP_RIGHT,
            UP,
            UP_LEFT,
            LEFT,
            DOWN_LEFT
        }

        public Sprite GetSpriteSheetTexture(Animations animation)
        {
            return _spriteSheets[(int)animation].texture;
        }

        public SpriteSheetAnimation GetSpriteSheetAnimation(Animations animation, Direction direction)
        {
            return _spriteSheets[(int)animation].animations[(int)direction];
        }
    }
}
