using System.Collections.Generic;
using ArchitectsLibrary.Interfaces;
using HarmonyLib;
using UnityEngine;

namespace ArchitectsLibrary.Patches
{
    internal class UpgradeConsolePatches
    {
        internal static readonly Dictionary<TechType, ICyclopsOnModulesUpdated> CyclopsOnModulesUpdated = new();

        internal static void Patch(Harmony harmony)
        {            
            harmony.Patch(AccessTools.Method(typeof(SubRoot), nameof(SubRoot.SetCyclopsUpgrades)),
                postfix: new HarmonyMethod(AccessTools.Method(typeof(UpgradeConsolePatches), nameof(SetCyclopsUpgrades_Postfix))));
        }

        static void SetCyclopsUpgrades_Postfix(SubRoot __instance)
        {
            if (__instance.upgradeConsole != null && __instance.live.IsAlive())
            {
                foreach (var pair in CyclopsOnModulesUpdated)
                {
                    var modulesCount = __instance.upgradeConsole.modules.GetCount(pair.Key);
                    pair.Value.OnModuleCountChanged(__instance, modulesCount);
                }
            }

        }
    }
}