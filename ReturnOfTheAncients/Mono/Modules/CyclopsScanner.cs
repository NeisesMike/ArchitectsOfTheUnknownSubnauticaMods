using ArchitectsLibrary.Utility;
using UnityEngine;

namespace RotA.Mono.Modules
{
    public class CyclopsScanner : MonoBehaviour
    {
        private SubRoot _cyclops;
        private CyclopsExternalCams _cyclopsExternalCams;
        private bool _showedInstructions;

        private bool _canScan;

        private const float kMaxScanDistance = 200f;

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
                    _canScan = true;
                }
            }
            else
            {
                _showedInstructions = false;
                _canScan = false;
            }

            if (_canScan)
            {
                UpdatePDAScannerTarget(kMaxScanDistance);
                if (GameInput.GetButtonHeld(GameInput.Button.AltTool))
                {
                    PDAScanner.Scan();
                }
            }
        }

        // edited uwe code from PDAScanner class, not mine
        private void UpdatePDAScannerTarget(float distance)
        {
            PDAScanner.ScanTarget newScanTarget = default;
            newScanTarget.Invalidate();
            GameObject candidate;
            GetTarget(distance, out candidate);
            newScanTarget.Initialize(candidate);
            if (PDAScanner.scanTarget.techType != newScanTarget.techType || PDAScanner.scanTarget.gameObject != newScanTarget.gameObject || PDAScanner.scanTarget.uid != newScanTarget.uid)
            {
                if (PDAScanner.scanTarget.isPlayer && PDAScanner.scanTarget.hasUID && PDAScanner.cachedProgress.ContainsKey(PDAScanner.scanTarget.uid))
                {
                    PDAScanner.cachedProgress[PDAScanner.scanTarget.uid] = 0f;
                }
                float progress;
                if (newScanTarget.hasUID && PDAScanner.cachedProgress.TryGetValue(newScanTarget.uid, out progress))
                {
                    newScanTarget.progress = progress;
                }
                PDAScanner.scanTarget = newScanTarget;
            }
        }

        private bool GetTarget(float maxDistance, out GameObject result)
        {
            Transform cameraTransform = Camera.current.transform;
            Vector3 position = cameraTransform.position;
            Vector3 forward = cameraTransform.forward;
            Ray ray = new Ray(position, forward);
            if (Physics.Raycast(ray, out RaycastHit hit, maxDistance, -1, QueryTriggerInteraction.Ignore))
            {
                result = hit.collider.gameObject;
                return true;
            }
            result = null;
            return false;
        }
    }
}
