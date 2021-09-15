using HarmonyLib;
using UnityEngine;

namespace RotA.Patches
{
    [HarmonyPatch(typeof(ScannerTool))]
    public class ScannerTool_Patches
    {
        /*
        // implements a case for when the Scanner Tool's screen state is our custom state (the one for the garg)
        [HarmonyPatch(nameof(ScannerTool.UpdateScreen))]
        [HarmonyPostfix]
        public static void UpdateScreenPostfix(ScannerTool __instance, ScannerTool.ScreenState state)
        {
            if (state != Mod.SizeLimitScannerScreenState) return;
            
            __instance.screenDefaultText.text = Language.main.Get("ScannerScreenSizeLimit");
            __instance.screenDefaultText.color = new Color(255f, 0f, 0f, 255f);
            __instance.screenAnimator.pulse = true;
            __instance.screenAnimator.pulseFrequency = 5f;
            __instance.screenAnimator.pulseMin = 0.1f;
            __instance.screenAnimator.pulseMax = 1f;
        }
        
        // the following code sets the "stateCurrent" of the Scanner Tool to warning you for "Size Limit", if the result from PDAScanner.Scan is "Size Limit"
        [HarmonyPatch(nameof(ScannerTool.Scan))]
        [HarmonyPostfix]
        public static void ScanPostfix(ScannerTool __instance, ref PDAScanner.Result __result)
        {
            if (__result == Mod.SizeLimitScanResult)
            {
                __instance.stateCurrent = Mod.SizeLimitScanState;
            }
        }
        
        // the following code calls "UpdateScreen" if the ScannerTool's current state is "Size Limit"
        [HarmonyPatch(nameof(ScannerTool.OnHover))]
        [HarmonyPostfix]
        public static void OnHoverPostfix(ScannerTool __instance)
        {
            if (__instance.stateCurrent == Mod.SizeLimitScanState)
            {
                __instance.UpdateScreen(Mod.SizeLimitScannerScreenState);
            }
        }
        */
    }
}
