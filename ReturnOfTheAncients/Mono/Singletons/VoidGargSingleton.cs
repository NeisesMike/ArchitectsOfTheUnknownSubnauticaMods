﻿using UnityEngine;

namespace RotA.Mono.Singletons
{
    // Ensures there is only ever one NATURALLY spawned adult.
    public class VoidGargSingleton : MonoBehaviour
    {
        private static VoidGargSingleton main;

        public static bool AdultGargExists
        {
            get
            {
                if (main == null)
                {
                    return false;
                }
                if (main.gameObject.activeSelf == false)
                {
                    return false;
                }
                return true;
            }
        }

        private void Start()
        {
            main = this;
            InvokeRepeating(nameof(CheckDistance), Random.value, 10f);
        }

        private void CheckDistance()
        {
            var playerBiome = Player.main.GetBiomeString();
            if (playerBiome.StartsWith("precursor", System.StringComparison.OrdinalIgnoreCase))
            {
                Destroy(gameObject, 2f);
            }
            else if (!VoidGargSpawner.IsVoidBiome(playerBiome) && !playerBiome.StartsWith("observatory", System.StringComparison.OrdinalIgnoreCase))
            {
                float distance = Vector3.Distance(MainCameraControl.main.transform.position, transform.position);
                if (distance > 500f)
                {
                    Destroy(gameObject);
                }
            }
        }

    }
}
