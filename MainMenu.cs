using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class MainMenu : MonoBehaviour {
    public void StartFirstLevel() {
        SceneManager.LoadScene(1);
    }

    public void Exit() {
        Application.Quit();
    }
}
