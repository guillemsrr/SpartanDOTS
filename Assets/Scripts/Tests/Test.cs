using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Mathematics;

namespace Test
{
    public class Test : MonoBehaviour
    {
        [SerializeField] private GameObject _spartanPrefab;
        [SerializeField] private GameObject _massCenter;
        [SerializeField] private GameObject _tracePoints;
        [SerializeField] Material redMat;
        List<Agent> _agentsList;
        int _numCols = 5;
        float _rowSeparation = 1f;
        float _colSeparation = 1f;
        int _numSpartans = 50;

        private void Awake()
        {
            UnityEngine.Random.InitState(1);
            _agentsList = new List<Agent>();
            for(int i = 0; i < _numSpartans; i++)
            {
                Vector3 pos = new Vector3(UnityEngine.Random.Range(-5f, 5f), 0f, UnityEngine.Random.Range(-5f, 5f));
                GameObject newSpartan = Instantiate(_spartanPrefab, pos, Quaternion.identity, transform);
                Agent spartan = new Agent(newSpartan, newSpartan.transform.GetChild(0).GetComponent<MeshRenderer>());
                _agentsList.Add(spartan);
            }
        }



        void Start()
        {

        }

        private void Update()
        {
            Formation();
            GoToFormation();
            //GoToFormationDirectly();
            //enabled = false;
        }

        private void Formation()
        {
            float3 averageVelocity = float3.zero;
            float3 averagePosition = float3.zero;

            foreach(Agent agent in _agentsList)
            {
                averageVelocity += agent.velocity;
                averagePosition += agent.GetPosition();
            }

            int numSpartans = _agentsList.Count;

            //job.Complete();//provisional

            averageVelocity /= numSpartans;
            averagePosition /= numSpartans;
            Instantiate(_massCenter, averagePosition, Quaternion.identity);

            float3 alignmentDirection = math.normalizesafe(averageVelocity);
            //Debug.Log("alignmentDirection " + alignmentDirection);
            float3 alignmentPerpendicular = math.cross(alignmentDirection, new float3(0, 1, 0));
            alignmentPerpendicular = math.normalizesafe(alignmentPerpendicular);
            //Debug.Log("alignmentPerpendicular " + alignmentPerpendicular);

            //Get furthest spartan
            int numEntity = 0;
            int entityInQueryIndex = 0;
            float lengthsq = math.lengthsq(alignmentDirection);

            float distance = 0;

            foreach(Agent agent in _agentsList)
            {
                float3 direction = math.normalizesafe(agent.GetPosition() - averagePosition);
                //TraceLine(agent.GetPosition(), direction, 5, agent._gameObject.transform);
                //TraceLine(agent.GetPosition(), alignmentDirection, 5, agent._gameObject.transform);
                //angle:
                float angle = Mathf.Rad2Deg*math.acos(math.dot(direction, alignmentDirection) / math.lengthsq(direction) * lengthsq);
                //Debug.Log("angle " + angle);
                agent._gameObject.transform.name = "entity num " + entityInQueryIndex + " angle " + angle.ToString();

                //mirar quin angles em dóna
                if (angle < 45f)
                {
                    float newDistance = math.length(agent.GetPosition() - averagePosition);
                    agent._gameObject.transform.name += " distance " + newDistance.ToString();
                    if (newDistance > distance)
                    {
                        numEntity = entityInQueryIndex;
                        distance = newDistance;
                    }
                }

                entityInQueryIndex++;
            }

            //visually:
            _agentsList[numEntity].SetMaterial(redMat);
            //Debug.Log("numEntity " + numEntity);

            var formationPositions = new List<int2>();
            var targetPositions = new List<float3>();
            var positionsList = new List<float3>();
            var placedEntities = new List<int>();

            
            for (int i = 0; i < numSpartans; i++)
            {
                //Maybe I should add instead of equal?
                formationPositions.Add(new int2());
                targetPositions.Add(new float3());
                float3 pos = _agentsList[i].GetPosition();
                positionsList.Add(pos);//CopyFrom?
            }

            float3 leaderPosition = positionsList[numEntity];
            targetPositions[numEntity] = leaderPosition;
            formationPositions[numEntity] = new int2(0, 0);//the leader

            //CHANGE
            //placedEntities.Add(numEntity);

            TraceLine(leaderPosition, alignmentPerpendicular, 10, _agentsList[numEntity]._gameObject.transform);
            TraceLine(leaderPosition, -alignmentPerpendicular, 10, _agentsList[numEntity]._gameObject.transform);
            TraceLine(leaderPosition, -alignmentDirection, 20, _agentsList[numEntity]._gameObject.transform);

            //center and right side of leader
            for (int i = 0; i < (_numCols - 1) / 2 + 1; i++)
            {
                //CHANGE
                for (int j = 0; j < numSpartans/ _numCols; j++)
                {
                    //calculate target pos
                    float3 targetPos = leaderPosition + alignmentPerpendicular * i * _colSeparation - alignmentDirection * j * _rowSeparation;//CHANGE
                    Instantiate(_massCenter, targetPos, Quaternion.identity);
                    //Debug.Log(" RIGHT -> i, j (" + i + " " + j + ") targetPos " + targetPos);
                    distance = 100f;
                    //get the closest agent
                    for (int z = 0; z < numSpartans; z++)
                    {
                        if (!placedEntities.Contains(z))
                        {
                            float newDistance = math.length(positionsList[z] - targetPos);
                            if (newDistance < distance)
                            {
                                numEntity = z;
                                distance = newDistance;
                            }
                        }
                    }

                    placedEntities.Add(numEntity);
                    Debug.LogError("I put numEntity " + numEntity);
                    foreach(int xs in placedEntities)
                    {
                        Debug.Log("placedEntities has " + xs);
                    }

                    //set to array
                    targetPositions[numEntity] = targetPos;
                    Debug.Log("new target RIGHT numEntity " + numEntity + " is " + targetPos);
                    formationPositions[numEntity] = new int2(i, j);
                }
            }

            Debug.LogError("placed Entities count: " + placedEntities.Count);

            //left side of leader
            for (int i = -1; i > -((_numCols - 1)/ 2 + 1); i--)// ---- REVISAR
            {
                for (int j = 0; j < numSpartans/_numCols; j++)
                {
                    //calculate target pos
                    float3 targetPos = leaderPosition + alignmentPerpendicular * i * _colSeparation - alignmentDirection * j * _rowSeparation;//CHANGE
                    //Debug.Log(" LEFT -> i, j (" + i + " " + j + ") targetPos " + targetPos);

                    distance = 100f;
                    //get the closest agent
                    for (int z = 0; z < numSpartans; z++)
                    {
                        if (!placedEntities.Contains(z))
                        {
                            float newDistance = math.length(positionsList[z] - targetPos);
                            if (newDistance < distance)
                            {
                                numEntity = z;
                                distance = newDistance;
                            }
                        }
                    }

                    placedEntities.Add(numEntity);
                    Debug.LogError("I put numEntity " + numEntity);
                    foreach (int xs in placedEntities)
                    {
                        Debug.Log("placedEntities has " + xs);
                    }

                    //set to array
                    targetPositions[numEntity] = targetPos;
                    Debug.Log("new target LEFT numEntity " + numEntity + " is " + targetPos);
                    formationPositions[numEntity] = new int2(i, j);
                }
            }

            entityInQueryIndex = 0;
            foreach (Agent agent in _agentsList)
            {
                agent.targetPosition = targetPositions[entityInQueryIndex];
                agent.formationPosition = formationPositions[entityInQueryIndex];
                Debug.Log("target final of num " + entityInQueryIndex + " is " + targetPositions[entityInQueryIndex]);
                entityInQueryIndex++;
            }
        }

        private void GoToFormation()
        {
            foreach(Agent agent in _agentsList)
            {
                float3 movingForce = agent.direction * agent.maxForce;
                float3 seekingForce = agent.Seek();

                float3 steeringForce = movingForce + seekingForce;
                float3 acceleration = steeringForce / 0.2f;

                agent.velocity += acceleration * Time.deltaTime;
                float speed = math.length(agent.velocity);
                if (speed > agent.maxSpeed)
                {
                    agent.velocity = math.normalizesafe(agent.velocity);
                    agent.velocity *= agent.maxSpeed;
                }

                agent._gameObject.transform.position += (Vector3)agent.velocity * Time.deltaTime;
            } 
        }

        private void GoToFormationDirectly()
        {
            foreach (Agent agent in _agentsList)
            {
                agent._gameObject.transform.position = agent.targetPosition;
                agent._gameObject.transform.name = "agent " + agent.formationPosition.ToString();
            }
        }

        private void TraceLine(float3 from, float3 direction, int numTraces, Transform parent)
        {
            float3 pos = from;
            for(int i = 0; i< numTraces; i++)
            {
                Instantiate(_tracePoints, pos, Quaternion.identity, parent);
                pos += direction * _rowSeparation;
            }
        }

    }

    public class Agent: MonoBehaviour
    {
        public GameObject _gameObject;
        private MeshRenderer _meshRenderer;
        //steering
        public float3 direction;
        public float3 velocity;

        //formation:
        public float3 targetPosition;
        public int2 formationPosition;

        public float maxForce = 1f;
        public float maxSpeed = 1f;

        public Agent(GameObject gameObject, MeshRenderer _mesh)
        {
            _gameObject = gameObject;
            _meshRenderer = _mesh;
            direction = new float3(0, 0, 1);
            velocity = new float3(0, 0, 1);
        }

        public float3 Seek()
        {
            float3 desiredVelocity = math.normalizesafe(targetPosition - GetPosition());
            desiredVelocity *= maxSpeed;

            float3 steeringForce = (desiredVelocity - velocity);
            steeringForce /= maxSpeed;
            steeringForce *= maxForce;
            return steeringForce;
        }

        public float3 GetPosition()
        {
            return _gameObject.transform.position;
        }

        public void SetMaterial(Material mat)
        {
            _meshRenderer.material = mat;
        }
    }
}
