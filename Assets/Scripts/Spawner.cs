using Unity.Entities;
using UnityEngine;
using UnityEngine.UI;

namespace Spartans
{
    public class Spawner : MonoBehaviour
    {
        SpawnerSystem _spawnerSystem;

        [SerializeField] Button _spawnButton;
        [SerializeField] Slider _slider;
        [SerializeField] Text _numSpawnText;

        private void Awake()
        {
            _spawnButton.onClick.AddListener(SpawnSpartans);
            _slider.onValueChanged.AddListener(SetSpawnNumber);
            if (_spawnerSystem == null)
            {
                _spawnerSystem = World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<SpawnerSystem>();
            }
        }

        private void Start()
        {
            _spawnerSystem.NumEntities = _slider.value;
            SpawnSpartans();
        }

        private void SpawnSpartans()
        {
            _spawnerSystem.Update();
        }

        private void SetSpawnNumber(float num)
        {
            _numSpawnText.text = num.ToString();
            _spawnerSystem.NumEntities = num;
        }
    }
}
