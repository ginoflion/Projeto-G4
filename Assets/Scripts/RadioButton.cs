using UnityEngine;

public class RadioButton : Interactable
{
    [Header("Configuração do Botão")]
    [Tooltip("Referência ao RadioController principal.")]
    public RadioController radioController;

    [Tooltip("Qual é o dígito que este botão controla (0 a 5)?")]
    [Range(0, 5)] public int digitIndex;

    [Tooltip("Se verdadeiro, aumenta o número. Se falso, diminui.")]
    public bool isIncreaseButton;

    public override void Interact(PlayerItem playerHand)
    {
        // Em vez de apanhar o item (comportamento padrão do Interactable base),
        // nós executamos a ação do rádio.
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

    // Opcional: Podes adicionar feedback visual de hover aqui se quiseres
    public override void OnHoverEnter()
    {
        base.OnHoverEnter();
    }

    public override void OnHoverExit()
    {
        base.OnHoverExit();
    }
}