using UnityEngine;
using UnityEngine.InputSystem;
using System;
using System.Collections.Generic;
using AI.Movement;
using AI.Blackboard;
using AI.Blackboard.Experts;
using AI.Formation;
using AI.Formation.Patterns;

namespace AI.Scenes
{
    // Controlador de la Escena 2
    // Los agentes mantienen una formacion con los tres patrones disponibles
    // Al hacer clic cerca de un agente se publica un objetivo en la pizarra
    // El FormationMoveExpert rompe la formacion, manda a todos al objetivo
    // y pasado reformDelay segundos reactiva la formacion en el nuevo punto
    public class Scene2Controller : MonoBehaviour
    {
        [Header("Agentes")]
        public List<AgentMovement> agents = new List<AgentMovement>();

        [Header("Formacion")]
        public Vector3 formationCenter = Vector3.zero;
        public float circleRadius = 4f;
        public float spacing = 2f;

        [Header("Clic")]
        public float clickProximityThreshold = 5f;
        public Camera mainCamera;

        [Header("Comportamiento")]
        [Tooltip("Segundos que esperan en el objetivo antes de volver a formar")]
        public float reformDelay = 3f;

        private FormationManager formationManager;
        private FormationPattern[] patterns;
        private int currentPatternIndex;
        private BlackboardSystem blackboard;

        // Se guardan referencias individuales a cada patron porque el FormationMoveExpert
        // necesita llamar a SetCenter o SetAnchor para mover el ancla al nuevo punto
        private CirclePattern circlePattern;
        private VPattern vPattern;
        private LinePattern linePattern;

        void Start()
        {
            if (mainCamera == null) mainCamera = Camera.main;

            // Crear los patrones guardando referencia individual a cada uno
            circlePattern = new CirclePattern(formationCenter, circleRadius);
            vPattern = new VPattern(formationCenter, spacing);
            linePattern = new LinePattern(formationCenter, spacing);

            patterns = new FormationPattern[] { circlePattern, vPattern, linePattern };

            formationManager = new FormationManager(patterns[0]);

            foreach (AgentMovement agent in agents)
            {
                SetupSeparation(agent);
                formationManager.AddAgent(agent);
            }

            // Iniciar la pizarra y publicar el indice del patron activo
            // para que el FormationMoveExpert sepa cual mover cuando llegue el clic
            blackboard = new BlackboardSystem();
            blackboard.SetData("patternIndex", 0);
            blackboard.RegisterExpert(
                new FormationMoveExpert(agents, formationManager, patterns, reformDelay));
        }

        void Update()
        {
            // Cambio de patron con teclas igual que en la Escena 1
            if (Keyboard.current.digit1Key.wasPressedThisFrame) ChangePattern(0);
            if (Keyboard.current.digit2Key.wasPressedThisFrame) ChangePattern(1);
            if (Keyboard.current.digit3Key.wasPressedThisFrame) ChangePattern(2);

            // Detectar clic del raton con el New Input System
            if (Mouse.current.leftButton.wasPressedThisFrame) HandleClick();

            // El arbitro de la pizarra evalua los expertos y ejecuta las acciones
            // del que mayor insistencia devuelva este frame
            Action[] actions = blackboard.Update();
            if (actions != null)
                foreach (Action action in actions)
                    action?.Invoke();

            formationManager.Update();
        }

        // Cambia el patron activo y actualiza el indice en la pizarra
        // para que el FormationMoveExpert mueva el ancla del patron correcto
        void ChangePattern(int index)
        {
            if (index == currentPatternIndex) return;
            currentPatternIndex = index;
            formationManager.SetPattern(patterns[currentPatternIndex]);
            blackboard.SetData("patternIndex", currentPatternIndex);
        }

        // Lanza un raycast desde la camara al plano XZ en la posicion del clic
        // Si algun agente esta dentro del umbral publica el punto en la pizarra
        void HandleClick()
        {
            Vector2 mousePos = Mouse.current.position.ReadValue();
            Ray ray = mainCamera.ScreenPointToRay(mousePos);
            Plane groundPlane = new Plane(Vector3.up, Vector3.zero);

            if (!groundPlane.Raycast(ray, out float distance)) return;

            Vector3 clickPoint = ray.GetPoint(distance);
            clickPoint.y = 0f;

            // Solo publica el objetivo si el clic cae cerca de algun agente
            // Esto evita que cualquier clic en el suelo active el comportamiento
            foreach (AgentMovement agent in agents)
            {
                if (Vector3.Distance(agent.transform.position, clickPoint)
                    <= clickProximityThreshold)
                {
                    blackboard.SetData("target", clickPoint);
                    return;
                }
            }
        }

        // Asigna a cada agente la lista de todos los demas como vecinos de separacion
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