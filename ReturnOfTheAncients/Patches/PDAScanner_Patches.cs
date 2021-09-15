using HarmonyLib;
using Story;
using UnityEngine;

namespace RotA.Patches
{
    [HarmonyPatch(typeof(PDAScanner))]
    public class PDAScanner_Patches
    {
        public static StoryGoal scanAdultGargGoal = new StoryGoal("ScanAdultGargantuan", Story.GoalType.Story, 0f);

        // checks if the scan time is too long to be scanned without a special tool
        [HarmonyPatch(nameof(PDAScanner.CanScan))]
        [HarmonyPostfix]
        public static bool CanScanPrefix(ref PDAScanner.Result __result)
        {
            if (PDAScanner.scanTarget.progress > 0.1f) // if you've already started scanning this object with a special tool, no need to check again
            {
                return true; // do run the original method
            }
            var entryData = PDAScanner.GetEntryData(PDAScanner.scanTarget.techType);
            if (entryData.scanTime >= Mod.kHandheldScannerScanTimeLimit)
            {
                __result = Mod.SizeLimitScanResult;
                return false;
            }

            return true;
        }
        
        [HarmonyPatch(nameof(PDAScanner.Unlock))]
        [HarmonyPostfix]
        public static void UnlockPostfix(PDAScanner.EntryData entryData)
        {
            if (entryData is not null && entryData.key == Mod.gargVoidPrefab.TechType)
            {
                if (StoryGoalManager.main.OnGoalComplete(scanAdultGargGoal.key) && !uGUI.isLoading)
                {
                    CustomPDALinesManager.PlayPDAVoiceLine(Mod.assetBundle.LoadAsset<AudioClip>("PDAGargScan"), "PDAScanAdultGargantuan");
                }
            }
        }
    }
}
