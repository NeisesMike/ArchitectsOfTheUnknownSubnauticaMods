using ArchitectsLibrary.API;
using ArchitectsLibrary.Interfaces;
using ProjectAncients.Mono.Modules;
using SMLHelper.V2.Crafting;
using SMLHelper.V2.Handlers;
using SMLHelper.V2.Utility;
using UnityEngine;

namespace ProjectAncients.Prefabs.Modules
{
    public class SeamothElectricalDefenseMK2 : SeaMothUpgrade, ISeaMothOnUse
    {
        public SeamothElectricalDefenseMK2()
            : base("SeamothElectricalDefenseMK2", "Ion Perimeter Defense System",
                "Generates a powerful ionic energy field designed to ward off large aggressive fauna. Doesn't stack.")
        {}
        public override QuickSlotType QuickSlotType => QuickSlotType.SelectableChargeable;
        public override TechType ModelTemplate => TechType.SeamothElectricalDefense;
        public override float? MaxCharge => 30f;
        public override float? EnergyCost => 5f;
        public override CraftTree.Type FabricatorType => CraftTree.Type.Workbench;
        public override string[] StepsToFabricatorTab => new string[] { "SeamothMenu" };
        public override TechCategory CategoryForPDA => TechCategory.VehicleUpgrades;
        public override TechGroup GroupForPDA => TechGroup.VehicleUpgrades;
        public override TechType RequiredForUnlock => Mod.architectElectricityMasterTech;

        #region Interface Implementation

        public float Cooldown => 5f;
        public void OnUpgradeUse(int slotID, SeaMoth seaMoth)
        {
            var obj = Object.Instantiate(seaMoth.seamothElectricalDefensePrefab);
            obj.name = "ElectricalDefenseMK2";

            var ed = obj.GetComponent<ElectricalDefense>() ?? obj.GetComponentInParent<ElectricalDefense>();
            if (ed is not null)
            {
                Object.Destroy(ed);
            }

            var edMk2 = obj.EnsureComponent<ElectricalDefenseMK2>();
            if (edMk2 is not null)
            {
                edMk2.fxElectSpheres = seaMoth.seamothElectricalDefensePrefab.GetComponent<ElectricalDefense>().fxElecSpheres;
                edMk2.defenseSound = seaMoth.seamothElectricalDefensePrefab.GetComponent<ElectricalDefense>().defenseSound;
            }

            float charge = seaMoth.quickSlotCharge[slotID];
            float slotCharge = seaMoth.GetSlotCharge(slotID);

            var electricalDefense = Utils
                .SpawnZeroedAt(obj, seaMoth.transform)
                .GetComponent<ElectricalDefenseMK2>();
            
            if (electricalDefense is not null)
            {
                electricalDefense.charge = charge;
                electricalDefense.chargeScalar = slotCharge;
            }
        }

        #endregion
        
        protected override TechData GetBlueprintRecipe()
        {
            return new TechData()
            {
                craftAmount = 1,
                Ingredients =
                {
                    new Ingredient(TechType.SeamothElectricalDefense, 1),
                    new Ingredient(TechType.AdvancedWiringKit, 2),
                    new Ingredient(TechType.PrecursorIonCrystal, 1),
                }
            };
        }

        protected override Atlas.Sprite GetItemSprite()
        {
            return ImageUtils.LoadSpriteFromTexture(Mod.assetBundle.LoadAsset<Texture2D>("SeamothElectricalDefenseMk2"));
        }
    }
}
