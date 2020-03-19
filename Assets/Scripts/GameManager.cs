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
        //spartanEntity = GameObjectConversionUtility.ConvertGameObjectHierarchy(spartanCapsulePrefab, settings);
        _spartanEntity = GameObjectConversionUtility.ConvertGameObjectHierarchy(_spartanSpritePrefab, settings);

        //StartCoroutine(StressTest());
        //SpawnSpartansCapsules();
        //entityArray.Dispose();
        SpriteSheetTest();
    }

    private IEnumerator StressTest()
    {
        Entity spartan;
        for (int i = 0; i<_numSpartans; i++)
        {
            spartan = _manager.Instantiate(_spartanEntity);

            //random pos
            Vector3 pos = new Vector3(UnityEngine.Random.Range(-5f, 5f), 0f, UnityEngine.Random.Range(-5f, 5f));
            Quaternion rot = Quaternion.identity;
            _manager.SetComponentData(spartan, new Translation { Value = pos });
            _manager.SetComponentData(spartan, new Rotation { Value = rot });

            //random velocity
            Vector3 dir = new Vector3(UnityEngine.Random.Range(-.5f, .5f), 0f, UnityEngine.Random.Range(-.5f, .5f)).normalized;
            Vector3 speed = dir * _spartanSpeed;

            PhysicsVelocity velocity = new PhysicsVelocity()
            {
                Linear = speed,
                Angular = float3.zero
            };

            _manager.AddComponentData(spartan, velocity);
            yield return new WaitForSeconds(_spawnTime);
        }
        //yield return null;
    }

    private void SpawnSpartansCapsules()
    {
        Entity spartan;
        float posX = 0f;
        float posZ = 0f;
        for (int i = 0; i < _numRows; i++)
        {
            posX += _separation;
            posZ = 0f;

            for(int j = 0; j< _numSpartans/_numRows; j++)
            {
                spartan = _manager.Instantiate(_spartanEntity);

                //random pos
                Vector3 pos = new Vector3(posX, 0f, posZ);
                posZ += _separation;
                Quaternion rot = Quaternion.identity;
                _manager.SetComponentData(spartan, new Translation { Value = pos });
                _manager.SetComponentData(spartan, new Rotation { Value = rot });

                //random velocity
                Vector3 dir = Vector3.forward;
                Vector3 speed = dir * _spartanSpeed;

                PhysicsVelocity velocity = new PhysicsVelocity()
                {
                    Linear = speed,
                    Angular = float3.zero
                };

                _manager.AddComponentData(spartan, velocity);
            }
        }
    }

    private void SpriteSheetTest()
    {
        Entity entity = _manager.Instantiate(_spartanEntity);
        _manager.AddComponentData(entity,
            new SpriteAnimationData
            {
                currentFrame = 0,
                minFrame = 0,
                maxFrame = 6,
                frameCount = 8,
                frameTimer = 0f,
                frameTimerMax = .2f,
            }
        );

        Vector3 pos = new Vector3(0f, 0f, 0f);
        Quaternion rot = Quaternion.identity;
        _manager.SetComponentData(entity, new Translation { Value = pos });
        _manager.SetComponentData(entity, new Rotation { Value = rot });
    }

    private void OnDestroy()
    {
        _blobAssetStore.Dispose();
    }
}
