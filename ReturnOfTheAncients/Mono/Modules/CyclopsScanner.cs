using ArchitectsLibrary.API;
using ArchitectsLibrary.Utility;
using RotA.Mono.Creatures.GargEssentials;
using System.Collections;
using ArchitectsLibrary.MonoBehaviours;
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
        
        // layer mask for when the cyclops scanner does a second pass for scanning
        private static readonly LayerMask _triggerRaycastLayerMask = LayerMask.GetMask(new string[]{"Default"});

        // overlay materials that show up on the object that is being scanned
        private Material _scanMaterialCircuitFX;
        private Material _scanMaterialOrganicFX;
        private Material _scanMaterialGargFX;
        private Material _scanMaterialPrecursorFX;

        private bool _canScan;
        private FMOD_CustomLoopingEmitter _scanSoundEmitter;

        // how far the cyclops scanner can scan, in meters
        private const float kMaxScanDistance = 2000;

        // how fast the cyclops scanner scans, compared to the handheld scanner
        private const int kCyclopsScannerPower = 5;

        private IEnumerator Start()
        {
            // set references
            _cyclops = GetComponent<SubRoot>();
            _cyclopsExternalCams = GetComponentInChildren<CyclopsExternalCams>();

            // make sure the sound emitter exists
            var scanLoopObj = gameObject.transform.Find("ScanLoopSFX")?.gameObject;
            if (scanLoopObj == null)
            {
                scanLoopObj = new GameObject("ScanLoopSFX");
                scanLoopObj.transform.SetParent(transform, false);
            }

            _scanSoundEmitter = scanLoopObj.EnsureComponent<FMOD_CustomLoopingEmitter>();
            _scanSoundEmitter.SetAsset(SNAudioEvents.GetFmodAsset(SNAudioEvents.Paths.ScannerScanningLoop));

            // set scanning fx and material stuff, using stuff from the handheld scanner
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

            _scanMaterialPrecursorFX = new Material(shader)
            {
                hideFlags = HideFlags.HideAndDontSave
            };
            _scanMaterialPrecursorFX.SetTexture(ShaderPropertyID._MainTex, scannerTool.scanCircuitTex);
            _scanMaterialPrecursorFX.SetColor(ShaderPropertyID._Color, new Color(0.54f, 1f, 0.54f));
        }

        private void Update()
        {
            // if you're using the camera system and are in the conning tower camera (camera index 1)... 
            if (_cyclopsExternalCams != null && _cyclopsExternalCams.usingCamera &&
                _cyclopsExternalCams.cameraIndex == 1)
            {
                if (
                    !_showedInstructions) // show a hint for using the scanner if you haven't already. don't want to spam it.
                {
                    ErrorMessage.AddMessage(LanguageCache.GetButtonFormat("CyclopsScannerInstructions",
                        GameInput.Button.AltTool)); // show the instructions of how to use this thing
                    _showedInstructions = true;
                }

                _canScan = true;
            }
            else // you're not in the correct camera, allow the instruction popup to show next time
            {
                _showedInstructions = false;
                _canScan = false;
            }

            var scanButtonHeld = GameInput.GetButtonHeld(GameInput.Button.AltTool); // by default the keyboard key is F
            var scanResult = PDAScanner.Result.None;
            if (_canScan)
            {
                UpdateScanTarget(kMaxScanDistance);
                if (scanButtonHeld)
                {
                    for (var i = 0; i < kCyclopsScannerPower; i++)
                    {
                        scanResult = PDAScanner.Scan();
                    }
                }
            }

            SetFXActive(_canScan & scanButtonHeld);
            SetSfxActive(_canScan & scanButtonHeld);
        }

        // simply turn the emitter on or off
        private void SetSfxActive(bool active)
        {
            if (active)
            {
                if (!_scanSoundEmitter.playing) _scanSoundEmitter.Play();
            }
            else
            {
                _scanSoundEmitter.Stop();
            }
        }

        // sets all non-audio-related effects active/inactive
        private void SetFXActive(bool active)
        {
            //_scanBeam.gameObject.SetActive(state);

            if (active && PDAScanner.scanTarget.isValid)
            {
                PlayOverlayScanFX();
                return;
            }

            StopScanFX();
        }

        // plays the overlay scan effect on the current scan target
        private void PlayOverlayScanFX()
        {
            var scanTarget = PDAScanner.scanTarget;
            if (!scanTarget.isValid) return;

            if (_scanFX != null) // check if there's already an instance of something being scanned
            {
                if (_scanFX.gameObject == scanTarget.gameObject)
                    return; // if you're already showing a scan overlay fx on the target, you don't need to add more

                StopScanFX(); // remove the existing overlay, if any
            }
            
            // add a new overlay

            _scanFX = scanTarget.gameObject.AddComponent<VFXOverlayMaterial>();
            
            // check for reasons to add special scan overlay effects
            
            if (scanTarget.gameObject.GetComponent<PrecursorObjectTag>())
            {
                _scanFX.ApplyOverlay(_scanMaterialPrecursorFX, "VFXOverlay: Scanning", false, null);
                return;
            }

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
            
            // otherwise, just use a normal scan effect
            
            _scanFX.ApplyOverlay(_scanMaterialCircuitFX, "VFXOverlay: Scanning", false, null);
        }

        private void StopScanFX()
        {
            if (_scanFX != null)
            {
                _scanFX.RemoveOverlay();
            }
        }

        // edited uwe code from PDAScanner class, not mine, I barely understand what it does
        private void UpdateScanTarget(float distance)
        {
            PDAScanner.ScanTarget newScanTarget = default;
            newScanTarget.Invalidate();
            GetTarget(distance, out var candidate, QueryTriggerInteraction.Ignore);
            if (candidate == null || CraftData.GetTechType(candidate) == TechType.None)
            {
                GetTarget(distance, out candidate, QueryTriggerInteraction.Collide);
            }
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

        private static void GetTarget(float maxDistance, out GameObject result, QueryTriggerInteraction queryTriggerInteraction)
        {
            var cameraTransform = Camera.current.transform;
            var position = cameraTransform.position;
            var forward = cameraTransform.forward;
            if (Physics.Raycast(position, forward, out var hit, maxDistance, queryTriggerInteraction == QueryTriggerInteraction.Collide? _triggerRaycastLayerMask : -1, queryTriggerInteraction))
            {
                result = hit.collider.gameObject;
                return;
            }

            result = null;
            return;
        }

        void OnDisable()
        {
            // when this component is disabled/removed, also disable any sfx/vfx related to it
            SetFXActive(false);
            SetSfxActive(false);
        }
    }
}