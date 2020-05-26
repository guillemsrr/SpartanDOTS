using Unity.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using Unity.Mathematics;
using Spartans.Quadrant;
using UnityEngine;

namespace Spartans.Steering
{
    [UpdateAfter(typeof(QuadrantSystem))]
    //[AlwaysSynchronizeSystem]
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
                .ForEach((int entityInQueryIndex, ref AgentData agent, ref Translation translation, ref Rotation rotation, in QuadrantTag quadrantTag) =>
                {
                    float3 frictionForce = math.normalizesafe(-agent.velocity);
                    float3 movingForce = agent.direction * settings.maxForce;
                    float3 seekingForce = SteeringPhysics.Seek(in agent, in settings);
                    float3 fleeingForce = float3.zero;
                    float3 flockingForce = float3.zero;

                    int neighbours;
                    var otherSpartans = GetAgentDatas(quadrantTag.numQuadrant, in QuadrantSystem.spartanQuadrantMultiHashMap, out neighbours);
                    //Debug.Log("neighbours " + neighbours);
                    var otherEnemies = GetAgentDatas(quadrantTag.numQuadrant, QuadrantSystem.enemyQuadrantMultiHashMap, out neighbours);
                    //var massCenter = QuadrantSystem.spartanMassCenterQuadrantHashMap[quadrantTag.numQuadrant];
                    //var alignment = QuadrantSystem.spartanAlignmentQuadrantHashMap[quadrantTag.numQuadrant];

                    fleeingForce = SteeringPhysics.Flee(in agent, in otherSpartans, in settings);
                    //fleeingForce += SteeringPhysics.Flee(in agent, in otherEnemies, in settings) * agent.enemyFleeRelation;
                    //flockingForce = SteeringPhysics.QuadrantFlock(neighbours, in agent, massCenter, alignment, settings);
                    flockingForce = SteeringPhysics.Flock(in agent, otherSpartans, settings);

                    agent.steeringForce = frictionForce*agent.frictionWeight + movingForce * agent.moveWeight + seekingForce * agent.seekWeight + fleeingForce * agent.fleeWeight + flockingForce * agent.flockWeight;

                    //DebugPhysicsLines(agent.position, fleeingForce, flockingForce);
                    //DebugContextLines(agent.position, fleeingForce, flockingForce);
                })
                .WithoutBurst()
                .ScheduleParallel(Dependency);

            Dependency = spartanSteeringJobHandle;

            JobHandle enemySteeringJobHandle = Entities
                .WithName("Enemy_Steering_Behavior")
                .WithAll<EnemyTag>()
                .ForEach((int entityInQueryIndex, ref AgentData agent, in QuadrantTag quadrantTag) =>
                {
                    float3 frictionForce = math.normalizesafe(-agent.velocity);
                    float3 movingForce = agent.direction * settings.maxForce;
                    float3 seekingForce = SteeringPhysics.Seek(in agent, in settings);
                    float3 fleeingForce = float3.zero;
                    float3 flockingForce = float3.zero;

                    int neighbours;
                    var otherSpartans = GetAgentDatas(quadrantTag.numQuadrant, in QuadrantSystem.spartanQuadrantMultiHashMap, out neighbours);
                    //var otherEnemies = GetAgentDatas(quadrantTag.numQuadrant, QuadrantSystem.enemyQuadrantMultiHashMap, out neighbours);
                    //var massCenter = QuadrantSystem.enemyMassCenterQuadrantHashMap[quadrantTag.numQuadrant];
                    //var alignment = QuadrantSystem.enemyAlignmentQuadrantHashMap[quadrantTag.numQuadrant];

                    fleeingForce = SteeringPhysics.Flee(in agent, in otherSpartans, in settings)* agent.enemyFleeRelation;
                    //fleeingForce += SteeringPhysics.Flee(in agent, in otherEnemies, in settings);
                    //flockingForce = SteeringPhysics.QuadrantFlock(neighbours, in agent, massCenter, alignment, settings);
                    flockingForce = SteeringPhysics.Flock(in agent, otherSpartans, settings);

                    agent.steeringForce = frictionForce*agent.frictionWeight + movingForce * agent.moveWeight + seekingForce * agent.seekWeight + fleeingForce * agent.fleeWeight + flockingForce * agent.flockWeight;
                })
                .WithoutBurst()
                .ScheduleParallel(Dependency);

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
                    agent.position += agent.velocity * deltaTime;

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

        private static NativeList<AgentData> GetAgentDatas(int hashMapKey, in NativeMultiHashMap<int, AgentData> agents, out int numAgents)
        {
            numAgents = 0;
            NativeList<AgentData> agentsList = new NativeList<AgentData>(0, Allocator.Temp);
            AgentData agent;
            NativeMultiHashMapIterator<int> nativeMultiHashMapIterator;
            if(agents.TryGetFirstValue(hashMapKey, out agent, out nativeMultiHashMapIterator))
            {
                do
                {
                    numAgents++;
                    agentsList.Add(agent);
                }
                while (agents.TryGetNextValue(out agent, ref nativeMultiHashMapIterator));
            }

            return agentsList;
        }

        private static void DebugStreamPhysicsLines(float3 position, float3 flee, float3 flock)
        {
            DebugStream.Line fleeForceLine = new DebugStream.Line()
            {
                X0 = position,
                X1 = position + flee * 10,
                Color = UnityEngine.Color.black,
            };

            fleeForceLine.Draw();

            DebugStream.Line flockForceLine = new DebugStream.Line()
            {
                X0 = position,
                X1 = position + flock * 10,
                Color = UnityEngine.Color.red,
            };

            flockForceLine.Draw();
        }

        private static void DebugContextLines(float3 position, float3 flee, float3 flock)
        {
            DebugStream.Context context = new DebugStream.Context();
            context.Line(position, position + flee * 10, Color.black);
            context.Line(position, position + flock * 10, Color.red);

            //DebugStream.Line fleeForceLine = new DebugStream.Line()
            //{
            //    X0 = position,
            //    X1 = position + flee * 10,
            //    Color = UnityEngine.Color.black,
            //};

            //fleeForceLine.Draw();

            //DebugStream.Line flockForceLine = new DebugStream.Line()
            //{
            //    X0 = position,
            //    X1 = position + flock * 10,
            //    Color = UnityEngine.Color.red,
            //};

            //flockForceLine.Draw();
        }

        private static void DebugPhysicsLines(float3 position, float3 flee, float3 flock)
        {
            Debug.DrawLine(position, position + flee, Color.red);
            Debug.DrawLine(position, position + flock, Color.black);
        }

    }
}


