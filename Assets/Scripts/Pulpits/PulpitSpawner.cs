using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Doofus.Config;
using Doofus.Core;

namespace Doofus.Pulpits
{
    // Owns pulpit spawning: at most 2 pulpits alive at once, placed adjacent to the
    // previously spawned pulpit with a randomized lifetime from Doofus's Diary.
    //
    // The next pulpit spawns once the most-recently-spawned one has burned through a
    // score-scaled fraction of its own lifetime - minSpawnLifeFraction (40%) at score 0,
    // ramping linearly up to maxSpawnLifeFraction (70%) by scoreForMaxSpawnPercent, so
    // the window to react shrinks as the run goes on.
    public class PulpitSpawner : MonoBehaviour
    {
        [SerializeField] private Pulpit pulpitPrefab;
        [SerializeField] private float pulpitSize = 9f;
        [SerializeField] private float pulpitHeight = 0f;
        [SerializeField] private float minSpawnLifeFraction = 0.4f;
        [SerializeField] private float maxSpawnLifeFraction = 0.7f;
        [SerializeField] private int scoreForMaxSpawnPercent = 50;
        private const int MaxActivePulpits = 2;

        private readonly List<Pulpit> _activePulpits = new List<Pulpit>();
        private Vector2Int _lastGridPosition;
        private Vector2Int? _previousGridPosition;
        private Pulpit _lastSpawnedPulpit;
        private Coroutine _spawnLoop;
        private GameConfig _config;
        private int _currentScore;
        private bool _running;

        public Pulpit FirstPulpit { get; private set; }

        private void OnEnable()
        {
            GameEvents.OnGameStart += HandleGameStart;
            GameEvents.OnGameReset += HandleGameReset;
            GameEvents.OnScoreChanged += HandleScoreChanged;
        }

        private void OnDisable()
        {
            GameEvents.OnGameStart -= HandleGameStart;
            GameEvents.OnGameReset -= HandleGameReset;
            GameEvents.OnScoreChanged -= HandleScoreChanged;
        }

        public Vector3 GetOriginWorldPosition()
        {
            return PulpitGrid.ToWorldPosition(Vector2Int.zero, pulpitSize, pulpitHeight);
        }

        private void HandleScoreChanged(int score)
        {
            _currentScore = score;
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
            _previousGridPosition = null;
            _lastSpawnedPulpit = null;
            _currentScore = 0;
            FirstPulpit = null;
        }

        private IEnumerator SpawnLoop()
        {
            while (_running)
            {
                yield return null;

                _activePulpits.RemoveAll(p => p == null);
                if (_activePulpits.Count >= MaxActivePulpits) continue;

                // Safety net: if there's nothing alive to watch (shouldn't normally
                // happen since a slot only frees up when a pulpit despawns, and its
                // replacement is watched from then on), spawn immediately rather than
                // stalling the game with no pulpits.
                bool ready = _lastSpawnedPulpit == null
                    || !_lastSpawnedPulpit.IsAlive
                    || _lastSpawnedPulpit.LifeFraction >= GetSpawnLifeFractionThreshold();

                if (ready)
                {
                    // Never place the new pulpit back at the position Doofus most
                    // recently moved from, even if that pulpit has since despawned
                    // (its cell would otherwise read as "unoccupied" again).
                    Vector2Int nextPos = PulpitGrid.GetRandomAdjacent(_lastGridPosition, _activePulpits, _previousGridPosition);
                    _previousGridPosition = _lastGridPosition;
                    SpawnPulpit(nextPos);
                }
            }
        }

        private float GetSpawnLifeFractionThreshold()
        {
            float t = scoreForMaxSpawnPercent > 0
                ? Mathf.Clamp01((float)_currentScore / scoreForMaxSpawnPercent)
                : 1f;
            return Mathf.Lerp(minSpawnLifeFraction, maxSpawnLifeFraction, t);
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
            _lastSpawnedPulpit = pulpit;
        }
    }
}
