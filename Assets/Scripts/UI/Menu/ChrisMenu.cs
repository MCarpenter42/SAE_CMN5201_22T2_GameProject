using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ChrisMenu : UI
{
    public void Quit()
    {
        Debug.Log("QUIT");
        Application.Quit();
    }
    public void Level1()
    {
        SceneManager.LoadScene(1);
    }
}