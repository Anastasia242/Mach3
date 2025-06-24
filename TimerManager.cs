using System;
using UnityEngine;
using TMPro;

public class TimerManager : MonoBehaviour
{
    [Header("UI")]
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI moveText;

    private float timeLeft;
    private int movesLeft;

    private bool isTimerActive = false;
    private bool isMoveLimitActive = false;

    public Action OnTimeOver;
    public Action OnMovesOver;

    private void Update()
    {
        if (isTimerActive)
        {
            timeLeft -= Time.deltaTime;
            UpdateTimerUI();

            if (timeLeft <= 0f)
            {
                isTimerActive = false;
                OnTimeOver?.Invoke();
            }
        }
    }

    public void StartCountdown(int seconds)
    {
        timeLeft = seconds;
        isTimerActive = true;
        isMoveLimitActive = false;
        UpdateTimerUI();
    }

    public void SetMoves(int moves)
    {
        movesLeft = moves;
        isMoveLimitActive = true;
        isTimerActive = false;
        UpdateMoveUI();
    }

    public void DecrementMove()
    {
        if (!isMoveLimitActive) return;

        movesLeft--;
        UpdateMoveUI();

        if (movesLeft <= 0)
        {
            isMoveLimitActive = false;
            OnMovesOver?.Invoke();
        }
    }

    private void UpdateTimerUI()
    {
        TimeSpan time = TimeSpan.FromSeconds(Mathf.Max(0, timeLeft));
        timerText.text = $"{time.Minutes:D2}:{time.Seconds:D2}";
    }

    private void UpdateMoveUI()
    {
        moveText.text = $"moves: {movesLeft}";
    }

    public void StopAll()
    {
        isTimerActive = false;
        isMoveLimitActive = false;
    }

}
