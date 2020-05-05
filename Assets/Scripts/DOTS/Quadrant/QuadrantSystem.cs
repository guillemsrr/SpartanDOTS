using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using Unity.Mathematics;
using Spartans.Obstacle;

namespace Spartans.Quadrant
{
    [AlwaysSynchronizeSystem]
    public class QuadrantSystem : SystemBase
    {
        private const int _yMultiplier = 1000;
        private const int _cellSize = 5;
        public static NativeMultiHashMap<int, AgentData> spartanQuadrantMultiHashMap;
        public static NativeMultiHashMap<int, AgentData> enemyQuadrantMultiHashMap;
        public static NativeMultiHashMap<int, ObstacleData> obstacleQuadrantMultiHashMap;
        public static NativeHashMap<int, float3> spartanMassCenterQuadrantHashMap;
        public static NativeHashMap<int, float3> spartanAlignmentQuadrantHashMap;
        public static NativeHashMap<int, float3> enemyMassCenterQuadrantHashMap;
        public static NativeHashMap<int, float3> enemyAlignmentQuadrantHashMap;

        EntityQuery _spartanQuery;
        EntityQuery _enemyQuery;
        EntityQuery _obstacleQuery;
        

        protected override void OnCreate()
        {
            _spartanQuery   = GetEntityQuery(typeof(SpartanTag));
            _enemyQuery     = GetEntityQuery(typeof(EnemyTag));
            _obstacleQuery  = GetEntityQuery(typeof(ObstacleData));

            spartanQuadrantMultiHashMap  = new NativeMultiHashMap<int, AgentData>(0, Allocator.Persistent);
            enemyQuadrantMultiHashMap    = new NativeMultiHashMap<int, AgentData>(0, Allocator.Persistent);
            obstacleQuadrantMultiHashMap = new NativeMultiHashMap<int, ObstacleData>(0, Allocator.Persistent);
            spartanMassCenterQuadrantHashMap = new NativeHashMap<int, float3>(0, Allocator.Persistent);
            spartanAlignmentQuadrantHashMap = new NativeHashMap<int, float3>(0, Allocator.Persistent);
            enemyMassCenterQuadrantHashMap = new NativeHashMap<int, float3>(0, Allocator.Persistent);
            enemyAlignmentQuadrantHashMap = new NativeHashMap<int, float3>(0, Allocator.Persistent);

            RequireForUpdate(_spartanQuery);
            RequireForUpdate(_enemyQuery);
            RequireForUpdate(_obstacleQuery);

            base.OnCreate();
        }

        protected override void OnDestroy()
        {
            spartanQuadrantMultiHashMap.Dispose();
            enemyQuadrantMultiHashMap.Dispose();
            obstacleQuadrantMultiHashMap.Dispose();
            spartanMassCenterQuadrantHashMap.Dispose();
            spartanAlignmentQuadrantHashMap.Dispose();
            enemyMassCenterQuadrantHashMap.Dispose();
            enemyAlignmentQuadrantHashMap.Dispose();

            base.OnDestroy();
        }

        protected override void OnUpdate()
        {
            spartanQuadrantMultiHashMap.Clear();
            enemyQuadrantMultiHashMap.Clear();
            obstacleQuadrantMultiHashMap.Clear();

            int queryLength = _spartanQuery.CalculateEntityCount();
            if (queryLength == 0)
            {
                return;
            }
            if (queryLength > spartanQuadrantMultiHashMap.Capacity)
            {
                spartanQuadrantMultiHashMap.Capacity = queryLength;
            }
            
            var spartansQuadrantJobHandle = Entities
                    .WithName("SpartansQuadrantJob")
                    .WithAll<SpartanTag>()
                    .ForEach((ref QuadrantTag quadrantTag, in AgentData agent) =>
                    {
                        int hashMapKey = GetPositionHashMapKey(agent.position);
                        quadrantTag.numQuadrant = hashMapKey;

                        spartanQuadrantMultiHashMap.Add(hashMapKey, agent);

                        if (!spartanMassCenterQuadrantHashMap.ContainsKey(hashMapKey))
                            spartanMassCenterQuadrantHashMap.Add(hashMapKey, agent.position);
                        else
                            spartanMassCenterQuadrantHashMap[hashMapKey] += agent.position;

                        if (!spartanAlignmentQuadrantHashMap.ContainsKey(hashMapKey))
                            spartanAlignmentQuadrantHashMap.Add(hashMapKey, agent.position);
                        else
                            spartanAlignmentQuadrantHashMap[hashMapKey] += agent.position;
                    })
                    .WithoutBurst()
                    .ScheduleParallel(Dependency);

            Dependency = spartansQuadrantJobHandle;

            queryLength = _enemyQuery.CalculateEntityCount();
            if (queryLength == 0)
                return;
            if (queryLength > enemyQuadrantMultiHashMap.Capacity)
            {
                enemyQuadrantMultiHashMap.Capacity = queryLength;
            }

            var enemyQuadrantJobHandle = Entities
                    .WithName("EnemyQuadrantJob")
                    .WithAll<EnemyTag>()
                    .ForEach((ref QuadrantTag quadrantTag, in AgentData agent) =>
                    {
                        int hashMapKey = GetPositionHashMapKey(agent.position);
                        quadrantTag.numQuadrant = hashMapKey;

                        enemyQuadrantMultiHashMap.Add(hashMapKey, agent);

                        if (!enemyMassCenterQuadrantHashMap.ContainsKey(hashMapKey))
                            enemyMassCenterQuadrantHashMap.Add(hashMapKey, agent.position);
                        else
                            enemyMassCenterQuadrantHashMap[hashMapKey] += agent.position;

                        if (!enemyAlignmentQuadrantHashMap.ContainsKey(hashMapKey))
                            enemyAlignmentQuadrantHashMap.Add(hashMapKey, agent.position);
                        else
                            enemyAlignmentQuadrantHashMap[hashMapKey] += agent.position;

                    })
                    .WithoutBurst()
                    .ScheduleParallel(Dependency);

            Dependency = enemyQuadrantJobHandle;

            queryLength = _obstacleQuery.CalculateEntityCount();
            if (queryLength == 0)
                return;
            if (queryLength > obstacleQuadrantMultiHashMap.Capacity)
            {
                obstacleQuadrantMultiHashMap.Capacity = queryLength;
            }

            var obstacleQuadrantJobHandle = Entities
                    .WithName("ObstacleQuadrantJob")
                    .ForEach((ref QuadrantTag quadrantTag, in ObstacleData obstacle) =>
                    {
                        int hashMapKey = GetPositionHashMapKey(obstacle.position);
                        quadrantTag.numQuadrant = hashMapKey;

                        obstacleQuadrantMultiHashMap.Add(hashMapKey, obstacle);
                    })
                    .WithoutBurst()
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
