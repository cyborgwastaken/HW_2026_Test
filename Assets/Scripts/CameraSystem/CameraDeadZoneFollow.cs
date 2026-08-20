using UnityEngine;
using Doofus.Core;

namespace Doofus.CameraSystem
{
    // Perspective chase camera: rests at a fixed offset behind and above the target,
    // tilted down at a constant pitch angle - deliberately NOT rotating to track the
    // target's facing direction (kept fixed per earlier direction, rather than orbiting
    // like the third-person controller's own look camera would).
    //
    // Pulled back far (offsetFromAnchor) rather than sitting close, specifically so the
    // current pulpit (9x9) and an adjacent one (also 9x9, spawned in any of the 4 cardinal
    // directions - the layout is random) both stay in frame together. Field of view is
    // read straight off the Camera component - not managed here - so it stays in sync
    // with whatever's set directly on Main Camera.
    //
    // The dead zone is evaluated in SCREEN space (via WorldToViewportPoint), not world
    // space. A flat world-space XZ distance doesn't correspond to what's actually visible
    // once the camera sits at an angle: the same world-space movement toward/away from the
    // camera (compressed by perspective at this pitch) shows up very differently on screen
    // than the same movement side to side, so a world-distance threshold let the target
    // wander out of frame in some directions while still reading as "inside the dead zone"
    // in others. Checking the real projected viewport position fixes that regardless of
    // direction.
    //
    // Moves the camera directly in world space rather than the shared CameraController
    // parent - Doofus is a sibling under that same parent, so moving the parent would
    // also drag Doofus's world position along with it and fight the CharacterController's
    // own movement. Resets to the world origin - where Doofus always respawns - whenever
    // the game resets/restarts.
    public class CameraDeadZoneFollow : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private Vector3 offsetFromAnchor = new Vector3(0f, 51f, -60f);
        [SerializeField] private float pitchAngle = 50f;
        // Fraction of the way from screen center to the screen edge (0 = dead center,
        // 1 = edge of the viewport) the target may drift before the anchor starts
        // catching up.
        [SerializeField] private float deadZoneFraction = 0.4f;
        [SerializeField] private float smoothTime = 0.1f;

        private static readonly Vector3 InitialAnchor = Vector3.zero;

        private Camera _camera;
        private Vector3 _anchor;
        private Vector3 _velocity;

        public void SetTarget(Transform newTarget) => target = newTarget;

        private void Awake()
        {
            _camera = GetComponent<Camera>();

            _anchor = InitialAnchor;
            ApplyPose();
        }

        private void OnEnable()
        {
            GameEvents.OnGameReset += HandleReset;
        }

        private void OnDisable()
        {
            GameEvents.OnGameReset -= HandleReset;
        }

        private void HandleReset()
        {
            _anchor = InitialAnchor;
            _velocity = Vector3.zero;
            ApplyPose();
        }

        private void ApplyPose()
        {
            transform.position = _anchor + offsetFromAnchor;
            transform.rotation = Quaternion.Euler(pitchAngle, 0f, 0f);
        }

        private void LateUpdate()
        {
            if (target == null) return;

            Camera cam = _camera ??= GetComponent<Camera>();
            if (cam == null) return;

            Vector3 viewportPos = cam.WorldToViewportPoint(target.position);

            // Behind the camera entirely (shouldn't normally happen given the dead zone
            // keeps pulling the anchor in, but treat it as maximally out of bounds rather
            // than silently doing nothing if it ever does).
            bool behindCamera = viewportPos.z <= 0f;
            Vector2 offsetFromCenter = new Vector2((viewportPos.x - 0.5f) * 2f, (viewportPos.y - 0.5f) * 2f);
            float screenDistance = offsetFromCenter.magnitude;

            if (behindCamera || screenDistance > deadZoneFraction)
            {
                Vector3 targetFlat = new Vector3(target.position.x, 0f, target.position.z);
                _anchor = Vector3.SmoothDamp(_anchor, targetFlat, ref _velocity, smoothTime);
                transform.position = _anchor + offsetFromAnchor;
            }
        }
    }
}
