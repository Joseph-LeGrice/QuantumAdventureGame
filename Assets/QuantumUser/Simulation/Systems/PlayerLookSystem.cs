using UnityEngine.Scripting;
using Photon.Deterministic;

namespace Quantum.AdventureGame
{
    [Preserve]
    public unsafe class PlayerLookSystem : SystemMainThreadFilter<PlayerLookSystem.Filter>
    {
        public struct Filter
        {
            public EntityRef EntityId;
            public PlayerLink* PlayerLink;
            public PlayerCharacterController* CharacterController;
        }

        public override void Update(Frame f, ref Filter filter)
        {
            RuntimePlayer rp = f.GetPlayerData(filter.PlayerLink->PlayerRef);
            PlayerCharacterData pcd = f.FindAsset(rp.CharacterData);
            
            Quantum.Input* PlayerInput = f.GetPlayerInput(filter.PlayerLink->PlayerRef);

            FP NextYaw = filter.CharacterController->LookYaw + pcd.CameraSensitivity * PlayerInput->LookDelta.X;
            filter.CharacterController->LookYaw = NextYaw % FP._360;

            FP NextPitch = filter.CharacterController->LookPitch + pcd.CameraSensitivity * PlayerInput->LookDelta.Y;
            filter.CharacterController->LookPitch = FPMath.Clamp(NextPitch, pcd.MinCameraPitch, pcd.MaxCameraPitch);
        }
    }
}
