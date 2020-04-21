using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using System.ComponentModel;

namespace Spartans
{
    #region Components
    public struct AgentData : IComponentData
    {
        //steering
        public float3 direction;
        public float3 velocity;
        public float3 targetPosition;
        public float3 steeringForce;

        //weights:
        public float moveWeight;
        public float seekWeight;
        public float fleeWeight;
        public float enemyFleeRelation;
        public float flockWeight;

        //smooths:
        public float orientationSmooth;
    }
    #endregion

    #region SharedComponents

    public struct AgentSettings : ISharedComponentData
    {
        [DefaultValue(0.2f)]
        public float mass;
        [DefaultValue(1.95f)]
        public float maxSpeed;
        [DefaultValue(1.75f)]
        public float maxForce;
        [DefaultValue(5f)]
        public float separationWeight;
        [DefaultValue(1.6f)]
        public float cohesionWeight;
        [DefaultValue(1f)]
        public float alignmentWeight;
        [DefaultValue(1f)]
        public float neighborRadius;
    }

    #endregion


}
