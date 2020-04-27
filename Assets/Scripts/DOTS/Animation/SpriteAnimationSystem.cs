using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using Unity.Mathematics;

namespace Spartans.Animation
{
	[DisableAutoCreation]
	//[AlwaysSynchronizeSystem]
	public class SpriteAnimationSystem : ComponentSystem
	{
		protected override void OnUpdate()
		{
			float deltaTime = Time.DeltaTime;

			Entities.ForEach((ref SpriteAnimationData spriteAnimationData, ref Translation translation, ref Rotation rotation) =>
			{
				spriteAnimationData.frameTimer += deltaTime;
				while (spriteAnimationData.frameTimer >= spriteAnimationData.frameTimerMax)
				{
					spriteAnimationData.frameTimer -= spriteAnimationData.frameTimerMax;

					SpriteSheetInfo.SpriteSheetAnimation animation = Bootstrap.Instance.SpriteSheetInfo.GetSpriteSheetAnimation(spriteAnimationData.animation, spriteAnimationData.direction);
					spriteAnimationData.currentCell.x++;
					if (spriteAnimationData.currentCell.x > animation.endCell.x)
					{
						spriteAnimationData.currentCell.x = animation.startCell.x;
					}

					float uvWidth = 1f / 8f;
					float uvHeight = 1f / 8f;

					spriteAnimationData.uv = new Vector4(uvWidth, uvHeight, spriteAnimationData.currentCell.x * uvWidth, (7 - spriteAnimationData.currentCell.y) * uvHeight);
					spriteAnimationData.matrix = Matrix4x4.TRS(translation.Value, rotation.Value, Vector3.one);
				}

			});
		}
	}
}
