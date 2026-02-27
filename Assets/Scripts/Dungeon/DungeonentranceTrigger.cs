using UnityEngine;

public class DungeonentranceTrigger : MonoBehaviour
{
    [SerializeField] private GameObject dungeonEntrance;

    [SerializeField] private KeyCode interactKey = KeyCode.E;
    [SerializeField] private string playerTag = "Player";
    private bool isPlayerInRange;


    private void Update()
    {
        if (!isPlayerInRange)
        {
            return;
        }

        if (Input.GetKeyDown(interactKey))
        {
            Debug.Log("Player pressed the interact key");
            SceneFlowManager.Instance.GoToDungeon();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(playerTag))
        {
            isPlayerInRange = true;
            Debug.Log("Player entered the dungeon entrance");
        }
    }
    
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag(playerTag))
        {
            isPlayerInRange = false;
        }
    }

}