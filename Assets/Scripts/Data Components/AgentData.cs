using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Core
{
    [AddComponentMenu("DOTS Components/Movement")]
    [GenerateAuthoringComponent]
    public struct AgentData : IComponentData
    {
        public float mass;//0.2f
        public float maxSpeed;//1.95f
        public float maxForce;//1.75f
        public float3 velocity;
        public float3 direction;
        public float4 rotation;
        public float3 steeringForce;

    }
}
