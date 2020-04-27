using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using Unity.Mathematics;
using Spartans.Obstacle;

namespace Spartans.Quadrant
{
    public class QuadrantSystem : SystemBase
    {
        private const int _yMultiplier = 1000;
        private const int _cellSize = 5;
        public static NativeHashMap<int, QuadrantAgentData> spartanQuadrantHashMap;
        public static NativeHashMap<int, QuadrantAgentData> enemyQuadrantHashMap;
        public static NativeHashMap<int, QuadrantObstacleData> obstacleQuadrantHashMap;

        EntityQuery _spartanQuery;
        EntityQuery _enemyQuery;
        EntityQuery _obstacleQuery;
        

        protected override void OnCreate()
        {
            _spartanQuery   = GetEntityQuery(typeof(SpartanTag));
            _enemyQuery     = GetEntityQuery(typeof(EnemyTag));
            _obstacleQuery  = GetEntityQuery(typeof(ObstacleData));

            spartanQuadrantHashMap  = new NativeHashMap<int, QuadrantAgentData>(0, Allocator.Persistent);
            enemyQuadrantHashMap    = new NativeHashMap<int, QuadrantAgentData>(0, Allocator.Persistent);
            obstacleQuadrantHashMap = new NativeHashMap<int, QuadrantObstacleData>(0, Allocator.Persistent);

            RequireForUpdate(_spartanQuery);
            RequireForUpdate(_enemyQuery);
            RequireForUpdate(_obstacleQuery);

            base.OnCreate();
        }

        protected override void OnDestroy()
        {
            spartanQuadrantHashMap.Dispose();
            enemyQuadrantHashMap.Dispose();
            obstacleQuadrantHashMap.Dispose();

            base.OnDestroy();
        }

        protected override void OnUpdate()
        {
            spartanQuadrantHashMap.Clear();
            enemyQuadrantHashMap.Clear();
            obstacleQuadrantHashMap.Clear();

            int queryLength = _spartanQuery.CalculateEntityCount();
            if (queryLength > spartanQuadrantHashMap.Capacity)
            {
                spartanQuadrantHashMap.Capacity = queryLength;
            }
            queryLength = _enemyQuery.CalculateEntityCount();
            if (queryLength > enemyQuadrantHashMap.Capacity)
            {
                enemyQuadrantHashMap.Capacity = queryLength;
            }
            queryLength = _obstacleQuery.CalculateEntityCount();
            if (queryLength > obstacleQuadrantHashMap.Capacity)
            {
                obstacleQuadrantHashMap.Capacity = queryLength;
            }

            var spartansQuadrantJobHandle = Entities
                    .WithName("SpartansQuadrantJob")
                    .WithAll<SpartanTag>()
                    .ForEach((ref QuadrantTag quadrantTag, in AgentData agent) =>
                    {
                        QuadrantAgentData quadrantData;
                        int hashMapKey = GetPositionHashMapKey(agent.position);
                        quadrantTag.numQuadrant = hashMapKey;

                        if (!spartanQuadrantHashMap.TryGetValue(hashMapKey, out quadrantData))
                        {
                            spartanQuadrantHashMap.Add(hashMapKey, new QuadrantAgentData
                            {
                                numAgents = 0,
                                //agentsData = new NativeList<AgentData>(),
                                quadrantMassCenter = float3.zero,
                                quadrantAlignment = float3.zero
                            });
                        }

                        //spartanQuadrantHashMap[hashMapKey].agentsData.Add(agent);
                        //spartanQuadrantHashMap[hashMapKey].AddMassCenter(agent.position);
                        //spartanQuadrantHashMap[hashMapKey].AddAlignment(agent.velocity);
                    })
                    .ScheduleParallel(Dependency);

            Dependency = spartansQuadrantJobHandle;

            var enemyQuadrantJobHandle = Entities
                    .WithName("EnemyQuadrantJob")
                    .WithAll<EnemyTag>()
                    .ForEach((ref QuadrantTag quadrantTag, in AgentData agent) =>
                    {
                        QuadrantAgentData quadrantData;
                        int hashMapKey = GetPositionHashMapKey(agent.position);
                        quadrantTag.numQuadrant = hashMapKey;

                        if (!enemyQuadrantHashMap.TryGetValue(hashMapKey, out quadrantData))
                        {
                            enemyQuadrantHashMap.Add(hashMapKey, new QuadrantAgentData
                            {
                                numAgents = 0,
                                //agentsData = new NativeList<AgentData>(),
                                quadrantMassCenter = float3.zero,
                                quadrantAlignment = float3.zero
                            });
                        }

                        //enemyQuadrantHashMap[hashMapKey].agentsData.Add(agent);
                        //enemyQuadrantHashMap[hashMapKey].AddMassCenter(agent.position);
                        //enemyQuadrantHashMap[hashMapKey].AddAlignment(agent.velocity);

                    })
                    .ScheduleParallel(Dependency);

            Dependency = enemyQuadrantJobHandle;

            var obstacleQuadrantJobHandle = Entities
                    .WithName("ObstacleQuadrantJob")
                    .ForEach((ref QuadrantTag quadrantTag, in ObstacleData obstacle) =>
                    {
                        QuadrantObstacleData quadrantData;
                        int hashMapKey = GetPositionHashMapKey(obstacle.position);
                        quadrantTag.numQuadrant = hashMapKey;

                        if (!obstacleQuadrantHashMap.TryGetValue(hashMapKey, out quadrantData))
                        {
                            obstacleQuadrantHashMap.Add(hashMapKey, new QuadrantObstacleData
                            {
                                numObstacles = 0,
                                //obstaclesData = new NativeList<ObstacleData>()
                            });
                        }

                        //obstacleQuadrantHashMap[hashMapKey].obstaclesData.Add(obstacle);
                    })
                    .ScheduleParallel(Dependency);

            Dependency = obstacleQuadrantJobHandle;


            _spartanQuery.AddDependency(Dependency);
            _enemyQuery.AddDependency(Dependency);
            _obstacleQuery.AddDependency(Dependency);

            _spartanQuery.ResetFilter();
            _enemyQuery.ResetFilter();
            _obstacleQuery.ResetFilter();
        }

        private static int GetPositionHashMapKey(float3 position)
        {
            int hash;
            //hash = (int)math.hash(new int3(math.floor(position / _cellSize)));
            hash = (int)(math.floor(position.x / _cellSize) + (_yMultiplier * math.floor(position.z / _cellSize)));
            return hash;
        }

    }

}
