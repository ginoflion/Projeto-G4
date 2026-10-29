using UnityEngine;

public class UVLantern : MonoBehaviour, IUsable
{
    public Light uvLight;
    [Header("Áudio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip clickSound;
    private bool isLightOn = false;

    void Start()
    {
        if (uvLight != null)
        {
            uvLight.enabled = isLightOn;
        }
    }

    public void Use()
    {
        audioSource.PlayOneShot(clickSound);
        Debug.Log(isLightOn);
        isLightOn = !isLightOn;
        if (uvLight != null)
        {
            uvLight.enabled = isLightOn;
        }
    }
}
