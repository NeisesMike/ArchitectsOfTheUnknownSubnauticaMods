using UnityEngine;

namespace RotA.Mono.Creatures
{
    public class AggressiveWhenSeeNonPlayer : AggressiveWhenSeeTarget
    {
        public override GameObject GetAggressionTarget()
        {
            var ecoTarget = EcoRegionManager.main.FindNearestTarget(targetType, transform.position, IsTargetValid, maxSearchRings);
            return ecoTarget?.GetGameObject();
        }
        
        private new bool IsTargetValid(IEcoTarget target)
        {
            return IsTargetValid(target.GetGameObject());
        }
        
        private new bool IsTargetValid(GameObject target)
        {
            if (target == Player.main.gameObject)
                return false;
            
            return base.IsTargetValid(target);
        }
    }
}