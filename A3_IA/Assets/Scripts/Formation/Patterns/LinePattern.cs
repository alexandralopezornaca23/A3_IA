using UnityEngine;
using AI.Movement;

namespace AI.Formation.Patterns
{
    // Patron de formacion en linea recta escalable
    // Distribuye los agentes de forma simetrica sobre el eje X centrados en el ancla
    public class LinePattern : FormationPattern
    {
        private Vector3 anchor;
        private float spacing;
        private int maxSlots;

        // Se guarda en cache el numero de slots para calcular el centro de la linea
        // Se actualiza cada vez que cambia el numero de agentes en la formacion
        private int cachedSlotCount = 1;

        public LinePattern(Vector3 anchor, float spacing = 2f, int maxSlots = 20)
        {
            this.anchor = anchor;
            this.spacing = spacing;
            this.maxSlots = maxSlots;
        }

        // Permite mover el ancla de la linea en tiempo de ejecucion
        // Lo usa el FormationMoveExpert en la Escena 2
        public void SetAnchor(Vector3 newAnchor) => anchor = newAnchor;

        // Calcula la posicion local de cada slot dentro de la linea
        // Resta la mitad del total de slots para centrar la formacion en el ancla
        // Por ejemplo con 5 agentes: posiciones -2, -1, 0, 1, 2 multiplicadas por spacing
        public AgentData GetSlotTransform(int slotIndex)
        {
            int half = cachedSlotCount / 2;
            float xOffset = (slotIndex - half) * spacing;
            return new AgentData(new Vector3(xOffset, 0f, 0f));
        }

        // Devuelve el punto de anclaje global de la formacion
        // Es el centro alrededor del cual se distribuyen los slots en la linea
        public AgentData GetAnchorPoint() => new AgentData(anchor);

        // Actualiza el numero de slots en cache y devuelve desplazamiento cero
        // Se llama cada vez que se anade o elimina un agente de la formacion
        public AgentData GetDriftOffset(SlotAssignment[] slots)
        {
            cachedSlotCount = Mathf.Max(slots.Length, 1);
            return new AgentData(Vector3.zero);
        }

        // Comprueba si la formacion puede aceptar el numero de slots indicado
        public bool SupportsSlots(int slotCount) =>
            slotCount > 0 && slotCount <= maxSlots;
    }
}