using Photon.Deterministic;

namespace Quantum.AdventureGame
{
    public static class ComponentHelpers
    {
        public static FPQuaternion GetLookRotation(this PlayerCharacterController pcc)
        {
            return FPQuaternion.Euler(pcc.LookPitch, pcc.LookYaw, FP._0);
        }
        
        public static FPVector3 GetLookDirection(this PlayerCharacterController pcc)
        {
            return GetLookRotation(pcc) * FPVector3.Forward;
        }
    }
}