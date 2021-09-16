using ECCLibrary.Internal;

namespace RotA.Mono.Creatures.GargEssentials
{
    using ECCLibrary;
    using Prefabs.Creatures;
    using System.Collections;
    using UnityEngine;
    using static GargantuanConditions;

    internal class GargantuanMouthAttack : MeleeAttack
    {
        private AudioSource _attackSource;
        private ECCAudio.AudioClipPool _biteClipPool;
        private ECCAudio.AudioClipPool _cinematicClipPool;
        private GargantuanBehaviour _behaviour;
        private GargantuanGrab _grab;
        private PlayerCinematicController _playerDeathCinematic;
        private Creature _garg;
        private float _aggressionThreshold = 0.2f;
        
        private static readonly int _gargBiteAnimParam = Animator.StringToHash("bite");
        private static readonly int _gargRandomAnimParam = Animator.StringToHash("random");

        public GameObject throat;
        public bool canAttackPlayer = true;
        public bool oneShotPlayer;
        public string attachBoneName;
        public bool canPerformCyclopsCinematic;
        public GargGrabFishMode grabFishMode;

        private void Start()
        {
            _garg = GetComponent<Creature>();
            _grab = GetComponent<GargantuanGrab>();
            _attackSource = gameObject.AddComponent<AudioSource>();
            _attackSource.minDistance = 10f;
            _attackSource.maxDistance = 40f;
            _attackSource.spatialBlend = 1f;
            _attackSource.volume = ECCHelpers.GetECCVolume();
            _attackSource.playOnAwake = false;
            _biteClipPool = ECCAudio.CreateClipPool("GargBiteAttack");
            _cinematicClipPool = ECCAudio.CreateClipPool("GargBiteAttack5");
            throat = gameObject.SearchChild("Head");
            gameObject.SearchChild("Mouth").EnsureComponent<OnTouch>().onTouch = new OnTouch.OnTouchEvent();
            gameObject.SearchChild("Mouth").EnsureComponent<OnTouch>().onTouch.AddListener(OnTouch);
            _behaviour = GetComponent<GargantuanBehaviour>();

            _playerDeathCinematic = gameObject.AddComponent<PlayerCinematicController>();
            _playerDeathCinematic.animatedTransform = gameObject.SearchChild(attachBoneName).transform;
            _playerDeathCinematic.animator = creature.GetAnimator();
            _playerDeathCinematic.animParamReceivers = new GameObject[0];
            _playerDeathCinematic.animParam = "cin_player";
            _playerDeathCinematic.playerViewAnimationName = "seadragon_attack";
        }

        public override void OnTouch(Collider collider)
        {
            // should never happen, cuz the garg is unable to die but just in case.
            if (!liveMixin.IsAlive() || Time.time < _behaviour.timeCanAttackAgain || _playerDeathCinematic.IsCinematicModeActive())
                return;

            var target = GetTarget(collider);
            if (!_behaviour.CanEat(target)) // check if we can do anything with the target, otherwise early exit.
                return;
            
            if (_grab.IsHoldingVehicle()) // shouldn't perform another attack if already attacking something.
                return;

            var targetLm = target.GetComponent<LiveMixin>();
            var player = target.GetComponent<Player>();
            if (player) // player-based attacks
            {
                if (!PlayerAttackable(player) || _garg.Aggression.Value < _aggressionThreshold)
                    return;
                
                // if we cant attack the player, then we're the baby and should nibble his hand.
                if (!canAttackPlayer)
                {
                    GargantuanBabyNibble();
                    _behaviour.timeCanAttackAgain = Time.time + 1f;
                    return;
                }
                // otherwise screw him up like a truck.
                
                var damage = oneShotPlayer ? 1000f : 80f;

                var num = DamageSystem.CalculateDamage(damage, DamageType.Normal, target);
                if (targetLm.health - num <= 0f) // check if the player's gonna die during this attack
                {
                    StartCoroutine(PerformPlayerCinematic(player));
                    return;
                }
                
                // if not, then just bite him.
                StartCoroutine(PerformBiteAttack(target, damage));
                _behaviour.timeCanAttackAgain = Time.time + 2f;
                return;
            }

            if (canAttackPlayer && _grab.GetCanGrabVehicle()) // vehicles-based attacks
            {
                var vehicle = target.GetComponent<Vehicle>();

                if (vehicle)
                {
                    if (vehicle is Exosuit && !vehicle.docked) // of it's an exosuit, we grab it like an exosuit
                        _grab.GrabExosuit(vehicle);
                    else if (!vehicle.docked) // if it's whatever else type of vehicles, we grab it like a generic one.
                        _grab.GrabGenericSub(vehicle);

                    _garg.Aggression.Value -= 0.5f;
                    return;
                }

                if (canPerformCyclopsCinematic)
                {
                    var subRoot = target.GetComponent<SubRoot>();
                    if (subRoot && !subRoot.rb.isKinematic && subRoot.live) // checks for cyclops
                    {
                        _grab.GrabLargeSub(subRoot);
                        _behaviour.roar.DelayTimeOfNextRoar(8f);
                        _garg.Aggression.Value -= 1f;
                        return;
                    }
                }
            }
            if (!targetLm)
                return;
                
            if (!targetLm.IsAlive()) // shouldn't attack dead meat
                return;

            var targetCreature = target.GetComponent<Creature>();
                
            if (!targetCreature) // only attack creatures
                return;
            
            switch (grabFishMode)
            {
                // Leviathan attack animations
                case GargGrabFishMode.LeviathansOnlyAndSwallow or GargGrabFishMode.LeviathansOnlyNoSwallow:
                {
                    if ((grabFishMode == GargGrabFishMode.LeviathansOnlyAndSwallow && AdultCanGrab(target))
                        ||
                        (grabFishMode == GargGrabFishMode.LeviathansOnlyNoSwallow && JuvenileCanGrab(target)))
                    {
                        GargantuanCreatureAttack(ref targetCreature, false);
                    }
                    return;
                }
                // baby "play with food" animation
                case GargGrabFishMode.PickupableOnlyAndSwalllow:
                {
                    if (target.GetComponent<Pickupable>() != null && target.GetComponent<GargantuanRoar>() == null)
                    {
                        GargantuanCreatureAttack(ref targetCreature, true);
                    }
                    return;
                }
            }
            
            if (!CanAttackTargetFromPosition(target)) // any attack past this point must not have collisions between the garg and the target
                return;
            
            if (CanSwallowWhole(target, targetLm))
            {
                creature.GetAnimator().SetTrigger("bite");
                _garg.Hunger.Value -= 0.15f;
                var swallowing = target.AddComponent<BeingSuckedInWhole>();
                swallowing.target = throat.transform;
                swallowing.animationLength = 1f;
            }
            else if (canAttackPlayer || !IsVehicle(target))
            {
                StartCoroutine(PerformBiteAttack(target, GetBiteDamage(target)));
                _behaviour.timeCanAttackAgain = Time.time + 2f;
                if (canAttackPlayer)
                {
                    creature.Aggression.Value = 0f;
                }

                _garg.Aggression.Value -= 0.15f;
            }
        }

        private void GargantuanCreatureAttack(ref Creature targetCreature, bool isGargBaby)
        {
            _garg.Aggression.Value -= 0.6f;
            _garg.Hunger.Value = 0f;
            targetCreature.flinch = 1f; 
            targetCreature.Scared.Value = 1f;
            _grab.GrabFish(targetCreature.gameObject);
            if (isGargBaby)
                targetCreature.liveMixin.TakeDamage(1f, targetCreature.transform.position);
            Destroy(targetCreature.GetComponent<EcoTarget>());
        }

        // Looks for what's the player is holding and eats it if it's a creature.
        private void GargantuanBabyNibble()
        {
            var held = Inventory.main.GetHeld();
            if (held is not null && held.GetComponent<Creature>() != null)
            {
                var heldLm = held.GetComponent<LiveMixin>();
                if (heldLm.maxHealth < 100f)
                {
                    animator.SetFloat("random", Random.value);
                    animator.SetTrigger("bite");
                    _attackSource.clip = _biteClipPool.GetRandomClip();
                    _attackSource.Play();
                    _garg.Hunger.Value -= 0.2f;
                    _garg.Happy.Value += 0.05f;
                    Destroy(held.gameObject);
                }
            }
        }

        private bool PlayerAttackable(Player player)
        {
            return player.CanBeAttacked() || player.liveMixin.IsAlive() || !player.cinematicModeActive || PlayerIsKillable();
        }

        private bool CanAttackTargetFromPosition(GameObject target) // A quick raycast check to stop the Gargantuan from attacking through walls. Taken from the game's code (shh).
        {
            var direction = target.transform.position - transform.position;
            var magnitude = direction.magnitude;
            var num = UWE.Utils.RaycastIntoSharedBuffer(transform.position, direction, magnitude, -5, QueryTriggerInteraction.Ignore);
            for (int i = 0; i < num; i++)
            {
                var collider = UWE.Utils.sharedHitBuffer[i].collider;
                var attachedRigidbody = collider.attachedRigidbody;
                var hitGameObject = attachedRigidbody ? attachedRigidbody.gameObject : collider.gameObject;
                if (hitGameObject != target && hitGameObject != gameObject && hitGameObject.GetComponent<Creature>() == null)
                {
                    return false;
                }
            }

            return true;
        }

        public override float GetBiteDamage(GameObject target) // Extra damage to Cyclops. Otherwise, does its base damage.
        {
            if (target.GetComponent<SubControl>() != null)
            {
                return 500f; // cyclops damage
            }

            return biteDamage; // base damage
        }

        public void OnVehicleReleased() // Called by gargantuan behavior. Gives a cooldown until the next bite.
        {
            _behaviour.timeCanAttackAgain = Time.time + 4f;
        }

        private IEnumerator PerformBiteAttack(GameObject target, float damage) // A delayed attack, to let him chomp down first.
        {
            animator.SetFloat(_gargRandomAnimParam, Random.value);
            animator.SetTrigger(_gargBiteAnimParam);
            _attackSource.clip = _biteClipPool.GetRandomClip();
            _attackSource.Play();
            yield return new WaitForSeconds(0.5f);
            if (target)
            {
                var targetLm = target.GetComponent<LiveMixin>();
                if (targetLm)
                {
                    targetLm.TakeDamage(damage, transform.position, DamageType.Normal, gameObject);
                    if (!targetLm.IsAlive())
                    {
                        creature.Aggression.Value = 0f;
                        creature.Hunger.Value = 0f;
                    }
                }
            }
        }

        private IEnumerator PerformPlayerCinematic(Player player)
        {
            if (oneShotPlayer)
            {
                CustomPDALinesManager.PlayPDAVoiceLine(ECCAudio.LoadAudioClip("PDADeathImminent"), "PDADeathImminent");
            }

            _playerDeathCinematic.enabled = true;
            _playerDeathCinematic.StartCinematicMode(player);
            float length = 1.8f;
            _attackSource.clip = _cinematicClipPool.GetRandomClip();
            _attackSource.Play();
            _behaviour.timeCanAttackAgain = Time.time + length;
            yield return new WaitForSeconds(length / 3f);
            var position = transform.position;
            Player.main.liveMixin.TakeDamage(5f, position, DamageType.Normal, gameObject);
            yield return new WaitForSeconds(length / 3f);
            Player.main.liveMixin.TakeDamage(5f, position, DamageType.Normal, gameObject);
            yield return new WaitForSeconds(length / 3f);
            _playerDeathCinematic.enabled = false;
            Player.main.liveMixin.TakeDamage(250f, position, DamageType.Normal, gameObject);
        }
    }
}