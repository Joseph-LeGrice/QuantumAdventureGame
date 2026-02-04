
namespace Quantum.AdventureGame
{
    using Quantum;
    using Quantum.Collections;
    using Photon.Deterministic;
    using UnityEngine.Scripting;

    [Preserve]
    public unsafe class PlayerSpawnSystem : SystemSignalsOnly, ISignalOnPlayerAdded
    {
        public override void OnEnabled(Frame f)
        {
            base.OnEnabled(f);
            f.AllocateDictionary(out f.Global->PlayerLookup);
        }

        public override void OnDisabled(Frame f)
        {
            base.OnDisabled(f);
            f.FreeDictionary(f.Global->PlayerLookup);
        }

        public void OnPlayerAdded(Frame f, PlayerRef player, bool firstTime)
        {
            FPVector3 SpawnPosition = FPVector3.Zero;
            FPQuaternion SpawnRotation = FPQuaternion.Identity;
            var possibleSpawns = f.Filter<SpawnPoint, Transform3D>();
            while (possibleSpawns.Next(out EntityRef EntityPrototype, out SpawnPoint sp, out Transform3D transform))
            {
                SpawnPosition = transform.Position;
                SpawnRotation = transform.Rotation;
            }
            
            RuntimePlayer rp = f.GetPlayerData(player);
            
            EntityPrototype ep = f.FindAsset(rp.PlayerAvatar);
            EntityRef localPlayerEntityRef = f.Create(ep);
            
            PlayerLink* playerEntityLink = f.Unsafe.GetPointer<PlayerLink>(localPlayerEntityRef);
            playerEntityLink->PlayerRef = player;
            
            Transform3D* playerTransform = f.Unsafe.GetPointer<Transform3D>(localPlayerEntityRef);
            playerTransform->Position = SpawnPosition;
            playerTransform->Rotation = SpawnRotation;

            QDictionary<PlayerRef, EntityRef> PlayerLookup = f.ResolveDictionary(f.Global->PlayerLookup);
            PlayerLookup[player] = localPlayerEntityRef;
            
            EntityPrototype cameraPrototype = f.FindAsset(rp.PlayerCameraPrototype);
            EntityRef localCameraEntity = f.Create(cameraPrototype);
            if (f.AddOrGet(localCameraEntity, out PlayerLink* cameraPlayerLink))
            {
                cameraPlayerLink->PlayerRef = player;
            }
        }
    }
}
