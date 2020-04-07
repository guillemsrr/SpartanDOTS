using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Core
{
    [GenerateAuthoringComponent]
    public struct UnitFormationData : IComponentData
    {
        public int2 position;
    }
}
