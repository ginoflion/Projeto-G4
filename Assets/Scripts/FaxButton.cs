using UnityEngine;

public class FaxButton : Interactable
{
    public enum ButtonType { Subir, Descer, Enviar }
    
    [Header("Configuração do Botão")]
    public ButtonType type;
    public FaxMachine faxMachine;
    [Header("Áudio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip clickSound;

    void Start()
    {
        itemName = "Botão " + type.ToString(); 
    }

    public override void Interact(PlayerItem playerHand)
    {
        if (faxMachine == null) return;
        switch (type)
        {
            case ButtonType.Subir:
                faxMachine.CycleStation(1);
                audioSource.PlayOneShot(clickSound);
                break;
            case ButtonType.Descer:
                faxMachine.CycleStation(-1);
                audioSource.PlayOneShot(clickSound);
                break;
            case ButtonType.Enviar:
                faxMachine.SendFax();
                audioSource.PlayOneShot(clickSound);
                break;
        }
    }
}