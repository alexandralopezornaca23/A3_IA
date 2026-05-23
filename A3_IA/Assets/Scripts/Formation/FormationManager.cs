using System.Collections.Generic;
using UnityEngine;
using AI.Movement;

namespace AI.Formation
{
    public class FormationManager
    {
        private readonly List<SlotAssignment> slots = new List<SlotAssignment>();
        private FormationPattern pattern;
        private AgentData driftOffset = new AgentData();
        private bool isActive = true;

        public int SlotCount => slots.Count;
        public bool IsActive => isActive;

        public FormationManager(FormationPattern pattern)
        {
            this.pattern = pattern;
        }

        public void SetPattern(FormationPattern newPattern)
        {
            pattern = newPattern;
            UpdateSlotAssignments();

            // Al cambiar de patrón, resetear la patrulla de todos los agentes
            // para que vayan directamente a su nuevo slot
            foreach (SlotAssignment slot in slots)
                slot.agent.ResetPatrol();
        }

        public void SetActive(bool active) => isActive = active;

        // Diapositiva 14
        private void UpdateSlotAssignments()
        {
            for (int i = 0; i < slots.Count; i++)
                slots[i].index = i;

            driftOffset = pattern.GetDriftOffset(slots.ToArray());
        }

        // Diapositiva 15
        public bool AddAgent(AgentMovement agent)
        {
            if (!pattern.SupportsSlots(slots.Count + 1)) return false;

            slots.Add(new SlotAssignment { agent = agent });
            UpdateSlotAssignments();
            return true;
        }

        // Diapositiva 16
        public bool RemoveAgent(AgentMovement agent)
        {
            SlotAssignment found = slots.Find(s => s.agent == agent);
            if (found == null) return false;

            slots.Remove(found);
            UpdateSlotAssignments();
            return true;
        }

        // Diapositiva 17
        public void Update()
        {
            if (!isActive || slots.Count <= 1) return;

            foreach (SlotAssignment slot in slots)
            {
                AgentData slotTransform = pattern.GetSlotTransform(slot.index);

                Vector3 targetPos = pattern.GetAnchorPoint().position
                                  + slotTransform.position
                                  - driftOffset.position;
                targetPos.y = 0f;

                // SetTarget ya gestiona si ir al slot o patrullar
                slot.agent.SetTarget(targetPos);
            }
        }

        public List<SlotAssignment> GetSlots() => slots;
    }
}