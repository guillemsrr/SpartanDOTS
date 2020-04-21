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
        float _numSpartanEntities;
        float _numEnemyEntities = 10;

        public float NumEntities { set { _numSpartanEntities = value; } }
        protected override void OnCreate()
        {
            _mainGroup = GetEntityQuery(
                ComponentType.ReadOnly<SpartanSpawn>(),
                ComponentType.ReadOnly<Translation>());
        }

        protected override void OnUpdate()
        {
            //SPARTANS
            Entities.ForEach((Entity spawnerEntity, ref SpartanSpawn spartanSpawn, ref Translation translation) =>
            {
                for (int i = 0; i< _numSpartanEntities; i++)
                {
                    var newEntity = PostUpdateCommands.Instantiate(spartanSpawn.Prefab);
                    Vector3 pos = new Vector3(UnityEngine.Random.Range(-5f, 5f), 0f, UnityEngine.Random.Range(-5f, 5f) - 10f);
                    PostUpdateCommands.SetComponent(newEntity, new Translation { Value = pos });
                    PostUpdateCommands.AddComponent(newEntity, new Rotation { Value = Quaternion.identity });
                    PostUpdateCommands.AddComponent(newEntity, new SpartanData { });
                    PostUpdateCommands.AddComponent(newEntity, new SpartanTag { });
                    PostUpdateCommands.AddComponent(newEntity, new AgentData
                    {
                        targetPosition = pos,
                        moveWeight = 1.5f,
                        seekWeight = 2f,
                        fleeWeight = 0.9f,
                        enemyFleeRelation = 2f,
                        flockWeight = 1f,
                        orientationSmooth = 0.5f
                    });
                }
            });

            //ENEMIES
            Entities.ForEach((Entity spawnerEntity, ref EnemySpawn enemySpawn, ref Translation translation) =>
            {
                for (int i = 0; i < _numEnemyEntities; i++)
                {
                    var newEntity = PostUpdateCommands.Instantiate(enemySpawn.Prefab);
                    Vector3 pos = new Vector3(UnityEngine.Random.Range(-5f, 5f), 0f, UnityEngine.Random.Range(-5f, 5f) + 10f);
                    PostUpdateCommands.SetComponent(newEntity, new Translation { Value = pos });
                    PostUpdateCommands.AddComponent(newEntity, new Rotation { Value = Quaternion.identity });
                    PostUpdateCommands.AddComponent(newEntity, new EnemyTag {});
                    PostUpdateCommands.AddComponent(newEntity, new AgentData
                    {
                        targetPosition = pos,
                        moveWeight = 1.5f,
                        seekWeight = 2f,
                        fleeWeight = 0.9f,
                        enemyFleeRelation = 2f,
                        flockWeight = 1f,
                        orientationSmooth = 0.5f
                    });
                }
            });
        }
    }

}
