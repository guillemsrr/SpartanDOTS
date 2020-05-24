using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using Spartans;

namespace OOP.Test
{
    public class FormationSystemTest
    {
        int _numCols = 5;
        float _rowSeparation = 1f;
        float _colSeparation = 1f;

        public void UpdateFormation(List<AgentTest> agentsList)
        {
            float3 averageVelocity = float3.zero;
            float3 averagePosition = float3.zero;

            foreach (AgentTest agent in agentsList)
            {
                averageVelocity += agent.velocity;
                averagePosition += agent.position;
            }

            int numSpartans = agentsList.Count;

            //job.Complete();//provisional

            averageVelocity /= numSpartans;
            averagePosition /= numSpartans;

            float3 alignmentDirection = math.normalizesafe(averageVelocity);
            //Debug.Log("alignmentDirection " + alignmentDirection);
            float3 alignmentPerpendicular = math.cross(alignmentDirection, new float3(0, 1, 0));
            alignmentPerpendicular = math.normalizesafe(alignmentPerpendicular);
            //Debug.Log("alignmentPerpendicular " + alignmentPerpendicular);

            //Get furthest spartan
            int numEntity = 0;
            int entityInQueryIndex = 0;
            float lengthsq = math.lengthsq(alignmentDirection);

            float distance = 0;

            foreach (AgentTest agent in agentsList)
            {
                float3 direction = math.normalizesafe(agent.position - averagePosition);
                //TraceLine(agent.GetPosition(), direction, 5, agent._gameObject.transform);
                //TraceLine(agent.GetPosition(), alignmentDirection, 5, agent._gameObject.transform);
                //angle:
                float angle = Mathf.Rad2Deg * math.acos(math.dot(direction, alignmentDirection) / math.lengthsq(direction) * lengthsq);
                //Debug.Log("angle " + angle);
                //agent.transform.name = "entity num " + entityInQueryIndex + " angle " + angle.ToString();

                //mirar quin angles em dóna
                if (angle < 45f)
                {
                    float newDistance = math.length(agent.position - averagePosition);
                    //agent.transform.name += " distance " + newDistance.ToString();
                    if (newDistance > distance)
                    {
                        numEntity = entityInQueryIndex;
                        distance = newDistance;
                    }
                }

                entityInQueryIndex++;
            }

            //Debug.Log("numEntity " + numEntity);

            var formationPositions = new List<int2>();
            var targetPositions = new List<float3>();
            var positionsList = new List<float3>();
            var placedEntities = new List<int>();


            for (int i = 0; i < numSpartans; i++)
            {
                //Maybe I should add instead of equal?
                formationPositions.Add(new int2());
                targetPositions.Add(new float3());
                float3 pos = agentsList[i].position;
                positionsList.Add(pos);//CopyFrom?
            }

            float3 leaderPosition = positionsList[numEntity];
            targetPositions[numEntity] = leaderPosition;
            formationPositions[numEntity] = new int2(0, 0);//the leader

            //CHANGE
            //placedEntities.Add(numEntity);

            //center and right side of leader
            for (int i = 0; i < (_numCols - 1) / 2 + 1; i++)
            {
                //CHANGE
                for (int j = 0; j < numSpartans / _numCols; j++)
                {
                    //calculate target pos
                    float3 targetPos = leaderPosition + alignmentPerpendicular * i * _colSeparation - alignmentDirection * j * _rowSeparation;//CHANGE
                    //Debug.Log(" RIGHT -> i, j (" + i + " " + j + ") targetPos " + targetPos);
                    distance = 100f;
                    //get the closest agent
                    for (int z = 0; z < numSpartans; z++)
                    {
                        if (!placedEntities.Contains(z))
                        {
                            float newDistance = math.length(positionsList[z] - targetPos);
                            if (newDistance < distance)
                            {
                                numEntity = z;
                                distance = newDistance;
                            }
                        }
                    }

                    placedEntities.Add(numEntity);
                    //Debug.LogError("I put numEntity " + numEntity);
                    foreach (int xs in placedEntities)
                    {
                        //Debug.Log("placedEntities has " + xs);
                    }

                    //set to array
                    targetPositions[numEntity] = targetPos;
                    //Debug.Log("new target RIGHT numEntity " + numEntity + " is " + targetPos);
                    formationPositions[numEntity] = new int2(i, j);
                }
            }

            //Debug.LogError("placed Entities count: " + placedEntities.Count);

            //left side of leader
            for (int i = -1; i > -((_numCols - 1) / 2 + 1); i--)// ---- REVISAR
            {
                for (int j = 0; j < numSpartans / _numCols; j++)
                {
                    //calculate target pos
                    float3 targetPos = leaderPosition + alignmentPerpendicular * i * _colSeparation - alignmentDirection * j * _rowSeparation;//CHANGE
                    //Debug.Log(" LEFT -> i, j (" + i + " " + j + ") targetPos " + targetPos);

                    distance = 100f;
                    //get the closest agent
                    for (int z = 0; z < numSpartans; z++)
                    {
                        if (!placedEntities.Contains(z))
                        {
                            float newDistance = math.length(positionsList[z] - targetPos);
                            if (newDistance < distance)
                            {
                                numEntity = z;
                                distance = newDistance;
                            }
                        }
                    }

                    placedEntities.Add(numEntity);
                    //Debug.LogError("I put numEntity " + numEntity);
                    foreach (int xs in placedEntities)
                    {
                        //Debug.Log("placedEntities has " + xs);
                    }

                    //set to array
                    targetPositions[numEntity] = targetPos;
                    //Debug.Log("new target LEFT numEntity " + numEntity + " is " + targetPos);
                    formationPositions[numEntity] = new int2(i, j);
                }
            }

            entityInQueryIndex = 0;
            foreach (AgentTest agent in agentsList)
            {
                agent.targetPosition = targetPositions[entityInQueryIndex];
                //agent.formationPosition = formationPositions[entityInQueryIndex];
                //Debug.Log("target final of num " + entityInQueryIndex + " is " + targetPositions[entityInQueryIndex]);
                entityInQueryIndex++;
            }
        }
    }
}
