using System.Collections;
using SUPERCharacter;
using UnityEngine;
using UnityEngine.UI;

public class Door : Interactable
{
    [Header("Configuração de Estado")]
    public static bool CartaSent = false;

    [Header("Referências de Jogo")]
    public Transform spawn;

    [Header("Configuração de Fade")]
    public Image fadeImage;
    public float fadeDuration = 1f;

    [Header("Áudio Normal da Porta")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip doorunlock;
    [SerializeField] private AudioClip doorlock;

    [Header("Configuração do Final (Último Dia / Dia 5)")]
    [Tooltip("Número do dia final que dispara a sequência especial.")]
    public int diaFinal = 5;

    [Header("Referências de Câmara e Jogador (Estilo RadioTable)")]
    public Camera playerCamera;
    public SUPERCharacterAIO playerController;
    public Transform salaPretaCameraPosition; 
    public InteractionSystem interactionSystem;
    private CrosshairController crosshairController;

    [Header("Áudios do Evento Final")]
    [SerializeField] private AudioClip somAposFadePreto;
    [SerializeField] private AudioClip somAposChegarObjeto;

    [Header("Objeto Especial e Animação")]
    [Tooltip("Objeto que vai aparecer e rodopiar na sala preta.")]
    public GameObject objetoFinal;
    [Tooltip("Ponto inicial onde o objeto começa (mais longe da câmara).")]
    public Transform pontoInicialObjeto;
    [Tooltip("Ponto final onde o objeto para (mais perto da câmara).")]
    public Transform pontoFinalObjeto;
    public float duracaoAproximacao = 3f;
    public Vector3 velocidadeRotacao = new Vector3(0f, 180f, 0f);

    [Header("UI do Final")]
    public GameObject botaoUIFinal;
    public GameObject botaoUIFinalD;
    public float tempoEsperaParaAtivarUI = 2f;

    private void Start()
    {
        crosshairController = FindFirstObjectByType<CrosshairController>();
    }

    public override void Interact(PlayerItem playerItem)
    {
        if (CartaSent && !playerItem.HasItem()) //AQUI---------------------------------------------------------------------------------------------------------------
        {
            if (audioSource != null && doorunlock != null)
            {
                audioSource.PlayOneShot(doorunlock);
            }

            int diaAtual = 1;
            if (DayManager.Instance != null)
            {
                diaAtual = DayManager.Instance.GetCurrentDay();
            }

            if (diaAtual >= diaFinal)
            {
                StartCoroutine(SequenciaFinalRoutine(playerItem));
            }
            else
            {
                StartCoroutine(TransitionRoutine(playerItem));
            }
        }
        else
        {
            if (audioSource != null && doorlock != null)
            {
                audioSource.PlayOneShot(doorlock);
            }
        }
    }

    private IEnumerator TransitionRoutine(PlayerItem playerItem)
    {
        CartaSent = false;

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

    private IEnumerator SequenciaFinalRoutine(PlayerItem playerItem)
    {
        CartaSent = false;

        if (interactionSystem != null) interactionSystem.enabled = false;

        if (crosshairController != null)
        {
            crosshairController.SetCrosshairVisibility(false);
        }

        if (fadeImage != null)
        {
            fadeImage.gameObject.SetActive(true);
            yield return StartCoroutine(Fade(0f, 1f));
        }

        if (playerController != null)
        {
            playerController.PausePlayer(PauseModes.FreezeInPlace);
        }

        if (salaPretaCameraPosition != null && playerCamera != null && playerItem != null)
        {
            CharacterController cc = playerItem.GetComponent<CharacterController>();
            if (cc != null) cc.enabled = false;

            playerItem.transform.position = salaPretaCameraPosition.position;
            playerItem.transform.rotation = salaPretaCameraPosition.rotation;

            playerCamera.transform.position = salaPretaCameraPosition.position;
            playerCamera.transform.rotation = salaPretaCameraPosition.rotation;

            if (cc != null) cc.enabled = true;
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (audioSource != null && somAposFadePreto != null)
        {
            audioSource.PlayOneShot(somAposFadePreto);
            yield return new WaitForSeconds(somAposFadePreto.length);
        }

        if (fadeImage != null)
        {
            yield return StartCoroutine(Fade(1f, 0f));
            fadeImage.gameObject.SetActive(false);
        }

        if (objetoFinal != null)
        {
            objetoFinal.SetActive(true);

            Vector3 posInicial = (pontoInicialObjeto != null) ? pontoInicialObjeto.position : objetoFinal.transform.position;
            Quaternion rotInicial = (pontoInicialObjeto != null) ? pontoInicialObjeto.rotation : objetoFinal.transform.rotation;

            Vector3 posFinal = (pontoFinalObjeto != null) ? pontoFinalObjeto.position : posInicial;
            Quaternion rotFinal = (pontoFinalObjeto != null) ? pontoFinalObjeto.rotation : rotInicial;

            objetoFinal.transform.position = posInicial;
            objetoFinal.transform.rotation = rotInicial;

            int voltasCompletas = 2;

            float tempo = 0f;
            while (tempo < duracaoAproximacao)
            {
                tempo += Time.deltaTime;
                float t = Mathf.Clamp01(tempo / duracaoAproximacao);

                objetoFinal.transform.position = Vector3.Lerp(posInicial, posFinal, t);

                Quaternion rotacaoBase = Quaternion.Slerp(rotInicial, rotFinal, t);

                float anguloExtra = 360f * voltasCompletas * (1f - t);
                Quaternion rotacaoVoltas = Quaternion.AngleAxis(anguloExtra, velocidadeRotacao.normalized);

                objetoFinal.transform.rotation = rotacaoBase * rotacaoVoltas;

                yield return null;
            }

            objetoFinal.transform.position = posFinal;
            objetoFinal.transform.rotation = rotFinal;
        }

        if (audioSource != null && somAposChegarObjeto != null)
        {
            audioSource.PlayOneShot(somAposChegarObjeto);
        }

        yield return new WaitForSeconds(tempoEsperaParaAtivarUI);

        if (botaoUIFinal != null)
        {
            botaoUIFinal.SetActive(true);
            botaoUIFinalD.SetActive(false);

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
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