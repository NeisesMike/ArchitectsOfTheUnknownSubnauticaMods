using UnityEngine;

namespace RotA.Mono.Creatures.GargEssentials
{
    class GargEyeTracker : MonoBehaviour
    {
        private Transform _upReference;
        private bool _xUp;
        private bool _clamp;
        private Vector3 _localRotationLimitsMin;
        private Vector3 _localRotationLimitsMax;
        private Vector3 _localRotationOffset;
        private GargantuanBehaviour _garg;
        private Quaternion _defaultRotation;
        private Vector3 _defaultLocalDirection;

        public void InitializeValues(GargantuanBehaviour garg, Transform upReference, bool xUp, bool clamp, Vector3 localRotationLimitsMin, Vector3 localRotationLimitsMax, Vector3 localRotationOffset = default)
        {
            this._garg = garg;
            this._upReference = upReference;
            this._xUp = xUp;
            this._clamp = clamp;
            this._localRotationLimitsMin = localRotationLimitsMin;
            this._localRotationLimitsMax = localRotationLimitsMax;
            this._localRotationOffset = localRotationOffset;
        }
        private void Start()
        {
            _defaultRotation = transform.localRotation;
            _defaultLocalDirection = transform.InverseTransformDirection(transform.up);
            _target = Player.main.transform;
        }
        private void LateUpdate()
        {
            if (_target == null)
            {
                transform.localRotation = _defaultRotation;
                return;
            }
            Vector3 lookDir = (_target.transform.position - transform.position).normalized;
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
