using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using TMPro; // if you use TextMeshPro

public class MainMenuManager : MonoBehaviour
{
    [Header("Panels")]
    public GameObject mainPanel;
    public GameObject optionsPanel;
    public GameObject idSettings;

    [Header("UI")]
    public TMP_InputField idInput;          // drag the TMP_InputField from the MainMenu here
    public TMP_InputField gameIdInput;
    public Button joinButton;               // drag the PLAY button here
    public TMP_Text playerIDErrorText;
    public TMP_Text gameIdErrorText;

    [Header("API / Config")]
    public ApiClient apiClient;             // drag your ApiClient here
    public string gameId = "defaultRoom";   // change if your game uses another gameId in the API

    private int playerID = -1;              // stores the ID only if it passes validation
    public static int SavedGameID = -1;
    private bool isChecking = false;        // prevents multiple simultaneous checks
    private bool idIsValid = false;
    private float checkTimeoutSeconds = 3f; // timeout in case the API doesn’t respond

    private void Start()
    {
        if (joinButton != null)
            joinButton.onClick.AddListener(OnDeselectedPlayerId);
        else
            Debug.LogWarning("[MainMenuManager] joinButton not assigned in the inspector.");

        if (idInput == null)
            Debug.LogWarning("[MainMenuManager] idInput not assigned in the inspector.");

        if (apiClient == null)
            Debug.LogWarning("[MainMenuManager] apiClient not assigned in the inspector.");
    }

    public void OnDeselectedPlayerId()
    {
        if (isChecking) return; // already checking

        string txt = (idInput != null) ? idInput.text.Trim() : "";
        if (!int.TryParse(txt, out int id))
        {
            Debug.LogWarning("[MainMenuManager] The ID must be an integer.");
            playerIDErrorText.text = $"<b>The ID must be an integer.</b>";
            return;
        }

        playerIDErrorText.text = $"<b>Checking if the ID exists...</b>";
        StartCoroutine(CheckAndAssignIdCoroutine(id));
    }

    private IEnumerator CheckAndAssignIdCoroutine(int id)
    {
        isChecking = true;
        bool exists = false;
        bool callbackFired = false;

        // Handler for the ApiClient response
        void OnDataReceivedHandler(int receivedId, ServerData data)
        {
            // If the server responds with data for that ID, we consider it "taken"
            if (receivedId == id)
            {
                exists = true;
            }
            callbackFired = true;
        }

        // Subscribe to the event (if it exists)
        if (apiClient != null)
            apiClient.OnDataReceived += OnDataReceivedHandler;
        else
        {
            Debug.LogWarning("[MainMenuManager] apiClient is null — skipping remote verification.");
            callbackFired = true; // allow continuation (will be considered not-taken)
        }

        // Call the coroutine that queries the server
        if (apiClient != null)
        {
            // Call the public coroutine that already exists in ApiClient
            yield return StartCoroutine(apiClient.GetPlayerData(gameId, id.ToString()));
        }

        // Wait until the callback is executed or timeout
        float t = 0f;
        while (!callbackFired && t < checkTimeoutSeconds)
        {
            t += Time.deltaTime;
            yield return null;
        }

        // Unsubscribe to avoid accumulating handlers
        if (apiClient != null)
            apiClient.OnDataReceived -= OnDataReceivedHandler;

        // Verification result
        if (exists)
        {
            Debug.LogWarning($"[MainMenuManager] ID {id} is already taken. Choose another.");
            playerIDErrorText.text = $"<b>ID {id} is already taken. Choose another.</b>";
        }
        else
        {
            playerID = id;
            Debug.Log($"[MainMenuManager] playerID assigned: {playerID}");
            playerIDErrorText.text = $"<b>playerID assigned: {playerID}</b>";
            idIsValid = true;
        }

        isChecking = false;
    }

    public void OnJoinClicked()
    {
        string txt = (gameIdInput != null) ? gameIdInput.text.Trim() : "";
        if (!int.TryParse(txt, out int id))
        {
            Debug.LogWarning("[GameIDMenuManager] The GameID must be an integer.");
            gameIdErrorText.text = $"<b>The ID must be an integer.</b>";
            return;
        }

        if (idIsValid == true)
        {
            SavedGameID = id;
            Debug.Log($"[GameIDMenuManager] GameID saved: {SavedGameID}");
            SceneManager.LoadScene("Main");
        }
        else
        {
            gameIdErrorText.text = $"<b>Invalid player ID.</b>";
        }
    }

    public void ChangePanel(GameObject activePanel)
    {
        // Deactivate all panels first
        mainPanel.SetActive(false);
        optionsPanel.SetActive(false);
        idSettings.SetActive(false);
        Debug.Log("Panel changed");

        // Activate the selected panel
        activePanel.SetActive(true);
    }

    public void BackToMainMenu()
    {
        // Activate only the main panel
        mainPanel.SetActive(true);
        optionsPanel.SetActive(false);
    }

    public void Exit()
    {
        Debug.Log("Exiting the game...");
        Application.Quit();
    }

    // Public method for other classes to get the assigned playerID (if needed)
    public int GetPlayerID()
    {
        return playerID;
    }

    public int GetSavedGameID()
    {
        return SavedGameID;
    }
}
