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
        public static float3 Seek(in AgentData agent, in AgentSettings settings)
        {
            float3 desiredVelocity = math.normalizesafe(agent.targetPosition - agent.position);
            desiredVelocity *= settings.maxSpeed;

            return (desiredVelocity - agent.velocity);
        }

        public static float3 Flee(in AgentData agent, in NativeArray<AgentData> otherAgents, in AgentSettings settings)
        {
            float3 fleeingForce = 0;
            for (int i = 0; i < otherAgents.Length; i++)
            {   
                if ( !Equals(otherAgents[i].position, agent.position))
                {
                    float3 steering = agent.position - otherAgents[i].position;
                    if (math.length(steering) < settings.neighborRadius)
                    {
                        steering = math.normalize(steering);

                        fleeingForce += steering * settings.maxSpeed - agent.velocity;
                        fleeingForce /= settings.maxSpeed;
                        fleeingForce *= settings.maxForce;
                    }
                }
            }

            return fleeingForce;
        }

        public static float3 Flock(in Translation translation, in NativeArray<AgentData> otherAgents, in AgentSettings settings)
        {
            int neighborCount = 0;

            float3 separationVector = float3.zero;
            float3 averagePosition = float3.zero;
            float3 averageVelocity = float3.zero;

            float3 separationDirection;
            float3 cohesionDirection;
            float3 alignmentDirection;

            for (int i = 0; i < otherAgents.Length; i++)
            {
                if (!Equals(otherAgents[i].position, translation.Value))
                {
                    float3 otherAgentPos = otherAgents[i].position;
                    float3 separation = translation.Value - otherAgentPos;
                    if (math.length(separation) < settings.neighborRadius)
                    {
                        separationVector += separation;
                        averagePosition += otherAgentPos;
                        averageVelocity += otherAgents[i].velocity;

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

        public static float3 QuadrantFlock(int neighbours, in AgentData agent, float3 massCenter, float3 alignment, in AgentSettings settings)
        {
            float3 separationDirection = float3.zero; // math.normalizesafe(massCenter / neighbours);
            float3 cohesionDirection = math.normalizesafe(massCenter / neighbours);
            float3 alignmentDirection = math.normalizesafe(alignment / neighbours);

            return separationDirection * settings.separationWeight + cohesionDirection * settings.cohesionWeight + alignmentDirection * settings.alignmentWeight;
        }
    }
}
