using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using System.ComponentModel;

namespace Spartans
{
    public struct SpartanTag : IComponentData { }

    public struct SpartanData : IComponentData
    {
        //formation:
        public int2 formationPosition;
    }

}
