using HarmonyLib;
using UnityEngine;

namespace RotA.Patches
{
    [HarmonyPatch(typeof(ScannerTool))]
    public class ScannerTool_Patches
    {
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
    }
}
