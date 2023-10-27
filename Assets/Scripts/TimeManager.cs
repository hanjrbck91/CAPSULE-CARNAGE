using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class TimerManager : MonoBehaviourPunCallbacks
{
    [SerializeField] CanvasGroup canvasGroupLeaderBoard;
    [SerializeField] CanvasGroup canvasGroupGameOver;

    public float gameDurationInSeconds = 300f; // 5 minutes in this example
    private float timer;

    [SerializeField] private Text timerText;

    private void Start()
    {
        timer = gameDurationInSeconds + 5;
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

            if (timer <= 5f)
            {
                // Game over logic
                // Call an RPC to handle game over actions across the network
                photonView.RPC("GameOver", RpcTarget.AllBuffered);
            }

            if (timer <= 0f)
            {
                // Game over logic
                // Call an RPC to handle game over actions across the network
                photonView.RPC("ShowLeaderBoard", RpcTarget.AllBuffered);
            }
        }
    }

    private void UpdateTimerUI()
    {
        int minutes = Mathf.FloorToInt(timer / 60f);
        int seconds = Mathf.FloorToInt(timer % 60f)-5;
        if(timer>= 5)
        {
            timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
        }
        else
        {
            return;
        }
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
}
