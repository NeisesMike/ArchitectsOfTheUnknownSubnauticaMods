using UnityEngine;

namespace RotA.Mono.Modules
{
    public class CyclopsScanner : MonoBehaviour
    {
        private SubRoot _cyclops;
        private CyclopsExternalCams _cyclopsExternalCams;

        void Start()
        {
            _cyclops = GetComponent<SubRoot>();
            _cyclopsExternalCams = GetComponentInChildren<CyclopsExternalCams>();
        }
    }
}
