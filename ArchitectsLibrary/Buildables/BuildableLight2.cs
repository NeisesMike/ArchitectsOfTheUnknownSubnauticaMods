﻿using ArchitectsLibrary.API;
using UnityEngine;
using ArchitectsLibrary.Utility;

namespace ArchitectsLibrary.Buildables
{
    class BuildableLight2 : GenericPrecursorDecoration
    {
        public BuildableLight2() : base("BuildablePrecursorLight2", LanguageSystem.Get("BuildablePrecursorLight2"), LanguageSystem.GetTooltip("BuildablePrecursorLight2"))
        {
        }

        protected override ConstructableSettings GetConstructableSettings => new ConstructableSettings(true, true, true, false, true, true, true, placeDefaultDistance: 2f, placeMinDistance: 2f, placeMaxDistance: 10f);

        protected override OrientedBounds[] GetBounds => new OrientedBounds[] { new OrientedBounds(new Vector3(0f, 0.5f, 0f), Quaternion.identity, new Vector3(0.4f, 0.4f, 0.4f)) };

        protected override string GetOriginalClassId => "6a02aa5c-8d4d-4801-aad4-ea61dccddae5";

        protected override void EditPrefab(GameObject prefab)
        {
            prefab.transform.GetChild(0).transform.localPosition = new Vector3(0f, -0.43f, 0f);
            prefab.GetComponentInChildren<SphereCollider>().radius = 6f;
        }

        protected override bool ExteriorOnly => false;

        protected override string GetSpriteName => "Light2";
    }
}
