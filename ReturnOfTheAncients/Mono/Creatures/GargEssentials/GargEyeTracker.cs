using UnityEngine;

namespace RotA.Mono.Creatures.GargEssentials
{
    class GargEyeTracker : MonoBehaviour
    {
        [SerializeField] private Transform _upReference;
        [SerializeField] private bool _xUp;
        [SerializeField] private bool _clamp;
        [SerializeField] private Vector3 _localRotationLimitsMin;
        [SerializeField] private Vector3 _localRotationLimitsMax;
        [SerializeField] private Vector3 _localRotationOffset;
        [SerializeField] private GargantuanBehaviour _garg;
        [SerializeField] private Quaternion _defaultRotation;
        [SerializeField] private Vector3 _defaultLocalDirection;

        public void InitializeValues(GargantuanBehaviour garg, Transform upReference, bool xUp, bool clamp, Vector3 localRotationLimitsMin, Vector3 localRotationLimitsMax, Vector3 localRotationOffset = default)
        {
            _garg = garg;
            _upReference = upReference;
            _xUp = xUp;
            _clamp = clamp;
            _localRotationLimitsMin = localRotationLimitsMin;
            _localRotationLimitsMax = localRotationLimitsMax;
            _localRotationOffset = localRotationOffset;
        }
        private void Start()
        {
            _defaultRotation = transform.localRotation;
            _defaultLocalDirection = transform.InverseTransformDirection(transform.up);
        }
        private void LateUpdate()
        {
            if (_garg.EyeTrackTarget == null)
            {
                transform.localRotation = _defaultRotation;
                return;
            }
            Vector3 lookDir = (_garg.EyeTrackTarget.transform.position - transform.position).normalized;
            Quaternion rotation = _xUp ? XLookRotation(_upReference.forward, lookDir) : Quaternion.LookRotation(_upReference.forward, lookDir);

            bool shouldLookStraight = Vector3.Dot(transform.TransformDirection(_defaultLocalDirection), lookDir) < 0.2f;
            if (shouldLookStraight)
            {
                transform.localRotation = _defaultRotation;
            }
            else
            {
                transform.rotation = rotation;
                Vector3 eulerAngles = transform.localEulerAngles;
                if (_clamp)
                {
                    eulerAngles = new Vector3(Mathf.Clamp(eulerAngles.x, _localRotationLimitsMin.x, _localRotationLimitsMax.x), Mathf.Clamp(eulerAngles.y, _localRotationLimitsMin.y, _localRotationLimitsMax.y), Mathf.Clamp(eulerAngles.z, _localRotationLimitsMin.z, _localRotationLimitsMax.z));
                }
                eulerAngles += _localRotationOffset;
                transform.localEulerAngles = eulerAngles;
            }
        }

        Quaternion XLookRotation(Vector3 right, Vector3 up = default)
        {
            if (up == default)
                up = Vector3.up;

            Quaternion rightToForward = Quaternion.Euler(0f, -90f, 0f);
            Quaternion forwardToTarget = Quaternion.LookRotation(right, up);

            return forwardToTarget * rightToForward;
        }
    }
}
