using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using Spartans;

namespace OOP.Test
{
    public class SteeringSystemTest
    {
        public const float mass = 0.2f;
        public const float maxSpeed = 1.95f;
        public const float maxForce = 1.75f;
        public const float separationWeight = 5f;
        public const float cohesionWeight = 1.6f;
        public const float alignmentWeight = 1f;
        public const float neighborRadius = 1f;

        public void UpdateSteering(List<AgentTest> agents)
        {
            foreach(AgentTest agent in agents)
            {
                float3 frictionForce = math.normalizesafe(-agent.velocity) * maxForce / 2f;
                float3 movingForce = agent.direction * maxForce;
                float3 seekingForce = SteeringPhysics.Seek(in agent);

                int neighbours;
                var otherSpartans = QuadrantSystemTest._quadrantDictionary[agent.cell]._agents;

                float3 fleeingForce = SteeringPhysics.Flee(in agent, in otherSpartans);
                float3 flockingForce = SteeringPhysics.Flock(in agent, otherSpartans);

                agent.steeringForce = frictionForce + movingForce * agent.moveWeight + seekingForce * agent.seekWeight + fleeingForce * agent.fleeWeight + flockingForce * agent.flockWeight;

                float3 acceleration = agent.steeringForce / mass;

                agent.velocity += acceleration * Time.deltaTime;
                float speed = math.length(agent.velocity);
                if (speed > maxSpeed)
                {
                    agent.velocity = math.normalizesafe(agent.velocity);
                    agent.velocity *= maxSpeed;
                }

                agent.velocity.y = 0;
                agent.position += agent.velocity * Time.deltaTime;
                agent.transform.position = agent.position;

                quaternion lookRotation = quaternion.LookRotationSafe(agent.velocity, new float3(0, 1, 0));
                agent.transform.rotation = math.slerp(agent.transform.rotation, lookRotation, agent.orientationSmooth * Time.deltaTime);

                Debug.DrawLine(agent.position, agent.position + frictionForce, Color.black);
                Debug.DrawLine(agent.position, agent.position + movingForce * agent.moveWeight, Color.blue);
                Debug.DrawLine(agent.position, agent.position + seekingForce * agent.seekWeight, Color.green);
                Debug.DrawLine(agent.position, agent.position + fleeingForce * agent.fleeWeight, Color.red);
                Debug.DrawLine(agent.position, agent.position + flockingForce * agent.flockWeight, Color.black);
            }
        }

        public class SteeringPhysics
        {
            public static float3 Seek(in AgentTest agent)
            {
                float3 desiredVelocity = math.normalizesafe(agent.targetPosition - agent.position);
                desiredVelocity *= maxSpeed;

                return (desiredVelocity - agent.velocity);
            }

            public static float3 Flee(in AgentTest agent, in List<AgentTest> otherAgents)
            {
                float3 fleeingForce = 0;
                for (int i = 0; i < otherAgents.Count; i++)
                {
                    if (!Equals(otherAgents[i].position, agent.position))
                    {
                        float3 steering = agent.position - otherAgents[i].position;
                        if (math.length(steering) < neighborRadius)
                        {
                            steering = math.normalize(steering);

                            fleeingForce += steering * maxSpeed - agent.velocity;
                            fleeingForce /= maxSpeed;
                            fleeingForce *= maxForce;
                        }
                    }
                }

                return fleeingForce;
            }

            public static float3 Flock(in AgentTest agent, in List<AgentTest> otherAgents)
            {
                int neighborCount = 0;

                float3 separationVector = float3.zero;
                float3 averagePosition = float3.zero;
                float3 averageVelocity = float3.zero;

                float3 separationDirection;
                float3 cohesionDirection;
                float3 alignmentDirection;

                for (int i = 0; i < otherAgents.Count; i++)
                {
                    if (!Equals(otherAgents[i], agent))
                    {
                        float3 otherAgentPos = otherAgents[i].position;
                        float3 separation = agent.position - otherAgentPos;
                        if (math.length(separation) < neighborRadius)
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
                averagePosition -= agent.position;
                cohesionDirection = math.normalizesafe(averagePosition);
                averageVelocity /= neighborCount;
                alignmentDirection = math.normalizesafe(averageVelocity);

                return separationDirection * separationWeight + cohesionDirection * cohesionWeight + alignmentDirection * alignmentWeight;
            }

            public static float3 QuadrantFlock(int neighbours, in AgentTest agent, float3 massCenter, float3 alignment)
            {
                float3 separationDirection = float3.zero; // math.normalizesafe(massCenter / neighbours);
                float3 cohesionDirection = math.normalizesafe(massCenter / neighbours);
                float3 alignmentDirection = math.normalizesafe(alignment / neighbours);

                return separationDirection * separationWeight + cohesionDirection * cohesionWeight + alignmentDirection * alignmentWeight;
            }
        }
    }
}
