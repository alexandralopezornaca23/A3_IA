using UnityEngine;
using AI.Movement;

namespace AI.Formation.Patterns
{
    // Patron de formacion en V escalable
    // El slot 0 va al frente como lider y el resto se coloca alternando
    // izquierda y derecha hacia atras formando la V
    public class VPattern : FormationPattern
    {
        private Vector3 anchor;
        private float spacing;
        private int maxSlots;

        public VPattern(Vector3 anchor, float spacing = 2f, int maxSlots = 20)
        {
            this.anchor = anchor;
            this.spacing = spacing;
            this.maxSlots = maxSlots;
        }

        // Permite mover el ancla de la V en tiempo de ejecucion
        // Lo usa el FormationMoveExpert en la Escena 2
        public void SetAnchor(Vector3 newAnchor) => anchor = newAnchor;

        // Calcula la posicion local de cada slot dentro de la V
        // El slot 0 ocupa la punta delantera de la V sin desplazamiento
        // Los slots impares van a la izquierda y los pares a la derecha
        // La fila aumenta con cada par de agentes alejandose hacia atras en Z
        public AgentData GetSlotTransform(int slotIndex)
        {
            // El lider siempre ocupa la punta de la V
            if (slotIndex == 0) return new AgentData(Vector3.zero);

            // row indica a que fila de la V pertenece este slot
            // side alterna entre izquierda (-1) y derecha (1) segun si el indice es impar o par
            int row = (slotIndex + 1) / 2;
            float side = (slotIndex % 2 == 1) ? -1f : 1f;

            // Z negativo coloca el slot detras del lider
            // X positivo o negativo lo coloca a un lado segun su turno
            Vector3 offset = new Vector3(side * row * spacing, 0f, -row * spacing);
            return new AgentData(offset);
        }

        // Devuelve el punto de anclaje global de la formacion
        // Es el punto donde se coloca la punta de la V
        public AgentData GetAnchorPoint() => new AgentData(anchor);

        // La V no necesita ajuste de desplazamiento porque el slot 0 ya esta en el origen
        public AgentData GetDriftOffset(SlotAssignment[] slots) =>
            new AgentData(Vector3.zero);

        // Comprueba si la formacion puede aceptar el numero de slots indicado
        public bool SupportsSlots(int slotCount) =>
            slotCount > 0 && slotCount <= maxSlots;
    }
}