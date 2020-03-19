using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

[GenerateAuthoringComponent]
public struct SpriteAnimationData : IComponentData
{
    public int currentFrame;
    public int minFrame;
    public int maxFrame;
    public int frameCount;
    public float frameTimer;
    public float frameTimerMax;
    public Vector4 uv;
    public Matrix4x4 matrix;
}
