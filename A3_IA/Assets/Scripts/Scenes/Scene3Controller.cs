using UnityEngine;
using System;
using System.Collections.Generic;
using AI.Movement;
using AI.Blackboard;
using AI.Blackboard.Experts;
using AI.Formation;
using AI.Formation.Patterns;

namespace AI.Scenes
{
    public class Scene3Controller : MonoBehaviour
    {
        [Header("Agentes")]
        public List<AgentMovement> agents = new List<AgentMovement>();

        [Header("Formación en movimiento")]
        public float circleRadius = 4f;
        public float formationMoveSpeed = 2f;
        public Vector3 moveDirection = Vector3.forward;

        [Header("Detección de obstáculos")]
        public float detectionRadius = 5f;
        public LayerMask obstacleLayer;

        [Header("Spawn de obstáculos")]
        public GameObject obstaclePrefab;
        public float spawnInterval = 4f;

        private FormationManager formationManager;
        private CirclePattern circlePattern;
        private BlackboardSystem blackboard;

        private Vector3 anchorPosition = Vector3.zero;
        private float spawnTimer;
        private List<Vector3> detectedObstacles = new List<Vector3>();
        private List<GameObject> spawnedObstacles = new List<GameObject>();

        void Start()
        {
            circlePattern = new CirclePattern(anchorPosition, circleRadius);
            formationManager = new FormationManager(circlePattern);

            foreach (AgentMovement agent in agents)
            {
                SetupAgent(agent);
                formationManager.AddAgent(agent);
            }

            blackboard = new BlackboardSystem();
            blackboard.RegisterExpert(new ObstacleExpert(agents));

            Debug.Log("Escena 3 La formación avanza y evita obstáculos.");
        }

        void Update()
        {
            anchorPosition += moveDirection.normalized * formationMoveSpeed * Time.deltaTime;
            circlePattern.SetCenter(anchorPosition);

            DetectObstacles();

            Action[] actions = blackboard.Update();
            if (actions != null)
                foreach (Action action in actions)
                    action?.Invoke();

            formationManager.Update();
            CleanPassedObstacles();

            if (obstaclePrefab != null)
            {
                spawnTimer += Time.deltaTime;
                if (spawnTimer >= spawnInterval)
                {
                    SpawnObstacleAhead();
                    spawnTimer = 0f;
                }
            }
        }

        void DetectObstacles()
        {
            detectedObstacles.Clear();

            foreach (AgentMovement agent in agents)
            {
                Collider[] hits = Physics.OverlapSphere(
                    agent.transform.position, detectionRadius, obstacleLayer);

                foreach (Collider hit in hits)
                {
                    Vector3 pos = hit.transform.position;
                    pos.y = 0f;
                    if (!detectedObstacles.Contains(pos))
                        detectedObstacles.Add(pos);
                }
            }

            if (detectedObstacles.Count > 0)
            {
                blackboard.SetData("obstacles", new List<Vector3>(detectedObstacles));
            }
            else if (blackboard.HasKey("obstacles"))
            {
                blackboard.RemoveData("obstacles");

                foreach (AgentMovement agent in agents)
                    agent.GetComponent<ObstacleAvoidanceBehaviour>()?.ClearObstacles();

                formationManager.SetActive(true);
            }
        }

        void CleanPassedObstacles()
        {
            for (int i = spawnedObstacles.Count - 1; i >= 0; i--)
            {
                if (spawnedObstacles[i] == null) { spawnedObstacles.RemoveAt(i); continue; }

                float behind = Vector3.Dot(
                    anchorPosition - spawnedObstacles[i].transform.position,
                    moveDirection.normalized);

                if (behind > circleRadius * 2f)
                {
                    Destroy(spawnedObstacles[i]);
                    spawnedObstacles.RemoveAt(i);
                }
            }
        }

        void SpawnObstacleAhead()
        {
            Vector3 spawnPos = anchorPosition
                + moveDirection.normalized * (circleRadius * 3f)
                + Vector3.right * UnityEngine.Random.Range(-circleRadius, circleRadius);
            spawnPos.y = 0.5f;

            spawnedObstacles.Add(Instantiate(obstaclePrefab, spawnPos, Quaternion.identity));
        }

        void SetupAgent(AgentMovement agent)
        {
            // Separación entre vecinos
            Separation sep = agent.GetComponent<Separation>();
            if (sep != null)
            {
                sep.neighbours = new List<Transform>();
                foreach (AgentMovement other in agents)
                    if (other != agent) sep.neighbours.Add(other.transform);
            }

            // Añade el componente ObstacleAvoidance si no existe
            ObstacleAvoidanceBehaviour avoidance =
                agent.GetComponent<ObstacleAvoidanceBehaviour>();
            if (avoidance == null)
                avoidance = agent.gameObject.AddComponent<ObstacleAvoidanceBehaviour>();

            // Lo registra en BlendedSteering automáticamente
            BlendedSteering blended = agent.GetComponent<BlendedSteering>();
            if (blended != null)
            {
                bool alreadyAdded = blended.behaviours.Exists(
                    b => b.behaviour is ObstacleAvoidanceBehaviour);

                if (!alreadyAdded)
                {
                    blended.behaviours.Add(new BlendedSteering.WeightedBehaviour
                    {
                        behaviour = avoidance,
                        weight = 1.2f
                    });
                }
            }

            // Desactivado por defecto hasta que haya obstáculos
            avoidance.enabled = false;
        }
    }
}