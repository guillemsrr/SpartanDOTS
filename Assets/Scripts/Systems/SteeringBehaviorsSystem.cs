using Unity.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using Unity.Mathematics;

namespace Spartans
{
    [UpdateAfter(typeof(PlayerInputSystem))]//maybe it's unnecessary or i should find another way. It's not in OnUpdate()
    //[UpdateAfter(typeof(FormationSystem))]//potential bottleneck
    public class SteeringBehaviorsSystem : SystemBase
    {
        EntityQuery _query;
        List<AgentSettings> _settings = new List<AgentSettings>();
        protected override void OnCreate()
        {
            base.OnCreate();
            _query = GetEntityQuery
                (
                    typeof(Translation),
                    ComponentType.ReadOnly<AgentData>()
                );

            _settings = new List<AgentSettings>();
        }
        protected override void OnUpdate()
        {
            float deltaTime = Time.DeltaTime* Environment.TimeSpeed;
            var translationArray = _query.ToComponentDataArray<Translation>(Unity.Collections.Allocator.Persistent);
            var agentsDataArray = _query.ToComponentDataArray<AgentData>(Unity.Collections.Allocator.Persistent);

            EntityManager.GetAllUniqueSharedComponentData(_settings);
            AgentSettings settings = _settings[1];

            Entities.ForEach((int entityInQueryIndex, ref Translation translation, ref AgentData agent) =>
            {
                float3 frictionForce = math.normalizesafe(-agent.velocity) * settings.maxForce/2f;
                float3 movingForce = agent.direction * settings.maxForce;
                float3 seekingForce = Seek(in translation, in agent, in settings);
                float3 fleeingForce = Flee(entityInQueryIndex , in translation, in agent, settings, in translationArray);
                float3 flockingForce = Flock(entityInQueryIndex, in translation, settings, in agentsDataArray, in translationArray);
                float3 steeringForce = frictionForce + movingForce* agent.moveWeight + seekingForce* agent.seekWeight + fleeingForce*agent.fleeWeight+ flockingForce*agent.flockWeight;
                float3 acceleration = steeringForce / settings.mass;

                agent.velocity += acceleration * deltaTime;
                float speed = math.length(agent.velocity);
                if(speed > settings.maxSpeed)
                {
                    agent.velocity = math.normalizesafe(agent.velocity);
                    agent.velocity *= settings.maxSpeed;
                }

                agent.velocity.y = 0;

                translation.Value += agent.velocity * deltaTime;

            }).ScheduleParallel();

            translationArray.Dispose(Dependency);
            agentsDataArray.Dispose(Dependency);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
        }

        private static float3 Seek(in Translation translation, in AgentData agent, in AgentSettings settings)
        {
            float3 desiredVelocity = math.normalizesafe(agent.targetPosition - translation.Value);
            desiredVelocity *= settings.maxSpeed;

            float3 steeringForce = (desiredVelocity - agent.velocity);
            steeringForce /= settings.maxSpeed;
            steeringForce *= settings.maxForce;
            return steeringForce;
        }

        private static float3 Flee(int index, in Translation translation, in AgentData agent, in AgentSettings settings, in NativeArray<Translation> translationArray)
        {
            float3 fleeingForce = 0;
            for (int i = 0; i < translationArray.Length; i++)
            {
                if(i != index)
                {
                    if (math.length(translationArray[i].Value - translation.Value) < settings.neighborRadius)
                    {
                        float3 steering = translation.Value - translationArray[i].Value;
                        steering = math.normalize(steering);

                        fleeingForce = steering * settings.maxSpeed - agent.velocity;
                        fleeingForce /= settings.maxSpeed;
                        fleeingForce *= settings.maxForce;
                    }
                }
            }

            return fleeingForce;
        }

        public static float3 Flock(int index, in Translation translation, in AgentSettings settings, in NativeArray<AgentData> agentsDataArray, in NativeArray<Translation> translationArray)
        {
            int neighborCount = 0;
            float3 separationVector = float3.zero;
            float3 averagePosition = float3.zero;
            float3 averageVelocity = float3.zero;

            float3 separationDirection;
            float3 cohesionDirection;
            float3 alignmentDirection;

            for (int i = 0; i < agentsDataArray.Length; i++)
            {
                if(i != index)
                {
                    float3 otherAgentPos = translationArray[i].Value;
                    if (math.length(otherAgentPos - translation.Value) < settings.neighborRadius)
                    {
                        separationVector += translation.Value - otherAgentPos;
                        averagePosition += otherAgentPos;
                        averageVelocity += agentsDataArray[i].velocity;

                        ++neighborCount;
                    }
                }
            }

            separationVector /= neighborCount;
            separationDirection = math.normalizesafe(separationVector);
            averagePosition /= neighborCount;
            averagePosition -= translation.Value;
            cohesionDirection = math.normalizesafe(averagePosition);
            averageVelocity /= neighborCount;
            alignmentDirection = math.normalizesafe(averageVelocity);

            return separationDirection * settings.separationWeight + cohesionDirection * settings.cohesionWeight + alignmentDirection * settings.alignmentWeight;
        }
    }
}
