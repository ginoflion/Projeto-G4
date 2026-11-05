using UnityEngine;

public class CrosshairController : MonoBehaviour
{
    public GameObject crosshairUI;
    
    public void SetCrosshairVisibility(bool isVisible)
    {
        if (crosshairUI != null)
        {
            crosshairUI.SetActive(isVisible);
        }
    }
}