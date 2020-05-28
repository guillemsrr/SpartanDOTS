using Unity.Entities;
using Unity.Transforms;
using UnityEngine;
using Spartans.Quadrant;

namespace Spartans
{
    [DisableAutoCreation]
    public class SpawnerSystem : EntityCommandBufferSystem
    {
        private float _numSpartanEntities = 100;
        private float _numEnemyEntities = 50;
        private float _maxSeparation = 10f;

        public float NumEntities { set { _numSpartanEntities = value; } }
        protected override void OnCreate()
        {
            
        }

        protected override void OnUpdate()
        {
            //SPARTANS
            Entities.ForEach((ref SpartanSpawn spartanSpawn) =>
            {
                for (int i = 0; i< _numSpartanEntities; i++)
                {
                    var spartanEntity = PostUpdateCommands.Instantiate(spartanSpawn.Prefab);
                    Vector3 pos = new Vector3(UnityEngine.Random.Range(-_maxSeparation, _maxSeparation), 0f, UnityEngine.Random.Range(-_maxSeparation, _maxSeparation) - _maxSeparation);
                    PostUpdateCommands.SetComponent(spartanEntity, new Translation { Value = pos });
                    PostUpdateCommands.AddComponent(spartanEntity, new Rotation { Value = Quaternion.identity });
                    PostUpdateCommands.AddComponent(spartanEntity, new SpartanData { });
                    PostUpdateCommands.AddComponent(spartanEntity, new SpartanTag { });
                    PostUpdateCommands.AddComponent(spartanEntity, new AgentData
                    {
                        position = pos,
                        targetPosition = pos,
                        frictionWeight = 1.75f / 2f, //half maxForce
                        moveWeight = 1.5f,
                        seekWeight = 1f,
                        fleeWeight = 0.5f,
                        flockWeight = 0.5f,
                        enemyFleeRelation = 2f,
                        orientationSmooth = 0.5f
                    });
                    PostUpdateCommands.AddComponent(spartanEntity, new QuadrantTag { });
                }
            });

            //ENEMIES
            Entities.ForEach((ref EnemySpawn enemySpawn) =>
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
                        seekWeight = 1f,
                        fleeWeight = 0.5f,
                        enemyFleeRelation = 2f,
                        flockWeight = 0.5f,
                        orientationSmooth = 0.5f
                    });
                    PostUpdateCommands.AddComponent(enemyEntity, new QuadrantTag { });
                }
            });
        }
    }

}
