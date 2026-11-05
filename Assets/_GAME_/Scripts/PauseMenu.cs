 using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    [SerializeField] GameObject pauseMenu;
    private bool pause;
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape)) //check if esc key has been presses and if so pause
        {
            if (pause == false)
            {
                Debug.Log("escaped");
                Pause();
            }
            else //if pressed while paused - resume
            {
                Resume();
            }
        }
    }
    public void Pause() //opens pause menu and slows the time down
    {
        pause = true;
        pauseMenu.SetActive(true);
        Time.timeScale = 0.2f;
    }
    public void Resume() //closes pause menu and resume time
    {
        pause = false;
        pauseMenu.SetActive(false);
        Time.timeScale = 1f;
    }
    public void MainMenu() //changes scene to main menu
    {
        SceneManager.LoadSceneAsync(0);
    }

    public void Quit() //exit the game
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
