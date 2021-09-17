using UnityEngine;

namespace RotA.Mono.Creatures.CreatureActions
{
    [RequireComponent(typeof(SwimBehaviour))]
    public class AvoidObstaclesUwU : CreatureAction
    {
        public LastTarget lastTarget;
        
        public Vector3 positionOffset = Vector3.zero;
        
        public bool avoidTerrainOnly = true;
        
        public float avoidanceIterations = 10f;
        public float avoidanceDistance = 5f;
        public float avoidanceDuration = 2f;
        public float scanInterval = 1f;
        public float scanDistance = 2f;
        public float scanRadius;
        public float swimVelocity = 3f;
        public float swimInterval = 1f;

        protected float timeStartAvoidance;
        
        private Vector3 avoidancePosition;
        private bool swimDirectionFound;
        private float timeNextScan;
        private float timeNextSwim;

        public override float Evaluate(Creature creature)
        {
            if (Time.time < timeStartAvoidance + avoidanceDuration)
            {
                return GetEvaluatePriority();
            }

            if (Time.time > timeNextScan)
            {
                timeNextScan = Time.time + scanInterval;
                Transform transform = this.creature.transform;
                Vector3 origin = transform.TransformPoint(positionOffset);
                bool flag = false;
                RaycastHit raycastHit;
                if (scanRadius > 0f)
                {
                    if (Physics.SphereCast(origin, scanRadius, transform.forward, out raycastHit,
                        scanDistance, GetLayerMask(), QueryTriggerInteraction.Ignore))
                    {
                        flag = IsObstacle(raycastHit.collider);
                    }
                }
                else if (Physics.Raycast(origin, transform.forward, out raycastHit, scanDistance,
                    GetLayerMask(), QueryTriggerInteraction.Ignore))
                {
                    flag = IsObstacle(raycastHit.collider);
                }

                if (flag)
                {
                    swimDirectionFound = false;
                    return GetEvaluatePriority();
                }
            }

            return 0f;
        }
        
        public override void StopPerform(Creature creature)
        {
            timeStartAvoidance = 0f;
        }
        
        public override void Perform(Creature creature, float deltaTime)
        {
            if (Time.time > timeNextSwim)
            {
                if (!swimDirectionFound)
                {
                    FindSwimDirection();
                }

                timeNextSwim = Time.time + swimInterval;
                float velocity = Mathf.Lerp(swimVelocity, 0f, this.creature.Tired.Value);
                swimBehaviour.SwimTo(avoidancePosition, velocity);
            }
        }
        
        private void FindSwimDirection()
        {
            Vector3 vector = creature.transform.TransformPoint(positionOffset);
            avoidancePosition = vector;
            timeStartAvoidance = Time.time;
            swimDirectionFound = false;
            int layerMask = GetLayerMask();
            int num = 0;
            while ((float) num < avoidanceIterations)
            {
                Vector3 randomDirection = GetRandomDirection();
                RaycastHit raycastHit;
                if (!Physics.Raycast(vector, randomDirection, out raycastHit, avoidanceDistance, layerMask,
                    QueryTriggerInteraction.Ignore) || !IsObstacle(raycastHit.collider))
                {
                    avoidancePosition = vector + randomDirection * avoidanceDistance;
                    swimDirectionFound = true;
                    return;
                }

                num++;
            }

            timeStartAvoidance = 0f;
        }
        
        protected virtual Vector3 GetRandomDirection()
        {
            return Random.onUnitSphere;
        }
        
        protected int GetLayerMask()
        {
            if (!avoidTerrainOnly)
            {
                return -5;
            }

            return Voxeland.GetTerrainLayerMask();
        }
        
        protected virtual bool IsObstacle(Collider collider)
        {
            GameObject gameObject = (lastTarget != null) ? lastTarget.target : null;
            if (!avoidTerrainOnly && gameObject != null)
            {
                Rigidbody attachedRigidbody = collider.attachedRigidbody;
                if (((attachedRigidbody != null) ? attachedRigidbody.gameObject : collider.gameObject) == gameObject)
                {
                    return false;
                }
            }

            return true;
        }
        
        private void OnDrawGizmosSelected()
        {
            if (!enabled)
            {
                return;
            }

            Transform transform = creature.transform;
            Vector3 vector = transform.TransformPoint(positionOffset);
            bool flag = false;
            RaycastHit raycastHit;
            if (Physics.Raycast(vector, transform.forward, out raycastHit, scanDistance, GetLayerMask(),
                QueryTriggerInteraction.Ignore))
            {
                flag = IsObstacle(raycastHit.collider);
            }

            Gizmos.color = (flag ? Color.red : Color.green);
            Gizmos.DrawLine(vector, vector + transform.forward * scanDistance);
        }
    }
}