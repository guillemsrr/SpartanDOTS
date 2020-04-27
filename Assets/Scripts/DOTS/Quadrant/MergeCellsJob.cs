using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Spartans.Quadrant
{
    [BurstCompile]
    struct MergeCells : IJobNativeMultiHashMapMergedSharedKeyIndices
    {
        public NativeArray<int> cellIndices;
        public NativeArray<float3> cellAlignment;
        public NativeArray<float3> cellSeparation;
        public NativeArray<int> cellObstaclePositionIndex;
        public NativeArray<float> cellObstacleDistance;
        public NativeArray<int> cellTargetPositionIndex;
        public NativeArray<int> cellCount;
        [ReadOnly] public NativeArray<float3> targetPositions;
        [ReadOnly] public NativeArray<float3> obstaclePositions;

        void NearestPosition(NativeArray<float3> targets, float3 position, out int nearestPositionIndex, out float nearestDistance)
        {
            nearestPositionIndex = 0;
            nearestDistance = math.lengthsq(position - targets[0]);
            for (int i = 1; i < targets.Length; i++)
            {
                var targetPosition = targets[i];
                var distance = math.lengthsq(position - targetPosition);
                var nearest = distance < nearestDistance;

                nearestDistance = math.select(nearestDistance, distance, nearest);
                nearestPositionIndex = math.select(nearestPositionIndex, i, nearest);
            }
            nearestDistance = math.sqrt(nearestDistance);
        }

        public void ExecuteFirst(int index)
        {
            var position = cellSeparation[index] / cellCount[index];

            int obstaclePositionIndex;
            float obstacleDistance;
            NearestPosition(obstaclePositions, position, out obstaclePositionIndex, out obstacleDistance);
            cellObstaclePositionIndex[index] = obstaclePositionIndex;
            cellObstacleDistance[index] = obstacleDistance;

            int targetPositionIndex;
            float targetDistance;
            NearestPosition(targetPositions, position, out targetPositionIndex, out targetDistance);
            cellTargetPositionIndex[index] = targetPositionIndex;

            cellIndices[index] = index;
        }

        public void ExecuteNext(int cellIndex, int index)
        {
            cellCount[cellIndex] += 1;
            cellAlignment[cellIndex] = cellAlignment[cellIndex] + cellAlignment[index];
            cellSeparation[cellIndex] = cellSeparation[cellIndex] + cellSeparation[index];
            cellIndices[index] = cellIndex;
        }
    }
}
