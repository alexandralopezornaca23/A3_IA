using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using AI.Movement;
using AI.Formation;
using AI.Formation.Patterns;

namespace AI.Scenes
{
    public class Scene1Controller : MonoBehaviour
    {
        [Header("Agentes")]
        public List<AgentMovement> agents = new List<AgentMovement>();

        [Header("Formación")]
        public Vector3 formationCenter = Vector3.zero;
        public float circleRadius = 4f;
        public float spacing = 2f;

        private FormationManager formationManager;
        private FormationPattern[] patterns;
        private int currentPatternIndex;

        void Start()
        {
            patterns = new FormationPattern[]
            {
                new CirclePattern(formationCenter, circleRadius),
                new VPattern(formationCenter, spacing),
                new LinePattern(formationCenter, spacing)
            };

            formationManager = new FormationManager(patterns[0]);

            foreach (AgentMovement agent in agents)
            {
                SetupSeparation(agent);
                formationManager.AddAgent(agent);
            }

            Debug.Log("Escena 1 Teclas 1, 2, 3 para cambiar de formación.");
        }

        void Update()
        {
            if (Keyboard.current.digit1Key.wasPressedThisFrame) ChangePattern(0);
            if (Keyboard.current.digit2Key.wasPressedThisFrame) ChangePattern(1);
            if (Keyboard.current.digit3Key.wasPressedThisFrame) ChangePattern(2);

            formationManager.Update();
        }

        void ChangePattern(int index)
        {
            if (index == currentPatternIndex) return;
            currentPatternIndex = index;
            formationManager.SetPattern(patterns[currentPatternIndex]);
            Debug.Log($"Formación ? {patterns[currentPatternIndex].GetType().Name}");
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