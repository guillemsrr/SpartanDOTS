using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    [SerializeField] private int _numSpartans = 300;
    [SerializeField] private int _numRows = 10;

    public float _separation = 0.5f;
    public float _spartanSpeed = 1f;
    public float _spawnTime = 0.05f;
    [SerializeField] private GameObject _spartanCapsulePrefab;
    [SerializeField] private GameObject _spartanSpritePrefab;

    private Entity _spartanEntity;
    private EntityManager _manager;
    private BlobAssetStore _blobAssetStore;

    //testing sprite render
    public Mesh _quadMesh;
    public UnityEngine.Material _spriteMaterial;

    //NativeArray<Entity> entityArray = new NativeArray<Entity>(1000, Allocator.Temp);

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        _manager = World.DefaultGameObjectInjectionWorld.EntityManager;
        _blobAssetStore = new BlobAssetStore();

        GameObjectConversionSettings settings = GameObjectConversionSettings.FromWorld(World.DefaultGameObjectInjectionWorld, _blobAssetStore);
        _spartanEntity = GameObjectConversionUtility.ConvertGameObjectHierarchy(_spartanSpritePrefab, settings);

        SpriteSheetTest();
    }

    private void SpriteSheetTest()
    {
        for (int i = 0; i < _numSpartans; i++)
        {
            Entity entity = _manager.Instantiate(_spartanEntity);
            _manager.AddComponentData(entity,
                new SpriteAnimationData
                {
                    currentFrame = UnityEngine.Random.Range(0, 6),
                    minFrame = 0,
                    maxFrame = 6,
                    frameCount = 8,
                    frameTimer = 0f,
                    frameTimerMax = .2f,
                }
            );

            _manager.AddComponentData(entity,
                new MovementData
                {
                    speed = 1f,
                    direction = new float3(),
                    acceleration= new float3(),
                });

            _manager.AddComponentData(entity,
                new SpartanActionsData
                {

                });

            Vector3 pos = new Vector3(UnityEngine.Random.Range(-5f, 5f), 0f, UnityEngine.Random.Range(-5f, 5f));
            Quaternion rot = Quaternion.identity;
            _manager.SetComponentData(entity, new Translation { Value = pos });
            _manager.SetComponentData(entity, new Rotation { Value = rot });
        }
    }

    private void OnDestroy()
    {
        _blobAssetStore.Dispose();
    }
}
