using System;
using System.Collections.Generic;
using UnityEngine;
using AI.Movement;
using AI.Formation;

namespace AI.Blackboard.Experts
{
    // Experto de la escena 2 que reacciona cuando se publica un objetivo comun en la pizarra
    // Rompe la formacion y manda a todos los agentes hacia ese punto
    public class TargetExpert : Expert
    {
        private readonly List<AgentMovement> agents;
        private readonly FormationManager formationManager;

        // Flag que evita que el experto se ejecute mas de una vez
        // por cada objetivo publicado en la pizarra
        private bool executed = false;

        public TargetExpert(List<AgentMovement> agents, FormationManager formationManager)
        {
            this.agents = agents;
            this.formationManager = formationManager;
        }

        // Si no hay objetivo en la pizarra resetea el flag y devuelve 0
        // Si hay objetivo y aun no se ha ejecutado devuelve 0.95
        // Si ya se ejecuto devuelve 0 para no repetir la accion
        public override float GetInsistence(BlackboardSystem blackboard)
        {
            if (!blackboard.HasKey("target")) { executed = false; return 0f; }
            return executed ? 0f : 0.95f;
        }

        // Lee el objetivo de la pizarra, rompe la formacion
        // y dirige a todos los agentes hacia ese punto
        public override Action[] Run(BlackboardSystem blackboard)
        {
            Vector3 target = blackboard.GetValue<Vector3>("target");

            // Marcar como ejecutado antes de devolver la accion
            // para que GetInsistence devuelva 0 en el siguiente frame
            executed = true;

            return new Action[]
            {
                () =>
                {
                    // Desactiva la formacion para que los agentes se muevan libremente
                    formationManager.SetActive(false);

                    // Cancela la patrulla de cada agente y los manda al objetivo comun
                    foreach (AgentMovement agent in agents)
                    {
                        if (agent == null) continue;
                        agent.ResetPatrol();
                        agent.SetTarget(target);
                    }
                }
            };
        }
    }
}