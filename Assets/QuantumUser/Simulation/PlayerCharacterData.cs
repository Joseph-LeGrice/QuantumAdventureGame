using Photon.Deterministic;
using Quantum;

public class PlayerCharacterData : AssetObject
{
    public FP MaxMovementSpeed = 5;
    public FP AccelerationTime = 1;

    public FP CameraSensitivity = 1;
    public FP MinCameraPitch;
    public FP MaxCameraPitch;

    public FP AirBraking = FP._3;
    public FP GroundContactAngle = FP.Rad2Deg * FP.Rad_22_50;
    public FP GravityStrength = FP._9 + FP._0_10 * (FP._5 + FP._3);
    public FP JumpImpulseForce = FP._10;
    public FP DoubleJumpVelocityLimit = FP._5;
    
    public FP MinPenetration = FP._0_05;
    public FP MaxPenetration = FP._0_50;
    public FP PenetrationCorrectionRate = FP._0_50;

    public FP GetAcceleration()
    {
        return MaxMovementSpeed / AccelerationTime;
    }
}
