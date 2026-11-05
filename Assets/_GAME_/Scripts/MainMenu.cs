using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void playGame() //function for play button on main menu
    {
        SceneManager.LoadSceneAsync(1);
    }
    public void quitGame() //function for quit button on main menu
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
