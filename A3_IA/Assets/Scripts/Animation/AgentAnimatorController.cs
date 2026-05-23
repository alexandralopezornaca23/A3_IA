using UnityEngine;
using AI.Movement;

namespace AI.Animation
{
    [RequireComponent(typeof(AgentMovement))]
    public class AgentAnimatorController : MonoBehaviour
    {
        [Header("Referencia al Animator del modelo hijo")]
        public Animator animator;

        [Header("Umbrales de velocidad")]
        public float walkThreshold = 0.1f;
        public float runThreshold = 3f;

        [Header("Duración del dodge")]
        public float dodgeDuration = 1f;

        // Hashes de parámetros
        private static readonly int SpeedHash = Animator.StringToHash("speed");
        private static readonly int IsDodgingHash = Animator.StringToHash("isDodging");
        private static readonly int DodgeXHash = Animator.StringToHash("dodgeX");
        private static readonly int DodgeZHash = Animator.StringToHash("dodgeZ");

        private AgentMovement agentMovement;
        private ObstacleAvoidanceBehaviour avoidance;
        private float dodgeTimer;
        private bool isDodging;

        void Awake()
        {
            agentMovement = GetComponent<AgentMovement>();
            avoidance = GetComponent<ObstacleAvoidanceBehaviour>();

            if (animator == null)
                animator = GetComponentInChildren<Animator>();
        }

        void Update()
        {
            if (animator == null) return;

            UpdateSpeedParameter();
            UpdateDodgeParameter();
        }

        void UpdateSpeedParameter()
        {
            float speed = agentMovement.agentData.velocity.magnitude;
            float currentSpeed = animator.GetFloat(SpeedHash);
            float smoothSpeed = Mathf.Lerp(currentSpeed, speed, Time.deltaTime * 10f);
            animator.SetFloat(SpeedHash, smoothSpeed);
        }

        void UpdateDodgeParameter()
        {
            if (avoidance == null) return;

            if (avoidance.IsAvoiding && !isDodging)
            {
                isDodging = true;
                dodgeTimer = dodgeDuration;
                animator.SetBool(IsDodgingHash, true);

                // Calcular la dirección del dodge en espacio local del agente
                // para saber si esquiva hacia delante, atrás, izquierda o derecha
                CalculateDodgeDirection();
            }

            if (isDodging)
            {
                dodgeTimer -= Time.deltaTime;
                if (dodgeTimer <= 0f)
                {
                    isDodging = false;
                    animator.SetBool(IsDodgingHash, false);

                    // Resetear los parámetros del blend tree
                    animator.SetFloat(DodgeXHash, 0f);
                    animator.SetFloat(DodgeZHash, 0f);
                }
            }
        }

        void CalculateDodgeDirection()
        {
            Vector3 avoidanceForce = agentMovement.agentData.velocity;

            if (avoidanceForce.magnitude < 0.01f) return;

            // Convertir la fuerza de evasión a espacio local del agente
            // para saber la dirección relativa al personaje
            Vector3 localDirection = transform.InverseTransformDirection(avoidanceForce.normalized);

            // Suavizar la asignación al blend tree
            float targetX = Mathf.Clamp(localDirection.x, -1f, 1f);
            float targetZ = Mathf.Clamp(localDirection.z, -1f, 1f);

            // Normalizar para que el blend tree use siempre los extremos
            // y no valores intermedios sucios
            if (Mathf.Abs(targetX) > Mathf.Abs(targetZ))
            {
                // Esquive lateral predominante
                targetX = Mathf.Sign(targetX);
                targetZ = 0f;
            }
            else
            {
                // Esquive frontal/trasero predominante
                targetX = 0f;
                targetZ = Mathf.Sign(targetZ);
            }

            animator.SetFloat(DodgeXHash, targetX);
            animator.SetFloat(DodgeZHash, targetZ);

            // Log para debug
            string dir = targetZ > 0 ? "FRONT" :
                         targetZ < 0 ? "BACK" :
                         targetX > 0 ? "RIGHT" : "LEFT";
            Debug.Log($"[Dodge] {agentMovement.name} esquiva hacia: {dir}");
        }
    }
}