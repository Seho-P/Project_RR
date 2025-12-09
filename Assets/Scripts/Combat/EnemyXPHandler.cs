using UnityEngine;
using System;

public class EnemyXPHandler : MonoBehaviour
{
    [SerializeField] private int xpAmount = 10;
    [SerializeField] private Health enemyHealth;
    

    // private void Start(){
    //     enemyHealth.OnDeath += AddXP;
    // }

    // private void OnDestroy(){
    //     enemyHealth.OnDeath -= AddXP;
    // }

    // private void AddXP(){
    //     // PlayerXP.Instance.AddXP(xpAmount);
    // }
}