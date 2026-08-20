using UnityEngine;
using UnityEngine.InputSystem;
using Doofus.Config;
using Doofus.Core;

namespace Doofus.Player
{
    // WASD / arrow-key movement on the XZ plane. Speed comes from Doofus's Diary (falls back
    // to fallbackSpeed if the config hasn't loaded yet). Falling/death is handled separately
    // by DoofusFallDetector - this script only owns movement.
    [RequireComponent(typeof(Rigidbody))]
    public class DoofusController : MonoBehaviour
    {
        [SerializeField] private float fallbackSpeed = 3f;
        [SerializeField] private float turnSpeed = 15f;

        private Rigidbody _rigidbody;
        private Vector3 _moveInput;
        private float _speed;
        private bool _movementEnabled;

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
            _rigidbody.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
            _speed = fallbackSpeed;
        }

        private void OnEnable()
        {
            GameConfigLoader.OnConfigReady += HandleConfigReady;
            if (GameConfigLoader.Instance != null && GameConfigLoader.Instance.IsLoaded)
            {
                HandleConfigReady(GameConfigLoader.Instance.Config);
            }

            GameEvents.OnGameStart += HandleGameStart;
            GameEvents.OnGameOver += HandleGameStop;
            GameEvents.OnGameReset += HandleGameStop;
        }

        private void OnDisable()
        {
            GameConfigLoader.OnConfigReady -= HandleConfigReady;
            GameEvents.OnGameStart -= HandleGameStart;
            GameEvents.OnGameOver -= HandleGameStop;
            GameEvents.OnGameReset -= HandleGameStop;
        }

        private void HandleConfigReady(GameConfig config) => _speed = config.player_data.speed;
        private void HandleGameStart() => _movementEnabled = true;
        private void HandleGameStop() => _movementEnabled = false;

        private void Update()
        {
            if (!_movementEnabled || Keyboard.current == null)
            {
                _moveInput = Vector3.zero;
                return;
            }

            float x = 0f;
            float z = 0f;

            if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed) z += 1f;
            if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed) z -= 1f;
            if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) x += 1f;
            if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) x -= 1f;

            _moveInput = new Vector3(x, 0f, z);
            if (_moveInput.sqrMagnitude > 1f) _moveInput.Normalize();
        }

        private void FixedUpdate()
        {
            if (_moveInput.sqrMagnitude < 0.0001f) return;

            Vector3 delta = _moveInput * _speed * Time.fixedDeltaTime;
            _rigidbody.MovePosition(_rigidbody.position + delta);

            Quaternion targetRotation = Quaternion.LookRotation(_moveInput, Vector3.up);
            _rigidbody.MoveRotation(Quaternion.Slerp(_rigidbody.rotation, targetRotation, turnSpeed * Time.fixedDeltaTime));
        }

        public void ResetState(Vector3 position)
        {
            _moveInput = Vector3.zero;
            _rigidbody.linearVelocity = Vector3.zero;
            _rigidbody.angularVelocity = Vector3.zero;
            _rigidbody.position = position;
            _rigidbody.rotation = Quaternion.identity;
        }
    }
}
