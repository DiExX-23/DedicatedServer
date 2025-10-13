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
    [Tooltip("Asigna exactamente 4 spawn points en el inspector (orden 1..4).")]
    [SerializeField] private Transform[] spawnPoints = new Transform[4];

    // Estado de victoria (se pone true si el local gana usando el trigger)
    public static bool won = false;

    private Dictionary<int, GameObject> remotePlayers = new Dictionary<int, GameObject>();
    private GameObject localPlayerInstance = null;
    private int localPlayerId = -1;

    private void Start()
    {
        if (api != null)
            api.OnDataReceived += OnDataReceived;

        if (GameIDMenuManager.SavedGameID != -1)
        {
            gameId = GameIDMenuManager.SavedGameID.ToString();
            Debug.Log($"[GameManager] gameId actualizado desde GameIDMenuManager: {gameId}");
        }
        else
        {
            Debug.Log($"[GameManager] gameId en inspector: {gameId}");
        }

        int playerId = ResolveLocalPlayerId();
        if (playerId <= 0 || playerId > 4)
        {
            Debug.LogWarning($"[GameManager] playerId {playerId} inválido. No se instanciará player local.");
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

        if (GameIDMenuManager.SavedGameID != -1)
            return GameIDMenuManager.SavedGameID;

        return -1;
    }

    private void InstantiateLocalPlayer(int playerId)
    {
        if (localPlayerPrefab == null)
        {
            Debug.LogError("[GameManager] localPlayerPrefab faltante.");
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

        Debug.Log($"[GameManager] Player local instanciado con ID {playerId}.");
    }

    private void AssignColorById(GameObject playerObj, int id)
    {
        Color c;
        switch (id)
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
        // 1) Si el server informa de un jugador con ID == 10 -> carga EndGame.
        //    No modificamos 'won' aquí (es importante que se mantenga su valor actual).
        if (playerId == 10)
        {
            // Evitamos recargar EndGame si ya estamos en esa escena
            if (SceneManager.GetActiveScene().name != "EndGame")
            {
                Debug.Log($"[GameManager] Se recibió playerId==10 desde servidor (player {playerId}). Cambiando a EndGame (won no modificado).");
                SceneManager.LoadScene("EndGame");
            }
            return;
        }

        // 2) Si no es ID=10, manejamos el movimiento/instanciación normal
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
                    Debug.Log($"[GameManager] Nuevo jugador remoto {playerId} instanciado en {receivedPos}.");
                }
                else
                {
                    Debug.LogError("[GameManager] remotePlayerPrefab no asignado.");
                }
            }
        }

        var main = FindObjectOfType<MainPositionSender>();
        if (main != null)
            main.NotifyOtherPlayerDetected(playerId);
    }
}
