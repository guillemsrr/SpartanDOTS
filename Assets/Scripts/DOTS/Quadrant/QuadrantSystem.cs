using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using Unity.Mathematics;
using Spartans.Obstacle;
using UnityEngine;

namespace Spartans.Quadrant
{
    [DisableAutoCreation]
    public class QuadrantSystem : SystemBase
    {
        private const int _yMultiplier = 1000;
        private const int _cellSize = 10;

        private EntityQuery _spartanQuery;
        private EntityQuery _enemyQuery;
        private EntityQuery _obstacleQuery;

        public static NativeMultiHashMap<int, AgentData> spartanQuadrantMultiHashMap;
        public static NativeMultiHashMap<int, AgentData> enemyQuadrantMultiHashMap;
        public static NativeMultiHashMap<int, ObstacleData> obstacleQuadrantMultiHashMap;
        public static NativeHashMap<int, float3> spartanMassCenterQuadrantHashMap;
        public static NativeHashMap<int, float3> spartanAlignmentQuadrantHashMap;
        public static NativeHashMap<int, float3> enemyMassCenterQuadrantHashMap;
        public static NativeHashMap<int, float3> enemyAlignmentQuadrantHashMap;

        public int CellSize => _cellSize;

        protected override void OnCreate()
        {
            base.OnCreate();

            _spartanQuery = GetEntityQuery(ComponentType.ReadOnly<SpartanTag>());
            _enemyQuery = GetEntityQuery(ComponentType.ReadOnly<EnemyTag>());
            _obstacleQuery = GetEntityQuery(typeof(ObstacleData));

            spartanQuadrantMultiHashMap = new NativeMultiHashMap<int, AgentData>(0, Allocator.Persistent);
            enemyQuadrantMultiHashMap = new NativeMultiHashMap<int, AgentData>(0, Allocator.Persistent);
            obstacleQuadrantMultiHashMap = new NativeMultiHashMap<int, ObstacleData>(0, Allocator.Persistent);
            spartanMassCenterQuadrantHashMap = new NativeHashMap<int, float3>(0, Allocator.Persistent);
            spartanAlignmentQuadrantHashMap = new NativeHashMap<int, float3>(0, Allocator.Persistent);
            enemyMassCenterQuadrantHashMap = new NativeHashMap<int, float3>(0, Allocator.Persistent);
            enemyAlignmentQuadrantHashMap = new NativeHashMap<int, float3>(0, Allocator.Persistent);

            //RequireForUpdate(_spartanQuery);
            //RequireForUpdate(_enemyQuery);
            //RequireForUpdate(_obstacleQuery);
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
            spartanMassCenterQuadrantHashMap.Clear();
            spartanAlignmentQuadrantHashMap.Clear();
            enemyMassCenterQuadrantHashMap.Clear();
            enemyAlignmentQuadrantHashMap.Clear();

            int queryLength = _spartanQuery.CalculateEntityCount();
            if (queryLength == 0)
            {
                return;
            }
            if (queryLength > spartanQuadrantMultiHashMap.Capacity)
            {
                spartanQuadrantMultiHashMap.Capacity = queryLength;
                spartanMassCenterQuadrantHashMap.Capacity = queryLength;
                spartanAlignmentQuadrantHashMap.Capacity = queryLength;
            }

            var spartanParallelWriter = spartanQuadrantMultiHashMap.AsParallelWriter();
            var enemyParallelWriter = enemyQuadrantMultiHashMap.AsParallelWriter();
            var obstacleParallelWriter = obstacleQuadrantMultiHashMap.AsParallelWriter();

            
            var spartanMassCenterParallelWriter = spartanMassCenterQuadrantHashMap.AsParallelWriter();
            var spartanAlignmentParallelWriter = spartanAlignmentQuadrantHashMap.AsParallelWriter();
            

            //var spartansQuadrantJobHandle = Entities
                Entities
                    .WithName("SpartansQuadrantJob")
                    .WithAll<SpartanTag>()
                    .WithStructuralChanges()
                    //.WithStoreEntityQueryInField(ref _spartanQuery)
                    .ForEach((ref QuadrantTag quadrantTag, in AgentData agent) =>
                    {
                        int hashMapKey = GetPositionHashMapKey(agent.position);
                        quadrantTag.numQuadrant = hashMapKey;

                        spartanParallelWriter.Add(hashMapKey, agent);

                        if (!spartanMassCenterQuadrantHashMap.ContainsKey(hashMapKey))
                            spartanMassCenterParallelWriter.TryAdd(hashMapKey, agent.position);
                        else
                            spartanMassCenterQuadrantHashMap[hashMapKey] += agent.position;

                        if (!spartanAlignmentQuadrantHashMap.ContainsKey(hashMapKey))
                            spartanAlignmentParallelWriter.TryAdd(hashMapKey, agent.velocity);
                        else
                            spartanAlignmentQuadrantHashMap[hashMapKey] += agent.velocity;
                    })
                    .Run();

            //Dependency = spartansQuadrantJobHandle;

            queryLength = _enemyQuery.CalculateEntityCount();
            if (queryLength == 0)
                return;
            if (queryLength > enemyQuadrantMultiHashMap.Capacity)
            {
                enemyQuadrantMultiHashMap.Capacity = queryLength;
                enemyMassCenterQuadrantHashMap.Capacity = queryLength;
                enemyAlignmentQuadrantHashMap.Capacity = queryLength;
            }

            var enemyMassCenterParallelWriter = enemyMassCenterQuadrantHashMap.AsParallelWriter();
            var enemyAlignmentParallelWriter = enemyAlignmentQuadrantHashMap.AsParallelWriter();

            Entities
                    .WithName("EnemyQuadrantJob")
                    .WithAll<EnemyTag>()
                    //.WithStoreEntityQueryInField(ref _enemyQuery)
                    .ForEach((ref QuadrantTag quadrantTag, in AgentData agent) =>
                    {
                        int hashMapKey = GetPositionHashMapKey(agent.position);
                        quadrantTag.numQuadrant = hashMapKey;

                        enemyParallelWriter.Add(hashMapKey, agent);

                        if (!enemyMassCenterQuadrantHashMap.ContainsKey(hashMapKey))
                            enemyMassCenterParallelWriter.TryAdd(hashMapKey, agent.position);
                        else
                            enemyMassCenterQuadrantHashMap[hashMapKey] += agent.position;

                        if (!enemyAlignmentQuadrantHashMap.ContainsKey(hashMapKey))
                            enemyAlignmentParallelWriter.TryAdd(hashMapKey, agent.position);
                        else
                            enemyAlignmentQuadrantHashMap[hashMapKey] += agent.position;

                    })
                    .Run();

            //Dependency = enemyQuadrantJobHandle;

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

                        obstacleParallelWriter.Add(hashMapKey, obstacle);
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
