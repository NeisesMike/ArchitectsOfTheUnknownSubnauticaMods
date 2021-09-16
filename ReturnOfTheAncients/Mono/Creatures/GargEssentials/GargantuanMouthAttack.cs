namespace RotA.Mono.Creatures.GargEssentials
{
    using ECCLibrary;
    using ECCLibrary.Internal;
    using Prefabs.Creatures;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using static GargantuanConditions;

    class GargantuanMouthAttack : MeleeAttack
    {
        AudioSource attackSource;
        ECCAudio.AudioClipPool biteClipPool;
        ECCAudio.AudioClipPool cinematicClipPool;
        GargantuanBehaviour behaviour;
        GargantuanGrab grab;
        PlayerCinematicController playerDeathCinematic;

        public GameObject throat;
        public bool canAttackPlayer = true;
        public bool oneShotPlayer;
        public string attachBoneName;
        public bool canPerformCyclopsCinematic;
        public GargGrabFishMode grabFishMode;
        private static readonly int GargBiteAnimParam = Animator.StringToHash("bite");
        private static readonly int GargRandomAnimParam = Animator.StringToHash("random");

        void Start()
        {
            grab = GetComponent<GargantuanGrab>();
            attackSource = gameObject.AddComponent<AudioSource>();
            attackSource.minDistance = 10f;
            attackSource.maxDistance = 40f;
            attackSource.spatialBlend = 1f;
            attackSource.volume = ECCHelpers.GetECCVolume();
            attackSource.playOnAwake = false;
            biteClipPool = ECCAudio.CreateClipPool("GargBiteAttack");
            cinematicClipPool = ECCAudio.CreateClipPool("GargBiteAttack5");
            throat = gameObject.SearchChild("Head");
            gameObject.SearchChild("Mouth").EnsureComponent<OnTouch>().onTouch = new OnTouch.OnTouchEvent();
            gameObject.SearchChild("Mouth").EnsureComponent<OnTouch>().onTouch.AddListener(OnTouch);
            behaviour = GetComponent<GargantuanBehaviour>();

            playerDeathCinematic = gameObject.AddComponent<PlayerCinematicController>();
            playerDeathCinematic.animatedTransform = gameObject.SearchChild(attachBoneName).transform;
            playerDeathCinematic.animator = creature.GetAnimator();
            playerDeathCinematic.animParamReceivers = new GameObject[0];
            playerDeathCinematic.animParam = "cin_player";
            playerDeathCinematic.playerViewAnimationName = "seadragon_attack";
        }

        public override void OnTouch(Collider collider)
        {
            if (!liveMixin.IsAlive() || Time.time < behaviour.timeCanAttackAgain || playerDeathCinematic.IsCinematicModeActive())
                return;
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
            behaviour.timeCanAttackAgain = Time.time + 4f;
        }

        private IEnumerator PerformBiteAttack(GameObject target, float damage) // A delayed attack, to let him chomp down first.
        {
            animator.SetFloat(GargRandomAnimParam, UnityEngine.Random.value);
            animator.SetTrigger(GargBiteAnimParam);
            attackSource.clip = biteClipPool.GetRandomClip();
            attackSource.Play();
            yield return new WaitForSeconds(0.5f);
            if (target)
            {
                var targetLm = target.GetComponent<LiveMixin>();
                if (targetLm)
                {
                    targetLm.TakeDamage(damage, transform.position, DamageType.Normal, this.gameObject);
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

            playerDeathCinematic.enabled = true;
            playerDeathCinematic.StartCinematicMode(player);
            float length = 1.8f;
            attackSource.clip = cinematicClipPool.GetRandomClip();
            attackSource.Play();
            behaviour.timeCanAttackAgain = Time.time + length;
            yield return new WaitForSeconds(length / 3f);
            var position = transform.position;
            Player.main.liveMixin.TakeDamage(5f, position, DamageType.Normal, gameObject);
            yield return new WaitForSeconds(length / 3f);
            Player.main.liveMixin.TakeDamage(5f, position, DamageType.Normal, gameObject);
            yield return new WaitForSeconds(length / 3f);
            playerDeathCinematic.enabled = false;
            Player.main.liveMixin.TakeDamage(250f, position, DamageType.Normal, gameObject);
        }
    }
}