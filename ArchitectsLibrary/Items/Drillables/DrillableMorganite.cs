﻿namespace ArchitectsLibrary.Items.Drillables
{
    using UnityEngine;
    using API;
    using Handlers;
    using System.Collections.Generic;
    using UWE;
    
    class DrillableMorganite : ReskinSpawnable
    {
        protected override string ReferenceClassId => "b3db72b6-f0cf-4234-be74-d98bd4c49797";

        public DrillableMorganite() : base("DrillableMorganite", LanguageSystem.Get("Morganite"), LanguageSystem.GetTooltip("Morganite"))
        {
            OnFinishedPatching += () =>
            {
                AUHandler.DrillableMorganiteTechType = TechType;
            };
        }

        protected override void ApplyChangesToPrefab(GameObject prefab)
        {
            prefab.EnsureComponent<Light>().color = new Color(1f, 0f, 1f);
            prefab.EnsureComponent<ResourceTracker>().overrideTechType = AUHandler.MorganiteTechType;
            var drillable = prefab.GetComponent<Drillable>();
            drillable.resources[0] = new Drillable.ResourceType() { chance = 1f, techType = AUHandler.MorganiteTechType };
            drillable.kChanceToSpawnResources = 1f;
            drillable.maxResourcesToSpawn = 2;
            Renderer[] renderers = prefab.GetComponentsInChildren<Renderer>();
            foreach(Renderer renderer in renderers)
            {
                renderer.material.SetTexture("_MainTex", Main.assetBundle.LoadAsset<Texture2D>("Morganite_diffuse"));
                renderer.material.SetTexture("_Illum", Main.assetBundle.LoadAsset<Texture2D>("Morganite_illum"));
                renderer.material.SetTexture("_SpecTex", Main.assetBundle.LoadAsset<Texture2D>("Morganite_spec"));
                renderer.material.SetColor("_GlowColor", new Color(3f, 1f, 3f));
                renderer.material.SetColor("_SpecColor", new Color(2f, 1.5f, 2f));
                renderer.material.SetFloat("_SpecInt", 2f);
                renderer.material.SetFloat("_Shininess", 7f);
            }
        }

        public override WorldEntityInfo EntityInfo => new()
        {
            cellLevel = LargeWorldEntity.CellLevel.Medium,
            classId = ClassID,
            localScale = Vector3.one,
            slotType = EntitySlot.Type.Medium,
            techType = TechType
        };

        public override List<LootDistributionData.BiomeData> BiomesToSpawnIn => new()
        {
            new()
            {
                biome = BiomeType.BloodKelp_CaveFloor,
                count = 1,
                probability = 0.2f
            },
            new()
            {
                biome = BiomeType.BloodKelp_TrenchFloor,
                count = 1,
                probability = 0.1f
            },
            new()
            {
                biome = BiomeType.BloodKelp_Floor,
                count = 1,
                probability = 0.05f
            },
            new()
            {
                biome = BiomeType.BonesField_Ceiling,
                count = 1,
                probability = 0.08f
            },
            new()
            {
                biome = BiomeType.BonesField_Ground,
                count = 1,
                probability = 0.045f
            },
            new()
            {
                biome = BiomeType.GhostTree_Grass,
                count = 1,
                probability = 0.1f
            },
            new()
            {
                biome = BiomeType.GhostTree_Ground,
                count = 1,
                probability = 0.06f
            },
            new()
            {
                biome = BiomeType.TreeCove_Ground,
                count = 1,
                probability = 0.09f
            },
            new()
            {
                biome = BiomeType.BonesField_Corridor_Ground,
                count = 1,
                probability = 0.075f
            },
            new()
            {
                biome = BiomeType.LostRiverCorridor_Ground,
                count = 1,
                probability = 0.09f
            },
            new()
            {
                biome = BiomeType.LostRiverJunction_Ground,
                count = 1,
                probability = 0.09f
            }
        };
    }
}
