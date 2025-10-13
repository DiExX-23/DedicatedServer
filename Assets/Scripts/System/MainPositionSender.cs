using System.Collections;
using UnityEngine;

public class MainPositionSender : MonoBehaviour
{
    [SerializeField] private float sendInterval = 1f;
    [SerializeField] private float checkInterval = 10f;
    [SerializeField] private int maxPlayersToCheck = 10;

    private Coroutine sendRoutine;
    private Coroutine checkRoutine;
    private PlayerController playerController;
    private GameManager gameManager;

    private bool hasDetectedOtherPlayers = false;

    private void Awake()
    {
        playerController = GetComponent<PlayerController>();
        gameManager = FindObjectOfType<GameManager>();
    }

    private void OnEnable()
    {
        if (playerController != null && gameManager != null)
        {
            sendRoutine = StartCoroutine(SendPositionLoop());
            checkRoutine = StartCoroutine(CheckOtherPlayersFromServerLoop());
        }
        else
        {
            Debug.LogError("MainPositionSender: Missing PlayerController or GameManager reference.");
        }
    }

    private void OnDisable()
    {
        if (sendRoutine != null) StopCoroutine(sendRoutine);
        if (checkRoutine != null) StopCoroutine(checkRoutine);
    }

    private IEnumerator SendPositionLoop()
    {
        while (true)
        {
            if (playerController != null && gameManager != null)
            {
                Vector3 position = transform.position;
                ServerData data = new ServerData
                {
                    posX = position.x,
                    posY = position.y,
                    posZ = position.z
                };
                gameManager.SendPlayerPosition(playerController.PlayerId, data);
            }
            yield return new WaitForSeconds(sendInterval);
        }
    }

    private IEnumerator CheckOtherPlayersFromServerLoop()
    {
        while (true)
        {
            if (gameManager != null && playerController != null)
            {
                string mainId = playerController.PlayerId.ToString();
                for (int i = 1; i < maxPlayersToCheck; i++)
                {
                    if (i.ToString() != mainId)
                    {
                        gameManager.GetPlayerData(i);
                        Debug.Log($"[Main] Checking on server if player {i} is active...");
                        yield return new WaitForSeconds(0.2f);
                    }
                }
            }
            yield return new WaitForSeconds(checkInterval);
        }
    }

    public void NotifyOtherPlayerDetected(int playerId)
    {
        if (playerController != null && playerId != playerController.PlayerId)
        {
            if (!hasDetectedOtherPlayers)
            {
                hasDetectedOtherPlayers = true;
                Debug.Log($"[Main] Player {playerId} detected as active on the server! Adjusting checkInterval to sendInterval ({sendInterval}s).");
                checkInterval = sendInterval;
                if (checkRoutine != null)
                {
                    StopCoroutine(checkRoutine);
                }
                checkRoutine = StartCoroutine(CheckOtherPlayersFromServerLoop());
            }
        }
    }

    public bool HasDetectedOtherPlayers()
    {
        return hasDetectedOtherPlayers;
    }
}