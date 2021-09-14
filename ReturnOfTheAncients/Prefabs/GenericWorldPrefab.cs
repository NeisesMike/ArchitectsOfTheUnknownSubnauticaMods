using ArchitectsLibrary.Utility;
using ECCLibrary;
using SMLHelper.V2.Assets;
using System.Collections;
using ArchitectsLibrary.MonoBehaviours;
using UnityEngine;

namespace RotA.Prefabs
{
    /// <summary>
    /// Made for Precursor base structures
    /// </summary>
    public class GenericWorldPrefab : Spawnable
    {
        private readonly GameObject _model;
        protected GameObject prefab;
        private readonly UBERMaterialProperties _materialProperties;
        private readonly LargeWorldEntity.CellLevel _cellLevel;
        private readonly bool _applyPrecursorMaterial;
        private readonly bool _addPrecursorTag;

        public GenericWorldPrefab(string classId, string friendlyName, string description, GameObject model, UBERMaterialProperties materialProperties, LargeWorldEntity.CellLevel cellLevel, bool addPrecursorTag, bool applyPrecursorMaterial = true) : base(classId, friendlyName, description)
        {
            this._model = model;
            this._materialProperties = materialProperties;
            this._cellLevel = cellLevel;
            this._applyPrecursorMaterial = applyPrecursorMaterial;
            this._addPrecursorTag = addPrecursorTag;
        }

        public virtual void CustomizePrefab()
        {

        }

        public override GameObject GetGameObject()
        {
            if (prefab == null)
            {
                prefab = GameObject.Instantiate(_model);
                prefab.SetActive(false);
                prefab.EnsureComponent<LargeWorldEntity>().cellLevel = _cellLevel;
                prefab.EnsureComponent<PrefabIdentifier>().classId = ClassID;
                prefab.EnsureComponent<TechTag>().type = TechType;
                prefab.EnsureComponent<SkyApplier>().renderers = prefab.GetComponentsInChildren<Renderer>();
                var rb = prefab.EnsureComponent<Rigidbody>();
                rb.mass = 10000f;
                rb.isKinematic = true;
                prefab.EnsureComponent<ImmuneToPropulsioncannon>();
                ECCHelpers.ApplySNShaders(prefab, _materialProperties);
                if (_applyPrecursorMaterial)
                {
                    MaterialUtils.ApplyPrecursorMaterials(prefab, _materialProperties.SpecularInt);
                }

                if (_addPrecursorTag)
                {
                    prefab.EnsureComponent<PrecursorObjectTag>();
                }
                CustomizePrefab();
                foreach (Collider col in prefab.GetComponentsInChildren<Collider>())
                {
                    col.gameObject.EnsureComponent<ConstructionObstacle>();
                }
            }
            return prefab;
        }

        public override IEnumerator GetGameObjectAsync(IOut<GameObject> gameObject)
        {
            gameObject.Set(GetGameObject());
            yield break;
        }
    }
}
