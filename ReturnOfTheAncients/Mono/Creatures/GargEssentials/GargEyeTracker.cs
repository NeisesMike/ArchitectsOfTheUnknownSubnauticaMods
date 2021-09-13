using UnityEngine;

namespace RotA.Mono.Creatures.GargEssentials
{
    class GargEyeTracker : MonoBehaviour
    {
        public Transform upReference;
        public Transform target;
        public bool xUp;
        public bool clamp;
        public Vector3 localRotationLimitsMin;
        public Vector3 localRotationLimitsMax;
        public Vector3 localRotationOffset;

        private Quaternion _defaultRotation;
        private Vector3 _defaultLocalDirection;

        private void Start()
        {
            _defaultRotation = transform.localRotation;
            _defaultLocalDirection = transform.InverseTransformDirection(transform.up);
        }
        private void LateUpdate()
        {
            Vector3 lookDir = (target.transform.position - transform.position).normalized;
            Quaternion rotation = xUp ? XLookRotation(upReference.forward, lookDir) : Quaternion.LookRotation(upReference.forward, lookDir);

            bool shouldLookStraight = Vector3.Dot(transform.TransformDirection(_defaultLocalDirection), lookDir) < 0.2f;
            if (shouldLookStraight)
            {
                transform.localRotation = _defaultRotation;
            }
            else
            {
                transform.rotation = rotation;
                Vector3 eulerAngles = transform.localEulerAngles;
                if (clamp)
                {
                    eulerAngles = new Vector3(Mathf.Clamp(eulerAngles.x, localRotationLimitsMin.x, localRotationLimitsMax.x), Mathf.Clamp(eulerAngles.y, localRotationLimitsMin.y, localRotationLimitsMax.y), Mathf.Clamp(eulerAngles.z, localRotationLimitsMin.z, localRotationLimitsMax.z));
                }
                eulerAngles += localRotationOffset;
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
