using System;
using System.Collections.Generic;

namespace AI.Blackboard
{
    // La clase se llama BlackboardSystem para no colisionar con el namespace AI.Blackboard
    public class BlackboardSystem
    {
        private readonly List<BlackboardData> entries = new List<BlackboardData>();
        private readonly List<Expert> experts = new List<Expert>();

        // Datos
        public void SetData(string key, object value)
        {
            BlackboardData existing = GetDataByKey(key);
            if (existing != null) existing.value = value;
            else entries.Add(new BlackboardData(key, value));
        }

        public void RemoveData(string key) =>
            entries.RemoveAll(e => e.key == key);

        public bool HasKey(string key) =>
            entries.Exists(e => e.key == key);

        // diapositiva 16
        public BlackboardData GetDataByKey(string key)
        {
            foreach (BlackboardData entry in entries)
                if (entry.key == key) return entry;
            return null;
        }

        public T GetValue<T>(string key)
        {
            BlackboardData data = GetDataByKey(key);
            if (data != null && data.value is T typed) return typed;
            return default;
        }

        // Expertos
        public void RegisterExpert(Expert expert)
        {
            if (!experts.Contains(expert)) experts.Add(expert);
        }

        public void UnregisterExpert(Expert expert) =>
            experts.Remove(expert);

        // Árbitro: selecciona el experto de mayor insistencia
        // Sigue el pseudocódigo de la diapositiva 15
        public Action[] Update()
        {
            if (experts.Count == 0) return null;

            Expert bestExpert = null;
            float bestInsistence = float.MinValue;

            foreach (Expert expert in experts)
            {
                float insistence = expert.GetInsistence(this);
                if (insistence > bestInsistence)
                {
                    bestInsistence = insistence;
                    bestExpert = expert;
                }
            }

            return bestExpert?.Run(this);
        }
    }
}