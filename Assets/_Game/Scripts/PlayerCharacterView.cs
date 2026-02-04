using Photon.Deterministic;
using Quantum;
using Quantum.AdventureGame;
using UnityEngine;

public unsafe class PlayerCharacterView : QuantumEntityViewComponent
{
    [SerializeField]
    private Animator m_animator;
    [SerializeField]
    private Transform m_characterTransform;
    [SerializeField]
    private Transform m_lookDirectionTransform;

    public Transform GetCameraFollowTarget()
    {
        return m_lookDirectionTransform;
    }

    public override void OnUpdateView()
    {
        var characterController = PredictedFrame.Get<PlayerCharacterController>(EntityRef);

        m_lookDirectionTransform.rotation = Quaternion.LookRotation(characterController.GetLookDirection().ToUnityVector3(), Vector3.up);

        m_animator.SetBool("IsMoving", characterController.LocalVelocity.XOZ.SqrMagnitude > FP._0_01);

        if (characterController.DidJump.WasPressed)
        {
            m_animator.SetTrigger("Jump");
        }

        if (characterController.DidDoubleJump.WasPressed)
        {
            m_animator.SetTrigger("DoubleJump");
        }
        
        m_animator.SetBool("IsGrounded", characterController.IsGrounded);
    }
}
