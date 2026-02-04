using System;
using UnityEngine.Scripting;
using Photon.Deterministic;
using Quantum.Physics3D;

namespace Quantum.AdventureGame
{
    public struct GroundInfo
    {
        public bool IsGrounded;
        public FPVector3 GroundSurfaceNormal;
        public FP GroundPenetration;
    }
    
    [Preserve]
    public unsafe class PlayerMovementSystem : SystemMainThreadFilter<PlayerMovementSystem.Filter>
    {
        public struct Filter
        {
            public EntityRef EntityId;
            public PlayerLink* PlayerLink;
            public PlayerCharacterController* CharacterController;
            public PhysicsCollider3D* PhysicsCollider;
            public Transform3D* Transform;
        }

        public override void Update(Frame f, ref Filter filter)
        {
            FPVector3 WorldUpDirection = FPVector3.Up;
            
            RuntimePlayer rp = f.GetPlayerData(filter.PlayerLink->PlayerRef);
            PlayerCharacterData pcd = f.FindAsset(rp.CharacterData);
            
            GroundInfo groundInfo = GetGroundInfo(f, ref filter, ref pcd, ref WorldUpDirection);

            FPVector3 LocalVelocity = filter.CharacterController->LocalVelocity;
            
            Quantum.Input* PlayerInput = f.GetPlayerInput(filter.PlayerLink->PlayerRef);

            FP MaxDeltaV = pcd.GetAcceleration() * f.DeltaTime;
            FPVector2 TargetVelocityXZ = pcd.MaxMovementSpeed * PlayerInput->MovementDirection.Normalized;
            FP XDiff = TargetVelocityXZ.X - LocalVelocity.X;
            LocalVelocity.X += FPMath.Sign(XDiff) * FPMath.Min(FPMath.Abs(XDiff), MaxDeltaV);
            FP ZDiff = TargetVelocityXZ.Y - LocalVelocity.Z;
            LocalVelocity.Z += FPMath.Sign(ZDiff) * FPMath.Min(FPMath.Abs(ZDiff), MaxDeltaV);
            
            if (!groundInfo.IsGrounded)
            {
                LocalVelocity.X -= FPMath.Sign(LocalVelocity.X) *  FPMath.Min(FPMath.Abs(LocalVelocity.X), pcd.AirBraking * f.DeltaTime);
                LocalVelocity.Z -= FPMath.Sign(LocalVelocity.Z) *  FPMath.Min(FPMath.Abs(LocalVelocity.Z), pcd.AirBraking * f.DeltaTime);
            }
            else
            {
                LocalVelocity.Y = FP._0;
            }
            LocalVelocity.Y -= pcd.GravityStrength * f.DeltaTime;

            bool didJump = false;
            bool didDoubleJump = false;
            
            if (PlayerInput->WantsJump.WasPressed)
            {
                if (groundInfo.IsGrounded)
                {
                    didJump = true;
                    LocalVelocity.Y += pcd.JumpImpulseForce;
                }
                else if (!filter.CharacterController->DidDoubleJump && LocalVelocity.Y < pcd.DoubleJumpVelocityLimit)
                {
                    didDoubleJump = true;
                    LocalVelocity.Y = pcd.JumpImpulseForce;
                }
            }
            
            filter.CharacterController->DidJump.Update(f.Number, didJump);
            filter.CharacterController->DidDoubleJump.Update(f.Number, didDoubleJump);

            filter.CharacterController->IsGrounded = groundInfo.IsGrounded;
            filter.CharacterController->LocalVelocity = LocalVelocity;

            FPVector3 HorizontalVelocity = FPQuaternion.Euler(FP._0, filter.CharacterController->LookYaw, FP._0) * FPQuaternion.FromToRotation(FPVector3.Up, groundInfo.GroundSurfaceNormal) * LocalVelocity.XOZ;
            FPVector3 VerticalVelocity = LocalVelocity.Y * WorldUpDirection;
            
            filter.Transform->Position += (HorizontalVelocity + VerticalVelocity) * f.DeltaTime;
            
            if (groundInfo.GroundPenetration > pcd.MinPenetration)
            {
                FP PenetrationCorrection = FPMath.Min(groundInfo.GroundPenetration - pcd.MinPenetration, pcd.PenetrationCorrectionRate * f.DeltaTime);
                if (groundInfo.GroundPenetration > pcd.MaxPenetration)
                {
                    PenetrationCorrection = groundInfo.GroundPenetration - pcd.MaxPenetration;
                }
                filter.Transform->Position += PenetrationCorrection * groundInfo.GroundSurfaceNormal;
            }

            if (HorizontalVelocity.SqrMagnitude > FP.EN3)
            {
                filter.Transform->Rotation = FPQuaternion.LookRotation(HorizontalVelocity, WorldUpDirection);
            }
        }

        private static GroundInfo GetGroundInfo(Frame f, ref Filter filter, ref PlayerCharacterData pcd, ref FPVector3 WorldUpDirection)
        {
            GroundInfo move = new GroundInfo() {
                IsGrounded = false,
                GroundSurfaceNormal = FPVector3.Up
            };

            HitCollection3D queryHits = f.Physics3D.OverlapShape(
                filter.Transform->Position,
                FPQuaternion.Identity,
                filter.PhysicsCollider->Shape,
                1 << filter.PhysicsCollider->Layer,
                QueryOptions.HitSolids | QueryOptions.ComputeDetailedInfo
            );
            queryHits.Sort(filter.Transform->Position);
            
            for (int index = 0; index < queryHits.Count; ++index)
            {
                Hit3D hit = queryHits[index];
                if (hit.Entity != filter.EntityId)
                {
                    if (FPVector3.Angle(hit.Normal, WorldUpDirection) < pcd.GroundContactAngle)
                    {
                        move.IsGrounded = true;
                        move.GroundSurfaceNormal = hit.Normal;
                        move.GroundPenetration = hit.OverlapPenetration;
                        break;
                    }
                }
            }

            return move;
        }
    }
}
