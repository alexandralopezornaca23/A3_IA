using System;

namespace AI.Blackboard
{
    public abstract class Expert
    {
        public abstract float GetInsistence(BlackboardSystem blackboard);
        public abstract Action[] Run(BlackboardSystem blackboard);
    }
}