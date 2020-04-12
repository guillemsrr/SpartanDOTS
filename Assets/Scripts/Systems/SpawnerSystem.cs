using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace Spartans
{
    [DisableAutoCreation]
    public class SpawnerSystem : ComponentSystem
    {
        EntityQuery _mainGroup;
        float _numEntities;

        public float NumEntities { set { _numEntities = value; } }
        protected override void OnCreate()
        {
            _mainGroup = GetEntityQuery(
                ComponentType.ReadOnly<SpartanSpawn>(),
                ComponentType.ReadOnly<Translation>());
        }

        protected override void OnUpdate()
        {
            Entities.ForEach((Entity spawnerEntity, ref SpartanSpawn spawnerData, ref Translation translation) =>
            {
                for (int i = 0; i< _numEntities; i++)
                {
                    var newEntity = PostUpdateCommands.Instantiate(spawnerData.Prefab);
                    Vector3 pos = new Vector3(UnityEngine.Random.Range(-5f, 5f), 0f, UnityEngine.Random.Range(-5f, 5f));
                    PostUpdateCommands.SetComponent(newEntity, new Translation { Value = pos });
                    PostUpdateCommands.AddComponent(newEntity, new Rotation { Value = Quaternion.identity });
                    PostUpdateCommands.AddComponent(newEntity, new SpartanActionsData { });
                    PostUpdateCommands.AddComponent(newEntity, new AgentData {
                        moveWeight = 1.5f,
                        seekWeight = 100f,
                        fleeWeight = 0.9f,
                        flockWeight = 1f,
                        targetPosition = pos
                    });
                }
            });
        }
    }

}
