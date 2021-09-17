using ECCLibrary;
using RotA.Mono.AlienTech;
using UnityEngine;

namespace RotA.Prefabs.AlienBase
{
    public class VoidBaseModel : GenericWorldPrefab
    {
        public VoidBaseModel(string classId, string friendlyName, string description, GameObject model, UBERMaterialProperties materialProperties, LargeWorldEntity.CellLevel cellLevel) : base(classId, friendlyName, description, model, materialProperties, cellLevel, true, true)
        {
        }


        public override void CustomizePrefab()
        {
            prefab.EnsureComponent<VoidBaseReveal>();
        }
    }
}
