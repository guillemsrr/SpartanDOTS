using Unity.Entities;
using UnityEngine;
using UnityEngine.UI;

namespace Spartans.Quadrant
{
    // NOTE: Updating a manually-created system in FixedUpdate() as demonstrated below
    // is intended as a short-term workaround; the entire `SimulationSystemGroup` will
    // eventually use a fixed timestep by default.
    public class QuadrantSystemController : MonoBehaviour
    {
        private QuadrantSystem _quadrantSystem;
        private float _fixedDeltaTime = 0.1f;

        void Start()
        {
            _quadrantSystem = World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<QuadrantSystem>();
        }

        void FixedUpdate()
        {
            Time.fixedDeltaTime = _fixedDeltaTime;
            _quadrantSystem.Update();
        }

        private void OnDrawGizmos()
        {
            if (_quadrantSystem == null) return;

            Gizmos.color = Color.black;
            Vector3 size = new Vector3(_quadrantSystem.CellSize, _quadrantSystem.CellSize, _quadrantSystem.CellSize);
            Vector3 center;
            Vector3 initialCenter = center = new Vector3(-_quadrantSystem.CellSize * 5, 0, -_quadrantSystem.CellSize * 5);
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
    }
}
