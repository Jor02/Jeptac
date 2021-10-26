using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class WinScreen : MonoBehaviour
{
    public TextMeshProUGUI time;
    public TextMeshProUGUI jumps;

    private void Awake()
    {
        System.TimeSpan timeSpan = System.TimeSpan.FromSeconds(MenuScript.gameTime);
        time.text = timeSpan.ToString(@"mm\:ss\.fff");
        jumps.text = MenuScript.launches + " LAUNCHES";
    }

    public void Exit() => UnityEngine.SceneManagement.SceneManager.LoadScene(0);
}
