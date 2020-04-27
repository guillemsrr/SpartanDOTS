using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;
using Unity.Transforms;

namespace Spartans
{

    [RequiresEntityConversion]
    public class SpawnerAuthoringEnemy : MonoBehaviour, IDeclareReferencedPrefabs, IConvertGameObjectToEntity
    {
        public GameObject _enemyPrefab;

        public void DeclareReferencedPrefabs(List<GameObject> referencedPrefabs)
        {
            referencedPrefabs.Add(_enemyPrefab);
        }

        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            var spawnerData = new EnemySpawn
            {
                Prefab = conversionSystem.GetPrimaryEntity(_enemyPrefab),
            };
            dstManager.AddComponentData(entity, spawnerData);
        }
    }

    public struct EnemySpawn : IComponentData
    {
        public Entity Prefab;
    }
}
