using ArchitectsLibrary.API;
using ECCLibrary;
using RotA.Mono;
using RotA.Mono.Creatures.GargEssentials;
using UnityEngine;

namespace RotA.Prefabs.Creatures
{
    // class used for the adult gargantuan. no creature implements this class, but the void gargantuan inherits from
    public class AdultGargantuan : GargantuanBase
    {
        private static readonly int _zWrite = Shader.PropertyToID("_ZWrite");
        private static readonly int _fresnel = Shader.PropertyToID("_Fresnel");
        private static readonly int _shininess = Shader.PropertyToID("_Shininess");
        private static readonly int _specInt = Shader.PropertyToID("_SpecInt");
        private static readonly int _emissionLm = Shader.PropertyToID("_EmissionLM");
        private static readonly int _emissionLmNight = Shader.PropertyToID("_EmissionLMNight");
        private static readonly int _glowStrength = Shader.PropertyToID("_GlowStrength");
        private static readonly int _glowStrengthNight = Shader.PropertyToID("_GlowStrengthNight");

        private const float kSwimVelocity = 50f;
        private const float kChargeVelocity = 45f;

        public AdultGargantuan(string classId, string friendlyName, string description, GameObject model, Texture2D spriteTexture) : base(classId, friendlyName, description, model, spriteTexture)
        {
        }

        public override float BiteDamage => 5000f;

        public override bool EnableStealth => true;

        public override float VehicleDamagePerSecond => 70;

        public override bool OneShotsPlayer => true;

        public override float TentacleSnapSpeed => 35;

        public override bool CanBeScaredByElectricity => true;

        public override UBERMaterialProperties MaterialSettings => new UBERMaterialProperties(2f, 100, 3f);

        public override ScannableItemData ScannableSettings => new ScannableItemData(true, 60f, Mod.modEncyPath_gargantuan, Mod.gargAssetBundle.LoadAsset<Sprite>("Adult_Popup"), Mod.gargAssetBundle.LoadAsset<Texture2D>("Adult_Ency"));

        public override AttackLastTargetSettings AttackSettings => new AttackLastTargetSettings(0.4f, kChargeVelocity, 25f, 30f, 17f, 30f);

        public override float MaxVelocityForSpeedParameter => kChargeVelocity + 5f;

        public override SwimRandomData SwimRandomSettings => new SwimRandomData(true, new Vector3(250f, 125, 250f), kSwimVelocity, 4f, 0.1f);

        public override AvoidObstaclesData AvoidObstaclesSettings => new AvoidObstaclesData(1f, false, 30f);

        public override BehaviourLODLevelsStruct BehaviourLODSettings => new BehaviourLODLevelsStruct(20000, 40000, 100000);

        public override float TurnSpeed => 0.1f;

        public override float SpineBoneSnapSpeed => 0.075f;

        public override (float, float) RoarSoundMinMax => (200f, 10000f);

        public override bool RoarDoesDamage => true;

        public override GargCollisionsMode CollisionsMode => GargCollisionsMode.Trigger;

        public override bool HasEyeTracking => true;

        public override void AddCustomBehaviour(CreatureComponents components)
        {
            base.AddCustomBehaviour(components);
            
            // update materials arbitrarily for the transparent appearance
            
            Renderer mainRenderer = prefab.SearchChild("Gargantuan.004").GetComponent<SkinnedMeshRenderer>();
            Renderer eyeRenderer = prefab.SearchChild("Gargantuan.002").GetComponent<SkinnedMeshRenderer>();
            Renderer insideRenderer = prefab.SearchChild("Gargantuan.003").GetComponent<SkinnedMeshRenderer>();
            UpdateGargTransparentMaterial(mainRenderer.materials[0]);
            UpdateGargTransparentMaterial(mainRenderer.materials[1]);
            UpdateGargTransparentMaterial(mainRenderer.materials[2]);
            UpdateGargSolidMaterial(mainRenderer.materials[3]);
            UpdateGargSkeletonMaterial(insideRenderer.materials[0]);
            UpdateGargGutsMaterial(insideRenderer.materials[1]);
            UpdateGargEyeMaterial(eyeRenderer.materials[0]);
            
            // idle sounds that are unique to the adult
            
            var gargPresence = prefab.AddComponent<GargantuanSwimAmbience>();
            gargPresence.swimSoundPrefix = "GargPresence";
            gargPresence.delay = 54f; // 54 comes from the length of the GargPresence sound, so it loops *almost* perfectly
            
            components.locomotion.maxAcceleration = 45f;
            components.swimRandom.swimForward = 0.5f;
            components.swimRandom.onSphere = true;
            prefab.GetComponent<StayAtLeashPosition>().swimVelocity = kSwimVelocity;

            // fixes the turning so it's not insanely fast
            
            components.locomotion.forwardRotationSpeed = 0.23f;
            components.locomotion.upRotationSpeed = 0.5f;

            // voice line that plays when you're near the gargantuan
            
            prefab.AddComponent<GargantuanEncounterPDA>();

            var avoidObstacles = prefab.GetComponent<AvoidObstacles>();
            avoidObstacles.avoidanceDistance = 100f;
            avoidObstacles.avoidanceIterations = 20;
            avoidObstacles.scanDistance = 20;
            avoidObstacles.scanInterval = 0.2f;
            avoidObstacles.scanDistance = 100f;
            avoidObstacles.scanRadius = 100f;

            foreach(var renderer in prefab.GetComponentsInChildren<Renderer>(true))
            {
                if (renderer.gameObject.name != "GargEyeGloss") continue;
                renderer.material.SetFloat(_fresnel, 0.69f);
                renderer.material.SetFloat(_shininess, 6.14f);
                renderer.material.SetFloat(_specInt, 30f);
            }

            components.worldForces.waterDepth = -30f;
            components.worldForces.aboveWaterGravity = 25f;
        }

        public static void UpdateGargTransparentMaterial(Material material)
        {
            material.SetInt(_zWrite, 1);
            material.SetFloat(_fresnel, 1);
        }

        public static void UpdateGargSolidMaterial(Material material)
        {
            material.SetFloat(_fresnel, 0.6f);
            material.SetFloat(_shininess, 8f);
            material.SetFloat(_specInt, 30);
            material.SetFloat(_emissionLm, 0.1f);
            material.SetFloat(_emissionLmNight, 0.1f);
        }

        public static void UpdateGargEyeMaterial(Material material)
        {
            material.SetFloat(_specInt, 15f);
            material.SetFloat(_glowStrength, 0.70f);
            material.SetFloat(_glowStrengthNight, 0.70f);
        }

        public static void UpdateGargSkeletonMaterial(Material material)
        {
            material.SetFloat(_fresnel, 1);
            material.SetFloat(_specInt, 50);
            material.SetFloat(_glowStrength, 2.5f);
            material.SetFloat(_glowStrengthNight, 6f);
        }

        public static void UpdateGargGutsMaterial(Material material)
        {
            material.EnableKeyword("MARMO_ALPHA_CLIP");
            material.SetFloat(_fresnel, 1f);
            material.SetFloat(_specInt, 50);
            material.SetFloat(_glowStrength, 10f);
            material.SetFloat(_glowStrengthNight, 10f);

        }

        public override void ApplyAggression()
        {
            MakeAggressiveToSharksButPlayer(120f, 6, 0.2f, 0.5f);
            MakeAggressiveTo(60f, 2, EcoTargetType.Whale, 0.23f, 2.3f);
            MakeAggressiveTo(200f, 7, EcoTargetType.Leviathan, 0.3f, 3f);
            MakeAggressiveTo(200f, 7, Mod.SuperDecoyTargetType, 0f, 5f);
        }

        public override bool CanPerformCyclopsCinematic => true;

        public override float EyeFov => -1f;

        public override bool DoesScreenShake => true;

        public override float CloseRoarThreshold => 350f;

        public override GargGrabFishMode GrabFishMode => GargGrabFishMode.LeviathansOnlyAndSwallow;
    }
}
