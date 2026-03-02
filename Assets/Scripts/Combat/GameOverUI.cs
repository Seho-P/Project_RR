using UnityEngine;
using System;

public class GameOverUI : MonoBehaviour
{
    [SerializeField] private GameObject gameOverUI;
    [SerializeField] private Health playerHealth;

    private void Start()
    {
        if (gameOverUI != null)
        {
            gameOverUI.SetActive(false);
        }

        if (playerHealth != null)
        {
            Bind(playerHealth);
        }
    }

    private void OnDestroy()
    {
        Unbind();
    }

    public void Bind(Health health)
    {
        if (playerHealth != null)
        {
            playerHealth.OnDeath -= ShowGameOverUI;
        }

        playerHealth = health;

        if (playerHealth == null)
        {
            return;
        }

        if (gameOverUI != null)
        {
            gameOverUI.SetActive(false);
        }

        playerHealth.OnDeath += ShowGameOverUI;
    }

    public void Unbind()
    {
        if (playerHealth != null)
        {
            playerHealth.OnDeath -= ShowGameOverUI;
            playerHealth = null;
        }

        if (gameOverUI != null)
        {
            gameOverUI.SetActive(false);
        }
    }

    private void ShowGameOverUI()
    {
        if (gameOverUI != null)
        {
            gameOverUI.SetActive(true);
        }

        Time.timeScale = 0f;
    }
}