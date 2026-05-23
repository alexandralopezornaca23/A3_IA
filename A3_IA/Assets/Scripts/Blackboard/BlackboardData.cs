using System;

namespace AI.Blackboard
{
    [Serializable]
    public class BlackboardData
    {
        public string key;
        public Type type;
        public object value;

        public BlackboardData(string key, object value)
        {
            this.key = key;
            this.value = value;
            this.type = value?.GetType();
        }
    }
}