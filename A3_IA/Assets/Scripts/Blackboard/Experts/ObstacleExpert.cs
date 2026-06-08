using System;
using System.Collections.Generic;
using UnityEngine;
using AI.Movement;

namespace AI.Blackboard.Experts
{
    // Experto de la Escena 3 que reacciona cuando hay obstaculos publicados en la pizarra
    // Activa el componente de evasion en todos los agentes con las posiciones detectadas
    public class ObstacleExpert : Expert
    {
        private readonly List<AgentMovement> agents;

        public ObstacleExpert(List<AgentMovement> agents)
        {
            this.agents = agents;
        }

        // Devuelve 0.9 si hay obstaculos reales en la pizarra
        // Devuelve 0.1 si la clave no existe o la lista esta vacia
        // El valor minimo de 0.1 evita que el experto quede completamente inactivo
        public override float GetInsistence(BlackboardSystem blackboard)
        {
            if (!blackboard.HasKey("obstacles")) return 0.1f;

            List<Vector3> obstacles = blackboard.GetValue<List<Vector3>>("obstacles");
            return (obstacles != null && obstacles.Count > 0) ? 0.9f : 0.1f;
        }

        // Lee las posiciones de los obstaculos de la pizarra y se las pasa
        // al componente ObstacleAvoidanceBehaviour de cada agente para que los evite
        public override Action[] Run(BlackboardSystem blackboard)
        {
            List<Vector3> obstacles = blackboard.GetValue<List<Vector3>>("obstacles");

            return new Action[]
            {
                () =>
                {
                    foreach (AgentMovement agent in agents)
                    {
                        if (agent == null) continue;

                        // SetObstacles activa el componente y le pasa las posiciones
                        // Si el agente no tiene el componente el operador ?. evita el error
                        agent.GetComponent<ObstacleAvoidanceBehaviour>()
                             ?.SetObstacles(obstacles);
                    }
                }
            };
        }
    }
}