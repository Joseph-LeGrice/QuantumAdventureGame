using Photon.Deterministic;
using UnityEngine;

namespace Quantum.AdventureGame
{
    public class PlayerInput : MonoBehaviour
    {
        private void OnEnable()
        {
            QuantumCallback.Subscribe(this, (CallbackPollInput input) => PollInput(input));
            
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }

        private void PollInput(CallbackPollInput callback)
        {
            Vector2 MovementInput;
            MovementInput.x = UnityEngine.Input.GetKey(KeyCode.D) ? 1 : UnityEngine.Input.GetKey(KeyCode.A) ? -1 : 0;
            MovementInput.y = UnityEngine.Input.GetKey(KeyCode.W) ? 1 : UnityEngine.Input.GetKey(KeyCode.S) ? -1 : 0;

            Vector2 LookInputDelta;
            LookInputDelta.x = UnityEngine.Input.mousePositionDelta.x;
            LookInputDelta.y = -UnityEngine.Input.mousePositionDelta.y;
             
            Quantum.Input i = new Quantum.Input();
            i.MovementDirection = MovementInput.ToFPVector2();
            i.LookDelta = LookInputDelta.ToFPVector2();
            i.WantsJump = UnityEngine.Input.GetKey(KeyCode.Space);
            i.CameraZoomDelta = UnityEngine.Input.mouseScrollDelta.y.ToFP();
            callback.SetInput(i, DeterministicInputFlags.Repeatable);
        }
    }
}
