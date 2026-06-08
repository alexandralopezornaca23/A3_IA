using UnityEngine;
using AI.Movement;

namespace AI.Animation
{
    // Controla las animaciones del agente en funcion de su velocidad y estado de evasion
    [RequireComponent(typeof(AgentMovement))]
    public class AgentAnimatorController : MonoBehaviour
    {
        [Header("Referencia al Animator del modelo hijo")]
        public Animator animator;

        [Header("Umbrales de velocidad")]
        public float walkThreshold = 0.1f;
        public float runThreshold = 3f;

        [Header("Duracion del dodge")]
        public float dodgeDuration = 1f;

        // Hashes para acceder a los parametros del Animator de forma eficiente
        // Es mas rapido que pasar strings directamente cada frame
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

            // Si no se asigno el Animator en el Inspector, lo busca en los hijos
            // El modelo de Mixamo es un hijo del GameObject del agente
            if (animator == null)
                animator = GetComponentInChildren<Animator>();
        }

        void Update()
        {
            if (animator == null) return;

            UpdateSpeedParameter();
            UpdateDodgeParameter();
        }

        // Actualiza el parametro speed del Animator con la velocidad actual del agente
        // Usa Lerp para suavizar la transicion y evitar cambios bruscos entre estados
        void UpdateSpeedParameter()
        {
            float speed = agentMovement.agentData.velocity.magnitude;
            float currentSpeed = animator.GetFloat(SpeedHash);
            float smoothSpeed = Mathf.Lerp(currentSpeed, speed, Time.deltaTime * 10f);
            animator.SetFloat(SpeedHash, smoothSpeed);
        }

        // Detecta cuando el agente empieza a esquivar y gestiona la duracion del dodge
        void UpdateDodgeParameter()
        {
            if (avoidance == null) return;

            // Inicia el dodge cuando el componente de evasion se activa
            if (avoidance.IsAvoiding && !isDodging)
            {
                isDodging = true;
                dodgeTimer = dodgeDuration;
                animator.SetBool(IsDodgingHash, true);
                CalculateDodgeDirection();
            }

            // Cuenta el tiempo del dodge y lo termina cuando se acaba
            if (isDodging)
            {
                dodgeTimer -= Time.deltaTime;
                if (dodgeTimer <= 0f)
                {
                    isDodging = false;
                    animator.SetBool(IsDodgingHash, false);

                    // Resetea el Blend Tree para que vuelva al estado neutro
                    animator.SetFloat(DodgeXHash, 0f);
                    animator.SetFloat(DodgeZHash, 0f);
                }
            }
        }

        // Calcula hacia que direccion esquiva el agente y lo pasa al Blend Tree 2D
        // El Blend Tree elige entre DodgeForward, DodgeBack, DodgeLeft y DodgeRight
        void CalculateDodgeDirection()
        {
            Vector3 avoidanceForce = agentMovement.agentData.velocity;

            if (avoidanceForce.magnitude < 0.01f) return;

            // Convierte la velocidad de evasion a espacio local del agente
            // para saber la direccion relativa al personaje, no al mundo
            Vector3 localDirection = transform.InverseTransformDirection(avoidanceForce.normalized);

            float targetX = Mathf.Clamp(localDirection.x, -1f, 1f);
            float targetZ = Mathf.Clamp(localDirection.z, -1f, 1f);

            // Elige el eje predominante para que el Blend Tree use siempre
            // una sola animacion limpia y no una mezcla intermedia
            if (Mathf.Abs(targetX) > Mathf.Abs(targetZ))
            {
                targetX = Mathf.Sign(targetX);
                targetZ = 0f;
            }
            else
            {
                targetX = 0f;
                targetZ = Mathf.Sign(targetZ);
            }

            animator.SetFloat(DodgeXHash, targetX);
            animator.SetFloat(DodgeZHash, targetZ);

            string dir = targetZ > 0 ? "FRONT" :
                         targetZ < 0 ? "BACK" :
                         targetX > 0 ? "RIGHT" : "LEFT";
            Debug.Log($"[Dodge] {agentMovement.name} esquiva hacia: {dir}");
        }
    }
}