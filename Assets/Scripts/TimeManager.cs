using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class TimerManager : MonoBehaviourPunCallbacks
{
    [SerializeField] CanvasGroup canvasGroupLeaderBoard;
    [SerializeField] CanvasGroup canvasGroupGameOver;
    public float gameDurationInSeconds = 180f; // 3 minutes in this example
    private float timer;
    private bool IsGameOverTriggered = false;
    private bool IsLeaderboardTriggered = false;
    [SerializeField] private Text timerText;

    private void Start()
    {
        timer = gameDurationInSeconds;
        if (PhotonNetwork.IsMasterClient)
        {
            // Start the timer only on the master client
            photonView.RPC("StartTimer", RpcTarget.AllBufferedViaServer, gameDurationInSeconds);
        }
    }

    [PunRPC]
    private void StartTimer(float duration)
    {
        timer = duration;
        InvokeRepeating("UpdateTimer", 1f, 1f); // Update timer every second
    }

    private void UpdateTimer()
    {
        if (timer > 0)
        {
            timer -= 1f;
            UpdateTimerUI();
        }
        else if (timer <= 0f && !IsGameOverTriggered)
        {
            // Game over logic
            photonView.RPC("GameOver", RpcTarget.AllBuffered);
            IsGameOverTriggered = true;

            // Wait for 5 seconds before showing the leaderboard
            Invoke("ShowLeaderboardAfterDelay", 5f);
        }
    }

    private void ShowLeaderboardAfterDelay()
    {
        // Show leaderboard logic
        photonView.RPC("ShowLeaderBoard", RpcTarget.AllBuffered);
        IsLeaderboardTriggered = true;
    }

    private void UpdateTimerUI()
    {
        int minutes = Mathf.FloorToInt(timer / 60f);
        int seconds = Mathf.FloorToInt(timer % 60f);
        timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    [PunRPC]
    private void ShowLeaderBoard()
    {
        // Pause the game by setting the time scale to 0
        //Time.timeScale = 0f;

        canvasGroupLeaderBoard.alpha = 1;
        canvasGroupGameOver.alpha = 0;
        // Implement game over actions here
    }

    [PunRPC]
    private void GameOver()
    {
        // Pause the game by setting the time scale to 0
        //Time.timeScale = 0f;

        canvasGroupGameOver.alpha = 1;
        // Implement game over actions here
    }

    #region UIDropdown TimeSelection

    public void DropdownMenuToSetGameTime(int index)
    {
        switch (index)
        {
            case 0: gameDurationInSeconds = 180; break;
            case 1: gameDurationInSeconds = 300; break;
            case 2: gameDurationInSeconds = 420; break;
            default: gameDurationInSeconds = 180; break;
        }
    }

    #endregion
}
