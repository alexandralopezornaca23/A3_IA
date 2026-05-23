using UnityEngine;
using System.Collections.Generic;

namespace AI.Movement
{
    public class Separation : SteeringBehaviour
    {
        public List<Transform> neighbours = new List<Transform>();
        public float separationRadius = 2f;
        public float maxAcceleration = 15f;
        public float decayCoefficient = 3f;

        public override Vector3 GetSteering(AgentData agent)
        {
            Vector3 steering = Vector3.zero;

            foreach (Transform neighbour in neighbours)
            {
                if (neighbour == null) continue;

                Vector3 direction = agent.position - neighbour.position;
                float distance = direction.magnitude;

                if (distance > 0f && distance < separationRadius)
                {
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