using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using AI.Movement;
using AI.Formation;
using AI.Formation.Patterns;

namespace AI.Scenes
{
    // Controlador de la Escena 1
    // Los agentes mantienen una formacion y el usuario puede cambiar entre
    // los tres patrones con las teclas 1, 2 y 3
    public class Scene1Controller : MonoBehaviour
    {
        [Header("Agentes")]
        public List<AgentMovement> agents = new List<AgentMovement>();

        [Header("Formacion")]
        public Vector3 formationCenter = Vector3.zero;
        public float circleRadius = 4f;
        public float spacing = 2f;

        private FormationManager formationManager;
        private FormationPattern[] patterns;
        private int currentPatternIndex;

        void Start()
        {
            // Crea los tres patrones con los valores configurados en el Inspector
            patterns = new FormationPattern[]
            {
                new CirclePattern(formationCenter, circleRadius),
                new VPattern(formationCenter, spacing),
                new LinePattern(formationCenter, spacing)
            };

            // Inicia la formacion con el patron circulo por defecto
            formationManager = new FormationManager(patterns[0]);

            // Registra cada agente en la formacion y configura sus vecinos de separacion
            foreach (AgentMovement agent in agents)
            {
                SetupSeparation(agent);
                formationManager.AddAgent(agent);
            }

            Debug.Log("Escena 1 Teclas 1, 2, 3 para cambiar de formacion.");
        }

        void Update()
        {
            // New Input System: detecta la pulsacion de las teclas numericas
            if (Keyboard.current.digit1Key.wasPressedThisFrame) ChangePattern(0);
            if (Keyboard.current.digit2Key.wasPressedThisFrame) ChangePattern(1);
            if (Keyboard.current.digit3Key.wasPressedThisFrame) ChangePattern(2);

            // Actualiza los objetivos de todos los slots cada frame
            formationManager.Update();
        }

        // Cambia al patron del indice indicado si es diferente al activo
        // Evita recalcular la formacion si se pulsa la tecla del patron ya activo
        void ChangePattern(int index)
        {
            if (index == currentPatternIndex) return;
            currentPatternIndex = index;
            formationManager.SetPattern(patterns[currentPatternIndex]);
            Debug.Log($"Formacion cambiada a: {patterns[currentPatternIndex].GetType().Name}");
        }

        // Asigna a cada agente la lista de todos los demas como vecinos de separacion
        // Esto permite que Separation sepa de quien alejarse para evitar colisiones
        void SetupSeparation(AgentMovement agent)
        {
            Separation sep = agent.GetComponent<Separation>();
            if (sep == null) return;

            sep.neighbours = new List<Transform>();
            foreach (AgentMovement other in agents)
                if (other != agent) sep.neighbours.Add(other.transform);
        }
    }
}