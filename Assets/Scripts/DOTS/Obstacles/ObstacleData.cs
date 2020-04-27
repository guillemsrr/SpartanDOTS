using Unity.Entities;
using Unity.Mathematics;

namespace Spartans.Obstacle
{
    [GenerateAuthoringComponent]
    public struct ObstacleData : IComponentData
    {
        public float3 position;
        public float2 dimensions; 
    }
}
