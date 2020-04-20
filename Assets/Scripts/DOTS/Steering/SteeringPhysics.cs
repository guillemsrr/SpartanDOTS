using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;

namespace Spartans.Steering
{
    public class SteeringPhysics
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="translation"></param>
        /// <param name="agent"></param>
        /// <param name="settings"></param>
        /// <returns></returns>
        public static float3 Seek(in Translation translation, in AgentData agent, in AgentSettings settings)
        {
            float3 desiredVelocity = math.normalizesafe(agent.targetPosition - translation.Value);
            desiredVelocity *= settings.maxSpeed;

            return (desiredVelocity - agent.velocity);
        }

        public static float3 Flee(in Entity entity, in NativeArray<Entity> entityArray, in Translation translation, in AgentData agent, in AgentSettings settings, in NativeArray<Translation> translationArray)
        {
            float3 fleeingForce = 0;
            for (int i = 0; i < translationArray.Length; i++)
            {
                if (entityArray[i] != entity)
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
        public static float3 EnemyFlee(in Translation translation, in AgentData agent, in AgentSettings settings, in NativeArray<Translation> translationArray)
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
                if (entity != entityArray[i])
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
