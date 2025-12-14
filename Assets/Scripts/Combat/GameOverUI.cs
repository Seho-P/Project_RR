using UnityEngine;
using System;

public class GameOverUI : MonoBehaviour
{
    [SerializeField] private GameObject gameOverUI;
    [SerializeField] private Health playerHealth;

    private void Start()
    {
        gameOverUI.SetActive(false);
        playerHealth.OnDeath += ShowGameOverUI;
    }

    private void OnDestroy()
    {
        playerHealth.OnDeath -= ShowGameOverUI;
    }

    private void ShowGameOverUI()
    {
        gameOverUI.SetActive(true);
        Time.timeScale = 0f;
    }
}