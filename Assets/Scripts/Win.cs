using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Win : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            MenuScript.Instance.StopGame();
            MenuScript.Instance.curSettings.hasSave = false;
            UnityEngine.SceneManagement.SceneManager.LoadScene(1);
        }
    }
}
