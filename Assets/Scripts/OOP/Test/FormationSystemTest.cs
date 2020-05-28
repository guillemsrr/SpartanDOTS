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
        public AgentTest _leaderAgent;
        public int _numEntity;
        private const float DISTANCE_OFFSET = 0f;

        public void UpdateFormation(List<AgentTest> agentsList)
        {
            if(Equals(_leaderAgent.direction, float3.zero))
            {
                return;
            }

            float3 averageVelocity = float3.zero;
            float3 averagePosition = float3.zero;

            foreach (AgentTest agent in agentsList)
            {
                averageVelocity += agent.velocity;
                averagePosition += agent.position;
            }

            int numSpartans = agentsList.Count;

            averageVelocity /= numSpartans;
            averagePosition /= numSpartans;

            float3 alignmentDirection = math.normalizesafe(averageVelocity);
            float3 alignmentPerpendicular = math.cross(alignmentDirection, new float3(0, 1, 0));
            alignmentPerpendicular = math.normalizesafe(alignmentPerpendicular);
            

            //check leader position according to alignment
            int entityInQueryIndex = 0;
            float lengthsq = math.lengthsq(alignmentDirection);
            float3 direction = math.normalizesafe(_leaderAgent.position - averagePosition);
            float angle = Mathf.Rad2Deg * math.acos(math.dot(direction, alignmentDirection) / math.lengthsq(direction) * lengthsq);
            if (angle > 45f)
            {
                //look for a new leader
                //Get furthest spartan
                float distance = math.length(averagePosition - _leaderAgent.position) + DISTANCE_OFFSET;
                int newNumEntity = _numEntity;
                foreach (AgentTest agent in agentsList)
                {
                    direction = math.normalizesafe(agent.position - averagePosition);
                    angle = Mathf.Rad2Deg * math.acos(math.dot(direction, alignmentDirection) / math.lengthsq(direction) * lengthsq);

                    if (angle < 30f)
                    {
                        float newDistance = math.length(agent.position - averagePosition);
                        if (newDistance > distance)
                        {
                            newNumEntity = entityInQueryIndex;
                            distance = newDistance;
                        }
                    }

                    entityInQueryIndex++;
                }

                if(newNumEntity != _numEntity)
                {
                    _numEntity = newNumEntity;
                    _leaderAgent.meshRenderer.material.color = Color.yellow;
                    _leaderAgent = agentsList[_numEntity];
                    _leaderAgent.meshRenderer.material.color = Color.blue;
                }
            }

            
            var formationPositions = new List<int2>();
            var targetPositions = new List<float3>();
            var positionsList = new List<float3>();
            var placedEntities = new List<int>();


            for (int i = 0; i < numSpartans; i++)
            {
                formationPositions.Add(new int2());
                targetPositions.Add(new float3());
                float3 pos = agentsList[i].position;
                positionsList.Add(pos);//CopyFrom?
            }

            float3 leaderPosition = _leaderAgent.position;//positionsList[_numEntity];
            targetPositions[_numEntity] = leaderPosition;
            formationPositions[_numEntity] = new int2(0, 0);//the leader

            int numEntity = 0;
            //center and right side of leader
            for (int i = 0; i < (_numCols - 1) / 2 + 1; i++)
            {
                //CHANGE
                for (int j = 0; j < numSpartans / _numCols; j++)
                {
                    //calculate target pos
                    float3 targetPos = leaderPosition + alignmentPerpendicular * i * _colSeparation - alignmentDirection * j * _rowSeparation;
                    float distance = 100f;
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

                    //set to array
                    targetPositions[numEntity] = targetPos;
                    formationPositions[numEntity] = new int2(i, j);
                }
            }

            //left side of leader
            for (int i = -1; i > -((_numCols - 1) / 2 + 1); i--)
            {
                for (int j = 0; j < numSpartans / _numCols; j++)
                {
                    //calculate target pos
                    float3 targetPos = leaderPosition + alignmentPerpendicular * i * _colSeparation - alignmentDirection * j * _rowSeparation;

                    float distance = 100f;
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

                    //set to array
                    targetPositions[numEntity] = targetPos;
                    formationPositions[numEntity] = new int2(i, j);
                }
            }

            entityInQueryIndex = 0;
            foreach (AgentTest agent in agentsList)
            {
                agent.targetPosition = targetPositions[entityInQueryIndex];
                entityInQueryIndex++;
            }
        }
    }
}
