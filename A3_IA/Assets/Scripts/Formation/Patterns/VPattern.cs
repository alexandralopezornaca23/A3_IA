using UnityEngine;
using AI.Movement;

namespace AI.Formation.Patterns
{
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

        public void SetAnchor(Vector3 newAnchor) => anchor = newAnchor;

        public AgentData GetSlotTransform(int slotIndex)
        {
            if (slotIndex == 0) return new AgentData(Vector3.zero);

            int row = (slotIndex + 1) / 2;
            float side = (slotIndex % 2 == 1) ? -1f : 1f;
            Vector3 offset = new Vector3(side * row * spacing, 0f, -row * spacing);
            return new AgentData(offset);
        }

        public AgentData GetAnchorPoint() => new AgentData(anchor);
        public AgentData GetDriftOffset(SlotAssignment[] slots) =>
            new AgentData(Vector3.zero);
        public bool SupportsSlots(int slotCount) =>
            slotCount > 0 && slotCount <= maxSlots;
    }
}