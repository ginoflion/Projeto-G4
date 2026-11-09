using System.Collections;
using SUPERCharacter;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

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

    [Header("Configurações de Fade")]
    public Image fadeImage;
    public float fadeDuration = 1f;

    [Header("Configurações de Áudio")]
    public AudioSource audioSource; 
    public AudioClip menuMusic;      
    public AudioClip gameplayMusic;
    public AudioSource audioSourceB;
    public AudioClip Button;

    private void Start()
    {
        if (audioSource != null && menuMusic != null)
        {
            audioSourceB.clip = Button;
            audioSource.clip = menuMusic;
            audioSource.Play();
        }
    }

    public void PlayGame()
    {
        audioSourceB.Play();
        StartCoroutine(PlayGameWithFadeRoutine());
    }

    private IEnumerator PlayGameWithFadeRoutine()
    {
        if (audioSource != null && gameplayMusic != null)
        {
            audioSource.Stop();
            audioSource.clip = gameplayMusic;
            audioSource.Play();
        }

        if (fadeImage != null)
        {
            fadeImage.gameObject.SetActive(true);
            yield return StartCoroutine(FadeRoutine(0f, 1f));
        }

        MainMenuPanel.SetActive(false);
        MainCamera.enabled = true;
        MenuCamera.enabled = false;
        superCharacterAIO.enabled = true;
        MenuIcon.SetActive(false);

        if (fadeImage != null)
        {
            yield return StartCoroutine(FadeRoutine(1f, 0f));
            fadeImage.gameObject.SetActive(false);
        }
        crosshairController.SetCrosshairVisibility(true);
        if (CartasManager.Instance != null)
        {
            CartasManager.Instance.SpawnJornal1();
        }
    }

    private IEnumerator FadeRoutine(float startAlpha, float endAlpha)
    {
        float elapsedTime = 0f;
        Color color = fadeImage.color;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float currentAlpha = Mathf.Lerp(startAlpha, endAlpha, elapsedTime / fadeDuration);
            color.a = currentAlpha;
            fadeImage.color = color;
            yield return null;
        }

        color.a = endAlpha;
        fadeImage.color = color;
    }

    public void GoBackToMenu()
    {
        audioSourceB.Play();
        MainMenuPanel.SetActive(true);
        ControlsPanel.SetActive(false);
        OptionsPanel.SetActive(false);
        CreditsPanel.SetActive(false);

        if (audioSource != null && menuMusic != null && audioSource.clip != menuMusic)
        {
            audioSource.Stop();
            audioSource.clip = menuMusic;
            audioSource.Play();
        }
    }

    public void OpenControls()
    {
        audioSourceB.Play();
        ControlsPanel.SetActive(true);
        MainMenuPanel.SetActive(false);
        OptionsPanel.SetActive(false);
        CreditsPanel.SetActive(false);
    }

    public void OpenOptions()
    {
        audioSourceB.Play();
        ControlsPanel.SetActive(false);
        MainMenuPanel.SetActive(false);
        OptionsPanel.SetActive(true);
        CreditsPanel.SetActive(false);
    }

    public void OpenCredits()
    {
        audioSourceB.Play();
        ControlsPanel.SetActive(false);
        MainMenuPanel.SetActive(false);
        OptionsPanel.SetActive(false);
        CreditsPanel.SetActive(true);
    }

    public void QuitGame()
    {
        Time.timeScale = 1f; 
        audioSourceB.Play();
        Application.Quit();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
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
        audioSourceB.Play();
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
        audioSourceB.Play();
        pauseMenuControls.SetActive(true);
        pauseMenu.SetActive(false);
        pauseMenuOptions.SetActive(false);
    }

    public void OpenPauseOptions()
    {
        audioSourceB.Play();
        pauseMenuControls.SetActive(false);
        pauseMenu.SetActive(false);
        pauseMenuOptions.SetActive(true);
    }
}