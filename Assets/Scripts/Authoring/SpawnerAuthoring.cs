using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;
using Unity.Transforms;

namespace Spartans
{

    [RequiresEntityConversion]
    public class SpawnerAuthoring : MonoBehaviour, IDeclareReferencedPrefabs, IConvertGameObjectToEntity
    {
        public GameObject _prefab;

        // Referenced prefabs have to be declared so that the conversion system knows about them ahead of time
        public void DeclareReferencedPrefabs(List<GameObject> referencedPrefabs)
        {
            referencedPrefabs.Add(_prefab);
        }

        // Lets you convert the editor data representation to the entity optimal runtime representation
        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            var spawnerData = new SpartanSpawn
            {
                // The referenced prefab will be converted due to DeclareReferencedPrefabs.
                // So here we simply map the game object to an entity reference to that prefab.
                Prefab = conversionSystem.GetPrimaryEntity(_prefab),
            };
            dstManager.AddComponentData(entity, spawnerData);
        }
    }

    public struct SpartanSpawn : IComponentData
    {
        public Entity Prefab;
    }

    [UpdateInGroup(typeof(GameObjectConversionGroup))]
    public class AgentConversion : GameObjectConversionSystem
    {
        protected override void OnUpdate()
        {
            Entities.ForEach((SpawnerAuthoring spawnerAuthoring) =>
            {
                var entity = GetPrimaryEntity(spawnerAuthoring);

                DstEntityManager.AddSharedComponentData(entity, new AgentSettings
                {
                    mass = 0.2f,
                    maxSpeed = 1.95f,
                    maxForce = 1.75f,
                    separationWeight = 5f,
                    cohesionWeight = 1.6f,
                    alignmentWeight = 1f,
                    neighborRadius = 0.85f
                });

                // Remove default transform system components
                //DstEntityManager.RemoveComponent<Translation>(entity);
                //DstEntityManager.RemoveComponent<Rotation>(entity);
            });
        }
    }
}
