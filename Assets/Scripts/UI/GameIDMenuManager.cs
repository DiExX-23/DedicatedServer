using UnityEngine;
using UnityEngine.UI;
using TMPro; // si usas TextMeshPro
using UnityEngine.SceneManagement;

public class GameIDMenuManager : MonoBehaviour
{
    [Header("UI")]
    public TMP_InputField gameIdInput;   // arrastra aquí el TMP_InputField de GameIDMenu
    public Button saveButton;            // arrastra aquí el botón para guardar (opcional)
    public TMP_Text errorText;

    // Valor accesible estáticamente para que otras clases/escenas lo consulten
    // (GameManager podrá leer GameIDMenuManager.SavedGameID cuando lo necesite)
    public static int SavedGameID = -1;

    private void Start()
    {
        if (saveButton != null)
            saveButton.onClick.AddListener(OnSaveClicked);
        else
            Debug.LogWarning("[GameIDMenuManager] saveButton no asignado en el inspector.");

        if (gameIdInput == null)
            Debug.LogWarning("[GameIDMenuManager] gameIdInput no asignado en el inspector.");
    }

    private void OnSaveClicked()
    {
        string txt = (gameIdInput != null) ? gameIdInput.text.Trim() : "";

        if (!int.TryParse(txt, out int id))
        {
            Debug.LogWarning("[GameIDMenuManager] El GameID debe ser un número entero.");
            errorText.text = $"<b>El ID debe ser un número entero.</b>";
            return;
        }

        SavedGameID = id;
        Debug.Log($"[GameIDMenuManager] GameID guardado: {SavedGameID}");
        SceneManager.LoadScene("Main");
    }

    // Método público para que, si prefieres, otras clases obtengan el GameID sin usar la variable estática:
    public int GetSavedGameID()
    {
        return SavedGameID;
    }
}
