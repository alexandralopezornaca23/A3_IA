using AI.Movement;

namespace AI.Formation
{
    // Vincula un agente con su indice dentro de la formacion
    // El FormationManager usa el indice para calcular la posicion del slot en el patron
    public class SlotAssignment
    {
        public AgentMovement agent;
        public int index;
    }
}