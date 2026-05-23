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

        public TargetExpert(List<AgentMovement> agents, FormationManager formationManager)
        {
            this.agents = agents;
            this.formationManager = formationManager;
        }

        public override float GetInsistence(BlackboardSystem blackboard) =>
            blackboard.HasKey("target") ? 0.95f : 0f;

        public override Action[] Run(BlackboardSystem blackboard)
        {
            Vector3 target = blackboard.GetValue<Vector3>("target");

            return new Action[]
            {
                () =>
                {
                    formationManager.SetActive(false);
                    foreach (AgentMovement agent in agents)
                        agent?.SetTarget(target);
                }
            };
        }
    }
}