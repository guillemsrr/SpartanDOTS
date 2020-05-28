using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Jobs;
using Spartans.Quadrant;
using Spartans.Enemies;
using Unity.Transforms;
using Unity.Mathematics;

namespace Spartans.Attack
{
    public class CloseDamageAttackSystem : SystemBase
    {
        private const float MIN_DISTANCE = 1f;
        private const float DAMAGE_RATE = 5f;
        protected override void OnUpdate()
        {
            float deltaTime = Time.DeltaTime;
            Entities
                .WithName("SpartanCloseDamageJob")
                .WithAll<SpartanTag>()
                .WithStructuralChanges()
                .ForEach((ref HealthComponent healthData, in Entity entity, in QuadrantTag quadrant, in Translation translation) =>
                {
                    int neighbours;
                    var otherPositions = EnemyTargetSystem.GetPositions(quadrant.numQuadrant, QuadrantSystem.enemyQuadrantMultiHashMap, out neighbours);
                    for (int i = 0; i<otherPositions.Length; i++)
                    {
                        if(math.distance(translation.Value, otherPositions[i]) < MIN_DISTANCE)
                        {
                            healthData.health -= DAMAGE_RATE* deltaTime;
                            if(healthData.health < 0f)
                            {
                                World.DefaultGameObjectInjectionWorld.EntityManager.DestroyEntity(entity);
                                break;
                            }
                        }
                    }
                })
                .WithoutBurst()
                .Run();

            Entities
                .WithName("EnemyCloseDamageJob")
                .WithAll<EnemyTag>()
                .WithStructuralChanges()
                .ForEach((ref Entity entity, ref HealthComponent healthData, in QuadrantTag quadrant, in Translation translation) =>
                {
                    int neighbours;
                    var otherPositions = EnemyTargetSystem.GetPositions(quadrant.numQuadrant, QuadrantSystem.spartanQuadrantMultiHashMap, out neighbours);
                    for (int i = 0; i < otherPositions.Length; i++)
                    {
                        if (math.distance(translation.Value, otherPositions[i]) < MIN_DISTANCE)
                        {
                            healthData.health -= DAMAGE_RATE* deltaTime;
                            if (healthData.health < 0f)
                            {
                                World.DefaultGameObjectInjectionWorld.EntityManager.DestroyEntity(entity);
                                break;
                            }
                        }
                    }
                })
                .WithoutBurst()
                .Run();
        }
    }
}
