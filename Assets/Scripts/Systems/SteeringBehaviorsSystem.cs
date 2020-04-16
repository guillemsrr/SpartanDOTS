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
        EntityQuery _commonQuery;
        EntityQuery _spartanQuery;
        EntityQuery _enemyQuery;
        List<AgentSettings> _settings = new List<AgentSettings>();

        protected override void OnCreate()
        {
            base.OnCreate();

            _commonQuery = GetEntityQuery
                (
                    typeof(Translation),
                    typeof(AgentData)
                );

            _spartanQuery = GetEntityQuery(new EntityQueryDesc
            {
                None = new ComponentType[] { typeof(EnemyData) },
                All = new ComponentType[] {
                    ComponentType.ReadOnly<SpartanData>(),
                    typeof(Translation),
                    typeof(AgentData),
                }
            });

            _enemyQuery = GetEntityQuery(new EntityQueryDesc
            {
                None = new ComponentType[] { typeof(SpartanData) },
                All = new ComponentType[] {
                    ComponentType.ReadOnly<EnemyData>(),
                    typeof(Translation),
                    typeof(AgentData),
                }
            });

            RequireForUpdate(_commonQuery);
            RequireForUpdate(_spartanQuery);
            RequireForUpdate(_enemyQuery);

            _settings = new List<AgentSettings>();
        }
        protected override void OnUpdate()
        {
            float deltaTime = Time.DeltaTime* Environment.TimeSpeed;
            EntityManager.GetAllUniqueSharedComponentData(_settings);
            AgentSettings settings = _settings[1];

            var translationArray = _commonQuery.ToComponentDataArray<Translation>(Allocator.TempJob);
            var agentsDataArray = _commonQuery.ToComponentDataArray<AgentData>(Allocator.TempJob);
            var spartanAgentsArray = _spartanQuery.ToComponentDataArray<AgentData>(Allocator.TempJob);
            var spartanTranslationArray = _spartanQuery.ToComponentDataArray<Translation>(Allocator.TempJob);
            var enemyAgentsArray = _enemyQuery.ToComponentDataArray<AgentData>(Allocator.TempJob);
            var enemyTranslationArray = _enemyQuery.ToComponentDataArray<Translation>(Allocator.TempJob);

            //COMMON STEERING
            JobHandle commonSteeringJobHandle = Entities
                .ForEach((int entityInQueryIndex, ref Translation translation, ref AgentData agent) =>
                {
                    float3 frictionForce = math.normalizesafe(-agent.velocity) * settings.maxForce / 2f;
                    float3 movingForce = agent.direction * settings.maxForce;
                    float3 seekingForce = Seek(in translation, in agent, in settings);
                    float3 fleeingForce = Flee(entityInQueryIndex, in translation, in agent, settings, in translationArray);

                    agent.steeringForce = frictionForce + movingForce * agent.moveWeight + seekingForce * agent.seekWeight + fleeingForce * agent.fleeWeight;

                }).ScheduleParallel(Dependency);

            Dependency = commonSteeringJobHandle;

            //SPARTAN FLOCK
            JobHandle spartanFlockJobHandle = Entities
                .WithAll<SpartanData>()
                .ForEach((int entityInQueryIndex, ref Translation translation, ref AgentData agent, ref Rotation rotation) =>
                {
                    float3 flockingForce = Flock(entityInQueryIndex, in translation, settings, in spartanAgentsArray, in spartanTranslationArray);
                    agent.steeringForce += flockingForce * agent.flockWeight;

                }).ScheduleParallel(Dependency);

            Dependency = spartanFlockJobHandle;//OTHERWISE IT WILL CAUSE A NATIVE ARRAY NOT DISPOSED PROBLEM

            ////ENEMY FLOCK
            JobHandle enemyFlockJobHandle = Entities
                .WithAll<EnemyData>()
                .ForEach((int entityInQueryIndex, ref Translation translation, ref AgentData agent, ref Rotation rotation) =>
                {
                    float3 flockingForce = Flock(entityInQueryIndex, in translation, settings, in enemyAgentsArray, in enemyTranslationArray);
                    agent.steeringForce += flockingForce * agent.flockWeight;

                }).ScheduleParallel(Dependency);

            Dependency = enemyFlockJobHandle;

            ////COMMON MRUA
            JobHandle mruaJobHandle = Entities.ForEach((ref Translation translation, ref Rotation rotation, ref AgentData agent) =>
            {
                float3 acceleration = agent.steeringForce / settings.mass;

                agent.velocity += acceleration * deltaTime;
                float speed = math.length(agent.velocity);
                if (speed > settings.maxSpeed)
                {
                    agent.velocity = math.normalizesafe(agent.velocity);
                    agent.velocity *= settings.maxSpeed;
                }

                agent.velocity.y = 0;
                translation.Value += agent.velocity * deltaTime;

                if (speed > 0f)
                {
                    agent.forward = math.lerp(agent.forward, math.normalizesafe(agent.velocity), agent.forwardSmooth * deltaTime);
                    rotation.Value = quaternion.LookRotation(new float3(agent.forward.x, 0, agent.forward.z), new float3(0, 1, 0));
                }

            }).ScheduleParallel(Dependency);

            Dependency = mruaJobHandle;
            JobHandle disposeJobHandle = translationArray.Dispose(Dependency);
            disposeJobHandle = JobHandle.CombineDependencies(disposeJobHandle, agentsDataArray.Dispose(Dependency));
            disposeJobHandle = JobHandle.CombineDependencies(disposeJobHandle, spartanAgentsArray.Dispose(Dependency));
            disposeJobHandle = JobHandle.CombineDependencies(disposeJobHandle, spartanTranslationArray.Dispose(Dependency));
            disposeJobHandle = JobHandle.CombineDependencies(disposeJobHandle, enemyAgentsArray.Dispose(Dependency));
            disposeJobHandle = JobHandle.CombineDependencies(disposeJobHandle, enemyTranslationArray.Dispose(Dependency));
            Dependency = disposeJobHandle;

            _commonQuery.AddDependency(Dependency);
            _spartanQuery.AddDependency(Dependency);
            _enemyQuery.AddDependency(Dependency);

            _commonQuery.ResetFilter();
            _spartanQuery.ResetFilter();
            _enemyQuery.ResetFilter();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="translation"></param>
        /// <param name="agent"></param>
        /// <param name="settings"></param>
        /// <returns></returns>
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
