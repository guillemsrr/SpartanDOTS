using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Jobs;
using Unity.Burst;
using Unity.Transforms;
using Unity.Collections;
using Unity.Mathematics;

[UpdateAfter(typeof(SpriteAnimationSystem))]
public class SpriteRendererSystem : ComponentSystem
{
    protected override void  OnUpdate()
    {

        MaterialPropertyBlock materialPropertyBlock = new MaterialPropertyBlock();
        Vector4[] uv = new Vector4[1];
        Camera camera = Camera.main;
        Mesh quadMesh = GameManager.Instance._quadMesh;
        Material material = GameManager.Instance._spriteMaterial;
        int shaderPropertyId = Shader.PropertyToID("_MainTex_UV");

        Entities.ForEach((ref Translation translation, ref SpriteAnimationData spriteAnimationData) =>
        {
            uv[0] = spriteAnimationData.uv;
            materialPropertyBlock.SetVectorArray(shaderPropertyId, uv);

            Graphics.DrawMesh(
                quadMesh,
                spriteAnimationData.matrix,
                material,
                0,//layer
                camera,
                0,//submesh index
                materialPropertyBlock
            );
        });
    }
}
