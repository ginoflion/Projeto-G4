using UnityEngine;
using UnityEngine.InputSystem;

public class RadioVerify : MonoBehaviour
{
    [Header("Configuração")]
    [Tooltip("Arrasta o objeto que tem o script RadioController.")]
    public RadioManager radioController;

    [Header("Layer e Câmara")]
    [Tooltip("A câmara que está a ver o rádio.")]
    public Camera radioCamera;

    [Header("Áudio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip clickSound;

    void Awake()
    {
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }
    }

    void Update()
    {
        // Garante que o jogador está a olhar para o rádio (usando a mesma lógica dos outros botões)
        if (!RadioTable.isLookingAtRadio) return;

        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            CheckMouseClick();
        }
    }

    private void CheckMouseClick()
    {
        if (radioCamera == null)
        {
            radioCamera = Camera.main;
            if (radioCamera == null) return;
        }

        Ray ray = radioCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 100f))
        {
            // Verifica se o raio atingiu este exato objeto (o botão de verificar)
            if (hit.collider.gameObject == gameObject)
            {
                ExecuteVerification();
            }
        }
    }

    private void ExecuteVerification()
    {
        PlayClickSound();

        if (radioController != null)
        {
            // Chama a nova função que criámos no RadioController
            radioController.VerificarFrequencia();
        }
        else
        {
            Debug.LogWarning("RadioController não atribuído no botão de verificação: " + gameObject.name);
        }
    }

    private void PlayClickSound()
    {
        if (audioSource != null && clickSound != null)
        {
            audioSource.PlayOneShot(clickSound);
        }
    }
}