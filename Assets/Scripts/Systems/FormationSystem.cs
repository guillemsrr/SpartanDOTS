using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using Unity.Mathematics;

namespace Core
{
    public class FormationSystem : SystemBase
    {
        float _deltaTime;
        protected override void OnUpdate()
        {
            _deltaTime = Time.DeltaTime;

            Entities.ForEach((ref AgentData agent) =>
            {


            }).Schedule();

        }
    }
}
