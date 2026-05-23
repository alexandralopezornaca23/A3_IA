using UnityEngine;

namespace AI.Movement
{
    public abstract class SteeringBehaviour : MonoBehaviour
    {
        public abstract Vector3 GetSteering(AgentData agent);
    }
}