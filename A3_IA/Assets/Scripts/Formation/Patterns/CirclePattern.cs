using UnityEngine;
using AI.Movement;

namespace AI.Formation.Patterns
{
    public class CirclePattern : FormationPattern
    {
        private Vector3 center;
        private float radius;
        private int maxSlots;
        private int cachedSlotCount = 1;

        public CirclePattern(Vector3 center, float radius, int maxSlots = 20)
        {
            this.center = center;
            this.radius = radius;
            this.maxSlots = maxSlots;
        }

        public void SetCenter(Vector3 newCenter) => center = newCenter;

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

        public AgentData GetAnchorPoint() => new AgentData(center);

        public AgentData GetDriftOffset(SlotAssignment[] slots)
        {
            cachedSlotCount = Mathf.Max(slots.Length, 1);
            return new AgentData(Vector3.zero);
        }

        public bool SupportsSlots(int slotCount) =>
            slotCount > 0 && slotCount <= maxSlots;
    }
}
