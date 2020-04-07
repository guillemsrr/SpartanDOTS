using Unity.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using Unity.Mathematics;

namespace Core
{
    public class SteeringBehaviorsSystem : SystemBase
    {
        EntityQuery _query;
        const float K_SEPARATION_FORCE = 5f;
        const float K_COHESION_FORCE = 1.6f;
        const float K_ALIGNMENT_FORCE = 1f;

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
            float deltaTime = Time.DeltaTime;
            NativeArray<Translation> translationArray = _query.ToComponentDataArray<Translation>(Unity.Collections.Allocator.TempJob);
            NativeArray<AgentData> agentsDataArray = _query.ToComponentDataArray<AgentData>(Unity.Collections.Allocator.TempJob);
            NativeArray<Entity> entities = _query.ToEntityArray(Unity.Collections.Allocator.TempJob);

            Entities.ForEach((ref Translation translation, ref AgentData agent) =>
            {
                //SEEK or MOVE
                float3 seekingForce = agent.direction*agent.maxForce;

                //FLEE
                float3 fleeingForce = Flee(translation, agent, agentsDataArray, translationArray, 1f);
                
                //FLOCK
                float3 flockingForce = Flock(translation, agent, agentsDataArray, translationArray, 1f);
                flockingForce *= 10f;

                agent.steeringForce = seekingForce + fleeingForce + flockingForce;

                float3 acceleration = agent.steeringForce / agent.mass;
                if (math.length(agent.velocity) < agent.maxSpeed)
                {
                    agent.velocity += acceleration * deltaTime;
                }
                else
                {
                    agent.velocity += acceleration * deltaTime;
                    agent.velocity = math.normalizesafe(agent.velocity);
                    agent.velocity *= agent.maxSpeed;
                }

                translation.Value += agent.velocity * deltaTime;

            }).ScheduleParallel();


            
            //translationArray.Dispose();
            //agentsDataArray.Dispose();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
        }

        private static float3 Flee(Translation translation, AgentData agent, NativeArray<AgentData> agentsDataArray, NativeArray<Translation> translationArray, float neighborRadius)
        {
            float3 fleeingForce = 0;
            for (int i = 0; i < translationArray.Length; i++)
            {
                if (math.length(translationArray[i].Value - translation.Value) > 1f)
                {
                    float3 steering = translation.Value - translationArray[i].Value;
                    steering = math.normalize(steering);

                    fleeingForce = steering * agent.maxSpeed - agent.velocity;
                    fleeingForce /= agent.maxSpeed;
                    fleeingForce *= agent.maxForce;
                }
            }

            return fleeingForce /= 100f;
        }

        public static float3 Flock(Translation translation, AgentData agent, NativeArray<AgentData> agentsDataArray, NativeArray<Translation> translationArray , float neighborRadius)
        {
            int neighborCount = 0;
            float3 separationVector = float3.zero;
            float3 averagePosition = float3.zero;
            float3 averageVelocity = float3.zero;
            float3 separationDirection = float3.zero;
            float3 cohesionDirection = float3.zero;
            float3 alignmentDirection = float3.zero;

            for (int i = 0; i < agentsDataArray.Length; i++)
            {
                float3 otherAgentPos = translationArray[i].Value;
                if (math.length(otherAgentPos - translation.Value) < 10f && math.length(otherAgentPos - translation.Value) > 0)
                {
                    separationVector += translation.Value - otherAgentPos;
                    averagePosition += otherAgentPos;
                    averageVelocity += agentsDataArray[i].velocity;

                    ++neighborCount;
                }
            }
            if (neighborCount == 0)
            {
                Debug.Log("no neighbors");
                return 0f;
            }
            separationVector /= neighborCount;
            separationDirection = math.normalizesafe(separationVector);
            averagePosition /= neighborCount;
            averagePosition -= translation.Value;
            cohesionDirection = math.normalizesafe(averagePosition);
            averageVelocity /= neighborCount;
            alignmentDirection = math.normalizesafe(averageVelocity);

            return separationDirection * K_SEPARATION_FORCE + cohesionDirection * K_COHESION_FORCE + alignmentDirection * K_ALIGNMENT_FORCE;
        }
    }
}
