using ArchitectsLibrary.API;
using ArchitectsLibrary.Utility;
using RotA.Mono.Creatures.GargEssentials;
using System.Collections;
using UnityEngine;

namespace RotA.Mono.Modules
{
    public class CyclopsScanner : MonoBehaviour
    {
        private SubRoot _cyclops;
        private CyclopsExternalCams _cyclopsExternalCams;
        private bool _showedInstructions;
        private GameObject _scanBeam;
        private VFXOverlayMaterial _scanFX;
        private Material _scanMaterialCircuitFX;
        private Material _scanMaterialOrganicFX;
        private Material _scanMaterialGargFX;

        private bool _canScan;
        private FMOD_CustomLoopingEmitter _scanSound;

        private const float kMaxScanDistance = 200f;
        private const int kCyclopsScannerPower = 10;

        private IEnumerator Start()
        {
            _cyclops = GetComponent<SubRoot>();
            _cyclopsExternalCams = GetComponentInChildren<CyclopsExternalCams>();

            var scanLoopObj = gameObject.transform.Find("ScanLoopSFX")?.gameObject;
            if (scanLoopObj == null)
            {
                scanLoopObj = new GameObject("ScanLoopSFX");
                scanLoopObj.transform.SetParent(transform, false);
            }
            _scanSound = scanLoopObj.EnsureComponent<FMOD_CustomLoopingEmitter>();
            _scanSound.SetAsset(SNAudioEvents.GetFmodAsset(SNAudioEvents.Paths.ScannerScanningLoop));

            SetFXActive(false);

            var task = CraftData.GetPrefabForTechTypeAsync(TechType.Scanner);
            yield return task;
            var scannerPrefab = task.GetResult();
            var scannerTool = scannerPrefab.GetComponent<ScannerTool>();
            
            var shader = Shader.Find("FX/Scanning");
            if (shader == null) yield break;
            
            _scanMaterialCircuitFX = new Material(shader)
            {
                hideFlags = HideFlags.HideAndDontSave
            };
            _scanMaterialCircuitFX.SetTexture(ShaderPropertyID._MainTex, scannerTool.scanCircuitTex);
            _scanMaterialCircuitFX.SetColor(ShaderPropertyID._Color, scannerTool.scanCircuitColor);

            _scanMaterialOrganicFX = new Material(shader)
            {
                hideFlags = HideFlags.HideAndDontSave
            };
            _scanMaterialOrganicFX.SetTexture(ShaderPropertyID._MainTex, scannerTool.scanOrganicTex);
            _scanMaterialOrganicFX.SetColor(ShaderPropertyID._Color, scannerTool.scanOrganicColor);

            _scanMaterialGargFX = new Material(shader)
            {
                hideFlags = HideFlags.HideAndDontSave
            };
            _scanMaterialGargFX.SetTexture(ShaderPropertyID._MainTex, scannerTool.scanOrganicTex);
            _scanMaterialGargFX.SetColor(ShaderPropertyID._Color, new Color(0.63f, 0f, 1f));
        }

        private void Update()
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

            PDAScanner.Result scanResult = PDAScanner.Result.None;
            if (_canScan)
            {
                UpdateScanTarget(kMaxScanDistance);
                if (GameInput.GetButtonHeld(GameInput.Button.AltTool))
                {
                    for (var i = 0; i < kCyclopsScannerPower; i++)
                    {
                        scanResult = PDAScanner.Scan();
                    }
                }
            }
            SetFXActive(scanResult == PDAScanner.Result.Scan);
        }

        private void SetFXActive(bool active)
        {
            //_scanBeam.gameObject.SetActive(state);

            if (active && PDAScanner.scanTarget.isValid)
            {
                PlayOverlayScanFX();
                if (!_scanSound.playing)
                {
                    _scanSound.Play();
                }
                return;
            }
            StopScanFX();
            _scanSound.Stop();
        }

        private void PlayOverlayScanFX()
        {
            var scanTarget = PDAScanner.scanTarget;
            if (!scanTarget.isValid) return;
            
            if (_scanFX != null)
            {
                if (_scanFX.gameObject != scanTarget.gameObject)
                {
                    StopScanFX();
                    _scanFX = scanTarget.gameObject.AddComponent<VFXOverlayMaterial>();
                    if (scanTarget.gameObject.GetComponent<GargantuanBehaviour>() != null)
                    {
                        _scanFX.ApplyOverlay(_scanMaterialGargFX, "VFXOverlay: Scanning", false, null);
                        return;
                    }
                    if (scanTarget.gameObject.GetComponent<Creature>() != null)
                    {
                        _scanFX.ApplyOverlay(_scanMaterialOrganicFX, "VFXOverlay: Scanning", false, null);
                        return;
                    }
                    _scanFX.ApplyOverlay(_scanMaterialCircuitFX, "VFXOverlay: Scanning", false, null);
                    return;
                }
            }
            else
            {
                _scanFX = scanTarget.gameObject.AddComponent<VFXOverlayMaterial>();
                if (scanTarget.gameObject.GetComponent<GargantuanBehaviour>() != null)
                {
                    _scanFX.ApplyOverlay(_scanMaterialGargFX, "VFXOverlay: Scanning", false, null);
                    return;
                }
                if (scanTarget.gameObject.GetComponent<Creature>() != null)
                {
                    _scanFX.ApplyOverlay(_scanMaterialOrganicFX, "VFXOverlay: Scanning", false, null);
                    return;
                }
                _scanFX.ApplyOverlay(_scanMaterialCircuitFX, "VFXOverlay: Scanning", false, null);
            }
        }

        private void StopScanFX()
        {
            if (_scanFX != null)
            {
                _scanFX.RemoveOverlay();
            }
        }

        // edited uwe code from PDAScanner class, not mine
        private void UpdateScanTarget(float distance)
        {
            PDAScanner.ScanTarget newScanTarget = default;
            newScanTarget.Invalidate();
            GetTarget(distance, out var candidate);
            newScanTarget.Initialize(candidate);
            
            if (PDAScanner.scanTarget.techType == newScanTarget.techType &&
                PDAScanner.scanTarget.gameObject == newScanTarget.gameObject &&
                PDAScanner.scanTarget.uid == newScanTarget.uid) return;

            if (newScanTarget.hasUID && PDAScanner.cachedProgress.TryGetValue(newScanTarget.uid, out var progress))
            {
                newScanTarget.progress = progress;
            }
            
            PDAScanner.scanTarget = newScanTarget;
        }

        private bool GetTarget(float maxDistance, out GameObject result)
        {
            var cameraTransform = Camera.current.transform;
            var position = cameraTransform.position;
            var forward = cameraTransform.forward;
            if (Physics.Raycast(position, forward, out var hit, maxDistance, -1, QueryTriggerInteraction.Ignore))
            {
                result = hit.collider.gameObject;
                return true;
            }
            result = null;
            return false;
        }

        void OnDisable()
        {
            SetFXActive(false);
        }
    }
}
