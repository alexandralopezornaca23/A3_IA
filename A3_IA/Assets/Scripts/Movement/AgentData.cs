using UnityEngine;

namespace AI.Movement
{
    // Representa el estado cinematico de un agente: posicion, velocidad y velocidad maxima
    // Equivale al tipo Static del pseudocodigo de los apuntes
    [System.Serializable]
    public class AgentData
    {
        public Vector3 position;
        public Vector3 velocity;
        public float maxSpeed = 5f;

        public AgentData() { }

        // Constructor rapido para crear un AgentData solo con posicion
        // Lo usan los patrones para devolver la posicion de un slot
        public AgentData(Vector3 pos)
        {
            position = pos;
        }

        // Operadores de suma y resta para combinar posiciones y velocidades
        // Los usa el FormationManager para calcular la posicion global de cada slot:
        // posicion global = ancla + slot local - drift
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