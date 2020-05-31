using Unity.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Spartans.Quadrant;
using Spartans.Steering;

namespace Spartans.Enemies
{
    [UpdateBefore(typeof(SteeringBehaviorsSystem))]
    //[UpdateAfter(typeof(QuadrantSystem))]
    public class EnemyTargetSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            JobHandle enemyTargetJobHandle = Entities
                .WithName("EnemyTargetSystemJob")
                .WithAll<EnemyTag>()
                .ForEach((int entityInQueryIndex, ref AgentData agent, in QuadrantTag quadrantTag) =>
                {
                    int neighbours;
                    var spartansToAttack = GetPositions(quadrantTag.numQuadrant, QuadrantSystem.spartanQuadrantMultiHashMap, out neighbours);
                    //if (neighbours == 0) continue;

                    float distance = 100;
                    for(int i = 0; i<spartansToAttack.Length; i++)
                    {
                        float newDistance = math.length(spartansToAttack[i] - agent.position);
                        if (newDistance < distance)
                        {
                            distance = newDistance;
                            agent.targetPosition = spartansToAttack[i];
                        }
                    }

                })
                .WithoutBurst()
                .ScheduleParallel(Dependency);

            Dependency = enemyTargetJobHandle;
        }

        public static NativeList<float3> GetPositions(int hashMapKey, in NativeMultiHashMap<int, AgentData> agents, out int numAgents)
        {
            numAgents = 0;
            NativeList<float3> agentsList = new NativeList<float3>(0, Allocator.Temp);
            AgentData agent;
            NativeMultiHashMapIterator<int> nativeMultiHashMapIterator;
            if (agents.TryGetFirstValue(hashMapKey, out agent, out nativeMultiHashMapIterator))
            {
                do
                {
                    numAgents++;
                    agentsList.Add(agent.position);
                }
                while (agents.TryGetNextValue(out agent, ref nativeMultiHashMapIterator));
            }

            return agentsList;
        }
    }
}
