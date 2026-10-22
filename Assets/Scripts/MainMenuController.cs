using UnityEngine;

public class MainMenuController : MonoBehaviour
{
    public GameObject MainMenuPanel;
    
    public void PlayGame()
    {
        MainMenuPanel.SetActive(false);
        //GameManager.Instance.StartGame();
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
