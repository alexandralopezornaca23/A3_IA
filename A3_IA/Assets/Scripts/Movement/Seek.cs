using UnityEngine;

namespace AI.Movement
{
    // Algoritmo de persecucion basico que mueve al agente hacia el objetivo
    // a aceleracion maxima sin ningun tipo de desaceleracion al llegar
    public class Seek : SteeringBehaviour
    {
        public Vector3 targetPosition;
        public float maxAcceleration = 10f;

        public override Vector3 GetSteering(AgentData agent)
        {
            Vector3 direction = targetPosition - agent.position;

            // Evita normalizar un vector casi cero cuando el agente ya esta en el objetivo
            if (direction.magnitude < 0.01f) return Vector3.zero;

            // Devuelve siempre la aceleracion maxima en la direccion del objetivo
            // A diferencia de Arrive no frena al acercarse
            return direction.normalized * maxAcceleration;
        }
    }
}
