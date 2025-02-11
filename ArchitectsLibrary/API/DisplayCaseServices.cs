﻿using System.Collections.Generic;
using ArchitectsLibrary.MonoBehaviours;
using UnityEngine;

namespace ArchitectsLibrary.API
{
    /// <summary>
    /// Use this to allow/disallow items from your mod to be placed in Architect Library's display buildables (currently just relic tanks and pedestals).
    /// </summary>
    public static class DisplayCaseServices
    {
        internal static readonly List<TechType> WhitelistedTechTypes = new();
        internal static readonly List<TechType> BlacklistedTechTypes = new();
        
        static readonly Dictionary<TechType, float> overrideItemScaleInRelicTank = new();
        static readonly Dictionary<TechType, float> overrideItemScaleInPedestal = new();
        static readonly Dictionary<TechType, float> overrideItemScaleInSpecimenCase = new();
        static readonly Dictionary<TechType, Vector3> overrideItemOffset = new();
        static readonly Dictionary<TechType, Vector3> overrideRelicTankRotations = new();

        /// <summary>
        /// Allows the passed TechType to be added into relic tanks and onto pedestals.
        /// </summary>
        /// <param name="techType">the TechType to allow</param>
        public static void WhitelistTechType(TechType techType)
        {
            WhitelistedTechTypes.Add(techType);
            if (BlacklistedTechTypes.Contains(techType))
            {
                BlacklistedTechTypes.Remove(techType);
            }
        }

        /// <summary>
        /// Disallow <paramref name="techType"/> from being added into relic tanks and onto pedestals. This would rarely be used, given most troublesome items are already whitelisted, but is here just in case.
        /// </summary>
        /// <param name="techType"></param>
        public static void BlackListTechType(TechType techType)
        {
            BlacklistedTechTypes.Add(techType);
            if (WhitelistedTechTypes.Contains(techType))
            {
                WhitelistedTechTypes.Remove(techType);
            }
        }

        /// <summary>
        /// Sets the euler rotation of <paramref name="techType"/> when put in a small or large display case (does not affect the small specimen cases). Default rotation is <see cref="Vector3.zero"/>.
        /// </summary>
        /// <param name="techType">Tech type to set the rotation of</param>
        /// <param name="newRotation">The new rotation in euler angles. The Y rotation will be overriden automatically because the object spins.</param>
        public static void SetRotationInRelicTank(TechType techType, Vector3 newRotation)
        {
            overrideRelicTankRotations[techType] = newRotation;
        }

        /// <summary>
        /// Set the scale of <paramref name="techType"/> when put in a display case. Default scale is 1.25f.
        /// </summary>
        /// <param name="techType"></param>
        /// <param name="newScale"></param>
        public static void SetScaleInRelicTank(TechType techType, float newScale)
        {
            overrideItemScaleInRelicTank[techType] = newScale;
        }

        /// <summary>
        /// Set the scale of <paramref name="techType"/> when put on top of an item pedestal. Default scale is 1.
        /// </summary>
        /// <param name="techType"></param>
        /// <param name="newScale"></param>
        public static void SetScaleInPedestal(TechType techType, float newScale)
        {
            overrideItemScaleInPedestal[techType] = newScale;
        }

        /// <summary>
        /// Set th
        /// </summary>
        /// <param name="techType"></param>
        /// <param name="offset"></param>
        public static void SetOffset(TechType techType, Vector3 offset)
        {
            overrideItemOffset[techType] = offset;
        }

        /// <summary>
        /// Set the scale of <paramref name="techType"/> when put in a specimen case. Default scale is 0.25f.
        /// </summary>
        /// <param name="techType"></param>
        /// <param name="newScale"></param>
        public static void SetScaleInSpecimenCase(TechType techType, float newScale)
        {
            overrideItemScaleInSpecimenCase[techType] = newScale;
        }

        internal static float GetScaleForItem(TechType techType, DisplayCaseType displayCaseType) => displayCaseType switch
        {
            DisplayCaseType.RelicTank => overrideItemScaleInRelicTank.GetOrDefault(techType, 1.25f),
            DisplayCaseType.Pedestal => overrideItemScaleInPedestal.GetOrDefault(techType, 1f),
            DisplayCaseType.SpecimenCase => overrideItemScaleInSpecimenCase.GetOrDefault(techType, 0.75f),
            _ => 1f
        };

        internal static Vector3 GetOffsetForItem(TechType techType) => overrideItemOffset.GetOrDefault(techType, Vector3.zero);

        internal static Vector3 GetRotationForItem(TechType techType, DisplayCaseType displayCaseType)
        {
            if (displayCaseType == DisplayCaseType.RelicTank)
            {
                return overrideRelicTankRotations.GetOrDefault(techType, Vector3.zero);
            }
            return Vector3.zero;
        }
    }
}
