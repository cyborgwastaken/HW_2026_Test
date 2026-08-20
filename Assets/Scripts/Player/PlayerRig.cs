using UnityEngine;

namespace Doofus.Player
{
    // Generic reset/reposition adapter for a CharacterController-driven player (e.g. a
    // third-party third-person controller this project doesn't own/modify). Exists so
    // GameManager doesn't need to know which movement system the player uses - it just
    // calls ResetState(). Handles the standard Unity gotcha where setting transform.position
    // directly while a CharacterController is enabled fights with its internal move solver.
    [RequireComponent(typeof(CharacterController))]
    public class PlayerRig : MonoBehaviour
    {
        private CharacterController _controller;

        private CharacterController Controller => _controller != null ? _controller : (_controller = GetComponent<CharacterController>());

        public void ResetState(Vector3 position)
        {
            CharacterController controller = Controller;
            bool wasEnabled = controller.enabled;
            controller.enabled = false;
            transform.position = position;
            transform.rotation = Quaternion.identity;
            controller.enabled = wasEnabled;
        }
    }
}
