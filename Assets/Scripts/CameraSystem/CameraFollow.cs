using UnityEngine;

namespace Doofus.CameraSystem
{
    // Third-person chase camera: orbits to stay behind the target's current facing
    // direction (rotating localOffset by target.rotation each frame) rather than sitting
    // at a fixed world-space offset, so the camera swings around as the player turns to
    // move and keeps their back in view - like a standard third-person follow cam.
    public class CameraFollow : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private Vector3 localOffset = new Vector3(0f, 2.4f, -4.5f);
        [SerializeField] private Vector3 lookAtLocalOffset = new Vector3(0f, 1.2f, 0f);
        [SerializeField] private float positionSmoothTime = 0.12f;
        [SerializeField] private float rotationSmoothSpeed = 8f;

        private Vector3 _velocity;

        public void SetTarget(Transform newTarget) => target = newTarget;

        private void LateUpdate()
        {
            if (target == null) return;

            Vector3 desiredPosition = target.position + target.rotation * localOffset;
            transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref _velocity, positionSmoothTime);

            Vector3 lookPoint = target.position + lookAtLocalOffset;
            Quaternion desiredRotation = Quaternion.LookRotation(lookPoint - transform.position, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, desiredRotation, rotationSmoothSpeed * Time.deltaTime);
        }
    }
}
