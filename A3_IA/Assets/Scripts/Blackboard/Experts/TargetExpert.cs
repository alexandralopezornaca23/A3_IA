using System;
using System.Collections.Generic;
using UnityEngine;
using AI.Movement;
using AI.Formation;

namespace AI.Blackboard.Experts
{
    public class TargetExpert : Expert
    {
        private readonly List<AgentMovement> agents;
        private readonly FormationManager formationManager;
        private bool executed = false;

        public TargetExpert(List<AgentMovement> agents, FormationManager formationManager)
        {
            this.agents = agents;
            this.formationManager = formationManager;
        }

        public override float GetInsistence(BlackboardSystem blackboard)
        {
            // Solo actúa una vez por publicación de objetivo
            if (!blackboard.HasKey("target")) { executed = false; return 0f; }
            return executed ? 0f : 0.95f;
        }

        public override Action[] Run(BlackboardSystem blackboard)
        {
            Vector3 target = blackboard.GetValue<Vector3>("target");
            executed = true;

            return new Action[]
            {
                () =>
                {
                    // Rompe la formación
                    formationManager.SetActive(false);

                    // Resetea la patrulla de cada agente y manda al objetivo
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