using UnityEngine;
using System.Collections.Generic;

namespace AI.Movement
{
    // Behaviour de evasion de obstaculos que aplica una fuerza de repulsion
    // Solo se activa cuando el ObstacleExpert publica obstaculos en la pizarra
    public class ObstacleAvoidanceBehaviour : SteeringBehaviour
    {
        public float avoidRadius = 3f;   // distancia a partir de la que empieza a repeler
        public float avoidForce = 20f;  // intensidad maxima de la fuerza de repulsion

        // Propiedad publica de solo lectura para que AgentAnimatorController
        // pueda saber si el agente esta esquivando sin acceder a la lista interna
        public bool IsAvoiding { get; private set; }

        private List<Vector3> obstaclePositions = new List<Vector3>();

        // Recibe las posiciones de los obstaculos desde el ObstacleExpert
        // Activa el componente automaticamente si hay obstaculos en la lista
        public void SetObstacles(List<Vector3> obstacles)
        {
            obstaclePositions = obstacles ?? new List<Vector3>();
            IsAvoiding = obstaclePositions.Count > 0;

            // Activar o desactivar el componente controla si BlendedSteering lo incluye
            enabled = IsAvoiding;
        }

        // Limpia la lista y desactiva el componente cuando no hay obstaculos
        // Lo llama Scene3Controller cuando la pizarra queda sin obstaculos
        public void ClearObstacles()
        {
            obstaclePositions.Clear();
            IsAvoiding = false;
            enabled = false;
        }

        // Calcula la fuerza de repulsion sumando la contribucion de cada obstaculo cercano
        // La fuerza es inversamente proporcional a la distancia: mas cerca, mas intensa
        public override Vector3 GetSteering(AgentData agent)
        {
            Vector3 steering = Vector3.zero;

            foreach (Vector3 obs in obstaclePositions)
            {
                // Direccion desde el obstaculo hacia el agente (fuerza de alejamiento)
                Vector3 direction = agent.position - obs;
                direction.y = 0f;
                float distance = direction.magnitude;

                // Solo aplica la fuerza si el obstaculo esta dentro del radio de evasion
                // El check distance > 0 evita la division por cero si coinciden posiciones
                if (distance < avoidRadius && distance > 0f)
                    steering += direction.normalized * (avoidForce / distance);
            }

            return steering;
        }
    }
}