using Unity.Collections;
using Unity.Entities;
using Spartans.Obstacle;
using Unity.Mathematics;

namespace Spartans.Quadrant
{
    [GenerateAuthoringComponent]
    public struct QuadrantTag: IComponentData
    {
        public int numQuadrant;
    }

}
