using System;

namespace AI.Blackboard
{
    // Clase base de la que heredan todos los expertos de la pizarra
    // Cada experto especializado implementa su propia logica en estos dos metodos
    public abstract class Expert
    {
        // El arbitro llama a este metodo para saber cuanto quiere actuar este experto
        // Debe devolver un valor entre 0 y 1, siendo 1 la maxima urgencia
        public abstract float GetInsistence(BlackboardSystem blackboard);

        // El arbitro llama a este metodo solo en el experto con mayor insistencia
        // Devuelve un array de acciones que se ejecutan en el controlador de escena
        public abstract Action[] Run(BlackboardSystem blackboard);
    }
}