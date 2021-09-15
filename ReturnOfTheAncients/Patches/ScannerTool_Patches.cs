using HarmonyLib;
using UnityEngine;

namespace RotA.Patches
{
    [HarmonyPatch(typeof(ScannerTool_Patches))]
    public class ScannerTool_Patches
    {
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
    }
}
