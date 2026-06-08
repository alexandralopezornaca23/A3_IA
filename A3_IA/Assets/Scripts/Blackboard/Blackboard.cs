using System;
using System.Collections.Generic;

namespace AI.Blackboard
{
    // Pizarra central compartida por todos los agentes
    // Se llama BlackboardSystem en lugar de Blackboard para evitar colision
    // con el nombre del propio namespace AI.Blackboard en C#
    public class BlackboardSystem
    {
        // Lista de datos publicados en la pizarra en formato clave-valor
        private readonly List<BlackboardData> entries = new List<BlackboardData>();

        // Lista de expertos registrados que el arbitro evaluara cada frame
        private readonly List<Expert> experts = new List<Expert>();

        // Si la clave ya existe actualiza su valor, si no la crea nueva
        public void SetData(string key, object value)
        {
            BlackboardData existing = GetDataByKey(key);
            if (existing != null) existing.value = value;
            else entries.Add(new BlackboardData(key, value));
        }

        // Elimina todas las entradas que coincidan con la clave
        public void RemoveData(string key) =>
            entries.RemoveAll(e => e.key == key);

        // Comprueba si existe una entrada con esa clave sin devolver su valor
        public bool HasKey(string key) =>
            entries.Exists(e => e.key == key);

        // Recorre la lista de entradas y devuelve la que coincide con la clave
        // Devuelve null si no encuentra ninguna (diapositiva 16 del pseudocodigo)
        public BlackboardData GetDataByKey(string key)
        {
            foreach (BlackboardData entry in entries)
                if (entry.key == key) return entry;
            return null;
        }

        // Version generica de GetDataByKey que devuelve el valor ya convertido al tipo T
        // Devuelve el valor por defecto del tipo si la clave no existe o el tipo no coincide
        public T GetValue<T>(string key)
        {
            BlackboardData data = GetDataByKey(key);
            if (data != null && data.value is T typed) return typed;
            return default;
        }

        // Registra un experto en la pizarra evitando duplicados
        public void RegisterExpert(Expert expert)
        {
            if (!experts.Contains(expert)) experts.Add(expert);
        }

        // Elimina un experto de la lista para que el arbitro deje de evaluarlo
        public void UnregisterExpert(Expert expert) =>
            experts.Remove(expert);

        // Arbitro: evalua la insistencia de todos los expertos cada frame
        // Selecciona el de mayor valor y ejecuta sus acciones
        // Sigue el pseudocodigo de la diapositiva 15
        public Action[] Update()
        {
            if (experts.Count == 0) return null;

            Expert bestExpert = null;
            float bestInsistence = float.MinValue;

            // Recorre todos los expertos y guarda el que mayor insistencia devuelve
            foreach (Expert expert in experts)
            {
                float insistence = expert.GetInsistence(this);
                if (insistence > bestInsistence)
                {
                    bestInsistence = insistence;
                    bestExpert = expert;
                }
            }

            // El operador ?. evita el error si ningun experto devolvio insistencia valida
            return bestExpert?.Run(this);
        }
    }
}