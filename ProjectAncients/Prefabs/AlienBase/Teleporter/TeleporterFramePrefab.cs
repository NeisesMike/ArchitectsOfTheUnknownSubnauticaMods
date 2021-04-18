﻿using SMLHelper.V2.Assets;
using UnityEngine;
using UWE;
using System.Collections;
using ECCLibrary;

namespace ProjectAncients.Prefabs.AlienBase
{
    public class TeleporterFramePrefab : Spawnable
    {
        const string referenceClassId = "f0429c44-a387-42e6-b621-74ba4dd8c2da";
        private string teleporterId;
        private Vector3 teleportPosition;
        private float teleportAngle;

        public TeleporterFramePrefab(string classId, string teleporterId, Vector3 teleportPosition, float teleportAngle) : base(classId, "", "")
        {
            this.teleporterId = teleporterId;
            this.teleportPosition = teleportPosition;
            this.teleportAngle = teleportAngle;
        }

#if SN1
        public override GameObject GetGameObject()
        {
            PrefabDatabase.TryGetPrefab(referenceClassId, out GameObject prefab);
            GameObject obj = GameObject.Instantiate(prefab);

            obj.SetActive(false);
            var teleporter = obj.GetComponent<PrecursorTeleporter>();
            teleporter.teleporterIdentifier = teleporterId;
            teleporter.warpToPos = teleportPosition;
            teleporter.warpToAngle = teleportAngle;
            obj.GetComponent<TechTag>().type = TechType.PrecursorTeleporter;

            return obj;
        }
#elif SN1_exp
        public override IEnumerator GetGameObjectAsync(IOut<GameObject> gameObject)
        {
            IPrefabRequest request = PrefabDatabase.GetPrefabAsync(referenceClassId);
            yield return request;
            request.TryGetPrefab(out GameObject prefab);
            GameObject obj = GameObject.Instantiate(prefab);
            obj.SetActive(false);
            var teleporter = obj.GetComponent<PrecursorTeleporter>();
            teleporter.teleporterIdentifier = teleporterId;
            teleporter.warpToPos = teleportPosition;
            teleporter.warpToAngle = teleportAngle;
            obj.GetComponent<TechTag>().type = TechType.PrecursorTeleporter;

            gameObject.Set(obj);
        }
#endif

        public override WorldEntityInfo EntityInfo => new WorldEntityInfo()
        {
            classId = ClassID,
            cellLevel = LargeWorldEntity.CellLevel.Medium,
            localScale = Vector3.one,
            slotType = EntitySlot.Type.Large,
            techType = TechType.PrecursorTeleporter
        };
    }
}
