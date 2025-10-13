using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro; // si usas TextMeshPro

public class MainMenuManager : MonoBehaviour
{
    [Header("UI")]
    public TMP_InputField idInput;          // arrastra aquí el TMP_InputField del MainMenu
    public Button playButton;               // arrastra aquí el botón PLAY
    public TMP_Text errorText;

    [Header("API / Config")]
    public ApiClient apiClient;             // arrastra aquí tu ApiClient
    public string gameId = "defaultRoom";   // cambia si tu juego usa otro gameId en la API

    private int playerID = -1;              // aquí se guardará el ID sólo si pasa las validaciones
    private bool isChecking = false;        // evita múltiples comprobaciones simultáneas
    private float checkTimeoutSeconds = 3f; // timeout por si la API no responde

    private void Start()
    {
        if (playButton != null)
            playButton.onClick.AddListener(OnPlayClicked);
        else
            Debug.LogWarning("[MainMenuManager] playButton no asignado en el inspector.");

        if (idInput == null)
            Debug.LogWarning("[MainMenuManager] idInput no asignado en el inspector.");

        if (apiClient == null)
            Debug.LogWarning("[MainMenuManager] apiClient no asignado en el inspector.");
    }

    private void OnPlayClicked()
    {
        if (isChecking) return; // ya está verificando
        string txt = (idInput != null) ? idInput.text.Trim() : "";

        if (!int.TryParse(txt, out int id))
        {
            Debug.LogWarning("[MainMenuManager] El ID debe ser un número entero.");
            errorText.text = $"<b>El ID debe ser un número entero.</b>";
            return;
        }

        errorText.text = $"<b>Comprobando si el ID existe...</b>";
        StartCoroutine(CheckAndAssignIdCoroutine(id));
    }

    private IEnumerator CheckAndAssignIdCoroutine(int id)
    {
        isChecking = true;
        bool exists = false;
        bool callbackFired = false;

        // Handler para la respuesta del ApiClient.
        void OnDataReceivedHandler(int receivedId, ServerData data)
        {
            // Si el servidor responde con datos para ese ID, lo consideramos "elegido".
            if (receivedId == id)
            {
                exists = true;
            }
            callbackFired = true;
        }

        // Suscribimos al evento (si existe).
        if (apiClient != null)
            apiClient.OnDataReceived += OnDataReceivedHandler;
        else
        {
            Debug.LogWarning("[MainMenuManager] apiClient es null — se omite verificación remota.");
            callbackFired = true; // permitimos continuar (se considerará no-elegido)
        }

        // Llamamos a la coroutine que consulta al servidor.
        if (apiClient != null)
        {
            // Llamamos la coroutine pública que ya existe en ApiClient
            yield return StartCoroutine(apiClient.GetPlayerData(gameId, id.ToString()));
        }

        // Esperamos hasta que el callback se ejecute o hasta timeout
        float t = 0f;
        while (!callbackFired && t < checkTimeoutSeconds)
        {
            t += Time.deltaTime;
            yield return null;
        }

        // Nos desuscribimos para no acumular handlers
        if (apiClient != null)
            apiClient.OnDataReceived -= OnDataReceivedHandler;

        // Resultado de la verificación
        if (exists)
        {
            Debug.LogWarning($"[MainMenuManager] El ID {id} ya está elegido. Elige otro.");
            errorText.text = $"<b>El ID {id} ya está elegido. Elige otro.</b>";
        }
        else
        {
            playerID = id;
            Debug.Log($"[MainMenuManager] playerID asignado: {playerID}. Cargando escena GameIDMenu...");
            // Cambia a la escena "GameIDMenu" (tal como pediste)
            SceneManager.LoadScene("GameIDMenu");
        }

        isChecking = false;
    }

    // Método público para que otras clases obtengan el playerID asignado (si lo necesitas)
    public int GetPlayerID()
    {
        return playerID;
    }
}
