using UnityEngine;

namespace AI.Movement
{
    // Algoritmo de llegada con desaceleracion progresiva
    // El agente se acerca al objetivo a velocidad maxima, frena dentro del slowRadius
    // y se detiene completamente al entrar en el targetRadius
    public class Arrive : SteeringBehaviour
    {
        public Vector3 targetPosition;
        public float maxAcceleration = 10f;
        public float maxSpeed = 5f;
        public float targetRadius = 0.5f;  // distancia a la que se considera llegado
        public float slowRadius = 3f;    // distancia a la que empieza a frenar
        public float timeToTarget = 0.1f;  // tiempo para alcanzar la velocidad objetivo

        public override Vector3 GetSteering(AgentData agent)
        {
            Vector3 direction = targetPosition - agent.position;
            float distance = direction.magnitude;

            // Dentro del targetRadius aplica frenado activo en lugar de devolver cero
            // Esto cancela la velocidad acumulada del Rigidbody y evita la oscilacion
            if (distance < targetRadius)
                return -agent.velocity / timeToTarget;

            // Fuera del slowRadius va a velocidad maxima
            // Dentro del slowRadius la velocidad es proporcional a la distancia restante
            float targetSpeed = distance > slowRadius
                ? maxSpeed
                : maxSpeed * (distance / slowRadius);

            // Calcula la aceleracion necesaria para pasar de la velocidad actual a la objetivo
            Vector3 targetVelocity = direction.normalized * targetSpeed;
            Vector3 acceleration = (targetVelocity - agent.velocity) / timeToTarget;

            // Clamp para no superar la aceleracion maxima configurada
            if (acceleration.magnitude > maxAcceleration)
                acceleration = acceleration.normalized * maxAcceleration;

            return acceleration;
        }
    }
}