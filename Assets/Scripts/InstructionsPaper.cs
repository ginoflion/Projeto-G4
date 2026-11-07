using SUPERCharacter;
using UnityEngine;

public class InstructionsPaper : MonoBehaviour, IUsable
{
    public Vector3 normalPosition = Vector3.zero;
    public Vector3 normalRotation = Vector3.zero;
    private MainMenuController mainMenuController;
    void Start()
    {
        mainMenuController = FindObjectsByType<MainMenuController>(FindObjectsSortMode.None)[0];
    }
    
    public void Use()
    {
        mainMenuController.OpenPauseMenu();
    }
}