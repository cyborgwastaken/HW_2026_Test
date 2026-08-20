using UnityEngine;

namespace Doofus.CameraSystem
{
    // Fixed-angle follow camera: tracks the player's position from a constant
    // world-space offset and always looks at them. Deliberately does NOT orbit or
    // rotate with the player's facing/look direction (i.e. not coupled to the third-
    // person controller's own camera-target rotation) - the viewing angle stays put,
    // only the camera's position translates to keep up with the player.
    public class CameraFollow : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private Vector3 worldOffset = new Vector3(0f, 4.5f, -6f);
        [SerializeField] private Vector3 lookAtLocalOffset = new Vector3(0f, 1.2f, 0f);
        [SerializeField] private float positionSmoothTime = 0.12f;
        [SerializeField] private float rotationSmoothSpeed = 8f;

        private Vector3 _velocity;

        public void SetTarget(Transform newTarget) => target = newTarget;

        private void LateUpdate()
        {
            if (target == null) return;

            Vector3 desiredPosition = target.position + worldOffset;
            transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref _velocity, positionSmoothTime);

            Vector3 lookPoint = target.position + lookAtLocalOffset;
            Quaternion desiredRotation = Quaternion.LookRotation(lookPoint - transform.position, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, desiredRotation, rotationSmoothSpeed * Time.deltaTime);
        }
    }
}
