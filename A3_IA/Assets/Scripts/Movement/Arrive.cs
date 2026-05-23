using UnityEngine;

namespace AI.Movement
{
    public class Arrive : SteeringBehaviour
    {
        public Vector3 targetPosition;
        public float maxAcceleration = 10f;
        public float maxSpeed = 5f;
        public float targetRadius = 0.5f;
        public float slowRadius = 3f;
        public float timeToTarget = 0.1f;

        public override Vector3 GetSteering(AgentData agent)
        {
            Vector3 direction = targetPosition - agent.position;
            float distance = direction.magnitude;

            if (distance < targetRadius)
            {
                // Devolver una fuerza de frenado activa en lugar de zero
                // Esto para la velocidad acumulada del Rigidbody
                return -agent.velocity / timeToTarget;
            }

            float targetSpeed = distance > slowRadius
                ? maxSpeed
                : maxSpeed * (distance / slowRadius);

            Vector3 targetVelocity = direction.normalized * targetSpeed;
            Vector3 acceleration = (targetVelocity - agent.velocity) / timeToTarget;

            if (acceleration.magnitude > maxAcceleration)
                acceleration = acceleration.normalized * maxAcceleration;

            return acceleration;
        }
    }
}