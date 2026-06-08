using System;

namespace AI.Blackboard
{
    // Estructura de datos que representa una entrada en la pizarra
    // Cada entrada tiene una clave unica, el tipo del valor y el valor en si
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

            // Obtiene el tipo del valor automaticamente al crearlo
            // El operador ?. evita el error si value es null
            this.type = value?.GetType();
        }
    }
}