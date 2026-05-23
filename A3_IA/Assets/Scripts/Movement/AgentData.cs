using UnityEngine;

namespace AI.Movement
{
    [System.Serializable]
    public class AgentData
    {
        public Vector3 position;
        public Vector3 velocity;
        public float maxSpeed = 5f;

        public AgentData() { }

        public AgentData(Vector3 pos)
        {
            position = pos;
        }

        public static AgentData operator +(AgentData a, AgentData b) => new AgentData
        {
            position = a.position + b.position,
            velocity = a.velocity + b.velocity
        };

        public static AgentData operator -(AgentData a, AgentData b) => new AgentData
        {
            position = a.position - b.position,
            velocity = a.velocity - b.velocity
        };
    }
}