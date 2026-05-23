using UnityEngine;

namespace AI.Movement
{
    [RequireComponent(typeof(BlendedSteering))]
    [RequireComponent(typeof(Arrive))]
    [RequireComponent(typeof(Separation))]
    [RequireComponent(typeof(Rigidbody))]
    public class AgentMovement : MonoBehaviour
    {
        public AgentData agentData = new AgentData();
        public float maxSpeed = 5f;
        public float maxAcceleration = 10f;

        [Header("Patrulla en slot")]
        public float patrolRadius = 1.5f;   // radio dentro del slot donde patrulla
        public float idleWaitMin = 1f;     // segundos mínimos de idle
        public float idleWaitMax = 3f;     // segundos máximos de idle

        private BlendedSteering blended;
        private Arrive arrive;
        private Rigidbody rb;

        // Estado de patrulla
        private Vector3 slotPosition;           // posición del slot asignada por FormationManager
        private bool hasSlot = false;  // si tiene slot asignado
        private bool isPatrolling = false;  // si está en modo patrulla
        private float idleTimer = 0f;     // cuenta el tiempo de espera
        private bool isWaiting = false;  // si está esperando en idle

        void Awake()
        {
            blended = GetComponent<BlendedSteering>();
            arrive = GetComponent<Arrive>();
            rb = GetComponent<Rigidbody>();

            rb.freezeRotation = true;
            rb.constraints = RigidbodyConstraints.FreezePositionY
                               | RigidbodyConstraints.FreezeRotationX
                               | RigidbodyConstraints.FreezeRotationZ;

            agentData.position = transform.position;
            agentData.maxSpeed = maxSpeed;
        }

        void FixedUpdate()
        {
            agentData.position = transform.position;

            Vector3 steering = blended.GetSteering(agentData);
            agentData.velocity += steering * Time.fixedDeltaTime;

            agentData.velocity.y = 0f;

            if (agentData.velocity.magnitude > maxSpeed)
                agentData.velocity = agentData.velocity.normalized * maxSpeed;

            if (agentData.velocity.magnitude < 0.05f)
            {
                agentData.velocity = Vector3.zero;
                rb.linearVelocity = Vector3.zero;
            }

            rb.MovePosition(rb.position + agentData.velocity * Time.fixedDeltaTime);

            if (agentData.velocity.magnitude > 0.1f)
            {
                Quaternion targetRot = Quaternion.LookRotation(agentData.velocity);
                rb.MoveRotation(Quaternion.Slerp(
                    rb.rotation, targetRot, Time.fixedDeltaTime * 10f));
            }
        }

        void Update()
        {
            if (!isPatrolling || !hasSlot) return;

            float distToTarget = Vector3.Distance(transform.position, arrive.targetPosition);

            if (distToTarget < arrive.targetRadius)
            {
                if (!isWaiting)
                {
                    // Llegó al punto de patrulla, esperar un tiempo aleatorio en idle
                    isWaiting = true;
                    idleTimer = UnityEngine.Random.Range(idleWaitMin, idleWaitMax);
                }
                else
                {
                    idleTimer -= Time.deltaTime;
                    if (idleTimer <= 0f)
                    {
                        // Tiempo de idle terminado, ir a otro punto aleatorio del slot
                        isWaiting = false;
                        SetPatrolPoint();
                    }
                }
            }
        }

        // Llamado por FormationManager cada frame con la posición del slot
        public void SetTarget(Vector3 target)
        {
            if (arrive == null) return;

            slotPosition = target;
            hasSlot = true;

            float distToSlot = Vector3.Distance(transform.position, slotPosition);

            if (distToSlot > arrive.targetRadius * 2f)
            {
                // Aún no ha llegado al slot, ir directamente a él
                isPatrolling = false;
                isWaiting = false;
                arrive.targetPosition = slotPosition;
                arrive.targetPosition.y = 0f;
            }
            else
            {
                // Ya está en el slot, activar patrulla si no estaba activa
                if (!isPatrolling)
                {
                    isPatrolling = true;
                    isWaiting = false;
                    SetPatrolPoint();
                }
            }
        }

        // Elige un punto aleatorio dentro del patrolRadius alrededor del slot
        void SetPatrolPoint()
        {
            Vector2 randomCircle = UnityEngine.Random.insideUnitCircle * patrolRadius;
            Vector3 patrolPoint = slotPosition + new Vector3(randomCircle.x, 0f, randomCircle.y);

            arrive.targetPosition = patrolPoint;
            arrive.targetPosition.y = 0f;
        }

        // Llamado por FormationManager cuando cambia el patrón
        // para salir del modo patrulla y ir al nuevo slot
        public void ResetPatrol()
        {
            isPatrolling = false;
            isWaiting = false;
            hasSlot = false;
        }
    }
}