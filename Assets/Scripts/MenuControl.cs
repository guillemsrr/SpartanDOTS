using Unity.Entities;
using UnityEngine;
using UnityEngine.UI;

namespace Spartans
{
    public class MenuControl : MonoBehaviour
    {
        SpawnerSystem _spawnerSystem;

        [SerializeField] Button _spawnButton;
        [SerializeField] Slider _spawnSlider;
        [SerializeField] Text _numSpawnText;

        [SerializeField] Slider _columnsSlider;
        [SerializeField] Text _numColumnsText;

        [SerializeField] Slider _timeSpeedSlider;
        [SerializeField] Text _timeSpeedText;

        private void Awake()
        {
            _spawnButton.onClick.AddListener(SpawnSpartans);
            _spawnSlider.onValueChanged.AddListener(SetSpawnNumber);
            
            _timeSpeedSlider.onValueChanged.AddListener(SetTimeSpeed);

            _columnsSlider.onValueChanged.AddListener(SetColumnsNumber);

            if (_spawnerSystem == null)
            {
                _spawnerSystem = World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<SpawnerSystem>();
            }
        }

        private void Start()
        {
            _spawnerSystem.NumEntities = _spawnSlider.value;
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

        private void SetTimeSpeed(float speed)
        {
            _timeSpeedText.text = ((float)((int)(speed*10)/10f)).ToString();
            Environment.TimeSpeed = speed;
        }

        private void SetColumnsNumber(float num)
        {
            _numColumnsText.text = num.ToString();
            Environment.NumberColumns = (int)num;
        }

        public void CheckColumnsNumber()
        {
            if (Environment.NumberColumns != _columnsSlider.value)
            {
                _columnsSlider.value = Environment.NumberColumns;
                _numColumnsText.text = Environment.NumberColumns.ToString();
            }
        }
    }
}
