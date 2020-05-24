using Unity.Collections;
using Unity.Entities;
using Spartans.Obstacle;
using Unity.Mathematics;

namespace Spartans.Quadrant
{
    public struct QuadrantInfo : IBufferElementData
    {
        public float3 spartanMassCenter;
        public float3 enemyMassCenter;
        public float3 spartanAlignment;
        public float3 enemyAlignment;

        public void Initialize(float3 property)
        {
            if(!IsInitialized(property))
                property = new float3(0,0,0);
        }

        public bool IsInitialized(float3 property)
        {
            return Equals(property, new float3(0, 0, 0));
        }
    }

}