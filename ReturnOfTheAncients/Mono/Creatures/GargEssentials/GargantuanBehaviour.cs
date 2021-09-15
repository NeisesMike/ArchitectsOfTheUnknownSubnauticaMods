namespace RotA.Mono.Creatures.GargEssentials
{
    using Modules;
    using UnityEngine;
    
    class GargantuanBehaviour : MonoBehaviour, IOnTakeDamage, IOnArchitectElectricityZap
    {
        public Transform EyeTrackTarget { get; private set; }

        internal GameObject CachedBloodPrefab { get; private set; }
        
        internal GargantuanRoar roar;
        internal LastTarget lastTarget;
        internal float timeSpawnBloodAgain;
        internal float bloodDestroyTime;
        internal float timeCanAttackAgain;
        
        Creature _creature;
        GargantuanGrab _grab;
        GargantuanStealth _stealth;
        private float _timeUpdateEyeTargetAgain;
        private const float kUpdateEyeTargetInterval = 0.2f;

        void Start()
        {
            _grab = GetComponent<GargantuanGrab>();
            _creature = GetComponent<Creature>();
            roar = GetComponent<GargantuanRoar>();
            lastTarget = gameObject.GetComponent<LastTarget>();
            _stealth = gameObject.GetComponent<GargantuanStealth>();
        }

        public bool IsInStealthMode()
        {
            return _stealth != null && _stealth.StealthActive;
        }

        public bool CanEat(GameObject target)
        {
            return target.GetComponent<Creature>() || target.GetComponent<Player>() || target.GetComponent<Vehicle>() || target.GetComponent<SubRoot>() || target.GetComponent<CyclopsDecoy>();
        }

        private void Update()
        {
            if (Time.time > _timeUpdateEyeTargetAgain)
            {
                EyeTrackTarget = FindEyeTarget();
                _timeUpdateEyeTargetAgain = Time.time + kUpdateEyeTargetInterval;
            }
        }

        private Transform FindEyeTarget()
        {
            if (lastTarget.target != null)
            {
                return lastTarget.target.transform;
            }
            return MainCameraControl.main.transform;
        }

        public bool GetBloodEffectFromCreature(GameObject creature, float startSizeScale, float lifetimeScale)
        {
            if (creature == null)
            {
                return false;
            }
            LiveMixin lm = creature.GetComponent<LiveMixin>();
            if (lm == null)
            {
                return false;
            }
            if (lm.data == null)
            {
                return false;
            }
            GameObject prefab = lm.data.damageEffect;
            if (prefab == null)
            {
                return false;
            }
            CachedBloodPrefab = Instantiate(prefab);
            CachedBloodPrefab.SetActive(false);
            foreach (ParticleSystem ps in CachedBloodPrefab.GetComponentsInChildren<ParticleSystem>())
            {
                var main = ps.main;
                main.startLifetime = new ParticleSystem.MinMaxCurve(main.startLifetime.constant * lifetimeScale);
                main.startSize = new ParticleSystem.MinMaxCurve(main.startSize.constant * startSizeScale);
            }
            VFXDestroyAfterSeconds destroyAfterSeconds = CachedBloodPrefab.GetComponent<VFXDestroyAfterSeconds>();
            if (destroyAfterSeconds is not null)
            {
                bloodDestroyTime = (destroyAfterSeconds.lifeTime + 5f) * lifetimeScale;
                DestroyImmediate(destroyAfterSeconds);
            }
            else
            {
                bloodDestroyTime = 10f;
            }
            return true;
        }
        
        public Quaternion InverseRotation(Quaternion input)
        {
            return Quaternion.Euler(input.eulerAngles + new Vector3(0f, 180f, 0f));
        }
        public Quaternion FixSmallFishRotation(Quaternion input)
        {
            return Quaternion.Euler(input.eulerAngles + new Vector3(0f, 0f, 90f));
        }
        public Vector3 FixJuvenileFishHoldPosition(Transform holdPoint, Vector3 input)
        {
            return input + (holdPoint.up * 3f);
        }
        public void OnTakeDamage(DamageInfo damageInfo)
        {
            if (damageInfo.type == Mod.ArchitectElect)
            {
                OnDamagedByArchElectricity();
            }
        }

        public void OnDamagedByArchElectricity()
        {
            if (_grab.HeldVehicle is not null)
            {
                _grab.ReleaseHeld();
            }
            else
            {
                _creature.Scared.Value = 1f;
                _creature.Aggression.Value = 0f;
                timeCanAttackAgain = Time.time + 5f;
            }
            if (lastTarget != null) lastTarget.target = null;
        }
    }
}
