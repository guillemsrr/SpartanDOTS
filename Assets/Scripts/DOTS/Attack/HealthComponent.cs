using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

namespace Spartans.Attack
{
    public struct HealthComponent : IComponentData
    {
        public float health;
    }
}
