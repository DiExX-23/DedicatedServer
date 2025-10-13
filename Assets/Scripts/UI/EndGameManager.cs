using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class EndGameManager : MonoBehaviour
{
    [Header("UI")]
    public TMP_Text resultText;   // arrastra aquí el TMP (TextMeshPro - UI) que mostrará "ganaste" / "perdiste"
    public GameObject backButton; // opcional, para activar/desactivar o asignar listener si prefieres

    private void Start()
    {
        if (resultText == null)
        {
            Debug.LogError("[EndGameManager] resultText no asignado en el inspector.");
            return;
        }

        // Leemos la variable estática de GameManager
        if (GameManager.won)
        {
            resultText.text = "ganaste";
        }
        else
        {
            resultText.text = "perdiste";
        }
    }

    // Método público que debe enlazarse al botón "Volver a MainMenu"
    public void OnBackToMainMenu()
    {
        // Reseteamos el estado para la próxima partida
        GameManager.won = false;

        // Cambiamos a la escena del menú principal
        SceneManager.LoadScene("MainMenu");
    }
}
