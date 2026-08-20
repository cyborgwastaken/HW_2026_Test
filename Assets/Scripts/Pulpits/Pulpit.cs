using System.Collections;
using UnityEngine;
using Doofus.Core;

namespace Doofus.Pulpits
{
    // A single pulpit: counts down its own randomized lifetime, warns just before it
    // despawns, and reports the first time Doofus successfully lands on it (for scoring).
    [RequireComponent(typeof(Collider))]
    public class Pulpit : MonoBehaviour
    {
        [SerializeField] private Renderer[] renderers;
        [SerializeField] private float warningTimeRemaining = 1f;
        [SerializeField] private Color warningColor = new Color(1f, 0.35f, 0.15f);

        public Vector2Int GridPosition { get; private set; }
        public bool IsAlive { get; private set; } = true;

        private bool _hasBeenScored;
        private Coroutine _lifetimeRoutine;
        private Collider[] _colliders;

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
        }

        public void Initialize(Vector2Int gridPosition, float lifetimeSeconds)
        {
            GridPosition = gridPosition;
            IsAlive = true;
            _hasBeenScored = false;

            foreach (Collider c in _colliders)
            {
                if (c != null) c.enabled = true;
            }

            if (_lifetimeRoutine != null) StopCoroutine(_lifetimeRoutine);
            _lifetimeRoutine = StartCoroutine(LifetimeCountdown(Mathf.Max(0.01f, lifetimeSeconds)));
        }

        // Marks the starting pulpit as already scored so Doofus spawning on it doesn't
        // count as a "move" per the scoring rule (only moves to a *new* pulpit count).
        public void MarkPreScored()
        {
            _hasBeenScored = true;
        }

        private IEnumerator LifetimeCountdown(float lifetime)
        {
            float elapsed = 0f;
            bool warned = false;

            while (elapsed < lifetime)
            {
                elapsed += Time.deltaTime;

                if (!warned && lifetime - elapsed <= warningTimeRemaining)
                {
                    warned = true;
                    SetColor(warningColor);
                }

                yield return null;
            }

            Despawn();
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

            Destroy(gameObject, 0.15f);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (_hasBeenScored || !IsAlive) return;
            if (!other.CompareTag("Player")) return;

            _hasBeenScored = true;
            GameEvents.RaisePulpitLanded();
        }

        private void OnDestroy()
        {
            if (_lifetimeRoutine != null) StopCoroutine(_lifetimeRoutine);
        }
    }
}
