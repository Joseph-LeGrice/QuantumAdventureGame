using Quantum;
using Unity.Cinemachine;

public unsafe class PlayerCameraView : QuantumEntityViewComponent
{
    [UnityEngine.SerializeField]
    private CinemachineCamera m_camera;
    
    public override void OnActivate(Frame frame)
    {
        var pl = frame.Get<PlayerLink>(EntityRef);
        if (frame.ResolveDictionary(frame.Global->PlayerLookup).TryGetValue(pl.PlayerRef, out EntityRef PlayerEntityRef))
        {
            var PlayerView = EntityView.EntityViewUpdater.GetView(PlayerEntityRef);
            m_camera.Follow = PlayerView.GetComponent<PlayerCharacterView>().GetCameraFollowTarget();
        }
    }
}
