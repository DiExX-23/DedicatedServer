using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private ApiClient api;
    [SerializeField] private List<PlayerController> players;
    [SerializeField] private string gameId;

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

        // Avisar al MainPositionSender (si existe) que se detectó a otro jugador
        var main = FindObjectOfType<MainPositionSender>();
        if (main != null)
        {
            main.NotifyOtherPlayerDetected(playerId);
        }
    }

}