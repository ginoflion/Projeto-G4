using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Door : Interactable
{
    [Header("Configuração de Estado")]
    public bool CartaSent = true;

    [Header("Referências de Jogo")]
    public Transform spawn;

    [Header("Aviso (Carta não enviada)")]
    public TextMeshProUGUI warningText;
    public float warningDuration = 2f;
    private Coroutine warningCoroutine;

    [Header("Configuração de Fade")]
    public Image fadeImage;
    public float fadeDuration = 1f;

    [Header("Áudio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip doorunlock;
    [SerializeField] private AudioClip doorlock;
    public override void Interact(PlayerItem playerItem)
    {
        if (CartaSent)
        {
            audioSource.PlayOneShot(doorunlock);
            StartCoroutine(TransitionRoutine(playerItem));
        }
        else
        {
            audioSource.PlayOneShot(doorlock);
            ShowWarning("É melhor enviar esta carta primeiro");
        }
    }

    private IEnumerator TransitionRoutine(PlayerItem playerItem)
    {
        CartaSent = false;

        if (warningText != null) warningText.text = "";

        if (fadeImage != null)
        {
            fadeImage.gameObject.SetActive(true);
            yield return StartCoroutine(Fade(0f, 1f));
        }

        if (DayManager.Instance != null)
        {
            DayManager.Instance.AdvanceDay();
        }

        if (CartasManager.Instance != null)
        {
            CartasManager.Instance.SpawnarCarta(); 
        }

        if (spawn != null && playerItem != null)
        {
            CharacterController cc = playerItem.GetComponent<CharacterController>();
            if (cc != null) cc.enabled = false;

            playerItem.transform.position = spawn.position;
            playerItem.transform.rotation = spawn.rotation;

            if (cc != null) cc.enabled = true;
        }

        if (fadeImage != null)
        {
            yield return StartCoroutine(Fade(1f, 0f));
            fadeImage.gameObject.SetActive(false);
        }
    }

    private void ShowWarning(string message)
    {
        if (warningText == null) return;

        if (warningCoroutine != null)
        {
            StopCoroutine(warningCoroutine);
        }

        warningCoroutine = StartCoroutine(WarningRoutine(message));
    }

    private IEnumerator WarningRoutine(string message)
    {
        warningText.text = message;
        warningText.gameObject.SetActive(true);

        yield return new WaitForSeconds(warningDuration);

        warningText.text = "";
        warningText.gameObject.SetActive(false);
        warningCoroutine = null;
    }

    private IEnumerator Fade(float startAlpha, float endAlpha)
    {
        Color color = fadeImage.color;
        float elapsedTime = 0f;
        color.a = startAlpha;
        fadeImage.color = color;

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
}