using UnityEngine;

namespace Doofus.CameraSystem
{
    // Smooth dead-zone follow for the fixed overhead camera: it holds still while the
    // target is within deadZoneFraction of the camera's view radius from center, then
    // smoothly pulls just enough to keep the target from drifting further than that.
    //
    // Deliberately moves the camera directly in world space rather than the shared
    // CameraController parent - Doofus is a sibling under that same parent, so moving
    // the parent would also drag Doofus's world position along with it and fight the
    // CharacterController's own movement.
    public class CameraDeadZoneFollow : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private float deadZoneFraction = 0.4f;
        [SerializeField] private float smoothTime = 0.3f;
        [SerializeField] private float fallbackViewRadius = 20f;

        private Camera _camera;
        private Vector3 _velocity;

        public void SetTarget(Transform newTarget) => target = newTarget;

        private void Awake()
        {
            _camera = GetComponent<Camera>();
        }

        private void LateUpdate()
        {
            if (target == null) return;

            float viewRadius = (_camera != null && _camera.orthographic) ? _camera.orthographicSize : fallbackViewRadius;
            float deadZoneRadius = viewRadius * deadZoneFraction;

            Vector3 camPos = transform.position;
            Vector3 flatOffset = new Vector3(target.position.x - camPos.x, 0f, target.position.z - camPos.z);
            float distance = flatOffset.magnitude;

            if (distance <= deadZoneRadius) return;

            float excess = distance - deadZoneRadius;
            Vector3 desiredPosition = camPos + flatOffset.normalized * excess;
            desiredPosition.y = camPos.y;

            transform.position = Vector3.SmoothDamp(camPos, desiredPosition, ref _velocity, smoothTime);
        }
    }
}
