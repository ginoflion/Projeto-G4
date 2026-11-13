using System.Collections;
using SUPERCharacter;
using UnityEngine;
using UnityEngine.UI;

public class Door : Interactable, IUsable
{
    [Header("Configuração de Estado")]
    public static bool CartaSent = false;
    public GameObject helper;

    [Header("Estado de Tranca")]
    [Tooltip("Define se a porta está trancada por ação do jogador (Use).")]
    public bool isLocked = false;

    [Header("Referências de Jogo")]
    public Transform spawn;

    [Header("Configuração de Fade")]
    public Image fadeImage;
    public float fadeDuration = 1f;

    [Header("Áudios de Interação (Interact)")]
    [SerializeField] private AudioSource audioSource;
    [Tooltip("Som quando a porta abre com sucesso (avança o dia).")]
    [SerializeField] private AudioClip somPortaAbrirComSucesso;
    [Tooltip("Som quando o jogador tenta interagir mas a porta está trancada ou não pode avançar.")]
    [SerializeField] private AudioClip somTentativaFalhadaInteract;
    [SerializeField] private AudioSource Game;

    [Header("Áudios de Trancar/Destrancar (Use)")]
    [Tooltip("Som executado especificamente ao trancar a porta com o Use.")]
    [SerializeField] private AudioClip somTrancar;
    [Tooltip("Som executado especificamente ao destrancar a porta com o Use.")]
    [SerializeField] private AudioClip somDestrancar;

    [Header("Configuração do Final (Último Dia / Dia 5)")]
    [Tooltip("Número do dia final que dispara a sequência especial.")]
    public int diaFinal = 5;
    [Tooltip("Tempo de espera após trancar a porta no dia final para disparar a sequência.")]
    public float tempoEsperaParaFinalTrancada = 3f;

    [Header("Referências de Câmara e Jogador")]
    public Camera playerCamera;
    public SUPERCharacterAIO playerController;
    public Transform salaPretaCameraPosition;
    public InteractionSystem interactionSystem;
    private CrosshairController crosshairController;

    [Header("--- EVENTO FINAL 1 (Por Interação / Interact - Resposta Errada ou Padrão) ---")]
    [SerializeField] private AudioClip somAposFadePretoFinal1;
    [SerializeField] private AudioClip somAposChegarObjetoFinal1;
    [Tooltip("Objeto que vai aparecer e rodopiar no Final 1.")]
    public GameObject objetoFinal1;

    [Header("--- EVENTO FINAL 2 (Por Trancar a Porta / Use - Resposta Certa) ---")]
    [SerializeField] private AudioClip somAposFadePretoFinal2;
    [SerializeField] private AudioClip somAposChegarObjetoFinal2;
    [Tooltip("Objeto que vai aparecer e rodopiar no Final 2 (Trancado).")]
    public GameObject objetoFinal2;

    [Header("Configuração Comum do Objeto e Animação")]
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

    private Coroutine rotinaFinalTrancada;

    private void Start()
    {
        CartaSent = false;
        crosshairController = FindFirstObjectByType<CrosshairController>();
    }

    public override void Interact(PlayerItem playerItem)
    {
        if (isLocked)
        {
            if (audioSource != null && somTentativaFalhadaInteract != null)
            {
                audioSource.PlayOneShot(somTentativaFalhadaInteract);
            }
            return;
        }

        if (CartaSent && !playerItem.HasItem())
        {
            int diaAtual = 1;
            if (DayManager.Instance != null)
            {
                diaAtual = DayManager.Instance.GetCurrentDay();
            }

            if (diaAtual >= diaFinal)
            {
                bool cartaFoiCerta = (CartasManager.Instance != null &&
                                      CartasManager.Instance.GetUltimaEscolha().ToLower() == "certa");

                if (cartaFoiCerta)
                {
                    StartCoroutine(SequenciaFinalRoutine(playerItem, 2));
                }
                else
                {
                    StartCoroutine(SequenciaFinalRoutine(playerItem, 1));
                }
            }
            else
            {
                if (audioSource != null && somPortaAbrirComSucesso != null)
                {
                    audioSource.PlayOneShot(somPortaAbrirComSucesso);
                }
                StartCoroutine(TransitionRoutine(playerItem));
            }
        }
        else
        {
            if (audioSource != null && somTentativaFalhadaInteract != null)
            {
                audioSource.PlayOneShot(somTentativaFalhadaInteract);
            }
        }
    }

    private IEnumerator TransitionRoutine(PlayerItem playerItem)
    {
        CartaSent = false;
        RadioManager.jaSpawnouHoje = false;
        if (playerController != null)
        {
            playerController.enableMovementControl = false;
        }

        RadioManager radioManager = FindFirstObjectByType<RadioManager>();
        if (radioManager != null)
        {
            radioManager.ResetarDigitosParaZero();
        }

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

        Transform targetTransform = (playerController != null)
            ? playerController.transform
            : (playerItem != null ? playerItem.transform : null);

        if (spawn != null && targetTransform != null)
        {
            Rigidbody rb = targetTransform.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                rb.Sleep();
            }

            targetTransform.position = spawn.position;
            targetTransform.rotation = spawn.rotation;
            Physics.SyncTransforms();
        }

        if (fadeImage != null)
        {
            yield return StartCoroutine(Fade(1f, 0f));
            fadeImage.gameObject.SetActive(false);
        }
        if (playerController != null)
        {
            playerController.enableMovementControl = true;
        }
    }

    private IEnumerator SequenciaFinalRoutine(PlayerItem playerItem, int tipoFinal)
    {
        Game.Stop();
        CartaSent = false;
        helper.SetActive(false);

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

        AudioClip somFade = (tipoFinal == 2) ? somAposFadePretoFinal2 : somAposFadePretoFinal1;
        if (audioSource != null && somFade != null)
        {
            audioSource.PlayOneShot(somFade);
            yield return new WaitForSeconds(somFade.length);
        }

        if (fadeImage != null)
        {
            yield return StartCoroutine(Fade(1f, 0f));
            fadeImage.gameObject.SetActive(false);
        }

        GameObject objetoEscolhido = (tipoFinal == 2) ? objetoFinal2 : objetoFinal1;

        if (objetoEscolhido != null)
        {
            objetoEscolhido.SetActive(true);

            Vector3 posInicial = (pontoInicialObjeto != null) ? pontoInicialObjeto.position : objetoEscolhido.transform.position;
            Quaternion rotInicial = (pontoInicialObjeto != null) ? pontoInicialObjeto.rotation : objetoEscolhido.transform.rotation;

            Vector3 posFinal = (pontoFinalObjeto != null) ? pontoFinalObjeto.position : posInicial;
            Quaternion rotFinal = (pontoFinalObjeto != null) ? pontoFinalObjeto.rotation : rotInicial;

            objetoEscolhido.transform.position = posInicial;
            objetoEscolhido.transform.rotation = rotInicial;

            int voltasCompletas = 2;

            float tempo = 0f;
            while (tempo < duracaoAproximacao)
            {
                tempo += Time.deltaTime;
                float t = Mathf.Clamp01(tempo / duracaoAproximacao);

                objetoEscolhido.transform.position = Vector3.Lerp(posInicial, posFinal, t);

                Quaternion rotacaoBase = Quaternion.Slerp(rotInicial, rotFinal, t);
                float anguloExtra = 360f * voltasCompletas * (1f - t);
                Quaternion rotacaoVoltas = Quaternion.AngleAxis(anguloExtra, velocidadeRotacao.normalized);

                objetoEscolhido.transform.rotation = rotacaoBase * rotacaoVoltas;

                yield return null;
            }

            objetoEscolhido.transform.position = posFinal;
            objetoEscolhido.transform.rotation = rotFinal;
        }

        AudioClip somChegada = (tipoFinal == 2) ? somAposChegarObjetoFinal2 : somAposChegarObjetoFinal1;
        if (audioSource != null && somChegada != null)
        {
            audioSource.PlayOneShot(somChegada);
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

    public void Use()
    {
        isLocked = !isLocked;

        if (isLocked)
        {
            if (audioSource != null && somTrancar != null)
            {
                audioSource.PlayOneShot(somTrancar);
            }

            int diaAtual = 1;
            if (DayManager.Instance != null)
            {
                diaAtual = DayManager.Instance.GetCurrentDay();
            }

            PlayerItem playerItemRef = FindFirstObjectByType<PlayerItem>();

            bool cartaFoiCerta = (CartasManager.Instance != null &&
                                  CartasManager.Instance.GetUltimaEscolha().ToLower() == "certa");

            if (diaAtual >= diaFinal && CartaSent && cartaFoiCerta && playerItemRef != null)
            {
                if (rotinaFinalTrancada != null)
                {
                    StopCoroutine(rotinaFinalTrancada);
                }
                rotinaFinalTrancada = StartCoroutine(EsperarEIniciarFinal(playerItemRef));
            }
        }
        else
        {
            if (audioSource != null && somDestrancar != null)
            {
                audioSource.PlayOneShot(somDestrancar);
            }

            if (rotinaFinalTrancada != null)
            {
                StopCoroutine(rotinaFinalTrancada);
                rotinaFinalTrancada = null;
            }
        }
    }

    private IEnumerator EsperarEIniciarFinal(PlayerItem playerItem)
    {
        yield return new WaitForSeconds(tempoEsperaParaFinalTrancada);
        StartCoroutine(SequenciaFinalRoutine(playerItem, 2));
    }
}