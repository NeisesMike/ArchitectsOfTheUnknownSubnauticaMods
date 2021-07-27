using RotA.Interfaces;
using UnityEngine;

namespace RotA.Mono.Equipment.IonKnifeActions
{
    public class OmegaCubeAction : IIonKnifeAction
    {
        float hitForce = 5000f;

        public void Initialize(IonKnife ionKnife)
        {
            ionKnife.Damage = new[] { 40f, 60f, 40f };
            ionKnife.AttackDistance = 1.8f;
            ionKnife.DamageType = new[] { DamageType.Heat, DamageType.Electrical, DamageType.Normal };
            ionKnife.PlaySwitchSound("event:/env/damage/cold_loop");
            ionKnife.VfxEventType = VFXEventTypes.diamondBlade;
            ionKnife.ResourceBonus = 3;

            ionKnife.SetMaterialColors(new Color(.3f, .3f, .3f), Color.white,
                new Color(1f, 2f, 1.25f), new Color(.5f, .5f, .5f));

            ionKnife.SetLightAppearance(new Color(1f, 1.5f, 1.25f) / 1.5f, 20f, 1.5f);
        }

        public void OnHit(IonKnife ionKnife, LiveMixin hitLiveMixin)
        {
            if (IonKnife.IsCreature(hitLiveMixin))
            {
                Utils.PlayFMODAsset(ionKnife.StrongHitFishSound, ionKnife.transform);
            }
            Rigidbody hitRb = hitLiveMixin.GetComponent<Rigidbody>();
            if (hitRb != null)
            {
                Vector3 playerDirection = ionKnife.usingPlayer.viewModelCamera.transform.forward;
                hitRb.AddForce(playerDirection * hitForce, ForceMode.Impulse);
            }
        }

        public void OnUpdate(IonKnife ionKnife)
        {

        }
    }
}