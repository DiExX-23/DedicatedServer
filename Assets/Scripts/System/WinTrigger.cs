using UnityEngine;
using UnityEngine.SceneManagement;

public class WinTrigger : MonoBehaviour
{
    // Opcional: si quieres un ID diferente a 10, cámbialo aquí.
    [SerializeField] private int winId = 10;

    private void OnTriggerEnter(Collider other)
    {
        // Buscamos el PlayerController en el objeto que entró en el trigger
        PlayerController pc = other.GetComponent<PlayerController>();
        if (pc == null)
        {
            // buscar en padres por si el collider está en child del player
            pc = other.GetComponentInParent<PlayerController>();
        }

        if (pc != null)
        {
            Debug.Log($"[WinTrigger] Player detectado en trigger. Asignando ID {winId}, marcando won=true y cargando EndGame.");

            // 1) Poner el ID en el player (esto hará que GameManager/servidor pueda verlo si lo envías)
            pc.PlayerId = winId;

            // 2) Setear won = true porque el local activó el trigger
            GameManager.won = true;

            // 3) Cargar escena EndGame si no estamos ya
            if (SceneManager.GetActiveScene().name != "EndGame")
            {
                SceneManager.LoadScene("EndGame");
            }
        }
    }
}
