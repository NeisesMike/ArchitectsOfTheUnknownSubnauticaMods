﻿using System.Collections.Generic;
using UnityEngine;
using LeviathanEggs.MonoBehaviours;
using LeviathanEggs.Prefabs.API;
using static LeviathanEggs.Helpers.AssetsBundleHelper;
namespace LeviathanEggs.Prefabs
{
    class RobotEgg : EggPrefab
    {
        public RobotEgg()
            :base("RobotEgg", "Alien Robot Egg", "Alien Robots are deployed from these.")
        {}
        public override OverrideTechType MakeATechTypeToOverride =>
            new OverrideTechType("RobotEggUndiscovered", "Unknown Alien Artifact", "Unknown Alien technology that appears to store some kind of device.");
        public override GameObject Model => LoadGameObject("RobotEgg.prefab");
        public override TechType HatchingCreature => TechType.PrecursorDroid;
        public override float HatchingTime => 3f;
        public override Sprite ItemSprite => LoadSprite("RobotEgg");
        public override bool AcidImmune => true;
        public override string AssetsFolder => Main.AssetsFolder;
        public override List<LootDistributionData.BiomeData> BiomesToSpawnIn => new List<LootDistributionData.BiomeData>()
        {
            new LootDistributionData.BiomeData()
            {
                biome = BiomeType.TreeCove_LakeFloor,
                count = 1,
                probability = 0.2f,
            },
            new LootDistributionData.BiomeData()
            {
                biome = BiomeType.TreeCove_Ground,
                count = 1,
                probability = 0.4f
            }
        };

        public override GameObject GetGameObject()
        {
            var prefab = base.GetGameObject();
            
            Material material = new Material(Shader.Find("MarmosetUBER"))
            {
                mainTexture = LoadTexture2D("RobotEggDiffuse"),
            };
            material.EnableKeyword("MARMO_NORMALMAP");
            material.EnableKeyword("MARMO_SPECMAP");
            material.EnableKeyword("MARMO_EMISSION");

            material.SetTexture(ShaderPropertyID._Illum, LoadTexture2D("RobotEggIllum"));
            material.SetTexture(ShaderPropertyID._SpecTex, LoadTexture2D("RobotEggDiffuse"));
            material.SetTexture(ShaderPropertyID._BumpMap, LoadTexture2D("RobotEggNormal"));

            Renderer[] renderers = prefab.GetAllComponentsInChildren<Renderer>();
            foreach (var rend in renderers)
            {
                rend.material = material;
                rend.sharedMaterial = material;
            }
            
            ResourceTracker resourceTracker = prefab.EnsureComponent<ResourceTracker>();
            resourceTracker.techType = this.TechType;
            resourceTracker.overrideTechType = TechType.GenericEgg;
            resourceTracker.rb = prefab.GetComponent<Rigidbody>();
            resourceTracker.prefabIdentifier = prefab.GetComponent<PrefabIdentifier>();
            resourceTracker.pickupable = prefab.GetComponent<Pickupable>();

            prefab.AddComponent<SpawnLocations>();
            prefab.EnsureComponent<RobotEggPulsating>();

            return prefab;
        }
        public override Vector2int SizeInInventory => new Vector2int(2, 2);
    }
}
