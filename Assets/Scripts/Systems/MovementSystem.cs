using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;

public class MovementSystem : JobComponentSystem
{
    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        float deltaTime = Time.DeltaTime;

        JobHandle job = Entities.ForEach((ref Translation trans, in MovementData movement) =>
        {
            trans.Value += movement.direction * movement.speed * deltaTime;

        }).Schedule(inputDeps);
        return job;
    }
}
