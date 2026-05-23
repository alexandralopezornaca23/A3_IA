using UnityEngine;
using System.Collections.Generic;

namespace AI.Movement
{
    public class ObstacleAvoidanceBehaviour : SteeringBehaviour
    {
        public float avoidRadius = 3f;
        public float avoidForce = 20f;

        public bool IsAvoiding { get; private set; }

        private List<Vector3> obstaclePositions = new List<Vector3>();

        public void SetObstacles(List<Vector3> obstacles)
        {
            obstaclePositions = obstacles ?? new List<Vector3>();
            IsAvoiding = obstaclePositions.Count > 0;
            enabled = IsAvoiding;
        }

        public void ClearObstacles()
        {
            obstaclePositions.Clear();
            IsAvoiding = false;
            enabled = false;
        }

        public override Vector3 GetSteering(AgentData agent)
        {
            Vector3 steering = Vector3.zero;

            foreach (Vector3 obs in obstaclePositions)
            {
                Vector3 direction = agent.position - obs;
                direction.y = 0f;
                float distance = direction.magnitude;

                if (distance < avoidRadius && distance > 0f)
                    steering += direction.normalized * (avoidForce / distance);
            }

            return steering;
        }
    }
}