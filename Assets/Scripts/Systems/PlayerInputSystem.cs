using Unity.Entities;
using Unity.Jobs;
using UnityEngine;
using Unity.Mathematics;

[AlwaysSynchronizeSystem]
public class PlayerInputSystem : JobComponentSystem
{
    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        Entities.ForEach((ref SpartanActionsData action, ref MovementData movement) =>
        {
            float3 direction = new Vector3(Input.GetAxisRaw("Horizontal"), 0f, Input.GetAxisRaw("Vertical"));
            movement.direction = direction;

        }).Run();
        return default;
    }
}
