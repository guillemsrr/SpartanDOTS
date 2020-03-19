using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using Unity.Mathematics;

[AlwaysSynchronizeSystem]
public class SpriteAnimationSystem : JobComponentSystem
{
    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
		float deltaTime = Time.DeltaTime;

		Entities.ForEach((ref SpriteAnimationData spriteAnimationData, ref Translation translation) =>
		{
			spriteAnimationData.frameTimer += deltaTime;
			while (spriteAnimationData.frameTimer >= spriteAnimationData.frameTimerMax)
			{
				spriteAnimationData.frameTimer -= spriteAnimationData.frameTimerMax;
				spriteAnimationData.currentFrame = (spriteAnimationData.currentFrame + 1) % spriteAnimationData.frameCount;
				if(spriteAnimationData.currentFrame > spriteAnimationData.maxFrame - 1)
				{
					spriteAnimationData.currentFrame = 0;
				}
				float uvWidth = 1f / spriteAnimationData.frameCount;
				float uvHeight = 1f/8f;
				float uvOffsetX = uvWidth * spriteAnimationData.currentFrame;
				float uvOffsetY = 1/8f * 7f; ;
				spriteAnimationData.uv = new Vector4(uvWidth, uvHeight, uvOffsetX, uvOffsetY);

				float3 position = translation.Value;
				position.z = position.y * .01f;
				spriteAnimationData.matrix = Matrix4x4.TRS(translation.Value, Quaternion.identity, Vector3.one);
			}

		}).Run();
		return default;
	}
}
