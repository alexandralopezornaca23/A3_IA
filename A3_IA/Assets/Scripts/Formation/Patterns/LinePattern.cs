using UnityEngine;
using AI.Movement;

namespace AI.Formation.Patterns
{
    public class LinePattern : FormationPattern
    {
        private Vector3 anchor;
        private float spacing;
        private int maxSlots;
        private int cachedSlotCount = 1;

        public LinePattern(Vector3 anchor, float spacing = 2f, int maxSlots = 20)
        {
            this.anchor = anchor;
            this.spacing = spacing;
            this.maxSlots = maxSlots;
        }

        public void SetAnchor(Vector3 newAnchor) => anchor = newAnchor;

        public AgentData GetSlotTransform(int slotIndex)
        {
            int half = cachedSlotCount / 2;
            float xOffset = (slotIndex - half) * spacing;
            return new AgentData(new Vector3(xOffset, 0f, 0f));
        }

        public AgentData GetAnchorPoint() => new AgentData(anchor);

        public AgentData GetDriftOffset(SlotAssignment[] slots)
        {
            cachedSlotCount = Mathf.Max(slots.Length, 1);
            return new AgentData(Vector3.zero);
        }

        public bool SupportsSlots(int slotCount) =>
            slotCount > 0 && slotCount <= maxSlots;
    }
}