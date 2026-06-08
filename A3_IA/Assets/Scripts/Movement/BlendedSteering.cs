using UnityEngine;
using System.Collections.Generic;

namespace AI.Movement
{
    // Combina multiples algoritmos de steering sumando sus aceleraciones con pesos
    // Permite mezclar Arrive, Separation y ObstacleAvoidance en un solo resultado
    public class BlendedSteering : MonoBehaviour
    {
        // Cada entrada de la lista asocia un behaviour con su peso de influencia
        [System.Serializable]
        public class WeightedBehaviour
        {
            public SteeringBehaviour behaviour;
            [Range(0f, 1f)] public float weight = 1f;
        }

        public List<WeightedBehaviour> behaviours = new List<WeightedBehaviour>();
        public float maxAcceleration = 10f;

        // Suma las aceleraciones de todos los behaviours activos ponderadas por su peso
        // Solo incluye los behaviours que no son null y cuyo componente esta habilitado
        // Esto permite activar y desactivar comportamientos en tiempo de ejecucion
        public Vector3 GetSteering(AgentData agent)
        {
            Vector3 steering = Vector3.zero;

            foreach (WeightedBehaviour wb in behaviours)
            {
                if (wb.behaviour != null && wb.behaviour.enabled)
                    steering += wb.behaviour.GetSteering(agent) * wb.weight;
            }

            // Clamp del resultado final para no superar la aceleracion maxima
            if (steering.magnitude > maxAcceleration)
                steering = steering.normalized * maxAcceleration;

            return steering;
        }
    }
}
