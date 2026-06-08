using UnityEngine;

namespace AI.Movement
{
    // Clase base abstracta de la que heredan todos los algoritmos de steering
    // Cada algoritmo implementa GetSteering devolviendo una aceleracion en Vector3
    public abstract class SteeringBehaviour : MonoBehaviour
    {
        // Calcula y devuelve la aceleracion que este behaviour quiere aplicar al agente
        public abstract Vector3 GetSteering(AgentData agent);
    }
}