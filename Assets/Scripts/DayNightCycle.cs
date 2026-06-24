using UnityEngine;

// Cycle jour / nuit : a placer sur la lumiere directionnelle (le soleil).
// Pilote l orientation du soleil, son intensite, sa couleur, l ambiance et le skybox.
[RequireComponent(typeof(Light))]
public class DayNightCycle : MonoBehaviour
{
    [Header("Duree")]
    [Tooltip("Duree d un cycle complet (jour + nuit) en secondes")]
    public float cycleDuration = 120f;
    [Range(0f, 1f)]
    [Tooltip("Moment de depart : 0 = minuit, 0.25 = aube, 0.5 = midi, 0.75 = crepuscule")]
    public float startTimeOfDay = 0.30f;

    [Header("Soleil - intensite")]
    public float dayIntensity   = 1.0f;
    public float nightIntensity = 0.05f;

    [Header("Soleil - couleurs")]
    public Color dayColor     = new Color(1.00f, 0.96f, 0.84f);
    public Color horizonColor = new Color(1.00f, 0.50f, 0.22f);
    public Color nightColor   = new Color(0.20f, 0.30f, 0.55f);

    [Header("Ambiance (mode Flat)")]
    public Color dayAmbient   = new Color(0.606f, 0.648f, 0.726f);
    public Color nightAmbient = new Color(0.05f, 0.06f, 0.12f);

    [Header("Skybox")]
    public bool  controlSkybox    = true;
    public float daySkyExposure   = 1.0f;
    public float nightSkyExposure  = 0.10f;

    [Header("Debug (lecture seule)")]
    [Range(0f, 1f)] public float timeOfDay;

    Light    _sun;
    float    _yaw, _roll;
    Material _skyInstance, _skyOriginal;

    void Awake()
    {
        _sun  = GetComponent<Light>();
        var e = transform.eulerAngles;
        _yaw  = e.y;   // on conserve l azimut d origine
        _roll = e.z;
        timeOfDay = startTimeOfDay;
    }

    void OnEnable()
    {
        // On travaille sur une COPIE du skybox pour ne jamais modifier l asset.
        if (controlSkybox && RenderSettings.skybox != null)
        {
            _skyOriginal = RenderSettings.skybox;
            _skyInstance = new Material(_skyOriginal);
            RenderSettings.skybox = _skyInstance;
        }
    }

    void OnDisable()
    {
        if (_skyOriginal != null) RenderSettings.skybox = _skyOriginal;
        if (_skyInstance != null) { Destroy(_skyInstance); _skyInstance = null; }
        _skyOriginal = null;
    }

    void Update()
    {
        timeOfDay += Time.deltaTime / Mathf.Max(cycleDuration, 1f);
        if (timeOfDay >= 1f) timeOfDay -= 1f;

        // Orientation : 1 tour complet par cycle. Minuit a t=0, midi a t=0.5.
        float pitch = timeOfDay * 360f - 90f;
        transform.rotation = Quaternion.Euler(pitch, _yaw, _roll);

        // Elevation du soleil : +1 = midi (au zenith), 0 = horizon, -1 = minuit.
        float sunDot = Vector3.Dot(-transform.forward, Vector3.up);

        // Facteur "jour" lisse avec une bande de crepuscule autour de l horizon.
        float day = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01((sunDot + 0.15f) / 0.30f));

        // Intensite (le soleil s eteint la nuit -> petite valeur "lune")
        _sun.intensity = Mathf.Lerp(nightIntensity, dayIntensity, day);
        _sun.enabled   = _sun.intensity > 0.001f;

        // Couleur : orange a l horizon, blanc chaud le jour, bleu la nuit.
        Color c = sunDot >= 0f
            ? Color.Lerp(horizonColor, dayColor,   Mathf.Clamp01(sunDot / 0.40f))
            : Color.Lerp(horizonColor, nightColor, Mathf.Clamp01(-sunDot / 0.30f));
        _sun.color = c;

        // Ambiance globale (mode Flat = RenderSettings.ambientLight est la couleur)
        RenderSettings.ambientLight = Color.Lerp(nightAmbient, dayAmbient, day);

        // Skybox : on baisse l exposition la nuit.
        if (_skyInstance != null && _skyInstance.HasProperty("_Exposure"))
            _skyInstance.SetFloat("_Exposure", Mathf.Lerp(nightSkyExposure, daySkyExposure, day));
    }
}
