using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Doofus.Config;
using Doofus.Core;

namespace Doofus.Pulpits
{
    // Owns pulpit spawning: at most 2 pulpits alive at once, a new one every
    // pulpit_spawn_time seconds (measured from the previous spawn), placed adjacent to
    // the previously spawned pulpit with a randomized lifetime from Doofus's Diary.
    public class PulpitSpawner : MonoBehaviour
    {
        [SerializeField] private Pulpit pulpitPrefab;
        [SerializeField] private float pulpitSize = 9f;
        [SerializeField] private float pulpitHeight = 0f;
        private const int MaxActivePulpits = 2;

        private readonly List<Pulpit> _activePulpits = new List<Pulpit>();
        private Vector2Int _lastGridPosition;
        private Coroutine _spawnLoop;
        private GameConfig _config;
        private bool _running;

        public Pulpit FirstPulpit { get; private set; }

        private void OnEnable()
        {
            GameEvents.OnGameStart += HandleGameStart;
            GameEvents.OnGameReset += HandleGameReset;
        }

        private void OnDisable()
        {
            GameEvents.OnGameStart -= HandleGameStart;
            GameEvents.OnGameReset -= HandleGameReset;
        }

        public Vector3 GetOriginWorldPosition()
        {
            return PulpitGrid.ToWorldPosition(Vector2Int.zero, pulpitSize, pulpitHeight);
        }

        private void HandleGameStart()
        {
            _config = GameConfigLoader.Instance != null && GameConfigLoader.Instance.IsLoaded
                ? GameConfigLoader.Instance.Config
                : new GameConfig();

            _running = true;

            SpawnPulpit(Vector2Int.zero);
            FirstPulpit = _activePulpits[0];
            FirstPulpit.MarkPreScored();

            _spawnLoop = StartCoroutine(SpawnLoop());
        }

        private void HandleGameReset()
        {
            _running = false;
            if (_spawnLoop != null)
            {
                StopCoroutine(_spawnLoop);
                _spawnLoop = null;
            }

            foreach (Pulpit p in _activePulpits)
            {
                if (p != null) Destroy(p.gameObject);
            }
            _activePulpits.Clear();
            _lastGridPosition = Vector2Int.zero;
            FirstPulpit = null;
        }

        private IEnumerator SpawnLoop()
        {
            while (_running)
            {
                yield return new WaitForSeconds(Mathf.Max(0.1f, _config.pulpit_data.pulpit_spawn_time));
                if (!_running) yield break;

                _activePulpits.RemoveAll(p => p == null);
                if (_activePulpits.Count >= MaxActivePulpits) continue;

                Vector2Int nextPos = PulpitGrid.GetRandomAdjacent(_lastGridPosition, _activePulpits);
                SpawnPulpit(nextPos);
            }
        }

        private void SpawnPulpit(Vector2Int gridPos)
        {
            if (pulpitPrefab == null)
            {
                Debug.LogError("[PulpitSpawner] No pulpit prefab assigned.");
                return;
            }

            Vector3 worldPos = PulpitGrid.ToWorldPosition(gridPos, pulpitSize, pulpitHeight);
            Pulpit pulpit = Instantiate(pulpitPrefab, worldPos, Quaternion.identity, transform);

            float min = _config.pulpit_data.min_pulpit_destroy_time;
            float max = Mathf.Max(min, _config.pulpit_data.max_pulpit_destroy_time);
            float lifetime = Random.Range(min, max);
            pulpit.Initialize(gridPos, lifetime);

            _activePulpits.Add(pulpit);
            _lastGridPosition = gridPos;
        }
    }
}
