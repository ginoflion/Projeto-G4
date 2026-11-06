using SUPERCharacter;
using UnityEngine;

public class MainMenuController : MonoBehaviour
{
    public GameObject MainMenuPanel;
    public GameObject ControlsPanel;
    public GameObject OptionsPanel;
    public GameObject CreditsPanel;
    public Camera MainCamera;
    public Camera MenuCamera;
    public SUPERCharacterAIO superCharacterAIO;
    public GameObject MenuIcon;
    public CrosshairController crosshairController;
    public GameObject pauseMenu;
    public GameObject pauseMenuControls;
    public GameObject pauseMenuOptions;
    public GameObject Crosshair;
    
    public void PlayGame()
    {
        MainMenuPanel.SetActive(false);
        //PlayCoroutine de Fade
        if (CartasManager.Instance != null)
        {
            CartasManager.Instance.SpawnJornal1();
        }
        MainCamera.enabled = true;
        MenuCamera.enabled = false;
        superCharacterAIO.enabled = true;
        MenuIcon.SetActive(false);
        crosshairController.SetCrosshairVisibility(true);
    }

    public void GoBackToMenu()
    {
        MainMenuPanel.SetActive(true);
        ControlsPanel.SetActive(false);
        OptionsPanel.SetActive(false);
        CreditsPanel.SetActive(false);
    }

    public void OpenControls()
    {
        ControlsPanel.SetActive(true);
        MainMenuPanel.SetActive(false);
        OptionsPanel.SetActive(false);
        CreditsPanel.SetActive(false);
    }

    public void OpenOptions()
    {
        ControlsPanel.SetActive(false);
        MainMenuPanel.SetActive(false);
        OptionsPanel.SetActive(true);
        CreditsPanel.SetActive(false);
    }

    public void OpenCredits()
    {
        ControlsPanel.SetActive(false);
        MainMenuPanel.SetActive(false);
        OptionsPanel.SetActive(false);
        CreditsPanel.SetActive(true);
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void OpenPauseMenu()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        MenuIcon.SetActive(true);
        pauseMenu.SetActive(true);
        Crosshair.SetActive(false);
        Time.timeScale = 0f;
    }

    public void ReturnToGame()
    {
        pauseMenu.SetActive(false);
        MenuIcon.SetActive(false);
        Crosshair.SetActive(true);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        Time.timeScale = 1f;
    }
    public void GoToPauseMenu()
    {
        
        pauseMenu.SetActive(true);
        pauseMenuOptions.SetActive(false);
        pauseMenuControls.SetActive(false);
    }
    
    public void OpenPauseControls()
    {
        pauseMenuControls.SetActive(true);
        pauseMenu.SetActive(false);
        pauseMenuOptions.SetActive(false);
    }

    public void OpenPauseOptions()
    {
        pauseMenuControls.SetActive(false);
        pauseMenu.SetActive(false);
        pauseMenuOptions.SetActive(true);
    }
}
