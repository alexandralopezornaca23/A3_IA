using AI.Movement;

namespace AI.Formation
{
    public interface FormationPattern
    {
        AgentData GetSlotTransform(int slotIndex);
        AgentData GetAnchorPoint();
        AgentData GetDriftOffset(SlotAssignment[] slots);
        bool SupportsSlots(int slotCount);
    }
}