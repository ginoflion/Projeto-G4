using SUPERCharacter;
using UnityEngine;

public class ReadablePaper : MonoBehaviour, IUsable
{
    public Vector3 normalPosition = Vector3.zero;
    public Vector3 normalRotation = Vector3.zero;
    
    public Vector3 zoomedPosition = new Vector3(0f, 0f, 0.4f); 
    public Vector3 zoomedRotation = new Vector3(90f, 0f, 0f);
    
    private bool isReading = false;
    private SUPERCharacterAIO playerController;
    private InteractionSystem interactionSystem;
    private GameObject child;
    
    void Start()
    {
        playerController = FindObjectsByType<SUPERCharacterAIO>(FindObjectsSortMode.None)[0];
        interactionSystem = FindObjectsByType<InteractionSystem>(FindObjectsSortMode.None)[0];
        child = transform.GetChild(0).gameObject;
    }
    
    public void Use()
    {
        isReading = !isReading;

        if (isReading)
        {
            child.layer = LayerMask.NameToLayer("IgnoreCRT");
            transform.localPosition = zoomedPosition;
            transform.localRotation = Quaternion.Euler(zoomedRotation);
            if (playerController != null) 
            {
                playerController.PausePlayer(PauseModes.FreezeInPlace);
            }
            if (interactionSystem != null) interactionSystem.enabled = false;
        }
        else
        {
            child.layer = LayerMask.NameToLayer("Default");
            transform.localPosition = normalPosition;
            transform.localRotation = Quaternion.Euler(normalRotation);

            if (playerController != null) 
            {
                playerController.UnpausePlayer();
            }
            if (interactionSystem != null) interactionSystem.enabled = true;
        }
    }
}
