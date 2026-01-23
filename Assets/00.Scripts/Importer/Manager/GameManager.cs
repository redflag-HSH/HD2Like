using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    private bool _isDead;
    private bool _isPaused;
    static GameManager instance = null;
    // Start is called before the first frame update
    void Start()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else
            Destroy(this.gameObject);
        SceneManager.sceneLoaded += OnSceneLoad;
    }

    public static GameManager Instance
    {
        get
        {
            if (instance == null)
                return null;
            return instance;
        }
    }
    private void Update()
    {
        /*if (Input.GetKeyDown(KeyCode.Escape))
        {
            _isPaused = !_isPaused;
            if (_isPaused)
                PauseGame();
            else
                ContinueGame();
        }*/
    }
    public void DeadStopGame()
    {
        _isDead = true;
        PauseGame();
    }

    public void PauseGame()
    {
        if (SceneManager.GetActiveScene().name != "0.Main")
        {
            _isPaused = true;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            Time.timeScale = 0;
            /* if (!_isDead)
                UIManager.Instance.PauseMenu = true;
            else
                UIManager.Instance.DeadMenu = true;*/
            //FindObjectOfType<InputManager>().enabled = false;
        }
        else
        {
            _isPaused = false;
            print("mouse using Scene");
        }
    }

    public void ContinueGame()
    {
        if (SceneManager.GetActiveScene().name == "0.Main")
        {
            //UIManager.Instance.ResetUI();
            _isPaused = false;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            Time.timeScale = 1;
            print("mouse using Scene");
        }
        else if (!_isDead)
        {
            _isPaused = false;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            Time.timeScale = 1;
            //UIManager.Instance.PauseMenu = false;
            //FindObjectOfType<InputManager>().enabled = true;
        }
        else
            _isPaused = false;
    }

   /* public void RestartGame()
    {
        _isDead = false;
        UIManager.Instance.ResetUI();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }*/

    public void QuitGame()
    {
        Application.Quit();
    }
    /*public void ChangeScene(string name)
    {
        if (name == "0.Main")
            UIManager.Instance.MainSwitch(true);
        else
            UIManager.Instance.MainSwitch(false);
        SceneManager.LoadScene(name);
    }*/
    public void OnSceneLoad(Scene scene, LoadSceneMode mode)
    {
        ContinueGame();
    }
    /*public void TestScene(int index)
    {
        switch (index)
        {
            case 1:
                ChangeScene("FPSTest");
                break;
            case 2:
                ChangeScene("AITest");
                break;
        }
    }*/ 
}
