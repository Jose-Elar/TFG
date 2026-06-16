using UnityEngine;

public class DisableBackground : MonoBehaviour
{
    private ParallaxLayer[] _parallaxLayers;

    void Awake()
    {
        _parallaxLayers = GetComponentsInChildren<ParallaxLayer>();
    }

    public void DisableParallax()
    {
        foreach (ParallaxLayer layer in _parallaxLayers)
            layer.enabled = false;
    }

    public void EnableParallax()
    {
        foreach (ParallaxLayer layer in _parallaxLayers)
            layer.enabled = true;
    }
}