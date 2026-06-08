using UnityEngine;

namespace AI.Movement
{
    // Componente principal del agente que aplica el steering y gestiona la patrulla en slot
    // Requiere BlendedSteering, Arrive, Separation y Rigidbody en el mismo GameObject
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
        public float patrolRadius = 1.5f;
        public float idleWaitMin = 1f;
        public float idleWaitMax = 3f;

        private BlendedSteering blended;
        private Arrive arrive;
        private Rigidbody rb;

        // Variables que controlan el estado de la patrulla dentro del slot
        private Vector3 slotPosition;
        private bool hasSlot = false;
        private bool isPatrolling = false;
        private float idleTimer = 0f;
        private bool isWaiting = false;

        void Awake()
        {
            blended = GetComponent<BlendedSteering>();
            arrive = GetComponent<Arrive>();
            rb = GetComponent<Rigidbody>();

            // Se configuran las restricciones del Rigidbody por codigo
            // para evitar que el agente vuele, caiga o se tumbe por las fisicas
            rb.freezeRotation = true;
            rb.constraints = RigidbodyConstraints.FreezePositionY
                               | RigidbodyConstraints.FreezeRotationX
                               | RigidbodyConstraints.FreezeRotationZ;

            agentData.position = transform.position;
            agentData.maxSpeed = maxSpeed;
        }

        // FixedUpdate se usa con Rigidbody para sincronizar el movimiento con el ciclo de fisicas
        void FixedUpdate()
        {
            agentData.position = transform.position;

            // Suma la aceleracion combinada de todos los behaviours del BlendedSteering
            Vector3 steering = blended.GetSteering(agentData);
            agentData.velocity += steering * Time.fixedDeltaTime;

            // Fuerza el movimiento en el plano XZ eliminando cualquier velocidad vertical
            agentData.velocity.y = 0f;

            // Clamp para no superar la velocidad maxima configurada
            if (agentData.velocity.magnitude > maxSpeed)
                agentData.velocity = agentData.velocity.normalized * maxSpeed;

            // Detiene completamente el Rigidbody cuando la velocidad es casi cero
            // Esto evita el micro-movimiento de oscilacion al llegar al objetivo
            if (agentData.velocity.magnitude < 0.05f)
            {
                agentData.velocity = Vector3.zero;
                rb.linearVelocity = Vector3.zero;
            }

            // MovePosition respeta las colisiones del Rigidbody
            // a diferencia de modificar transform.position directamente
            rb.MovePosition(rb.position + agentData.velocity * Time.fixedDeltaTime);

            // Rota el agente suavemente hacia la direccion de movimiento
            if (agentData.velocity.magnitude > 0.1f)
            {
                Quaternion targetRot = Quaternion.LookRotation(agentData.velocity);
                rb.MoveRotation(Quaternion.Slerp(
                    rb.rotation, targetRot, Time.fixedDeltaTime * 10f));
            }
        }

        // Gestiona el ciclo de patrulla: llegar al punto, esperar en idle y elegir el siguiente
        void Update()
        {
            if (!isPatrolling || !hasSlot) return;

            float distToTarget = Vector3.Distance(transform.position, arrive.targetPosition);

            // Comprueba si el agente ha llegado al punto de patrulla actual
            if (distToTarget < arrive.targetRadius)
            {
                if (!isWaiting)
                {
                    // Inicia la espera con un tiempo aleatorio entre idleWaitMin e idleWaitMax
                    isWaiting = true;
                    idleTimer = UnityEngine.Random.Range(idleWaitMin, idleWaitMax);
                }
                else
                {
                    // Cuenta el tiempo de idle y cuando acaba elige un nuevo punto
                    idleTimer -= Time.deltaTime;
                    if (idleTimer <= 0f)
                    {
                        isWaiting = false;
                        SetPatrolPoint();
                    }
                }
            }
        }

        // El FormationManager llama a este metodo cada frame con la posicion del slot
        // Si el agente esta lejos del slot va directamente, si ya llego activa la patrulla
        public void SetTarget(Vector3 target)
        {
            if (arrive == null) return;

            slotPosition = target;
            hasSlot = true;

            float distToSlot = Vector3.Distance(transform.position, slotPosition);

            if (distToSlot > arrive.targetRadius * 2f)
            {
                // Lejos del slot: desactiva la patrulla y va directamente al slot
                isPatrolling = false;
                isWaiting = false;
                arrive.targetPosition = slotPosition;
                arrive.targetPosition.y = 0f;
            }
            else
            {
                // Ya en el slot: activa la patrulla si aun no estaba activa
                if (!isPatrolling)
                {
                    isPatrolling = true;
                    isWaiting = false;
                    SetPatrolPoint();
                }
            }
        }

        // Elige un punto aleatorio dentro del patrolRadius alrededor del centro del slot
        // Usa insideUnitCircle para distribuir los puntos en el plano XZ
        void SetPatrolPoint()
        {
            Vector2 randomCircle = UnityEngine.Random.insideUnitCircle * patrolRadius;
            Vector3 patrolPoint = slotPosition + new Vector3(randomCircle.x, 0f, randomCircle.y);
            arrive.targetPosition = patrolPoint;
            arrive.targetPosition.y = 0f;
        }

        // Cancela la patrulla y libera el slot para que el agente vaya al nuevo destino
        // Lo llaman FormationManager al cambiar patron y los expertos al publicar un objetivo
        public void ResetPatrol()
        {
            isPatrolling = false;
            isWaiting = false;
            hasSlot = false;
        }
    }
}