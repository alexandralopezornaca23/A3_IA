using UnityEngine;

// Hace que la camara siga a un objetivo manteniendo un desplazamiento fijo
// Se usa en la Escena 3 para seguir la formacion mientras avanza
public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public Vector3 offset = new Vector3(0, 12, -8);

    // LateUpdate se ejecuta despues de Update y FixedUpdate
    // Esto garantiza que la camara se mueve despues de que los agentes
    // hayan actualizado su posicion, evitando el efecto de temblor
    void LateUpdate()
    {
        if (target == null) return;

        // Coloca la camara en la posicion del objetivo mas el desplazamiento
        transform.position = target.position + offset;

        // Rota la camara para que siempre mire hacia el objetivo
        transform.LookAt(target.position);
    }
}
