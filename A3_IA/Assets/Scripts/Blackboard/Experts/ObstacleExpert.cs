using System;
using System.Collections.Generic;
using UnityEngine;
using AI.Movement;

namespace AI.Blackboard.Experts
{
    public class ObstacleExpert : Expert
    {
        private readonly List<AgentMovement> agents;

        public ObstacleExpert(List<AgentMovement> agents)
        {
            this.agents = agents;
        }

        public override float GetInsistence(BlackboardSystem blackboard)
        {
            if (!blackboard.HasKey("obstacles")) return 0.1f;
            List<Vector3> obstacles = blackboard.GetValue<List<Vector3>>("obstacles");
            return (obstacles != null && obstacles.Count > 0) ? 0.9f : 0.1f;
        }

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
                        agent.GetComponent<ObstacleAvoidanceBehaviour>()
                             ?.SetObstacles(obstacles);
                    }
                }
            };
        }
    }
}