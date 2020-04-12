using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;

namespace Spartans
{
    [DisableAutoCreation]
    public class RotationSystem : JobComponentSystem
    {
        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            Vector3 cameraPosition = Camera.main.transform.position;
            Quaternion cameraRotation = Camera.main.transform.rotation;
            float maxAngle = 360f / 8f;
            JobHandle job = Entities.ForEach((ref SpriteAnimationData spriteAnimationData, ref AgentData movement, ref Translation translation, ref Rotation rotation) =>
            {
                Vector3 horizontalRelation = new Vector3(translation.Value.x, cameraPosition.y, translation.Value.z);
                Vector3 relativePos = horizontalRelation - cameraPosition;
                Quaternion relativeRotation = Quaternion.LookRotation(relativePos);
                float angle = Quaternion.Angle(relativeRotation, cameraRotation);
                rotation.Value = relativeRotation;

                for (int i = 0; i < 8; i++)
                {
                    if (angle > maxAngle * i && angle < maxAngle * (i + 1))
                    {
                        spriteAnimationData.direction = (SpriteSheetInfo.Direction)i;
                        break;
                    }
                }

            }).Schedule(inputDeps);

            return job;
        }
    }
}
