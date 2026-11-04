using UnityEngine;
using UnityEngine.InputSystem;

public class RadioButton : MonoBehaviour
{
    public enum ButtonAction { MoveLeft, MoveRight, IncreaseValue, DecreaseValue }

    [Header("Configuração")]
    public RadioManager radioController;
    public ButtonAction actionType;

    [Header("Layer e Câmara")]
    public Camera radioCamera;

    [Header("Áudio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip clickSound;

    void Awake()
    {
        if (audioSource == null) audioSource = GetComponent<AudioSource>();
    }

    void Update()
    {
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
            if (hit.collider.gameObject == gameObject)
            {
                ExecuteAction();
            }
        }
    }

    private void ExecuteAction()
    {
        PlayClickSound();

        if (radioController == null)
        {
            Debug.LogWarning("RadioController não atribuído no botão: " + gameObject.name);
            return;
        }

        // Executa a função correspondente com base no tipo de botão configurado no Inspetor
        switch (actionType)
        {
            case ButtonAction.MoveLeft:
                radioController.SelectPreviousDigit();
                break;
            case ButtonAction.MoveRight:
                radioController.SelectNextDigit();
                break;
            case ButtonAction.IncreaseValue:
                radioController.IncreaseSelectedDigit();
                break;
            case ButtonAction.DecreaseValue:
                radioController.DecreaseSelectedDigit();
                break;
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