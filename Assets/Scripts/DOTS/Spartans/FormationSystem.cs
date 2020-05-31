using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using Unity.Mathematics;
using Spartans.Quadrant;
using UnityEditorInternal;
using Spartans.Steering;

namespace Spartans
{
    [AlwaysSynchronizeSystem]
    [UpdateBefore(typeof(SteeringBehaviorsSystem))]
    public class FormationSystem : SystemBase
    {
        EntityQuery _query;
        float _rowSeparation = 1f;
        float _colSeparation = 1f;
        AgentData _leaderAgentData;
        int _leaderEntityNum;

        protected override void OnCreate()
        {
            base.OnCreate();
            _query = GetEntityQuery
                (
                    ComponentType.ReadOnly<Translation>(),
                    ComponentType.ReadOnly<AgentData>(),
                    ComponentType.ReadOnly<SpartanTag>()
                );
        }

        protected override void OnUpdate()
        {
            //Detect if spartans have no direction
            var directionArray = _query.ToComponentDataArray<AgentData>(Allocator.TempJob);
            if (math.length(directionArray[0].direction) == 0)
            {
                directionArray.Dispose();
                return;
            }
            directionArray.Dispose();


            float3 averageVelocity = AverageVelocity();
            float3 averagePosition = AveragePosition();

            int numSpartans = _query.CalculateEntityCount();
            int numberColumns = Environment.NumberColumns;
            if (numSpartans/ numberColumns <1)
            {
                numberColumns = numSpartans - 1;
                Environment.NumberColumns = numberColumns;
            }

            averageVelocity /= numSpartans;
            averagePosition /= numSpartans;

            float3 alignmentDirection = math.normalizesafe(averageVelocity);
            float3 alignmentPerpendicular = math.cross(alignmentDirection, new float3(0, 1, 0));
            alignmentPerpendicular = math.normalizesafe(alignmentPerpendicular);

            //check leader position according to alignment
            //look for a new leader
            //Get furthest spartan
            float dist = 0;
            int numEntity = _leaderEntityNum;
            Entities
                .WithAll<SpartanTag>()
                .WithReadOnly(averagePosition)
                .WithReadOnly(averageVelocity)
                .ForEach((int entityInQueryIndex, in Translation translation, in AgentData agent) =>
                {
                    if(!Equals(agent, _leaderAgentData))
                    {
                        if (GetAngle(translation.Value, averagePosition, alignmentDirection) < 30f)//adjust
                        {
                            float newDistance = math.length(translation.Value - averagePosition);
                            if(newDistance > dist)
                            {
                                numEntity = entityInQueryIndex;
                                dist = newDistance;
                            }
                        }
                    }
                    else
                    {
                        _leaderEntityNum = entityInQueryIndex;
                        if (GetAngle(_leaderAgentData.position, averagePosition, alignmentDirection) < 45f)
                        {
                            //break;?¿?¿
                        }
                    }

                })
                .WithoutBurst()
                .Run();

            if(numEntity != _leaderEntityNum)
            {
                _leaderEntityNum = numEntity;
                var agents = _query.ToComponentDataArray<AgentData>(Allocator.Temp);
                _leaderAgentData = agents[numEntity];
            }

            var formationPositions = new NativeArray<int2>(numSpartans, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
            var targetPositions = new NativeArray<float3>(numSpartans, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
            var translationArray = _query.ToComponentDataArray<Translation>(Allocator.Persistent);
            NativeList<int> placedEntities = new NativeList<int>(numSpartans, Allocator.TempJob);

            float3 leaderPosition = translationArray[_leaderEntityNum].Value;
            targetPositions[_leaderEntityNum] = leaderPosition;
            formationPositions[_leaderEntityNum] = new int2(0, 0);//the leader

            numEntity = 0;
            //center and right side of leader
            for (int i = 0; i < (numberColumns - 1) / 2 + 1; i++)
            {
                for(int j = 0; j < numSpartans/numberColumns; j++)
                {
                    //calculate target pos
                    float3 targetPos = leaderPosition + alignmentPerpendicular*i*_colSeparation - alignmentDirection*j*_rowSeparation;
                    float distance = 1000f;
                    //get the closest agent
                    for (int z = 0; z<numSpartans; z++)
                    {
                        if (!placedEntities.Contains(z))
                        {
                            float newDistance = math.length(translationArray[z].Value - targetPos);
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

            //left side of leader
            for (int i = -1; i > -((numberColumns - 1) / 2 + 1); i--)
            {
                for (int j = 0; j < numSpartans/numberColumns; j++)
                {
                    //calculate target pos
                    float3 targetPos = leaderPosition + alignmentPerpendicular * i * _colSeparation - alignmentDirection * j * _rowSeparation;

                    float distance = 100f;
                    //get the closest agent
                    for (int z = 0; z < numSpartans; z++)
                    {
                        if (!placedEntities.Contains(z))
                        {
                            float newDistance = math.length(translationArray[z].Value - targetPos);
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

            JobHandle job = Entities.ForEach((int entityInQueryIndex, ref AgentData agent, ref SpartanData spartan) =>
            {
                agent.targetPosition = targetPositions[entityInQueryIndex];
                Debug.DrawLine(agent.position, agent.targetPosition, Color.black);
                spartan.formationPosition = formationPositions[entityInQueryIndex];

            }).Schedule(Dependency);

            job.Complete();

            translationArray.Dispose(Dependency);
            formationPositions.Dispose(Dependency);
            targetPositions.Dispose(Dependency);
            placedEntities.Dispose(Dependency);
        }

        private static float3 AverageVelocity()
        {
            float3 averageVelocity = float3.zero;
            var alignmentKeys = QuadrantSystem.spartanAlignmentQuadrantHashMap.GetKeyArray(Allocator.TempJob);
            foreach(var key in alignmentKeys)
            {
                averageVelocity += math.normalizesafe(QuadrantSystem.spartanAlignmentQuadrantHashMap[key]);
            }

            alignmentKeys.Dispose();

            return averageVelocity;
        }

        private static float3 AveragePosition()
        {
            float3 averagePosition = float3.zero;
            var alignmentKeys = QuadrantSystem.spartanAlignmentQuadrantHashMap.GetKeyArray(Allocator.TempJob);
            foreach (var key in alignmentKeys)
            {
                averagePosition += QuadrantSystem.spartanMassCenterQuadrantHashMap[key];
            }

            alignmentKeys.Dispose();

            return averagePosition;
        }

        private static float GetAngle(float3 translation, float3 averagePosition, float3 alignmentDirection)
        {
            float3 direction = math.normalizesafe(translation - averagePosition);
            return Mathf.Rad2Deg * math.acos(math.dot(direction, alignmentDirection) / math.lengthsq(direction) * math.lengthsq(alignmentDirection));
        }
    }
}
