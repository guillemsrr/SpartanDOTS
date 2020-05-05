using Unity.Entities;
using Unity.Transforms;
using UnityEngine;
using Spartans.Quadrant;

namespace Spartans
{
    [DisableAutoCreation]
    public class SpawnerSystem : ComponentSystem
    {
        float _numSpartanEntities;
        float _numEnemyEntities = 0;

        public float NumEntities { set { _numSpartanEntities = value; } }
        protected override void OnCreate()
        {
            
        }

        protected override void OnUpdate()
        {
            //SPARTANS
            Entities.ForEach((Entity spawnerEntity, ref SpartanSpawn spartanSpawn, ref Translation translation) =>
            {
                for (int i = 0; i< _numSpartanEntities; i++)
                {
                    var spartanEntity = PostUpdateCommands.Instantiate(spartanSpawn.Prefab);
                    Vector3 pos = new Vector3(UnityEngine.Random.Range(-5f, 5f), 0f, UnityEngine.Random.Range(-5f, 5f) - 10f);
                    PostUpdateCommands.SetComponent(spartanEntity, new Translation { Value = pos });
                    PostUpdateCommands.AddComponent(spartanEntity, new Rotation { Value = Quaternion.identity });
                    PostUpdateCommands.AddComponent(spartanEntity, new SpartanData { });
                    PostUpdateCommands.AddComponent(spartanEntity, new SpartanTag { });
                    PostUpdateCommands.AddComponent(spartanEntity, new AgentData
                    {
                        position = pos,
                        targetPosition = pos,
                        moveWeight = 1.5f,
                        seekWeight = 2f,
                        fleeWeight = 0.9f,
                        enemyFleeRelation = 2f,
                        flockWeight = 1f,
                        orientationSmooth = 0.5f
                    });
                    PostUpdateCommands.AddComponent(spartanEntity, new QuadrantTag { });
                }
            });

            //ENEMIES
            Entities.ForEach((Entity spawnerEntity, ref EnemySpawn enemySpawn, ref Translation translation) =>
            {
                for (int i = 0; i < _numEnemyEntities; i++)
                {
                    var enemyEntity = PostUpdateCommands.Instantiate(enemySpawn.Prefab);
                    Vector3 pos = new Vector3(UnityEngine.Random.Range(-5f, 5f), 0f, UnityEngine.Random.Range(-5f, 5f) + 10f);
                    PostUpdateCommands.SetComponent(enemyEntity, new Translation { Value = pos });
                    PostUpdateCommands.AddComponent(enemyEntity, new Rotation { Value = Quaternion.identity });
                    PostUpdateCommands.AddComponent(enemyEntity, new EnemyTag {});
                    PostUpdateCommands.AddComponent(enemyEntity, new AgentData
                    {
                        position = pos,
                        targetPosition = pos,
                        moveWeight = 1.5f,
                        seekWeight = 2f,
                        fleeWeight = 0.9f,
                        enemyFleeRelation = 2f,
                        flockWeight = 1f,
                        orientationSmooth = 0.5f
                    });
                    PostUpdateCommands.AddComponent(enemyEntity, new QuadrantTag { });
                }
            });
        }
    }

}
