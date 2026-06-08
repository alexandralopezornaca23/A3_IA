using AI.Movement;

namespace AI.Formation
{
    // Interfaz que deben implementar todos los patrones de formacion
    // Permite al FormationManager trabajar con cualquier patron sin conocer su geometria
    public interface FormationPattern
    {
        // Devuelve la posicion local del slot en el patron segun su indice
        AgentData GetSlotTransform(int slotIndex);

        // Devuelve el punto de anclaje global de la formacion
        // Es el centro alrededor del cual se calculan todas las posiciones de los slots
        AgentData GetAnchorPoint();

        // Devuelve el desplazamiento de ajuste para centrar la formacion
        // Se llama cada vez que cambia el numero de agentes para recalcular el offset
        AgentData GetDriftOffset(SlotAssignment[] slots);

        // Indica si la formacion puede aceptar el numero de slots indicado
        bool SupportsSlots(int slotCount);
    }
}