using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;

namespace OOP.Test
{
    public class AgentTest : MonoBehaviour
    {
        //steering
        public float3 position;
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

        //quadrant:
        public int cell;

        public void Init(float3 pos)
        {
            position = pos;
            targetPosition = pos;
            velocity = float3.zero;
            steeringForce = float3.zero;
            direction = float3.zero;
            moveWeight = 1.5f;
            seekWeight = 2f;
            fleeWeight = 0f;
            flockWeight = 0f;
            enemyFleeRelation = 2f;
            orientationSmooth = 0.5f;
            cell = 0;
        }
    }
}
