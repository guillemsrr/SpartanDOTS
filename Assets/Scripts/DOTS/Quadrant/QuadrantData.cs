using Unity.Collections;
using Unity.Entities;
using Spartans.Obstacle;
using Unity.Mathematics;

namespace Spartans.Quadrant
{
    [GenerateAuthoringComponent]
    public struct QuadrantTag: IComponentData
    {
        public int numQuadrant;
    }

    public struct QuadrantAgentData
    {
        public int numAgents;
        //public  NativeArray<AgentData> agentsData;
        public float3 quadrantMassCenter;
        public float3 quadrantAlignment;

        public void AddMassCenter(float3 value)
        {
            quadrantMassCenter += value;
        }
        public void AddAlignment(float3 value)
        {
            quadrantAlignment += value;
        }
    }

    public unsafe struct QuadrantObstacleData
    {
        public int numObstacles;
        //public  NativeArray<ObstacleData> obstaclesData;
    }

}
