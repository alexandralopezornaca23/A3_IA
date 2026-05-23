using UnityEngine;

namespace AI.Movement
{
    public class Seek : SteeringBehaviour
    {
        public Vector3 targetPosition;
        public float maxAcceleration = 10f;

        public override Vector3 GetSteering(AgentData agent)
        {
            Vector3 direction = targetPosition - agent.position;
            if (direction.magnitude < 0.01f) return Vector3.zero;
            return direction.normalized * maxAcceleration;
        }
    }
}
