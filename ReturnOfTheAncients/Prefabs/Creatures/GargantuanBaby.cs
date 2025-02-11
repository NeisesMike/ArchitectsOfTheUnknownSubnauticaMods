﻿using ArchitectsLibrary.API;
using ECCLibrary;
using RotA.Mono.Creatures.Baby;
using RotA.Mono.Creatures.CreatureActions;
using UnityEngine;

namespace RotA.Prefabs.Creatures
{
    public class GargantuanBaby : GargantuanBase
    {
        public override SwimRandomData SwimRandomSettings => new SwimRandomData(true, new Vector3(10f, 3f, 10f), 2f, 1f, 0.1f);
        public override StayAtLeashData StayAtLeashSettings => new StayAtLeashData(0.39f, 9f);
        public override AvoidObstaclesData AvoidObstaclesSettings => new AvoidObstaclesData(0.38f, true, 5f);
        public override VFXSurfaceTypes SurfaceType => VFXSurfaceTypes.organic;
        public override AttackLastTargetSettings AttackSettings => new AttackLastTargetSettings(0.4f, 15f, 6f, 7f, 2f, 15f);
        public override WaterParkCreatureParameters WaterParkParameters => new WaterParkCreatureParameters(0.1f, 0.5f, 0.5f, 1f, true);
        public override LargeWorldEntity.CellLevel CellLevel => LargeWorldEntity.CellLevel.Global;
        public override EcoTargetType EcoTargetType => EcoTargetType.CuteFish;

        public override ScannableItemData ScannableSettings => new ScannableItemData(true, 4f, Mod.modEncyPath_gargantuan, Mod.gargAssetBundle.LoadAsset<Sprite>("Juvenile_Popup"), Mod.gargAssetBundle.LoadAsset<Texture2D>("Baby_Ency"));

        public override string GetEncyTitle => LanguageSystem.Get("Ency_GargantuanBaby");
        public override string GetEncyDesc => LanguageSystem.Get("EncyDesc_GargantuanBaby");


        public GargantuanBaby(string classId, string friendlyName, string description, GameObject model, Texture2D spriteTexture) : base(classId, friendlyName, description, model, spriteTexture)
        {
        }

        public override void ApplyAggression()
        {
            prefab.AddComponent<GargantuanBabyAggression>();
        }

        public override float JawTentacleSnapSpeed => 4f;

        public override void AddCustomBehaviour(CreatureComponents components)
        {
            base.AddCustomBehaviour(components);
            CreatureFollowPlayer followPlayer = prefab.AddComponent<CreatureFollowPlayer>();
            followPlayer.distanceToPlayer = 7f;
            followPlayer.creature = components.creature;
            followPlayer.maxYPos = -8f;
            var babyComponent = prefab.AddComponent<GargantuanBabyTeleport>();
            components.locomotion.driftFactor = 1f;
            components.locomotion.forwardRotationSpeed = 1f;
            components.locomotion.upRotationSpeed = 3f;
            components.locomotion.maxAcceleration = 15f;
            prefab.GetComponent<AttackLastTarget>().swimInterval = 0.01f;
            GameObject target = prefab.SearchChild("CinematicTarget");
            target.AddComponent<GargBabyTarget>();
            babyComponent.cinematicTarget = target;
            var avoid = prefab.GetComponent<AvoidObstacles>();
            avoid.scanRadius = 8f;
            avoid.avoidanceDistance = 5f;
            avoid.avoidanceDuration = 2f;
            avoid.avoidTerrainOnly = true;

            prefab.EnsureComponent<GargantuanBabyGrowthManager>();

            components.creature.Hunger = new CreatureTrait(0f, -(1f / 45f));
        }

        public override bool UseSwimSounds => false;

        public override string CloseRoarPrefix => "GargBaby";
        public override string DistantRoarPrefix => "GargBaby";

        public override string AttachBoneName => "AttachBone";

        public override (float, float) RoarDelayMinMax => (18f, 40f);

        public override Vector2int SizeInInventory => new Vector2int(5, 3);

        public override (float, float) RoarSoundMinMax => (1f, 10f);

        public override float TentacleSnapSpeed => 1.5f;

        public override bool TentaclesHaveTrails => false;

        public override bool AttackPlayer => false;

        public override float Mass => 600f;

        public override float BiteDamage => 500f;

        public override float SpineBoneSnapSpeed => 1f;

        public override HeldFishData ViewModelSettings => new HeldFishData(TechType.Peeper, "GargModel", "GargModel_FP");

        public override bool Pickupable => true;

        public override float BioReactorCharge => 99999999999f;

        public override RespawnData RespawnSettings => new RespawnData(true, 15f);

        public override BehaviourLODLevelsStruct BehaviourLODSettings => new BehaviourLODLevelsStruct(50f, 150f, 500f);

        public override ItemSoundsType ItemSounds => ItemSoundsType.Fish;

        public override float TurnSpeed => 1f;

        public override GargGrabFishMode GrabFishMode => GargGrabFishMode.PickupableOnlyAndSwalllow;

        public override float EyeFov => 0.6f;

        public override float MaxVelocityForSpeedParameter => 20f;

        public override SmallVehicleAggressivenessSettings AggressivenessToSmallVehicles => new SmallVehicleAggressivenessSettings(0f, 0f);
    }
}
