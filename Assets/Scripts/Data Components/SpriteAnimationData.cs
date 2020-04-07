using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

namespace Core
{
    [GenerateAuthoringComponent]
    public struct SpriteAnimationData : IComponentData
    {
        public Vector2Int currentCell;
        public float frameTimer;
        public float frameTimerMax;
        public Vector4 uv;
        public Matrix4x4 matrix;
        public SpriteSheetInfo.Animations animation;
        public SpriteSheetInfo.Direction direction;
    }
}
