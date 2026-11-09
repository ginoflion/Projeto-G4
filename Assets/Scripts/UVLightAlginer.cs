using UnityEngine;

public class UVLightAligner : MonoBehaviour
{
    [Header("References")]
    public Transform mainCamera;
    public Light uvSpotlight;

    [Header("Settings")]
    public float rotationSpeed = 15f; 

    private void Start()
    {
        if (mainCamera == null && Camera.main != null)
        {
            mainCamera = Camera.main.transform;
        }
    }

    private void LateUpdate()
    {
        if (mainCamera == null || uvSpotlight == null) return;

        Quaternion targetRotation = Quaternion.LookRotation(mainCamera.forward);
        uvSpotlight.transform.rotation = Quaternion.Slerp(uvSpotlight.transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
    }
}