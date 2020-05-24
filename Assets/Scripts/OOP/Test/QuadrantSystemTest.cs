using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;

namespace OOP.Test
{
    public class QuadrantSystemTest
    {
        public static Dictionary<int, QuadrantInfo> _quadrantDictionary = new Dictionary<int, QuadrantInfo>();
        private const int _yMultiplier = 1000;
        private const int _cellSize = 5;

        public int CellSize => _cellSize;
        public int YMultiplier => _yMultiplier;

        public void AllSpartansInOneCell(List<AgentTest> agents)
        {
            foreach(AgentTest agent in agents)
            {
                if (!_quadrantDictionary.ContainsKey(0))
                {
                    _quadrantDictionary[0] = new QuadrantInfo(agent);
                }
                else
                {
                    _quadrantDictionary[0]._agents.Add(agent);
                    _quadrantDictionary[0].massCenter += agent.position;
                    _quadrantDictionary[0].alignment += agent.position;
                }
            }
        }

        public void UpdateQuadrant(List<AgentTest> agents)
        {
            _quadrantDictionary.Clear();

            foreach(AgentTest agent in agents)
            {
                int hashMapKey = GetPositionHashMapKey(agent.position);
                agent.cell = hashMapKey;
                if (!_quadrantDictionary.ContainsKey(hashMapKey))
                {
                    _quadrantDictionary[hashMapKey] = new QuadrantInfo(agent);
                }
                else
                {
                    _quadrantDictionary[hashMapKey]._agents.Add(agent);
                    _quadrantDictionary[hashMapKey].massCenter += agent.position;
                    _quadrantDictionary[hashMapKey].alignment += agent.position;
                }
            }
        }
         
        private int GetPositionHashMapKey(float3 position)
        {
            int hash;
            hash = (int)(math.floor(position.x / _cellSize) + (_yMultiplier * math.floor(position.z / _cellSize)));
            //Debug.Log("hashMapKey: " + hash);
            return hash;
        }

        //private void OnDrawGizmos()
        //{
        //    Debug.Log("giz");
        //    Gizmos.color = Color.black;
        //    Vector3 size = new Vector3(CellSize, CellSize, CellSize);
        //    Vector3 center = new Vector3(-10, 0, -10);
        //    for (int i = 0; i < 10; i++)
        //    {
        //        for (int j = 0; j < 10; j++)
        //        {
        //            Gizmos.DrawWireCube(center, size);
        //            center += new Vector3(0, 0, CellSize);
        //        }

        //        center = new Vector3(center.x + CellSize, 0, -10);
        //    }
        //}
    }

    public class QuadrantInfo
    {
        public List<AgentTest> _agents;
        public float3 massCenter;
        public float3 alignment;

        public QuadrantInfo(AgentTest agent)
        {
            _agents = new List<AgentTest>();
            _agents.Add(agent);
            massCenter = agent.position;
            alignment = agent.position;
        }
    }
}
