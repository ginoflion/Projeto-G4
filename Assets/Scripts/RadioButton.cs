using UnityEngine;
using UnityEngine.InputSystem;

public class RadioButton : MonoBehaviour
{
    [Header("Configuração do Botão")]
    public RadioController radioController;
    [Range(0, 5)] public int digitIndex;
    public bool isIncreaseButton;

    [Header("Layer e Câmara")]
    [Tooltip("A câmara que está a ver o rádio (arrasta a mesma câmara do RadioTable).")]
    public Camera radioCamera;
    [Tooltip("LayerMask para detetar o botão.")]
    public LayerMask interactableLayer;

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
        if (radioController != null)
        {
            if (isIncreaseButton)
            {
                radioController.IncreaseDigit(digitIndex);
            }
            else
            {
                radioController.DecreaseDigit(digitIndex);
            }
        }
        else
        {
            Debug.LogWarning("RadioController não atribuído no botão: " + gameObject.name);
        }
    }
}