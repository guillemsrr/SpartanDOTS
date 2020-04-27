using Unity.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using Unity.Mathematics;
using Spartans.Quadrant;

namespace Spartans.Steering
{
    public class SteeringBehaviorsSystem : SystemBase
    {
        List<AgentSettings> _settings = new List<AgentSettings>();

        protected override void OnCreate()
        {
            base.OnCreate();

            _settings = new List<AgentSettings>();
        }
        protected override void OnUpdate()
        {
            EntityManager.GetAllUniqueSharedComponentData(_settings);
            AgentSettings settings = _settings[1];

            float deltaTime = Time.DeltaTime * Environment.TimeSpeed;

            #region WITH_IJOBCHUNK

            //FullSteeringJob fullSteeringJob = new FullSteeringJob()
            //{
            //    DeltaTime = deltaTime,
            //    TranslationType = GetArchetypeChunkComponentType<Translation>(),
            //    RotationType = GetArchetypeChunkComponentType<Rotation>(),
            //    AgentDataType = GetArchetypeChunkComponentType<AgentData>(),
            //    Spartans = GetArchetypeChunkComponentType<SpartanTag>(),
            //    Enemies = GetArchetypeChunkComponentType<EnemyTag>(),
            //    EntityDataType = GetArchetypeChunkEntityType(),
            //    settings = settings,
            //    spartanTranslationArray = spartanTranslationArray,
            //    spartanEntityArray = spartanEntityArray,
            //    spartanAgentsArray = spartanAgentsArray,
            //    enemyAgentsArray = enemyAgentsArray,
            //    enemyTranslationArray = enemyTranslationArray,
            //    enemyEntityArray  = enemyEntityArray
            //};
            //BUGS->
            //Dependency = fullSteeringJob.ScheduleParallel(_agentQuery, Dependency);

            #endregion

            #region WITH_FOREACH

            JobHandle spartanSteeringJobHandle = Entities
                .WithName("Spartan_Steering_Behavior")
                .WithAll<SpartanTag>()
                .ForEach((int entityInQueryIndex, ref AgentData agent, in QuadrantTag quadrantTag) =>
                {
                    float3 frictionForce = math.normalizesafe(-agent.velocity) * settings.maxForce / 2f;
                    float3 movingForce = agent.direction * settings.maxForce;
                    float3 seekingForce = SteeringPhysics.Seek(in agent, in settings);
                    float3 fleeingForce = float3.zero;
                    float3 flockingForce = float3.zero;

                    //var otherSpartans = QuadrantSystem.spartanQuadrantHashMap[quadrantTag.numQuadrant].agentsData;
                    //var otherEnemies = QuadrantSystem.enemyQuadrantHashMap[quadrantTag.numQuadrant].agentsData;
                    //var neighbours = QuadrantSystem.spartanQuadrantHashMap[quadrantTag.numQuadrant].numAgents;
                    //var alignment = QuadrantSystem.spartanQuadrantHashMap[quadrantTag.numQuadrant].quadrantAlignment;
                    //var massCenter = QuadrantSystem.spartanQuadrantHashMap[quadrantTag.numQuadrant].quadrantMassCenter;

                    //fleeingForce = SteeringPhysics.Flee( in agent, in otherSpartans, in settings);
                    //fleeingForce += SteeringPhysics.Flee(in agent, in otherEnemies, in settings) * agent.enemyFleeRelation;
                    //flockingForce = SteeringPhysics.QuadrantFlock(neighbours, in agent, massCenter, alignment, settings);

                    agent.steeringForce = frictionForce + movingForce * agent.moveWeight + seekingForce * agent.seekWeight + fleeingForce * agent.fleeWeight + flockingForce * agent.flockWeight;

                }).ScheduleParallel(Dependency);

            Dependency = spartanSteeringJobHandle;

            JobHandle enemySteeringJobHandle = Entities
                .WithName("Enemy_Steering_Behavior")
                .WithAll<EnemyTag>()
                .ForEach((int entityInQueryIndex, ref AgentData agent, in QuadrantTag quadrantTag) =>
                {
                    float3 frictionForce = math.normalizesafe(-agent.velocity) * settings.maxForce / 2f;
                    float3 movingForce = agent.direction * settings.maxForce;
                    float3 seekingForce = SteeringPhysics.Seek(in agent, in settings);
                    float3 fleeingForce = float3.zero;
                    float3 flockingForce = float3.zero;

                    //var otherSpartans = QuadrantSystem.spartanQuadrantHashMap[quadrantTag.numQuadrant].agentsData;
                    //var otherEnemies = QuadrantSystem.enemyQuadrantHashMap[quadrantTag.numQuadrant].agentsData;
                    //var neighbours = QuadrantSystem.spartanQuadrantHashMap[quadrantTag.numQuadrant].numAgents;
                    //var alignment = QuadrantSystem.spartanQuadrantHashMap[quadrantTag.numQuadrant].quadrantAlignment;
                    //var massCenter = QuadrantSystem.spartanQuadrantHashMap[quadrantTag.numQuadrant].quadrantMassCenter;

                    //fleeingForce = SteeringPhysics.Flee(in agent, in otherSpartans, in settings)* agent.enemyFleeRelation;
                    //fleeingForce += SteeringPhysics.Flee(in agent, in otherEnemies, in settings);
                    //flockingForce = SteeringPhysics.QuadrantFlock(neighbours, in agent, massCenter, alignment, settings);

                    agent.steeringForce = frictionForce + movingForce * agent.moveWeight + seekingForce * agent.seekWeight + fleeingForce * agent.fleeWeight + flockingForce * agent.flockWeight;

                }).ScheduleParallel(Dependency);

            Dependency = JobHandle.CombineDependencies(spartanSteeringJobHandle, enemySteeringJobHandle);


            Dependency = Entities
                .WithName("MRUA")
                .ForEach((int entityInQueryIndex, ref Translation translation, ref Rotation rotation, ref AgentData agent) =>
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
                    agent.position = agent.velocity * deltaTime;

                    translation.Value = agent.position;
                    quaternion lookRotation = quaternion.LookRotationSafe(agent.velocity, new float3(0, 1, 0));
                    rotation.Value = math.slerp(rotation.Value, lookRotation, agent.orientationSmooth * deltaTime);

                }).ScheduleParallel(Dependency);

            #endregion

        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
        }


    }
}


