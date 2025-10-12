using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ScenesManager : MonoBehaviour
{
    public void goToMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
    public void goToMain()
    {
        SceneManager.LoadScene("Main");
    }
    public void goToEndGame()
    {
        SceneManager.LoadScene("EndGame");
    }
    public void goToGameIDMenu()
    {
        SceneManager.LoadScene("GameIDMenu");
    }
}
