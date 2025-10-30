using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    public string levelToLoad;
    // Start is called before the first frame update
    void Start()
    {
        GetComponent<Canvas>().enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        //if we press the escape key
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            //pause the game
            Time.timeScale = 0;
            //show our pause menu canvas
            GetComponent<Canvas>().enabled = true;
        }
    }

    public void ResumeGame()
    {
        //continue playing the game... somehow?
        Time.timeScale = 1;
        GetComponent<Canvas>().enabled = false;
    }
    public void QuitGame()
    {
        Application.Quit();
    }

    public void LoadMainMenu()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene(levelToLoad);
    }
}
