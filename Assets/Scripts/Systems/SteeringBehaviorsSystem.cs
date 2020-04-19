using Unity.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using Unity.Mathematics;
using UnityEngine;

namespace Spartans
{
    //[UpdateAfter(typeof(PlayerInputSystem))]//maybe it's unnecessary or i should find another way. It's not in OnUpdate()
    public class SteeringBehaviorsSystem : SystemBase
    {
        EntityQuery _spartanQuery;
        EntityQuery _enemyQuery;
        List<AgentSettings> _settings = new List<AgentSettings>();

        protected override void OnCreate()
        {
            base.OnCreate();

            _spartanQuery = GetEntityQuery(new EntityQueryDesc
            {
                None = new ComponentType[] { typeof(EnemyTag) },
                All = new ComponentType[] {
                    ComponentType.ReadOnly<SpartanTag>(),
                    typeof(Translation),
                    typeof(AgentData),
                }
            });

            _enemyQuery = GetEntityQuery(new EntityQueryDesc
            {
                None = new ComponentType[] { typeof(SpartanTag) },
                All = new ComponentType[] {
                    ComponentType.ReadOnly<EnemyTag>(),
                    typeof(Translation),
                    typeof(AgentData),
                }
            });

            RequireForUpdate(_spartanQuery);
            RequireForUpdate(_enemyQuery);

            _settings = new List<AgentSettings>();
        }
        protected override void OnUpdate()
        {
            float deltaTime = Time.DeltaTime* Environment.TimeSpeed;
            EntityManager.GetAllUniqueSharedComponentData(_settings);
            AgentSettings settings = _settings[1];

            var spartanAgentsArray = _spartanQuery.ToComponentDataArray<AgentData>(Allocator.TempJob);
            var spartanTranslationArray = _spartanQuery.ToComponentDataArray<Translation>(Allocator.TempJob);
            var spartanEntityArray = _spartanQuery.ToEntityArray(Allocator.TempJob);
            var enemyAgentsArray = _enemyQuery.ToComponentDataArray<AgentData>(Allocator.TempJob);
            var enemyTranslationArray = _enemyQuery.ToComponentDataArray<Translation>(Allocator.TempJob);
            var enemyEntityArray = _enemyQuery.ToEntityArray(Allocator.TempJob);

            var jobHandles = new NativeArray<JobHandle>(3, Allocator.TempJob);
            int numEntities = _spartanQuery.CalculateEntityCount() + _enemyQuery.CalculateEntityCount();
            ComponentDataFromEntity<SpartanTag> spartanFromEntity = GetComponentDataFromEntity<SpartanTag>(true);
            ComponentDataFromEntity<EnemyTag> enemyFromEntity = GetComponentDataFromEntity<EnemyTag>(true);

            //COMMON STEERING
            var commonSteeringForces = new NativeArray<float3>(numEntities, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
            JobHandle commonSteeringJobHandle = Entities
                .WithName("Common_Steering")
                .WithNativeDisableParallelForRestriction(commonSteeringForces)
                .ForEach((int entityInQueryIndex, ref Translation translation, ref AgentData agent) =>
                {
                    float3 frictionForce = math.normalizesafe(-agent.velocity) * settings.maxForce / 2f;
                    float3 movingForce = agent.direction * settings.maxForce;
                    float3 seekingForce = Seek(in translation, in agent, in settings);
                    float3 steeringForces = frictionForce + movingForce * agent.moveWeight + seekingForce * agent.seekWeight;

                    if (math.length(steeringForces) < 0.01f)
                    {
                        steeringForces = float3.zero;
                    }

                    commonSteeringForces[entityInQueryIndex] = steeringForces;
                    //temporal
                    agent.steeringForce = steeringForces;
                }).ScheduleParallel(Dependency);

            Dependency = commonSteeringJobHandle;

            //FLEE
            var fleeSteeringForces = new NativeArray<float3>(numEntities, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
            JobHandle fleeJobHandle = Entities
                .WithName("Flee")
                .WithReadOnly(spartanFromEntity)
                .WithReadOnly(enemyFromEntity)
                .WithReadOnly(spartanEntityArray)
                .WithReadOnly(enemyEntityArray)
                .WithReadOnly(spartanTranslationArray)
                .WithReadOnly(enemyTranslationArray)
                .WithNativeDisableParallelForRestriction(fleeSteeringForces)
                .ForEach((int entityInQueryIndex, Entity entity, ref Translation translation, ref AgentData agent) =>
                {
                    float3 fleeingForce = float3.zero;
                    if (spartanFromEntity.Exists(entity))//if is a Spartan
                    {
                        fleeingForce = Flee(in entity, in spartanEntityArray, in translation, in agent, settings, in spartanTranslationArray);
                        fleeingForce += EnemyFlee(in translation, in agent, settings, in enemyTranslationArray) * agent.enemyFleeRelation;
                    }
                    else if (enemyFromEntity.Exists(entity))//is an Enemy
                    {
                        fleeingForce = Flee(in entity, in enemyEntityArray, in translation, in agent, settings, in enemyTranslationArray);
                        fleeingForce += EnemyFlee(in translation, in agent, settings, in spartanTranslationArray) * agent.enemyFleeRelation;
                    }

                    if (math.length(fleeingForce) < 0.01f)
                    {
                        fleeingForce = float3.zero;
                    }

                    fleeSteeringForces[entityInQueryIndex] = fleeingForce * agent.fleeWeight;

                    //temporal
                    agent.steeringForce += fleeingForce * agent.fleeWeight;
                }).ScheduleParallel(Dependency);

            Dependency = fleeJobHandle;

            //FLOCK
            var flockSteeringForces = new NativeArray<float3>(numEntities, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
            JobHandle flockJobHandle = Entities
                .WithName("Flocking")
                .WithReadOnly(spartanFromEntity)
                .WithReadOnly(enemyFromEntity)
                .WithReadOnly(spartanEntityArray)
                .WithReadOnly(enemyEntityArray)
                .WithReadOnly(spartanAgentsArray)
                .WithReadOnly(spartanTranslationArray)
                .WithReadOnly(enemyAgentsArray)
                .WithReadOnly(enemyTranslationArray)
                .WithNativeDisableParallelForRestriction(flockSteeringForces)
                .ForEach((int entityInQueryIndex, Entity entity, ref Translation translation, ref AgentData agent, ref Rotation rotation) =>
                {
                    float3 flockingForce = float3.zero;
                    if (spartanFromEntity.Exists(entity))//if is a Spartan
                    {
                        flockingForce = Flock(in entity, in spartanEntityArray, in translation, settings, in spartanAgentsArray, in spartanTranslationArray);
                    }
                    else if (enemyFromEntity.Exists(entity))//is an Enemy
                    {
                        flockingForce = Flock(in entity, in enemyEntityArray, in translation, settings, in enemyAgentsArray, in enemyTranslationArray);
                    }

                    if(math.length(flockingForce) < 0.01f)
                    {
                        flockingForce = float3.zero;
                    }
                    flockSteeringForces[entityInQueryIndex] = flockingForce * agent.flockWeight;
                    agent.steeringForce += flockingForce * agent.flockWeight;
                }).ScheduleParallel(Dependency);


            jobHandles[0] = commonSteeringJobHandle;
            jobHandles[1] = fleeJobHandle;
            jobHandles[2] = flockJobHandle;

            Dependency = JobHandle.CombineDependencies(jobHandles);

            Dependency.Complete();
            //foreach (var x in commonSteeringForces)
            //{
            //    Debug.Log("1 COMMON FORCE: " + x);
            //}
            //foreach (var x in fleeSteeringForces)
            //{
            //    Debug.Log("2 FLEE FORCE: " + x);
            //}
            //foreach (var x in flockSteeringForces)
            //{
            //    Debug.Log("3 FLOCK FORCE: " + x);
            //}
            //Debug.Log("----------------");
            //Debug.Log("COMMON " + commonSteeringForces[0]);
            //Debug.Log("FLEE " + fleeSteeringForces[0]);
            //Debug.Log("FLOCK " + flockSteeringForces[0]);

            ////COMMON MRUA
            Dependency = Entities
                .WithName("Common_MRUA")
                //.WithReadOnly(commonSteeringForces)
                //.WithReadOnly(fleeSteeringForces)
                //.WithReadOnly(flockSteeringForces)
                .ForEach((int entityInQueryIndex, ref Translation translation, ref Rotation rotation, ref AgentData agent) =>
                {
                    //agent.steeringForce = commonSteeringForces[entityInQueryIndex] + fleeSteeringForces[entityInQueryIndex] + flockSteeringForces[entityInQueryIndex];
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
                    if (speed > 0.1f)
                    {
                        translation.Value += agent.velocity * deltaTime;
                        agent.forward = math.lerp(agent.forward, math.normalizesafe(agent.velocity), agent.forwardSmooth * deltaTime);
                        rotation.Value = quaternion.LookRotation(new float3(agent.forward.x, 0, agent.forward.z), new float3(0, 1, 0));
                    }

                }).ScheduleParallel(Dependency);

            JobHandle disposeJobHandle = spartanAgentsArray.Dispose(Dependency);
            disposeJobHandle = JobHandle.CombineDependencies(disposeJobHandle, spartanTranslationArray.Dispose(Dependency));
            disposeJobHandle = JobHandle.CombineDependencies(disposeJobHandle, spartanEntityArray.Dispose(Dependency));
            disposeJobHandle = JobHandle.CombineDependencies(disposeJobHandle, enemyAgentsArray.Dispose(Dependency));
            disposeJobHandle = JobHandle.CombineDependencies(disposeJobHandle, enemyTranslationArray.Dispose(Dependency));
            disposeJobHandle = JobHandle.CombineDependencies(disposeJobHandle, enemyEntityArray.Dispose(Dependency));
            disposeJobHandle = JobHandle.CombineDependencies(disposeJobHandle, jobHandles.Dispose(Dependency));
            disposeJobHandle = JobHandle.CombineDependencies(disposeJobHandle, commonSteeringForces.Dispose(Dependency));
            disposeJobHandle = JobHandle.CombineDependencies(disposeJobHandle, fleeSteeringForces.Dispose(Dependency));
            disposeJobHandle = JobHandle.CombineDependencies(disposeJobHandle, flockSteeringForces.Dispose(Dependency));
            Dependency = disposeJobHandle;

            _spartanQuery.AddDependency(Dependency);
            _enemyQuery.AddDependency(Dependency);

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
            
            return (desiredVelocity - agent.velocity);
        }

        private static float3 Flee(in Entity entity, in NativeArray<Entity> entityArray, in Translation translation, in AgentData agent, in AgentSettings settings, in NativeArray<Translation> translationArray) 
        {
            float3 fleeingForce = 0;
            for (int i = 0; i < translationArray.Length; i++)
            {
                if(entityArray[i] != entity)
                {
                    if (math.length(translationArray[i].Value - translation.Value) < settings.neighborRadius)
                    {
                        float3 steering = translation.Value - translationArray[i].Value;
                        steering = math.normalize(steering);

                        fleeingForce += steering * settings.maxSpeed - agent.velocity;
                        fleeingForce /= settings.maxSpeed;
                        fleeingForce *= settings.maxForce;
                    }
                }
            }

            return fleeingForce;
        }

        /// <summary>
        /// The same as Flee but without checking if it's the entity itself
        /// </summary>
        /// <param name="translation"></param>
        /// <param name="agent"></param>
        /// <param name="settings"></param>
        /// <param name="translationArray"></param>
        /// <returns></returns>
        private static float3 EnemyFlee(in Translation translation, in AgentData agent, in AgentSettings settings, in NativeArray<Translation> translationArray)
        {
            float3 fleeingForce = 0;
            for (int i = 0; i < translationArray.Length; i++)
            {
                if (math.length(translationArray[i].Value - translation.Value) < settings.neighborRadius)
                {
                    float3 steering = translation.Value - translationArray[i].Value;
                    steering = math.normalize(steering);

                    fleeingForce += steering * settings.maxSpeed - agent.velocity;
                    fleeingForce /= settings.maxSpeed;
                    fleeingForce *= settings.maxForce;
                }
            }

            return fleeingForce;
        }

        public static float3 Flock(in Entity entity, in NativeArray<Entity> entityArray, in Translation translation, in AgentSettings settings, in NativeArray<AgentData> agentsDataArray, in NativeArray<Translation> translationArray)
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
                if(entity != entityArray[i])
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
