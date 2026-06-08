using System.Collections.Generic;
using UnityEngine;
using AI.Movement;

namespace AI.Formation
{
    // Gestiona la lista de agentes en la formacion y calcula la posicion
    // objetivo de cada uno cada frame segun el patron activo
    public class FormationManager
    {
        private readonly List<SlotAssignment> slots = new List<SlotAssignment>();
        private FormationPattern pattern;

        // Desplazamiento de ajuste que devuelve el patron para centrar la formacion
        private AgentData driftOffset = new AgentData();

        // Cuando isActive es false los agentes se mueven libremente sin slots
        private bool isActive = true;

        public int SlotCount => slots.Count;
        public bool IsActive => isActive;

        public FormationManager(FormationPattern pattern)
        {
            this.pattern = pattern;
        }

        // Cambia el patron activo, recalcula los indices y cancela la patrulla
        // para que todos los agentes vayan directamente a sus nuevos slots
        public void SetPattern(FormationPattern newPattern)
        {
            pattern = newPattern;
            UpdateSlotAssignments();

            foreach (SlotAssignment slot in slots)
                slot.agent.ResetPatrol();
        }

        // Activa o desactiva la formacion sin eliminar los agentes de la lista
        public void SetActive(bool active) => isActive = active;

        // Reasigna los indices de todos los slots y actualiza el drift offset
        // Se llama siempre que cambia el numero de agentes o el patron (diapositiva 14)
        private void UpdateSlotAssignments()
        {
            for (int i = 0; i < slots.Count; i++)
                slots[i].index = i;

            driftOffset = pattern.GetDriftOffset(slots.ToArray());
        }

        // Anade un agente a la formacion si el patron soporta un slot mas
        // Devuelve false si la formacion ya esta llena (diapositiva 15)
        public bool AddAgent(AgentMovement agent)
        {
            if (!pattern.SupportsSlots(slots.Count + 1)) return false;

            slots.Add(new SlotAssignment { agent = agent });
            UpdateSlotAssignments();
            return true;
        }

        // Elimina un agente de la formacion y recalcula los indices restantes
        // Devuelve false si el agente no estaba en la formacion (diapositiva 16)
        public bool RemoveAgent(AgentMovement agent)
        {
            SlotAssignment found = slots.Find(s => s.agent == agent);
            if (found == null) return false;

            slots.Remove(found);
            UpdateSlotAssignments();
            return true;
        }

        // Calcula la posicion objetivo de cada slot y se la envia al agente
        // No se ejecuta si la formacion esta inactiva o hay menos de 2 agentes
        // SetTarget gestiona internamente si el agente debe ir al slot o patrullar (diapositiva 17)
        public void Update()
        {
            if (!isActive || slots.Count <= 1) return;

            foreach (SlotAssignment slot in slots)
            {
                // Posicion local del slot dentro del patron
                AgentData slotTransform = pattern.GetSlotTransform(slot.index);

                // Posicion global = ancla del patron + offset local del slot - drift
                Vector3 targetPos = pattern.GetAnchorPoint().position
                                  + slotTransform.position
                                  - driftOffset.position;
                targetPos.y = 0f;

                slot.agent.SetTarget(targetPos);
            }
        }

        public List<SlotAssignment> GetSlots() => slots;
    }
}