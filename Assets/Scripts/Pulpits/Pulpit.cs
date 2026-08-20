using System.Collections;
using UnityEngine;
using Doofus.Core;

namespace Doofus.Pulpits
{
    // A single pulpit: counts down its own randomized lifetime, reports the first time
    // Doofus successfully lands on it (for scoring), and gives two visual cues for how
    // much time is left - a countdown readout (always running) and a green-to-red color
    // gradient that only starts once Doofus has actually stood on it. Spawns in small
    // (centered on its grid cell) and grows to full size, and mirrors that by shrinking
    // back down before actually despawning.
    [RequireComponent(typeof(Collider))]
    public class Pulpit : MonoBehaviour
    {
        [SerializeField] private Renderer[] renderers;
        [SerializeField] private Color freshColor = new Color(0.05f, 0.85f, 0.25f);
        [SerializeField] private Color dangerColor = new Color(0.9f, 0.15f, 0.1f);
        [SerializeField] private TextMesh timerText;
        [SerializeField] private float spawnGrowDuration = 0.4f;
        [SerializeField] private float despawnShrinkDuration = 0.4f;
        [SerializeField] private float startScaleFraction = 0.05f;

        public Vector2Int GridPosition { get; private set; }
        public bool IsAlive { get; private set; } = true;

        // Fraction of this pulpit's randomized lifetime that has elapsed (0 = just
        // spawned, 1 = about to despawn). PulpitSpawner watches this to decide when to
        // spawn the next pulpit.
        public float LifeFraction => _lifetime > 0f ? Mathf.Clamp01(_elapsed / _lifetime) : 1f;

        private bool _hasBeenScored;
        private bool _hasLanded;
        private float _elapsed;
        private float _elapsedAtLanding;
        private float _lifetime;
        private Coroutine _lifetimeRoutine;
        private Coroutine _scaleRoutine;
        private Collider[] _colliders;
        private Vector3 _fullScale;

        private void Awake()
        {
            // Pulpit carries both a solid collider (physical support) and a trigger
            // collider (landing detection) - the player uses a CharacterController,
            // which never raises OnCollisionEnter against a static collider.
            _colliders = GetComponents<Collider>();
            if (renderers == null || renderers.Length == 0)
            {
                renderers = GetComponentsInChildren<Renderer>();
            }

            _fullScale = transform.localScale;
        }

        public void Initialize(Vector2Int gridPosition, float lifetimeSeconds)
        {
            GridPosition = gridPosition;
            IsAlive = true;
            _hasBeenScored = false;
            _hasLanded = false;
            _elapsed = 0f;
            _elapsedAtLanding = 0f;
            _lifetime = Mathf.Max(0.01f, lifetimeSeconds);

            foreach (Collider c in _colliders)
            {
                if (c != null) c.enabled = true;
            }

            SetColor(freshColor);
            UpdateTimerText(_lifetime);

            // Spawn in small (at the grid cell's center, since scaling around the
            // object's own pivot grows it outward evenly) and grow to full size.
            if (_scaleRoutine != null) StopCoroutine(_scaleRoutine);
            transform.localScale = SmallScale();
            _scaleRoutine = StartCoroutine(AnimateScale(transform.localScale, _fullScale, spawnGrowDuration, destroyAfter: false));

            if (_lifetimeRoutine != null) StopCoroutine(_lifetimeRoutine);
            _lifetimeRoutine = StartCoroutine(LifetimeCountdown());
        }

        // Marks the starting pulpit as already scored so Doofus spawning on it doesn't
        // count as a "move" per the scoring rule (only moves to a *new* pulpit count).
        public void MarkPreScored()
        {
            _hasBeenScored = true;
        }

        private IEnumerator LifetimeCountdown()
        {
            while (_elapsed < _lifetime)
            {
                _elapsed += Time.deltaTime;
                UpdateTimerText(_lifetime - _elapsed);

                if (_hasLanded)
                {
                    float denom = Mathf.Max(0.01f, _lifetime - _elapsedAtLanding);
                    float t = Mathf.Clamp01((_elapsed - _elapsedAtLanding) / denom);
                    SetColor(Color.Lerp(freshColor, dangerColor, t));
                }

                yield return null;
            }

            Despawn();
        }

        private void UpdateTimerText(float remaining)
        {
            if (timerText == null) return;
            timerText.text = Mathf.Max(0f, remaining).ToString("F1");
        }

        private void SetColor(Color color)
        {
            foreach (Renderer r in renderers)
            {
                if (r != null) r.material.color = color;
            }
        }

        private void Despawn()
        {
            if (!IsAlive) return;
            IsAlive = false;

            foreach (Collider c in _colliders)
            {
                if (c != null) c.enabled = false;
            }

            // Shrink back down (mirroring the spawn-in grow) before actually destroying.
            if (_scaleRoutine != null) StopCoroutine(_scaleRoutine);
            _scaleRoutine = StartCoroutine(AnimateScale(transform.localScale, SmallScale(), despawnShrinkDuration, destroyAfter: true));
        }

        private Vector3 SmallScale()
        {
            return new Vector3(_fullScale.x * startScaleFraction, _fullScale.y, _fullScale.z * startScaleFraction);
        }

        private IEnumerator AnimateScale(Vector3 from, Vector3 to, float duration, bool destroyAfter)
        {
            float elapsed = 0f;
            duration = Mathf.Max(0.01f, duration);

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                transform.localScale = Vector3.Lerp(from, to, elapsed / duration);
                yield return null;
            }

            transform.localScale = to;

            if (destroyAfter) Destroy(gameObject);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!IsAlive || !other.CompareTag("Player")) return;

            if (!_hasLanded)
            {
                _hasLanded = true;
                _elapsedAtLanding = _elapsed;
            }

            if (_hasBeenScored) return;

            _hasBeenScored = true;
            GameEvents.RaisePulpitLanded();
        }

        private void OnDestroy()
        {
            if (_lifetimeRoutine != null) StopCoroutine(_lifetimeRoutine);
            if (_scaleRoutine != null) StopCoroutine(_scaleRoutine);
        }
    }
}
