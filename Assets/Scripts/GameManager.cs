using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private ApiClient api;
    [SerializeField] private List<PlayerController> players;
    [SerializeField] private string gameId;
    [SerializeField] private GameObject remotePlayerPrefab;

    private Dictionary<int, GameObject> remotePlayers = new Dictionary<int, GameObject>();

    private void Start()
    {
        if (api != null)
        {
            api.OnDataReceived += OnDataReceived;
        }
    }

    public void SendPlayerPosition(int playerId, ServerData data)
    {
        if (api != null)
        {
            StartCoroutine(api.PostPlayerData(gameId, playerId.ToString(), data));
        }
    }

    public void GetPlayerData(int playerId)
    {
        if (api != null)
        {
            StartCoroutine(api.GetPlayerData(gameId, playerId.ToString()));
        }
    }

    private void OnDataReceived(int playerId, ServerData data)
    {
        if (playerId >= 0 && playerId < players.Count && players[playerId] != null)
        {
            players[playerId].MovePlayer(new Vector3(data.posX, data.posY, data.posZ));
        }
        else
        {
            if (remotePlayers.TryGetValue(playerId, out GameObject existing))
            {
                if (existing != null)
                {
                    existing.transform.position = new Vector3(data.posX, data.posY, data.posZ);
                }
                else
                {
                    remotePlayers.Remove(playerId);
                }
            }
            else
            {
                if (remotePlayerPrefab != null)
                {
                    GameObject newPlayer = Instantiate(remotePlayerPrefab);
                    newPlayer.name = $"RemotePlayer_{playerId}";
                    newPlayer.transform.position = new Vector3(data.posX, data.posY, data.posZ);
                    remotePlayers[playerId] = newPlayer;
                    Debug.Log($"[GameManager] Instantiated remote player {playerId} at ({data.posX}, {data.posY}, {data.posZ}).");
                }
                else
                {
                    Debug.LogError("[GameManager] Remote player prefab is not assigned.");
                }
            }
        }

        var main = FindObjectOfType<MainPositionSender>();
        if (main != null)
        {
            main.NotifyOtherPlayerDetected(playerId);
        }
    }
}