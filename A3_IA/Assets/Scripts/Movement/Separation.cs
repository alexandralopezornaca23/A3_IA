using UnityEngine;
using System.Collections.Generic;

namespace AI.Movement
{
    // Aleja al agente de sus vecinos para evitar que se solapen dentro de la formacion
    // La lista de vecinos se asigna desde el controlador de escena al inicio
    public class Separation : SteeringBehaviour
    {
        public List<Transform> neighbours = new List<Transform>();
        public float separationRadius = 2f;
        public float maxAcceleration = 15f;
        public float decayCoefficient = 3f;

        // Suma una fuerza de alejamiento por cada vecino dentro del radio de separacion
        // La fuerza crece cuanto mas cerca esta el vecino: decayCoefficient / distancia^2
        public override Vector3 GetSteering(AgentData agent)
        {
            Vector3 steering = Vector3.zero;

            foreach (Transform neighbour in neighbours)
            {
                if (neighbour == null) continue;

                // Direccion desde el vecino hacia el agente (fuerza de alejamiento)
                Vector3 direction = agent.position - neighbour.position;
                float distance = direction.magnitude;

                // Solo aplica la fuerza si el vecino esta dentro del radio
                // El check distance > 0 evita la division por cero si dos agentes coinciden
                if (distance > 0f && distance < separationRadius)
                {
                    // Mathf.Min clampea la fuerza para no superar maxAcceleration
                    // cuando la distancia es muy pequeña y el resultado seria muy alto
                    float strength = Mathf.Min(
                        decayCoefficient / (distance * distance),
                        maxAcceleration
                    );
                    steering += direction.normalized * strength;
                }
            }

            return steering;
        }
    }
}