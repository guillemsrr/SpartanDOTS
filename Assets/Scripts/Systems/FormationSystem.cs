using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using Unity.Mathematics;

namespace Spartans
{
    [AlwaysSynchronizeSystem]
    //[UpdateBefore(typeof(SteeringBehaviorsSystem))]
    public class FormationSystem : SystemBase
    {
        EntityQuery _query;
        float _rowSeparation = 2f;
        float _colSeparation = 2f;

        protected override void OnCreate()
        {
            base.OnCreate();
            _query = GetEntityQuery
                (
                    typeof(Translation),
                    ComponentType.ReadOnly<AgentData>()
                );
        }

        protected override void OnUpdate()
        {
            float3 averageVelocity = float3.zero;
            float3 averagePosition = float3.zero;
            Entities.ForEach((in Translation translation, in AgentData agent) =>
            {
                averageVelocity += agent.velocity;
                averagePosition += translation.Value;

            }).Run();

            int numSpartans = _query.CalculateEntityCount();
            int numberColumns = Environment.NumberColumns;
            if (numSpartans/ numberColumns <1)
            {
                numberColumns = numSpartans - 1;
                Environment.NumberColumns = numberColumns;
            }

            //job.Complete();//provisional

            averageVelocity /= numSpartans;
            averagePosition /= numSpartans;

            float3 alignmentDirection = math.normalizesafe(averageVelocity);
            float3 alignmentPerpendicular = math.cross(alignmentDirection, new float3(0, 1, 0));
            alignmentPerpendicular = math.normalizesafe(alignmentPerpendicular);

            //Get furthest spartan
            int numEntity = 0;
            float lengthsq = math.lengthsq(alignmentDirection);

            float distance = 0;
            Entities.ForEach((int entityInQueryIndex, in Translation translation, in AgentData agent) =>
            {
                float3 direction = math.normalizesafe(translation.Value - averagePosition);

                float angle = Mathf.Rad2Deg*math.acos(math.dot(direction, alignmentDirection) / math.lengthsq(direction) * lengthsq);

                if (angle < 30f)//adjust
                {
                    float newDistance = math.length(translation.Value - averagePosition);
                    if(newDistance > distance)
                    {
                        numEntity = entityInQueryIndex;
                        distance = newDistance;
                    }
                }

            }).Run();

            var formationPositions = new NativeArray<int2>(numSpartans, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
            var targetPositions = new NativeArray<float3>(numSpartans, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
            var translationArray = _query.ToComponentDataArray<Translation>(Unity.Collections.Allocator.Persistent);
            NativeList<Translation> positionsList = new NativeList<Translation>(numSpartans, Allocator.TempJob);
            NativeList<int> placedEntities = new NativeList<int>(numSpartans, Allocator.TempJob);

            for(int i = 0; i<numSpartans; i++)
            {
                //Maybe I should add instead of equal?
                positionsList.Add(translationArray[i]);//CopyFrom?
            }
            float3 leaderPosition = translationArray[numEntity].Value;
            targetPositions[numEntity] = leaderPosition;
            formationPositions[numEntity] = new int2(0, 0);//the leader

            distance = 0f;
            //center and right side of leader
            for (int i = 0; i < (numberColumns - 1) / 2 + 1; i++)
            {
                for(int j = 0; j < numSpartans/numberColumns; j++)
                {
                    //calculate target pos
                    float3 targetPos = leaderPosition + alignmentPerpendicular*i*_colSeparation - alignmentDirection*j*_rowSeparation;

                    distance = 100f;
                    //get the closest agent
                    for (int z = 0; z<numSpartans; z++)
                    {
                        if (!placedEntities.Contains(z))
                        {
                            float newDistance = math.length(positionsList[z].Value - targetPos);
                            if(newDistance < distance)
                            {
                                numEntity = z;
                                distance = newDistance;
                            }
                        }
                    }

                    placedEntities.Add(numEntity);

                    //set to array
                    targetPositions[numEntity] = targetPos;
                    formationPositions[numEntity] = new int2(i, j);
                }
            }

            distance = 0f;
            //left side of leader
            for (int i = -1; i > -((numberColumns - 1) / 2 + 1); i--)
            {
                for (int j = 0; j < numSpartans/numberColumns; j++)
                {
                    //calculate target pos
                    float3 targetPos = leaderPosition + alignmentPerpendicular * i * _colSeparation - alignmentDirection * j * _rowSeparation;

                    distance = 100f;
                    //get the closest agent
                    for (int z = 0; z < numSpartans; z++)
                    {
                        if (!placedEntities.Contains(z))
                        {
                            float newDistance = math.length(positionsList[z].Value - targetPos);
                            if (newDistance < distance)
                            {
                                numEntity = z;
                                distance = newDistance;
                            }
                        }
                    }

                    placedEntities.Add(numEntity);

                    //set to array
                    targetPositions[numEntity] = targetPos;
                    formationPositions[numEntity] = new int2(i, j);
                }
            }

            JobHandle job = Entities.ForEach((int entityInQueryIndex, ref AgentData agent) =>
            {
                agent.targetPosition = targetPositions[entityInQueryIndex];
                agent.formationPosition = formationPositions[entityInQueryIndex];

            }).Schedule(Dependency);

            job.Complete();

            translationArray.Dispose(Dependency);
            formationPositions.Dispose(Dependency);
            targetPositions.Dispose(Dependency);
            positionsList.Dispose(Dependency);
            placedEntities.Dispose(Dependency);
        }
    }
}
