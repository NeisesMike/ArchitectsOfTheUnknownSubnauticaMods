using HarmonyLib;
using RotA.Mono.Creatures.GargEssentials;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;
using Logger = QModManager.Utility.Logger;

namespace RotA.Patches
{
    [HarmonyPatch(typeof(LiveMixin))]
    internal static class LiveMixin_Patches
    {
        // the LiveMixin.invincible isn't serialized, therefore, we cant assign it to something in the prefab
        // initialization, so we'd need to patch LiveMixin.Start so it gets assigned to true as soon as the component is active.
        [HarmonyPatch(nameof(LiveMixin.Start))]
        [HarmonyPostfix]
        private static void Start_Postfix(LiveMixin __instance)
        {
            var garg = __instance.GetComponent<GargantuanBehaviour>();

            if (garg)
                __instance.invincible = true;
        }


        // Makes invincible anything not get killed.
        [HarmonyPatch(nameof(LiveMixin.Kill))]
        [HarmonyPrefix]
        private static bool Kill_Prefix(LiveMixin __instance)
        {
            if (__instance.invincible)
                return false;

            return true;
        }

        // Patch to activate the green electricity effect of the Architect Electricity.
        [HarmonyPatch(nameof(LiveMixin.TakeDamage))]
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> TakeDamage_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> codes = new(instructions);
            
            // Get the MethodInfo of the method we wanna invoke in this patch
            var architectElectEffect = AccessTools.Method(typeof(LiveMixin_Patches), nameof(ArchitectElectEffect));

            bool found = false;


            /*
             * So what we basically want to do is insert the LiveMixin_Patches.ArchitectElectEffect() method call
             * above the ProfilingUtils.BeginSample("LiveMixin.TakeDamage.DamageEffect"); call.
             * to find that IL location, we can check for the "LiveMixin.TakeDamage.DamageEffect" string in the stack,
             * which is only used in the location we want.
             *
             * Since the LiveMixin_Patches.ArchitectElectEffect() method takes a LiveMixin we need to feed it that too,
             * It also needs to read the DamageType that was passed as an argument in the LiveMixin.TakeDamage() call,
             * so we need to feed it that as well.
             *
             * Note: how IL stack works for methods is that u first push the method arguments to the stack, and then
             * you do the method call.
             */
            for (int i = 0; i < codes.Count(); i++)
            {
                /*
                 * OpCodes.Ldstr is the opcode of string literals, so we first check for that opcode, then we check
                 * what the operand is assigned to.
                 *
                 * Just in case and to verify that it's in fact used as a method parameter, we check for the Opcodes.Call opcode.
                 */
                if (codes[i].opcode == OpCodes.Ldstr &&
                    (string)codes[i].operand == "LiveMixin.TakeDamage.DamageEffect" &&
                    codes[i + 1].opcode == OpCodes.Call)
                {
                    found = true;
                    
                    // we load the current LiveMixin instance to the stack
                    codes.Insert(i + 2, new CodeInstruction(OpCodes.Ldarg_0));
                    // then the DamageType parameter of LiveMixin.TakeDamage()
                    codes.Insert(i + 3, new CodeInstruction(OpCodes.Ldarg_3));
                    // and then we call our LiveMixin_Patches.ArchitectElectEffect() method.
                    codes.Insert(i + 4, new CodeInstruction(OpCodes.Call, architectElectEffect));
                    break;
                }
            }

            if (found is false)
                Logger.Log(Logger.Level.Error, "Cannot find LiveMixin.TakeDamage target location.", showOnScreen: true);
            else
                Logger.Log(Logger.Level.Debug, "LiveMixin.TakeDamage Transpiler Succeeded.");

            return codes.AsEnumerable();
        }

        // Yoinks the vanilla electrical damage effect and changes it's color to green.
        private static void ArchitectElectEffect(LiveMixin liveMixin, DamageType type)
        {
            if (Time.time > liveMixin.timeLastElecDamageEffect + 2.5f && type == Mod.ArchitectElect &&
                liveMixin.electricalDamageEffect is not null)
            {
                var fixedBounds = liveMixin.gameObject.GetComponent<FixedBounds>();
                Bounds bounds;
                if (fixedBounds is not null)
                    bounds = fixedBounds.bounds;
                else
                    bounds = UWE.Utils.GetEncapsulatedAABB(liveMixin.gameObject);

                var electricFX = Object.Instantiate(liveMixin.electricalDamageEffect);
                var renderers = electricFX.GetComponentsInChildren<Renderer>(true);
                foreach (var renderer in renderers)
                {
                    renderer.material.SetColor("_Color", Color.green);
                }

                var obj = UWE.Utils.InstantiateWrap(electricFX, bounds.center,
                    Quaternion.identity);
                obj.transform.parent = liveMixin.transform;
                obj.transform.localScale = bounds.size * 0.65f;
                liveMixin.timeLastElecDamageEffect = Time.time;
            }
        }
    }
}
