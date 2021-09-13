using ArchitectsLibrary.Utility;
using UnityEngine;

namespace RotA.Mono.Modules
{
    public class CyclopsScanner : MonoBehaviour
    {
        private SubRoot _cyclops;
        private CyclopsExternalCams _cyclopsExternalCams;
        private bool _showedInstructions;

        void Start()
        {
            _cyclops = GetComponent<SubRoot>();
            _cyclopsExternalCams = GetComponentInChildren<CyclopsExternalCams>();
        }

        void Update()
        {
            if (_cyclopsExternalCams != null && _cyclopsExternalCams.usingCamera && _cyclopsExternalCams.cameraIndex == 1)
            {
                if (!_showedInstructions)
                {
                    ErrorMessage.AddMessage(LanguageCache.GetButtonFormat("CyclopsScannerInstructions", GameInput.Button.AltTool));
                    _showedInstructions = true;
                }
            }
            else
            {
                _showedInstructions = false;
            }
        }
    }
}
