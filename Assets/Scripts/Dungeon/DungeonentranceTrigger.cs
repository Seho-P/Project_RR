using UnityEngine;
using UnityEngine.SceneManagement;

public class DungeonentranceTrigger : MonoBehaviour
{
    [SerializeField] private GameObject dungeonEntrance;

    [SerializeField] private KeyCode interactKey = KeyCode.E;
    [SerializeField] private string playerTag = "Player";


    private void Update()
    {
        if (Input.GetKeyDown(interactKey))
        {
            Debug.Log("Player pressed the interact key");
            SceneManager.LoadScene("Dungeon");
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(playerTag))
        {
            Debug.Log("Player entered the dungeon entrance");
        }
    }
    
    // private void OnTriggerExit2D(Collider2D other)
    // {
    //     if (other.CompareTag(playerTag))
    //     {
    //         dungeonEntrance.SetActive(false);
    //     }
    // }

}