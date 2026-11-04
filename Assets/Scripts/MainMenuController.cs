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
    
    public void PlayGame()
    {
        MainMenuPanel.SetActive(false);
        //PlayCoroutine de Fade
        MainCamera.enabled = true;
        MenuCamera.enabled = false;
        superCharacterAIO.enabled = true;
        MenuIcon.SetActive(false);
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
}
