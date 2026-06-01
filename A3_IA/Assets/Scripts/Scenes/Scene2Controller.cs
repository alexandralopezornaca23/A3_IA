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
    public class Scene2Controller : MonoBehaviour
    {
        [Header("Agentes")]
        public List<AgentMovement> agents = new List<AgentMovement>();

        [Header("Formación")]
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

        private CirclePattern circlePattern;
        private VPattern vPattern;
        private LinePattern linePattern;

        void Start()
        {
            if (mainCamera == null) mainCamera = Camera.main;

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

            blackboard = new BlackboardSystem();
            blackboard.SetData("patternIndex", 0);
            blackboard.RegisterExpert(
                new FormationMoveExpert(agents, formationManager, patterns, reformDelay));
        }

        void Update()
        {
            if (Keyboard.current.digit1Key.wasPressedThisFrame) ChangePattern(0);
            if (Keyboard.current.digit2Key.wasPressedThisFrame) ChangePattern(1);
            if (Keyboard.current.digit3Key.wasPressedThisFrame) ChangePattern(2);

            if (Mouse.current.leftButton.wasPressedThisFrame) HandleClick();

            Action[] actions = blackboard.Update();
            if (actions != null)
                foreach (Action action in actions)
                    action?.Invoke();

            formationManager.Update();
        }

        void ChangePattern(int index)
        {
            if (index == currentPatternIndex) return;
            currentPatternIndex = index;
            formationManager.SetPattern(patterns[currentPatternIndex]);
            blackboard.SetData("patternIndex", currentPatternIndex);
        }

        void HandleClick()
        {
            Vector2 mousePos = Mouse.current.position.ReadValue();
            Ray ray = mainCamera.ScreenPointToRay(mousePos);
            Plane groundPlane = new Plane(Vector3.up, Vector3.zero);

            if (!groundPlane.Raycast(ray, out float distance)) return;

            Vector3 clickPoint = ray.GetPoint(distance);
            clickPoint.y = 0f;

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