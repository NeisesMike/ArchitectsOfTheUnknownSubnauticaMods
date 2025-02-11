using System.Collections;
using ArchitectsLibrary.API;
using ArchitectsLibrary.Utility;
using ArchitectsLibrary.Handlers;
using FMODUnity;
using SMLHelper.V2.Assets;
using SMLHelper.V2.Crafting;
using SMLHelper.V2.Handlers;
using SMLHelper.V2.Utility;
using UnityEngine;

namespace ArchitectsLibrary.Buildables
{
    class PrecursorFabricator : Buildable
    {
        GameObject _processedPrefab;
        internal CraftTree.Type TreeType { get; private set; }
        internal ModCraftTreeRoot Root { get; private set; }
        Atlas.Sprite sprite;

        public PrecursorFabricator()
            : base("PrecursorFabricator", LanguageSystem.Get("PrecursorFabricator"), LanguageSystem.GetTooltip("PrecursorFabricator"))
        {
            OnFinishedPatching += () =>
            {
                Root = CraftTreeHandler.CreateCustomCraftTreeAndType(ClassID, out CraftTree.Type craftTreeType);
                TreeType = craftTreeType;
            };
        }

        public override TechCategory CategoryForPDA { get; } = TechCategory.InteriorModule;
        public override TechGroup GroupForPDA { get; } = TechGroup.InteriorModules;

        public override bool UnlockedAtStart { get; } = false;

        protected override Atlas.Sprite GetItemSprite()
        {
            return sprite ??= new Atlas.Sprite(Main.assetBundle.LoadAsset<Sprite>("PrecFabricator_Icon"));
        }

#if SN1
        public override GameObject GetGameObject()
        {
            if (_processedPrefab != null)
            {
                var go = Object.Instantiate(_processedPrefab);
                go.SetActive(true);
                return go;
            }
            
            var prefab = Main.fabBundle.LoadAsset<GameObject>("PrecursorFabricator");

            var obj = new GameObject("PrecursorFabricator");
            
            var model = Object.Instantiate(prefab, obj.transform, true);
            model.name = "model";
            model.transform.localPosition = Vector3.zero;

            var sa = obj.EnsureComponent<SkyApplier>();
            sa.renderers = model.GetAllComponentsInChildren<Renderer>();
            sa.anchorSky = Skies.Auto;
            
            MaterialUtils.ApplySNShaders(model);
            MaterialUtils.ApplyPrecursorMaterials(model, 6f);

            obj.EnsureComponent<TechTag>().type = TechType;
            obj.EnsureComponent<PrefabIdentifier>().ClassId = ClassID;

            Rigidbody body = obj.AddComponent<Rigidbody>();
            body.isKinematic = true;

            obj.EnsureComponent<LargeWorldEntity>().cellLevel = LargeWorldEntity.CellLevel.Medium;

            Constructable con = obj.AddComponent<Constructable>();
            con.techType = TechType;
            con.allowedInBase = true;
            con.allowedInSub = true;
            con.allowedOnWall = true;
            con.allowedOutside = false;
            con.allowedOnCeiling = false;
            con.allowedOnGround = false;
            con.allowedOnConstructables = true;
            con.model = model;
            con.rotationEnabled = false;
            obj.EnsureComponent<ConstructableBounds>();
            
            var fabObj = CraftData.GetPrefabForTechType(TechType.Fabricator);
            Fabricator originalFab = fabObj.GetComponent<Fabricator>();

            CrafterLogic logic = obj.EnsureComponent<CrafterLogic>();
            var fab = obj.EnsureComponent<MonoBehaviours.PrecursorFabricator>();
            fab.animator = obj.GetComponentInChildren<Animator>();
            fab.animator.runtimeAnimatorController = originalFab.animator.runtimeAnimatorController;
            fab.animator.avatar = originalFab.animator.avatar;
            fab.sparksPS = originalFab.sparksPS;
            fab.craftTree = TreeType;
            fab.spawnAnimationDelay = 4f;
            
            var workbenchObj = CraftData.GetPrefabForTechType(TechType.Workbench);
            var originalWorkbench = workbenchObj.GetComponent<Workbench>();

            Material beamMat = new Material(originalWorkbench.fxLaserBeam[0].GetComponent<MeshRenderer>().material);
            beamMat.color = Color.green;

            // beams setup
            fab.leftBeam = model.transform.Find("FabricatorMain/submarine_fabricator_01/fabricator/overhead/printer_left/fabricatorBeam").gameObject;
            fab.leftBeam.transform.localPosition = new(-0.15f, 0f, 0.01f);
            fab.leftBeam.transform.localScale = new(0.5f, 0.5f, 0.55f);
            fab.leftBeam.transform.localEulerAngles = new(345f, 270.00f, 0f);
            fab.leftBeam.GetComponent<MeshRenderer>().material = beamMat;
            fab.rightBeam = model.transform.Find("FabricatorMain/submarine_fabricator_01/fabricator/overhead/printer_right/fabricatorBeam 1").gameObject;
            fab.rightBeam.transform.localPosition = new(0.15f, 0f, -0.01f);
            fab.rightBeam.transform.localScale = new(0.5f, 0.5f, 0.55f);
            fab.rightBeam.transform.localEulerAngles = new(15f, 90f, 180f);
            fab.rightBeam.GetComponent<MeshRenderer>().material = beamMat;

            fab.animator.SetBool(AnimatorHashID.open_fabricator, false);
            fab.crafterLogic = logic;
            fab.ghost = obj.EnsureComponent<CrafterGhostModel>();
            fab.ghost.itemSpawnPoint = model.transform.Find("FabricatorMain/submarine_fabricator_01/fabricator/printBed/spawnPoint");

            // sounds setup
            fab.openSound = ScriptableObject.CreateInstance<FMODAsset>();
            fab.openSound.path = "event:/player/key terminal_open";
            fab.closeSound = ScriptableObject.CreateInstance<FMODAsset>();
            fab.closeSound.path = "event:/player/key terminal_close";
            fab.soundOrigin = fab.transform;
            fab.fabricateSound = model.AddComponent<StudioEventEmitter>();
            fab.fabricateSound.Event = "event:/env/antechamber_scan_loop";

            //particles recolors
            var particleSystems = obj.GetComponentsInChildren<ParticleSystemRenderer>(true);
            foreach(ParticleSystemRenderer ps in particleSystems)
            {
                ps.material.SetColor(ShaderPropertyID._Color, new Color(0f, 1.5f, 0f));
            }

            fab.fabLight = model.transform.Find("FabLight").GetComponent<Light>();
            fab.fabLight.color = Color.green;
            fab.fabLight.shadows = LightShadows.Hard;
            fab.fabLight.range = 1.25f;
            fab.fabLight.intensity = 1.37f;
            var lightAnimator = fab.fabLight.gameObject.EnsureComponent<LightAnimator>();
            lightAnimator.flicker = new LightAnimator.FlickerParameters()
            {
                maxIntensity = 1f,
                minIntensity = 0f,
                minTime = 0f,
                maxTime = 0.03f
            };
            fab.fabLight.transform.localPosition = new Vector3(0f, -0.22f, 0.8f);

            fab.handOverText = LanguageSystem.Get("UseText_PrecursorFabricator");

            fab.powerRelay = PowerSource.FindRelay(fab.transform);
            
            _processedPrefab = Object.Instantiate(obj);
            _processedPrefab.SetActive(false);

            obj.SetActive(true);
            return obj;
        }
#else
        public override IEnumerator GetGameObjectAsync(IOut<GameObject> gameObject)
        {
            if (_processedPrefab != null)
            {
                var go = Object.Instantiate(_processedPrefab);
                go.SetActive(true);
                gameObject.Set(go);
            }
            
            var prefab = Main.fabBundle.LoadAsset<GameObject>("PrecursorFabricator");

            var obj = new GameObject("PrecursorFabricator");
            
            var model = Object.Instantiate(prefab, obj.transform, true);
            model.name = "model";
            model.transform.localPosition = Vector3.zero;

            var sa = obj.EnsureComponent<SkyApplier>();
            sa.renderers = model.GetAllComponentsInChildren<Renderer>();
            sa.anchorSky = Skies.Auto;
            
            MaterialUtils.ApplySNShaders(model);
            MaterialUtils.ApplyPrecursorMaterials(model, 6f);

            obj.EnsureComponent<TechTag>().type = TechType;
            obj.EnsureComponent<PrefabIdentifier>().ClassId = ClassID;

            Rigidbody body = obj.AddComponent<Rigidbody>();
            body.isKinematic = true;

            obj.EnsureComponent<LargeWorldEntity>().cellLevel = LargeWorldEntity.CellLevel.Medium;

            Constructable con = obj.AddComponent<Constructable>();
            con.techType = TechType;
            con.allowedInBase = true;
            con.allowedInSub = true;
            con.allowedOnWall = true;
            con.allowedOutside = false;
            con.allowedOnCeiling = false;
            con.allowedOnGround = false;
            con.allowedOnConstructables = true;
            con.model = model;
            con.rotationEnabled = false;
            obj.EnsureComponent<ConstructableBounds>();

            var task = CraftData.GetPrefabForTechTypeAsync(TechType.Fabricator);
            yield return task;
            var fabObj = task.GetResult();
            Fabricator originalFab = fabObj.GetComponent<Fabricator>();

            CrafterLogic logic = obj.EnsureComponent<CrafterLogic>();
            var fab = obj.EnsureComponent<MonoBehaviours.PrecursorFabricator>();
            fab.animator = obj.GetComponentInChildren<Animator>();
            fab.animator.runtimeAnimatorController = originalFab.animator.runtimeAnimatorController;
            fab.animator.avatar = originalFab.animator.avatar;
            fab.sparksPS = originalFab.sparksPS;
            fab.craftTree = TreeType;
            fab.spawnAnimationDelay = 4f;

            var task2 = CraftData.GetPrefabForTechTypeAsync(TechType.Workbench);
            yield return task2;
            var workbenchObj = task2.GetResult();
            var originalWorkbench = workbenchObj.GetComponent<Workbench>();

            Material beamMat = new Material(originalWorkbench.fxLaserBeam[0].GetComponent<MeshRenderer>().material);
            beamMat.color = Color.green;

            // beams setup
            fab.leftBeam = model.transform.Find("FabricatorMain/submarine_fabricator_01/fabricator/overhead/printer_left/fabricatorBeam").gameObject;
            fab.leftBeam.transform.localPosition = new(-0.15f, 0f, 0.01f);
            fab.leftBeam.transform.localScale = new(0.5f, 0.5f, 0.55f);
            fab.leftBeam.transform.localEulerAngles = new(345f, 270.00f, 0f);
            fab.leftBeam.GetComponent<MeshRenderer>().material = beamMat;
            fab.rightBeam = model.transform.Find("FabricatorMain/submarine_fabricator_01/fabricator/overhead/printer_right/fabricatorBeam 1").gameObject;
            fab.rightBeam.transform.localPosition = new(0.15f, 0f, -0.01f);
            fab.rightBeam.transform.localScale = new(0.5f, 0.5f, 0.55f);
            fab.rightBeam.transform.localEulerAngles = new(15f, 90f, 180f);
            fab.rightBeam.GetComponent<MeshRenderer>().material = beamMat;
            
            fab.animator.SetBool(AnimatorHashID.open_fabricator, false);
            fab.crafterLogic = logic;
            fab.ghost = obj.EnsureComponent<CrafterGhostModel>();
            fab.ghost.itemSpawnPoint = model.transform.Find("FabricatorMain/submarine_fabricator_01/fabricator/printBed/spawnPoint");

            // sounds setup
            fab.openSound = ScriptableObject.CreateInstance<FMODAsset>();
            fab.openSound.path = "event:/player/key terminal_open";
            fab.closeSound = ScriptableObject.CreateInstance<FMODAsset>();
            fab.closeSound.path = "event:/player/key terminal_close";
            fab.soundOrigin = fab.transform;
            fab.fabricateSound = model.AddComponent<StudioEventEmitter>();
            fab.fabricateSound.Event = "event:/env/antechamber_scan_loop";

            //particles recolors
            var particleSystems = obj.GetComponentsInChildren<ParticleSystemRenderer>(true);
            foreach (ParticleSystemRenderer ps in particleSystems)
            {
                ps.material.SetColor(ShaderPropertyID._Color, new Color(0f, 1.5f, 0f));
            }

            fab.fabLight = model.transform.Find("FabLight").GetComponent<Light>();
            fab.fabLight.color = Color.green;
            fab.fabLight.shadows = LightShadows.Hard;
            fab.fabLight.range = 1.25f;
            fab.fabLight.intensity = 1.37f;
            var lightAnimator = fab.fabLight.gameObject.EnsureComponent<LightAnimator>();
            lightAnimator.flicker = new LightAnimator.FlickerParameters()
            {
                maxIntensity = 1f,
                minIntensity = 0f,
                minTime = 0f,
                maxTime = 0.03f
            };
            fab.fabLight.transform.localPosition = new Vector3(0f, -0.22f, 0.8f);

            fab.handOverText = LanguageSystem.Get("UseText_PrecursorFabricator");

            fab.powerRelay = PowerSource.FindRelay(fab.transform);
            
            _processedPrefab = Object.Instantiate(obj);
            _processedPrefab.SetActive(false);

            obj.SetActive(true);
            gameObject.Set(obj);
        }
#endif

        protected override TechData GetBlueprintRecipe()
        {
            return new()
            {
                craftAmount = 1,
                Ingredients =
                {
                    new Ingredient(AUHandler.PrecursorAlloyIngotTechType, 2), new Ingredient(AUHandler.EmeraldTechType, 1), new Ingredient(AUHandler.SapphireTechType, 1)
                }
            };
        }
    }
}