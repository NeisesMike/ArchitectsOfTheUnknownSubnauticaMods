﻿using ArchitectsLibrary.API;
using RotA.Mono.AlienTech;
using SMLHelper.V2.Assets;
using UnityEngine;
using UWE;

namespace RotA.Prefabs.AlienBase
{
    public class InfectionTesterTerminal : Spawnable
    {
        GameObject _processedPrefab;
        const string baseClassId = "b1f54987-4652-4f62-a983-4bf3029f8c5b";

        public InfectionTesterTerminal(string classId)
            : base(classId, LanguageSystem.Get("InfectionTesterTerminal"), LanguageSystem.Default)
        {
        }

        public override WorldEntityInfo EntityInfo => new WorldEntityInfo()
        {
            classId = ClassID,
            cellLevel = LargeWorldEntity.CellLevel.Medium,
            localScale = Vector3.one,
            slotType = EntitySlot.Type.Large,
            techType = this.TechType
        };
#if SN1
        public override GameObject GetGameObject()
        {
            if (_processedPrefab)
            {
                _processedPrefab.SetActive(true);
                return _processedPrefab;
            }
            
            PrefabDatabase.TryGetPrefab(baseClassId, out GameObject prefab);
            GameObject obj = GameObject.Instantiate(prefab);
            PrecursorDisableGunTerminal disableGun = obj.GetComponentInChildren<PrecursorDisableGunTerminal>(true);
            DisableEmissiveOnStoryGoal disableEmissive = obj.GetComponent<DisableEmissiveOnStoryGoal>();
            if (disableEmissive)
            {
                Object.DestroyImmediate(disableEmissive);
            }
            var openDoor = disableGun.gameObject.AddComponent<InfectionTesterOpenDoor>();
            openDoor.glowMaterial = disableGun.glowMaterial;
            openDoor.glowRing = disableGun.glowRing;
            openDoor.useSound = disableGun.useSound;
            openDoor.openLoopSound = disableGun.openLoopSound;
            openDoor.curedUseSound = disableGun.curedUseSound;
            openDoor.accessGrantedSound = disableGun.accessGrantedSound;
            openDoor.accessDeniedSound = disableGun.accessDeniedSound;
            openDoor.cinematic = disableGun.cinematic;
            openDoor.onPlayerCuredGoal = disableGun.onPlayerCuredGoal;
            Object.DestroyImmediate(disableGun);

            var triggerArea_old = obj.GetComponentInChildren<PrecursorDisableGunTerminalArea>();
            var triggerArea = triggerArea_old.gameObject.AddComponent<InfectionTesterTriggerArea>();
            Object.DestroyImmediate(triggerArea_old);
            triggerArea.terminal = openDoor;
            obj.SetActive(false);

            _processedPrefab = GameObject.Instantiate(obj);
            _processedPrefab.SetActive(false);
            return obj;
        }
#else
        public override IEnumerator GetGameObjectAsync(IOut<GameObject> gameObject)
        {
            if (_processedPrefab)
            {
                _processedPrefab.SetActive(true);
                gameObject.Set(_processedPrefab);
                yield break;
            }

            IPrefabRequest request = PrefabDatabase.GetPrefabAsync(baseClassId);
            yield return request;
            request.TryGetPrefab(out GameObject prefab);
            GameObject obj = GameObject.Instantiate(prefab);
            PrecursorDisableGunTerminal disableGun = obj.GetComponentInChildren<PrecursorDisableGunTerminal>(true);
            DisableEmissiveOnStoryGoal disableEmissive = obj.GetComponent<DisableEmissiveOnStoryGoal>();
            if (disableEmissive)
            {
                Object.DestroyImmediate(disableEmissive);
            }
            var openDoor = disableGun.gameObject.AddComponent<InfectionTesterOpenDoor>();
            openDoor.glowMaterial = disableGun.glowMaterial;
            openDoor.glowRing = disableGun.glowRing;
            openDoor.useSound = disableGun.useSound;
            openDoor.openLoopSound = disableGun.openLoopSound;
            openDoor.curedUseSound = disableGun.curedUseSound;
            openDoor.accessGrantedSound = disableGun.accessGrantedSound;
            openDoor.accessDeniedSound = disableGun.accessDeniedSound;
            openDoor.cinematic = disableGun.cinematic;
            openDoor.onPlayerCuredGoal = disableGun.onPlayerCuredGoal;
            Object.DestroyImmediate(disableGun);

            var triggerArea_old = obj.GetComponentInChildren<PrecursorDisableGunTerminalArea>();
            var triggerArea = triggerArea_old.gameObject.AddComponent<InfectionTesterTriggerArea>();
            Object.DestroyImmediate(triggerArea_old);
            triggerArea.terminal = openDoor;
            obj.SetActive(false);
            
            _processedPrefab = GameObject.Instantiate(obj);
            _processedPrefab.SetActive(false);
            gameObject.Set(obj);
        }
#endif
    }
}
