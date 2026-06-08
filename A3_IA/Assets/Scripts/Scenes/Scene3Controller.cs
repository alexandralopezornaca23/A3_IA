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
    // Controlador de la Escena 3
    // La formacion avanza en una direccion constante mientras aparecen obstaculos
    // Cuando un agente detecta un obstaculo lo publica en la pizarra
    // El ObstacleExpert activa la evasion y al rebasarlos los agentes retoman la formacion
    public class Scene3Controller : MonoBehaviour
    {
        [Header("Agentes")]
        public List<AgentMovement> agents = new List<AgentMovement>();

        [Header("Formacion en movimiento")]
        public float circleRadius = 4f;
        public float formationMoveSpeed = 2f;
        public Vector3 moveDirection = Vector3.forward;

        [Header("Deteccion de obstaculos")]
        public float detectionRadius = 5f;
        public LayerMask obstacleLayer;

        [Header("Spawn de obstaculos")]
        public GameObject obstaclePrefab;
        public float spawnInterval = 4f;

        private FormationManager formationManager;
        private CirclePattern circlePattern;
        private BlackboardSystem blackboard;

        // Posicion actual del centro de la formacion, avanza cada frame
        private Vector3 anchorPosition = Vector3.zero;
        private float spawnTimer;
        private List<Vector3> detectedObstacles = new List<Vector3>();
        private List<GameObject> spawnedObstacles = new List<GameObject>();

        void Start()
        {
            // Se guarda referencia al CirclePattern para poder mover su centro cada frame
            circlePattern = new CirclePattern(anchorPosition, circleRadius);
            formationManager = new FormationManager(circlePattern);

            foreach (AgentMovement agent in agents)
            {
                SetupAgent(agent);
                formationManager.AddAgent(agent);
            }

            blackboard = new BlackboardSystem();
            blackboard.RegisterExpert(new ObstacleExpert(agents));

            Debug.Log("Escena 3 La formacion avanza y evita obstaculos.");
        }

        void Update()
        {
            // Desplaza el ancla de la formacion en la direccion configurada
            // y actualiza el centro del patron para que los slots sigan avanzando
            anchorPosition += moveDirection.normalized * formationMoveSpeed * Time.deltaTime;
            circlePattern.SetCenter(anchorPosition);

            DetectObstacles();

            // El arbitro ejecuta las acciones del experto con mayor insistencia
            Action[] actions = blackboard.Update();
            if (actions != null)
                foreach (Action action in actions)
                    action?.Invoke();

            formationManager.Update();
            CleanPassedObstacles();

            // Cuenta el tiempo y spawna un nuevo obstaculo al llegar al intervalo
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

        // Comprueba cada frame si algun agente tiene obstaculos dentro de su radio
        // Si los hay los publica en la pizarra, si no los hay los limpia
        void DetectObstacles()
        {
            detectedObstacles.Clear();

            foreach (AgentMovement agent in agents)
            {
                // OverlapSphere filtra por el layer Obstacles para no detectar otros agentes
                Collider[] hits = Physics.OverlapSphere(
                    agent.transform.position, detectionRadius, obstacleLayer);

                foreach (Collider hit in hits)
                {
                    Vector3 pos = hit.transform.position;
                    pos.y = 0f;

                    // Evita duplicados si varios agentes detectan el mismo obstaculo
                    if (!detectedObstacles.Contains(pos))
                        detectedObstacles.Add(pos);
                }
            }

            if (detectedObstacles.Count > 0)
            {
                // Publica la lista de obstaculos en la pizarra para que el ObstacleExpert actue
                blackboard.SetData("obstacles", new List<Vector3>(detectedObstacles));
            }
            else if (blackboard.HasKey("obstacles"))
            {
                // Sin obstaculos: limpia la pizarra, desactiva la evasion
                // y reactiva la formacion para que los agentes vuelvan a sus slots
                blackboard.RemoveData("obstacles");

                foreach (AgentMovement agent in agents)
                    agent.GetComponent<ObstacleAvoidanceBehaviour>()?.ClearObstacles();

                formationManager.SetActive(true);
            }
        }

        // Elimina los obstaculos que han quedado suficientemente atras de la formacion
        // Usa producto escalar para medir cuanto ha sobrepasado la formacion al obstaculo
        void CleanPassedObstacles()
        {
            for (int i = spawnedObstacles.Count - 1; i >= 0; i--)
            {
                if (spawnedObstacles[i] == null) { spawnedObstacles.RemoveAt(i); continue; }

                // Dot positivo significa que el ancla esta por delante del obstaculo
                // Si supera el doble del radio la formacion lo ha rebasado completamente
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

        // Instancia un obstaculo delante de la formacion con desplazamiento lateral aleatorio
        // La distancia de spawn es tres veces el radio para dar tiempo a los agentes a reaccionar
        void SpawnObstacleAhead()
        {
            Vector3 spawnPos = anchorPosition
                + moveDirection.normalized * (circleRadius * 3f)
                + Vector3.right * UnityEngine.Random.Range(-circleRadius, circleRadius);
            spawnPos.y = 0.5f;

            spawnedObstacles.Add(Instantiate(obstaclePrefab, spawnPos, Quaternion.identity));
        }

        // Configura cada agente con separacion de vecinos y el componente de evasion
        // El ObstacleAvoidanceBehaviour se anade por codigo y se registra en el BlendedSteering
        void SetupAgent(AgentMovement agent)
        {
            // Asigna todos los demas agentes como vecinos para el algoritmo de separacion
            Separation sep = agent.GetComponent<Separation>();
            if (sep != null)
            {
                sep.neighbours = new List<Transform>();
                foreach (AgentMovement other in agents)
                    if (other != agent) sep.neighbours.Add(other.transform);
            }

            // Anade el componente de evasion si el prefab no lo tiene ya
            ObstacleAvoidanceBehaviour avoidance =
                agent.GetComponent<ObstacleAvoidanceBehaviour>();
            if (avoidance == null)
                avoidance = agent.gameObject.AddComponent<ObstacleAvoidanceBehaviour>();

            // Registra el componente en BlendedSteering con peso 1.2 para que tenga
            // prioridad sobre Arrive y Separation cuando haya obstaculos cerca
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

            // Se desactiva por defecto y solo se activa cuando el ObstacleExpert lo ordena
            avoidance.enabled = false;
        }
    }
}