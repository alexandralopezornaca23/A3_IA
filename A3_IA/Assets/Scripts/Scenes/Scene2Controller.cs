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

        [Header("Clic")]
        public float clickProximityThreshold = 5f;
        public Camera mainCamera;

        private FormationManager formationManager;
        private BlackboardSystem blackboard;

        void Start()
        {
            if (mainCamera == null) mainCamera = Camera.main;

            formationManager = new FormationManager(
                new CirclePattern(formationCenter, circleRadius));

            foreach (AgentMovement agent in agents)
            {
                SetupSeparation(agent);
                formationManager.AddAgent(agent);
            }

            blackboard = new BlackboardSystem();
            blackboard.RegisterExpert(new TargetExpert(agents, formationManager));

            Debug.Log("Escena 2 Clic cerca de un agente para publicar objetivo.");
        }

        void Update()
        {
            formationManager.Update();

            if (Mouse.current.leftButton.wasPressedThisFrame) HandleClick();

            Action[] actions = blackboard.Update();
            if (actions != null)
                foreach (Action action in actions)
                    action?.Invoke();
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
                    Debug.Log($"Objetivo publicado: {clickPoint}");
                    return;
                }
            }

            Debug.Log("Ningún agente dentro del umbral.");
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