using UnityEngine;
using System.Collections.Generic;

namespace AI.Movement
{
    public class BlendedSteering : MonoBehaviour
    {
        [System.Serializable]
        public class WeightedBehaviour
        {
            public SteeringBehaviour behaviour;
            [Range(0f, 1f)] public float weight = 1f;
        }

        public List<WeightedBehaviour> behaviours = new List<WeightedBehaviour>();
        public float maxAcceleration = 10f;

        public Vector3 GetSteering(AgentData agent)
        {
            Vector3 steering = Vector3.zero;

            foreach (WeightedBehaviour wb in behaviours)
            {
                if (wb.behaviour != null && wb.behaviour.enabled)
                    steering += wb.behaviour.GetSteering(agent) * wb.weight;
            }

            if (steering.magnitude > maxAcceleration)
                steering = steering.normalized * maxAcceleration;

            return steering;
        }
    }
}
