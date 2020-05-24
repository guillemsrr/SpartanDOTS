using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Mathematics;


namespace OOP.Test
{
    public class Test : MonoBehaviour
    {
        [SerializeField] private GameObject _spartanPrefab;
        [SerializeField] private GameObject _pointPrefab;
        public List<AgentTest> _agentsList = new List<AgentTest>();
        int _numSpartans = 50;
        QuadrantSystemTest _quadrantSystem;
        SteeringSystemTest _steeringSystem;
        FormationSystemTest _formationSystem;
        public Camera _camera;
        public static Settings settings;
        public static Test instance;

        private void Awake()
        {
            instance = this;

            _quadrantSystem = new QuadrantSystemTest();
            _formationSystem = new FormationSystemTest();
            _steeringSystem = new SteeringSystemTest();
        }


        void Start()
        {
            Time.timeScale = 3f;
            _camera = Camera.main;
            UnityEngine.Random.InitState(1);
            for (int i = 0; i < _numSpartans; i++)
            {
                Vector3 pos = new Vector3(UnityEngine.Random.Range(-5f, 5f), 0f, UnityEngine.Random.Range(-5f, 5f));
                GameObject newSpartan = Instantiate(_spartanPrefab, pos, Quaternion.identity, transform);
                newSpartan.AddComponent<AgentTest>();
                AgentTest agent = newSpartan.GetComponent<AgentTest>();
                agent.Init(newSpartan.transform.position);
                _agentsList.Add(agent);
            }

            //HashKeyTest();
            //_quadrantSystem.AllSpartansInOneCell(_agentsList);
        }

        private void FixedUpdate()
        {
            _quadrantSystem.UpdateQuadrant(_agentsList);
            _formationSystem.UpdateFormation(_agentsList);
            _steeringSystem.UpdateSteering(_agentsList);
        }

        public void OnMove(InputValue inputValue)
        {
            float2 vector2 = new float2(inputValue.Get<Vector2>());
            float3 moveInput = vector2.x * _camera.transform.right + vector2.y * Vector3.Scale(_camera.transform.forward, new Vector3(1, 0, 1)).normalized;

            foreach(AgentTest agent in _agentsList)
            {
                agent.direction = moveInput;
            }
        }

        public void OnStopMove(InputValue inputValue)
        {
            foreach (AgentTest agent in _agentsList)
            {
                agent.direction = float3.zero;
            }
        }

        private void OnDrawGizmos()
        {
            if (_quadrantSystem == null) return;

            Gizmos.color = Color.black;
            Vector3 size = new Vector3(_quadrantSystem.CellSize, _quadrantSystem.CellSize, _quadrantSystem.CellSize);
            Vector3 center;
            Vector3 initialCenter = center = new Vector3(-_quadrantSystem.CellSize * 5, 0, -_quadrantSystem.CellSize*5);
            for (int i = 0; i < 10; i++)
            {
                for (int j = 0; j < 10; j++)
                {
                    Gizmos.DrawWireCube(center, size);
                    center += new Vector3(0, 0, _quadrantSystem.CellSize);
                }

                center = new Vector3(center.x + _quadrantSystem.CellSize, 0, initialCenter.z);
            }
        }
        private void HashKeyTest()
        {
            Vector3 center;
            Vector3 initialCenter = center = new Vector3(-_quadrantSystem.CellSize * 5, 0, -_quadrantSystem.CellSize * 5);
            for (int i = 0; i < 10; i++)
            {
                for (int j = 0; j < 10; j++)
                {
                    center += new Vector3(0, 0, _quadrantSystem.CellSize);
                    GameObject t = Instantiate(_pointPrefab, center, Quaternion.identity, transform);
                    t.transform.name = GetPositionHashMapKey(center).ToString();
                }

                center = new Vector3(center.x + _quadrantSystem.CellSize, 0, initialCenter.z);
            }
        }

        private int GetPositionHashMapKey(float3 position)
        {
            int hash;
            hash = (int)(math.floor(position.x / _quadrantSystem.CellSize) + (_quadrantSystem.YMultiplier * math.floor(position.z / _quadrantSystem.CellSize)));
            return hash;
        }

        public struct Settings
        {
            public const float mass = 0.2f;
            public const float maxSpeed = 1.95f;
            public const float maxForce = 1.75f;
            public const float separationWeight = 5f;
            public const float cohesionWeight = 1.6f;
            public const float alignmentWeight = 1f;
            public const float neighborRadius = 0.85f;
            public const float CellRadius = 2f;
        }
    }


    
}
