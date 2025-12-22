using UnityEngine;
using System;

public class EnemyXPHandler : MonoBehaviour
{
    [SerializeField] private int xpAmount = 10;
    [SerializeField] private Health enemyHealth;
    

    private void Start(){
        enemyHealth.OnDeath += AddXP;
    }

    private void OnDestroy(){
        enemyHealth.OnDeath -= AddXP;
    }

    private void AddXP(){
        PlayerLevelSystem playerLevelSystem = FindFirstObjectByType<PlayerLevelSystem>();
        if (playerLevelSystem != null)
        {
            playerLevelSystem.AddXP(xpAmount);
        }
        else
        {
            Debug.LogWarning("EnemyXPHandler: PlayerLevelSystem을 찾을 수 없습니다!");
        }
    }
}