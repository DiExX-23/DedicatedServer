using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [Header("API & Players")]
    [SerializeField] private ApiClient api;
    [SerializeField] private List<PlayerController> players;

    [Header("Game / Prefabs")]
    [SerializeField] private string gameId;
    [SerializeField] private GameObject remotePlayerPrefab;
    [SerializeField] private GameObject localPlayerPrefab;

    [Header("Spawns")]
    [Tooltip("Assign exactly 4 spawn points in the inspector (order 1..4).")]
    [SerializeField] private Transform[] spawnPoints = new Transform[4];

    // Win state (set to true if the local player wins using the trigger)
    public static bool won = false;

    private Dictionary<int, GameObject> remotePlayers = new Dictionary<int, GameObject>();
    private GameObject localPlayerInstance = null;
    private int localPlayerId = -1;

    private void Start()
    {
        if (api != null)
            api.OnDataReceived += OnDataReceived;

        if (MainMenuManager.SavedGameID != -1)
        {
            gameId = MainMenuManager.SavedGameID.ToString();
            Debug.Log($"[GameManager] gameId updated from MainMenuManager: {gameId}");
        }
        else
        {
            Debug.Log($"[GameManager] gameId from inspector: {gameId}");
        }

        int playerId = ResolveLocalPlayerId();
        if (playerId <= 0 || playerId > 4)
        {
            Debug.LogWarning($"[GameManager] playerId {playerId} invalid. Local player will not be instantiated.");
            return;
        }

        localPlayerId = playerId;
        InstantiateLocalPlayer(playerId);
    }

    private int ResolveLocalPlayerId()
    {
        PlayerController existing = FindObjectOfType<PlayerController>();
        if (existing != null && existing.PlayerId >= 1 && existing.PlayerId <= 4)
            return existing.PlayerId;

        if (MainMenuManager.SavedGameID != -1)
            return MainMenuManager.SavedGameID;

        return -1;
    }

    private void InstantiateLocalPlayer(int playerId)
    {
        if (localPlayerPrefab == null)
        {
            Debug.LogError("[GameManager] Missing localPlayerPrefab.");
            return;
        }

        Vector3 spawnPos = Vector3.zero;
        if (spawnPoints != null && spawnPoints.Length >= 4 && spawnPoints[playerId - 1] != null)
            spawnPos = spawnPoints[playerId - 1].position;

        localPlayerInstance = Instantiate(localPlayerPrefab, spawnPos, Quaternion.identity);
        localPlayerInstance.name = $"LocalPlayer_{playerId}";

        PlayerController pc = localPlayerInstance.GetComponent<PlayerController>();
        if (pc != null)
            pc.PlayerId = playerId;

        AssignColorById(localPlayerInstance, playerId);

        EnsurePlayersListSize(playerId + 1);
        if (pc != null)
            players[playerId] = pc;
        else
            players[playerId] = null;

        Debug.Log($"[GameManager] Local player instantiated with ID {playerId}.");
    }

    private void AssignColorById(GameObject playerObj, int playerId)
    {
        Color c;
        switch (playerId)
        {
            case 1: c = Color.red; break;
            case 2: c = Color.green; break;
            case 3: c = Color.blue; break;
            case 4: c = Color.yellow; break;
            default: c = Color.white; break;
        }

        Renderer r = playerObj.GetComponentInChildren<Renderer>();
        if (r != null)
        {
            r.material = new Material(r.material);
            r.material.color = c;
        }
    }

    private void EnsurePlayersListSize(int minSize)
    {
        if (players == null)
            players = new List<PlayerController>();

        while (players.Count < minSize)
            players.Add(null);
    }

    public void SendPlayerPosition(int playerId, ServerData data)
    {
        if (api != null)
            StartCoroutine(api.PostPlayerData(gameId, playerId.ToString(), data));
    }

    public void GetPlayerData(int playerId)
    {
        if (api != null)
            StartCoroutine(api.GetPlayerData(gameId, playerId.ToString()));
    }

    private void OnDataReceived(int playerId, ServerData data)
    {
        // 1) If the server reports a player with ID == 10 -> load EndGame.
        //    We do not modify 'won' here (it's important to keep its current value).
        if (playerId == 10)
        {
            // Avoid reloading EndGame if already in that scene
            if (SceneManager.GetActiveScene().name != "EndGame")
            {
                Debug.Log($"[GameManager] Received playerId==10 from server (player {playerId}). Changing to EndGame (won not modified).");
                SceneManager.LoadScene("EndGame");
            }
            return;
        }

        // 2) If not ID=10, handle normal movement/instantiation
        Vector3 receivedPos = new Vector3(data.posX, data.posY, data.posZ);

        if (playerId >= 0 && playerId < players.Count && players[playerId] != null)
        {
            players[playerId].MovePlayer(receivedPos);
        }
        else
        {
            if (remotePlayers.TryGetValue(playerId, out GameObject existing))
            {
                if (existing != null)
                    existing.transform.position = receivedPos;
                else
                    remotePlayers.Remove(playerId);
            }
            else
            {
                if (remotePlayerPrefab != null)
                {
                    GameObject newPlayer = Instantiate(remotePlayerPrefab, receivedPos, Quaternion.identity);
                    newPlayer.name = $"RemotePlayer_{playerId}";
                    remotePlayers[playerId] = newPlayer;
                    Debug.Log($"[GameManager] New remote player {playerId} instantiated at {receivedPos}.");
                }
                else
                {
                    Debug.LogError("[GameManager] remotePlayerPrefab not assigned.");
                }
            }
        }

        var main = FindObjectOfType<MainPositionSender>();
        if (main != null)
            main.NotifyOtherPlayerDetected(playerId);
    }
}
