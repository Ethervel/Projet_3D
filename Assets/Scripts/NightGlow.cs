using UnityEngine;

// Place un halo lumineux (point light) AUTOUR du personnage pour le reperer la nuit.
// Le corps du garde n est PAS rendu emissif : seule la lumiere alentour brille.
public class NightGlow : MonoBehaviour
{
    [Header("Couleur")]
    public Color glowColor = new Color(1.0f, 0.80f, 0.30f); // ambre chaud

    [Header("Halo lumineux")]
    public float lightIntensity = 3.0f;
    public float lightRange     = 9.0f;
    public float heightOffset   = 2.5f;   // hauteur du halo (au-dessus du garde), en metres reels

    [Header("Pulsation")]
    public float pulseSpeed  = 2.0f;
    [Range(0f, 1f)] public float pulseAmount = 0.25f;

    Light _light;

    void Start()
    {
        var go = new GameObject("NightGlowLight");
        go.transform.SetParent(transform, false);
        // heightOffset est en metres reels -> on compense l echelle du parent (le garde est x6)
        float sy = Mathf.Max(transform.lossyScale.y, 0.0001f);
        go.transform.localPosition = new Vector3(0f, heightOffset / sy, 0f);

        _light = go.AddComponent<Light>();
        _light.type      = LightType.Point;
        _light.color     = glowColor;
        _light.intensity = lightIntensity;
        _light.range     = lightRange;
        _light.shadows   = LightShadows.None;
    }

    void Update()
    {
        if (_light == null) return;
        float pulse = 1f + Mathf.Sin(Time.time * pulseSpeed) * pulseAmount;
        _light.intensity = lightIntensity * pulse;
    }
}
