using SUPERCharacter;
using UnityEngine;

public class InstructionsPaper : MonoBehaviour, IUsable
{
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