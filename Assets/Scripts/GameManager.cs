using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;

namespace Core
{
    [RequireComponent(typeof(SpriteSheetInfo))]
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance;

        private SpriteSheetInfo _spriteSheetInfo;
        public SpriteSheetInfo SpriteSheetInfo { get => _spriteSheetInfo; }

        [SerializeField] private int _numSpartans = 300;
        [SerializeField] private int _numRows = 10;

        private float _separation = 1.3f;
        public float _spartanSpeed = 1f;
        public float _spawnTime = 0.05f;

        [SerializeField] private GameObject _spartanSpritePrefab;
        private Entity _spartanEntity;
        private EntityManager _manager;
        private BlobAssetStore _blobAssetStore;

        //testing sprite render
        public Mesh _quadMesh;
        public UnityEngine.Material _spriteMaterial;


        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;

            _spriteSheetInfo = GetComponent<SpriteSheetInfo>();

            _manager = World.DefaultGameObjectInjectionWorld.EntityManager;
            _blobAssetStore = new BlobAssetStore();

            GameObjectConversionSettings settings = GameObjectConversionSettings.FromWorld(World.DefaultGameObjectInjectionWorld, _blobAssetStore);
            _spartanEntity = GameObjectConversionUtility.ConvertGameObjectHierarchy(_spartanSpritePrefab, settings);

            SpawnSpartans();
        }

        private void SpawnSpartans()
        {
            for (int i = 0; i < _numSpartans/_numRows; i++)
            {
                for(int j = 0; j < _numRows; j++)
                {
                    Entity entity = _manager.Instantiate(_spartanEntity);
                    Vector3 pos = new Vector3(_separation * i, 0f, _separation * j);
                    Quaternion rot = Quaternion.identity;
                    _manager.SetComponentData(entity, new Translation { Value = pos });
                    _manager.SetComponentData(entity, new Rotation { Value = rot });
                }
            }
        }

        private void OnDestroy()
        {
            _blobAssetStore.Dispose();
        }
    }
}
