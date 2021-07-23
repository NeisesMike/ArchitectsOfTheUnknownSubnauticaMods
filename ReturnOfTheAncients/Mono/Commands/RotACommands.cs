﻿using RotA.Mono.Creatures.GargEssentials;
using RotA.Mono.Cinematics;
using UnityEngine;

namespace RotA.Mono.Commands
{
    public class RotACommands : MonoBehaviour
    {
        void Start()
        {
            DevConsole.RegisterConsoleCommand(this, "togglecinematic", false, true);
            DevConsole.RegisterConsoleCommand(this, "sunbeamgarg", false, true);
            DevConsole.RegisterConsoleCommand(this, "secretbasecutscene", false, true);
        }

        private void OnConsoleCommand_sunbeamgarg(NotificationCenter.Notification n)
        {
            new GameObject("SunbeamGargController").AddComponent<SunbeamGargController>();
        }

        private void OnConsoleCommand_secretbasecutscene(NotificationCenter.Notification n)
        {
            new GameObject("SecretBaseGargController").AddComponent<SecretBaseGargController>();
        }

        private void OnConsoleCommand_togglecinematic(NotificationCenter.Notification n)
        {
            GameObject[] gameObjects = Object.FindObjectsOfType<GameObject>();
            foreach (GameObject go in gameObjects)
            {
                if (ECCLibrary.ECCHelpers.CompareStrings(go.name, "GargantuanVoid(Clone)", ECCLibrary.ECCStringComparison.Equals))
                {
                    go.FindChild("AdultGargModel").SetActive(false);
                    GameObject trollFace = go.FindChild("TrollFace");
                    trollFace.SetActive(true);
                    trollFace.EnsureComponent<TrollFaceTracker>().enabled = true;
                    GargantuanRoar roar = go.GetComponent<GargantuanRoar>();
                    if (roar != null)
                    {
                        roar.audioSource.enabled = false;
                        roar.enabled = false;
                    }
                    go.EnsureComponent<TrollVoice>().enabled = true;
                    go.name = "GargantuanVoidTroll";
                    continue;
                }
                if (ECCLibrary.ECCHelpers.CompareStrings(go.name, "GargantuanVoidTroll", ECCLibrary.ECCStringComparison.Contains))
                {
                    go.FindChild("AdultGargModel").SetActive(true);
                    GameObject trollFace = go.FindChild("TrollFace");
                    trollFace.SetActive(false);
                    GargantuanRoar roar = go.GetComponent<GargantuanRoar>();
                    if (roar != null)
                    {
                        roar.audioSource.enabled = true;
                        roar.enabled = true;
                    }
                    go.name = "GargantuanVoid(Clone)";
                    go.GetComponent<TrollVoice>().enabled = false;
                    trollFace.GetComponent<TrollFaceTracker>().enabled = false;
                    continue;
                }
            }
        }
    }
}
