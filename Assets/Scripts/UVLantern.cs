using UnityEngine;

public class UVLantern : MonoBehaviour, IUsable
{
    public Light uvLight;
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
        Debug.Log(isLightOn);
        isLightOn = !isLightOn;
        if (uvLight != null)
        {
            uvLight.enabled = isLightOn;
        }
    }
}
