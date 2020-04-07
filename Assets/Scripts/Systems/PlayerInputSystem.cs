using Unity.Entities;
using Unity.Jobs;
using UnityEngine;
using Unity.Mathematics;

namespace Core
{
    public class PlayerInputSystem : ComponentSystem
    {
        protected override void OnUpdate()
        {
            float3 direction = Camera.main.transform.forward;
            Entities.ForEach((ref SpartanActionsData action, ref AgentData agent) =>
            {
                float3 moveInput = new Vector3(Input.GetAxisRaw("Horizontal"), 0f, Input.GetAxisRaw("Vertical"));
                direction = moveInput;//TransformDirection(direction);
                direction.y = 0;
                agent.direction = direction;
            });
        }
    }
}
