using UnityEngine;
using AI.Movement;

namespace AI.Formation.Patterns
{
    // Patron de formacion en circulo escalable
    // Distribuye los agentes equitativamente alrededor de un punto central
    public class CirclePattern : FormationPattern
    {
        private Vector3 center;
        private float radius;
        private int maxSlots;

        // Se guarda en cache el numero de slots para calcular los angulos correctamente
        // Se actualiza cada vez que cambia el numero de agentes en la formacion
        private int cachedSlotCount = 1;

        public CirclePattern(Vector3 center, float radius, int maxSlots = 20)
        {
            this.center = center;
            this.radius = radius;
            this.maxSlots = maxSlots;
        }

        // Permite mover el centro del circulo en tiempo de ejecucion
        // Lo usa el FormationMoveExpert en la Escena 2 y Scene3Controller en la Escena 3
        public void SetCenter(Vector3 newCenter) => center = newCenter;

        // Calcula la posicion local de cada slot dentro del circulo
        // Divide el circulo completo (2*PI) entre el numero de agentes
        // y asigna a cada slot el angulo correspondiente a su indice
        public AgentData GetSlotTransform(int slotIndex)
        {
            float angle = slotIndex * (2f * Mathf.PI / cachedSlotCount);
            Vector3 offset = new Vector3(
                Mathf.Cos(angle) * radius,
                0f,
                Mathf.Sin(angle) * radius
            );
            return new AgentData(offset);
        }

        // Devuelve el punto de anclaje global de la formacion
        // Es el centro alrededor del cual se distribuyen los slots
        public AgentData GetAnchorPoint() => new AgentData(center);

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