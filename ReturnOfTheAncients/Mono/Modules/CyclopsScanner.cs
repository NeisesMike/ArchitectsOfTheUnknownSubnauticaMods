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

        IEnumerator Start()
        {
            _cyclops = GetComponent<SubRoot>();
            _cyclopsExternalCams = GetComponentInChildren<CyclopsExternalCams>();

            GameObject scanLoopSFXObj = gameObject.transform.Find("ScanLoopSFX")?.gameObject;
            if (scanLoopSFXObj)
            {
                GameObject scanLoopSFX = new GameObject("ScanLoopSFX");
                scanLoopSFX.transform.SetParent(transform, false);
            }
            _scanSound = scanLoopSFXObj.EnsureComponent<FMOD_CustomLoopingEmitter>();
            _scanSound.SetAsset(SNAudioEvents.GetFmodAsset(SNAudioEvents.Paths.ScannerScanningLoop));

            SetFXActive(false);

            var task = CraftData.GetPrefabForTechTypeAsync(TechType.Scanner);
            yield return task;
            var scannerPrefab = task.GetResult();
            var scannerTool = scannerPrefab.GetComponent<ScannerTool>();
            
            Shader shader = Shader.Find("FX/Scanning");
            if (shader != null)
            {
                _scanMaterialCircuitFX = new Material(shader);
                _scanMaterialCircuitFX.hideFlags = HideFlags.HideAndDontSave;
                _scanMaterialCircuitFX.SetTexture(ShaderPropertyID._MainTex, scannerTool.scanCircuitTex);
                _scanMaterialCircuitFX.SetColor(ShaderPropertyID._Color, scannerTool.scanCircuitColor);

                _scanMaterialOrganicFX = new Material(shader);
                _scanMaterialOrganicFX.hideFlags = HideFlags.HideAndDontSave;
                _scanMaterialOrganicFX.SetTexture(ShaderPropertyID._MainTex, scannerTool.scanOrganicTex);
                _scanMaterialOrganicFX.SetColor(ShaderPropertyID._Color, scannerTool.scanOrganicColor);

                _scanMaterialGargFX = new Material(shader);
                _scanMaterialGargFX.hideFlags = HideFlags.HideAndDontSave;
                _scanMaterialGargFX.SetTexture(ShaderPropertyID._MainTex, scannerTool.scanOrganicTex);
                _scanMaterialGargFX.SetColor(ShaderPropertyID._Color, new Color(0.63f, 0f, 1f));
            }
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

            bool fxActive = false;
            if (_canScan)
            {
                UpdatePDAScannerTarget(kMaxScanDistance);
                if (GameInput.GetButtonHeld(GameInput.Button.AltTool))
                {
                    for (int i = 0; i < kCyclopsScannerPower; i++)
                    {
                        PDAScanner.Scan();
                    }
                    fxActive = true;
                }
            }
            SetFXActive(fxActive);
        }

        private void SetFXActive(bool active)
        {
            //_scanBeam.gameObject.SetActive(state);

            if (active && PDAScanner.scanTarget.isValid)
            {
                PlayScanFX();
                if (!_scanSound.playing)
                {
                    _scanSound.Play();
                }
                return;
            }
            StopScanFX();
            _scanSound.Stop();
        }

        private void PlayScanFX()
        {
            PDAScanner.ScanTarget scanTarget = PDAScanner.scanTarget;
            if (scanTarget.isValid)
            {
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
        }

        private void StopScanFX()
        {
            if (_scanFX != null)
            {
                _scanFX.RemoveOverlay();
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

        void OnDisable()
        {
            SetFXActive(false);
        }
    }
}
