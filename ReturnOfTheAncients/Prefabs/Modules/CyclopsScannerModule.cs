using ArchitectsLibrary.API;
using ArchitectsLibrary.Handlers;
using ArchitectsLibrary.Interfaces;
using RotA.Mono.Modules;
using SMLHelper.V2.Crafting;
using SMLHelper.V2.Handlers;
using UnityEngine;
using UnityEngine.UI;

namespace RotA.Prefabs.Modules
{
    public class CyclopsScannerModule : CyclopsUpgrade, ICyclopsOnModulesUpdated
    {
        public CyclopsScannerModule() : base("CyclopsScannerModule", LanguageSystem.Get("CyclopsScannerModule"), LanguageSystem.GetTooltip("CyclopsScannerModule"))
        {
            OnFinishedPatching += () =>
            {
                KnownTechHandler.SetAnalysisTechEntry(TechType, new TechType[0],
                    UnlockSprite: Mod.assetBundle.LoadAsset<Sprite>("AlienUpgrade_Popup"));
            };
        }

        public override TechCategory CategoryForPDA => TechCategory.VehicleUpgrades;

        public override TechGroup GroupForPDA => TechGroup.VehicleUpgrades;

        public override CraftTree.Type FabricatorType => CraftTree.Type.CyclopsFabricator;

        public override bool UnlockedAtStart => false;

        public void OnModuleCountChanged(SubRoot cyclops, int modulesCount)
        {
            var cameraButtonImage = GetCameraButtonImage(cyclops);
            if (_normalCameraButtonSprite == null)
            {
                _normalCameraButtonSprite = cameraButtonImage.sprite;
            }
            if (_scannerCameraButtonSprite == null)
            {
                _scannerCameraButtonSprite = Mod.assetBundle.LoadAsset<Sprite>("CyclopsScannerModuleButton");
            }

            if (modulesCount > 0)
            {
                cameraButtonImage.sprite = _scannerCameraButtonSprite;
                cyclops.gameObject.EnsureComponent<CyclopsScanner>();
            }
            else
            {
                cameraButtonImage.sprite = _normalCameraButtonSprite;
                Object.Destroy(cyclops.gameObject.GetComponent<CyclopsScanner>());
            }
        }

        static Image GetCameraButtonImage(SubRoot cyclops)
        {
            return cyclops.transform.Find("HelmHUD/Canvas_RightHUD/Abilities/Button_Camera").gameObject.GetComponent<Image>();
        }

        // the camera button icon that is used when this module is NOT installed
        static Sprite _normalCameraButtonSprite;
        // the camera button icon that is used when this module IS installed
        static Sprite _scannerCameraButtonSprite;

        protected override TechData GetBlueprintRecipe()
        {
            return new()
            {
                craftAmount = 1,
                Ingredients =
                {
                    new Ingredient(TechType.AdvancedWiringKit, 1),
                    new Ingredient(TechType.Polyaniline, 1),
                    new Ingredient(AUHandler.ReinforcedGlassTechType, 1)
                }
            };
        }
    }
}
