using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Spartans.Steering
{
    [BurstCompile]
    struct FullSteeringJob : IJobChunk
    {
        public float DeltaTime;
        public ArchetypeChunkComponentType<Translation> TranslationType;
        public ArchetypeChunkComponentType<Rotation> RotationType;
        public ArchetypeChunkComponentType<AgentData> AgentDataType;
        [ReadOnly] public ArchetypeChunkEntityType EntityDataType;

        [ReadOnly] public ArchetypeChunkComponentType<SpartanTag> Spartans;
        [ReadOnly] public ArchetypeChunkComponentType<EnemyTag> Enemies;
        [ReadOnly] public AgentSettings settings;
        [ReadOnly] public NativeArray<Translation> spartanTranslationArray;
        [ReadOnly] public NativeArray<Translation> enemyTranslationArray;
        [ReadOnly] public NativeArray<Entity> spartanEntityArray;
        [ReadOnly] public NativeArray<Entity> enemyEntityArray;
        [ReadOnly] public NativeArray<AgentData> spartanAgentsArray;
        [ReadOnly] public NativeArray<AgentData> enemyAgentsArray;


        public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
        {
            var chunkTranslations = chunk.GetNativeArray(TranslationType);
            var chunkRotations = chunk.GetNativeArray(RotationType);
            var chunkAgentsData = chunk.GetNativeArray(AgentDataType);
            var chunkEntityData = chunk.GetNativeArray(EntityDataType);

            for (int i = 0; i<chunk.Count; i++)
            {
                //float3 frictionForce = math.normalizesafe(-chunkAgentsData[i].velocity) * settings.maxForce / 2f;
                //float3 movingForce = chunkAgentsData[i].direction * settings.maxForce;
                //float3 seekingForce = SteeringPhysics.Seek(chunkTranslations[i], chunkAgentsData[i], in settings);

                //float3 fleeingForce = float3.zero;
                //float3 flockingForce = float3.zero;
                //if (chunk.HasChunkComponent(Spartans))
                //{
                //    fleeingForce = SteeringPhysics.Flee(chunkEntityData[i], in spartanEntityArray, chunkTranslations[i], chunkAgentsData[i], settings, in spartanTranslationArray);
                //    fleeingForce += SteeringPhysics.EnemyFlee(chunkTranslations[i], chunkAgentsData[i] , settings, in enemyTranslationArray) * chunkAgentsData[i].enemyFleeRelation;
                //    flockingForce = SteeringPhysics.Flock(chunkEntityData[i], in spartanEntityArray, chunkTranslations[i], settings, in spartanAgentsArray, in spartanTranslationArray);
                //}
                //else if (chunk.HasChunkComponent(Enemies))
                //{
                //    fleeingForce = SteeringPhysics.Flee(chunkEntityData[i], in enemyEntityArray, chunkTranslations[i], chunkAgentsData[i], settings, in enemyTranslationArray);
                //    fleeingForce += SteeringPhysics.EnemyFlee(chunkTranslations[i], chunkAgentsData[i], settings, in spartanTranslationArray) * chunkAgentsData[i].enemyFleeRelation;
                //    flockingForce = SteeringPhysics.Flock(chunkEntityData[i], in enemyEntityArray, chunkTranslations[i], settings, in enemyAgentsArray, in enemyTranslationArray);
                //}

                //float3 steeringForce = frictionForce
                //    + movingForce * chunkAgentsData[i].moveWeight
                //    + seekingForce * chunkAgentsData[i].seekWeight
                //    + fleeingForce * chunkAgentsData[i].fleeWeight
                //    + flockingForce * chunkAgentsData[i].flockWeight;

                //float3 acceleration = steeringForce / settings.mass;


                //float3 newvelocity = chunkAgentsData[i].velocity;
                //newvelocity += acceleration * DeltaTime;
                //float speed = math.length(chunkAgentsData[i].velocity);
                //if (speed > settings.maxSpeed)
                //{
                //    newvelocity = math.normalizesafe(chunkAgentsData[i].velocity);
                //    newvelocity *= settings.maxSpeed;
                //}

                //newvelocity.y = 0;

                //AgentData newAgentData = new AgentData
                //{
                //    direction = chunkAgentsData[i].direction,
                //    velocity = newvelocity,
                //    targetPosition = chunkAgentsData[i].targetPosition,
                //    moveWeight = chunkAgentsData[i].moveWeight,
                //    seekWeight = chunkAgentsData[i].seekWeight,
                //    fleeWeight = chunkAgentsData[i].fleeWeight,
                //    enemyFleeRelation = chunkAgentsData[i].enemyFleeRelation,
                //    flockWeight = chunkAgentsData[i].flockWeight,
                //    orientationSmooth = chunkAgentsData[i].orientationSmooth
                //};

                //chunkAgentsData[i] = newAgentData;

                //if (speed > 0.1f)
                //{
                //    chunkTranslations[i] = new Translation { Value = chunkAgentsData[i].velocity * DeltaTime };
                //    quaternion lookRotation = quaternion.LookRotationSafe(chunkAgentsData[i].velocity, new float3(0, 1, 0));
                //    chunkRotations[i] = new Rotation { Value = math.slerp(chunkRotations[i].Value, lookRotation, chunkAgentsData[i].orientationSmooth * DeltaTime) };
                //}
            }
        }
    }

}
