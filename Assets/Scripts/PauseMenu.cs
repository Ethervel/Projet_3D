using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Menu pause (touche Echap) : Reprendre / Recommencer / Quitter.
public class PauseMenu : MonoBehaviour
{
    public static PauseMenu Instance { get; private set; }
    public static bool IsPaused { get; private set; }

    public KeyCode toggleKey = KeyCode.Escape;

    static readonly Color C_Overlay = new Color(0f, 0f, 0f, 0.78f);
    static readonly Color C_Card    = new Color(0.03f, 0.16f, 0.03f, 0.98f);
    static readonly Color C_Border  = new Color(0.30f, 0.80f, 0.30f, 0.55f);
    static readonly Color C_Gold    = new Color(1.00f, 0.82f, 0.20f, 1.00f);
    static readonly Color C_Resume  = new Color(0.18f, 0.55f, 0.22f, 1f);
    static readonly Color C_Restart = new Color(0.20f, 0.40f, 0.65f, 1f);
    static readonly Color C_Quit    = new Color(0.60f, 0.22f, 0.20f, 1f);

    GameObject _root;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(this); return; }
        Instance = this;
        UiKit.EnsureEventSystem();
        Build();
    }

    void Update()
    {
        // Pas de pause pendant le menu de debut ni si la partie est finie.
        if (StartMenu.IsActive) return;
        if (GameManager.Instance != null && GameManager.Instance.GameComplete) return;
        if (Input.GetKeyDown(toggleKey))
            Toggle();
    }

    public void Toggle() { if (IsPaused) Resume(); else Pause(); }

    public void Pause()
    {
        IsPaused        = true;
        Time.timeScale  = 0f;
        _root.SetActive(true);
        GameActions.ShowCursor(true);
        GameActions.FreezePlayer(true);
    }

    public void Resume()
    {
        IsPaused       = false;
        Time.timeScale = 1f;
        _root.SetActive(false);
        GameActions.ShowCursor(false);
        GameActions.FreezePlayer(false);
    }

    // ── Construction de l UI ───────────────────────────────────────────────────
    void Build()
    {
        var canvas = UiKit.Canvas("PauseCanvas", 100);
        _root = canvas;

        var overlay = UiKit.Panel(canvas.transform, "Overlay", C_Overlay, blockRaycast: true);
        UiKit.Stretch(overlay.rectTransform);

        var card = UiKit.Panel(overlay.transform, "Card", C_Card, blockRaycast: true);
        UiKit.Center(card.rectTransform, new Vector2(560, 520), Vector2.zero);

        var border = UiKit.Panel(card.transform, "Border", C_Border);
        border.rectTransform.offsetMin = new Vector2(6, 6);
        border.rectTransform.offsetMax = new Vector2(-6, -6);
        var inner = UiKit.Panel(border.transform, "Inner", C_Card);
        inner.rectTransform.offsetMin = inner.rectTransform.offsetMax = Vector2.zero;
        inner.rectTransform.offsetMin = new Vector2(3, 3);
        inner.rectTransform.offsetMax = new Vector2(-3, -3);

        var title = UiKit.Label(inner.transform, "Title", "PAUSE", 60, C_Gold,
                                TextAnchor.MiddleCenter, true);
        title.rectTransform.anchorMin = new Vector2(0, 0.74f);
        title.rectTransform.anchorMax = new Vector2(1, 0.98f);
        title.rectTransform.offsetMin = title.rectTransform.offsetMax = Vector2.zero;

        var size = new Vector2(380, 78);
        var b1 = UiKit.Button(inner.transform, "Reprendre",   size, C_Resume,  Color.white, Resume);
        var b2 = UiKit.Button(inner.transform, "Recommencer", size, C_Restart, Color.white, GameActions.Restart);
        var b3 = UiKit.Button(inner.transform, "Quitter",     size, C_Quit,    Color.white, GameActions.Quit);
        UiKit.Center(b1.GetComponent<RectTransform>(), size, new Vector2(0,  100));
        UiKit.Center(b2.GetComponent<RectTransform>(), size, new Vector2(0,    5));
        UiKit.Center(b3.GetComponent<RectTransform>(), size, new Vector2(0,  -90));

        var hint = UiKit.Label(inner.transform, "Hint", "Echap pour reprendre", 20,
                               new Color(0.6f, 0.85f, 0.6f, 0.8f), TextAnchor.MiddleCenter, false);
        hint.rectTransform.anchorMin = new Vector2(0, 0.02f);
        hint.rectTransform.anchorMax = new Vector2(1, 0.12f);
        hint.rectTransform.offsetMin = hint.rectTransform.offsetMax = Vector2.zero;

        _root.SetActive(false);
    }

    void OnDestroy()
    {
        if (Instance == this) { Instance = null; IsPaused = false; Time.timeScale = 1f; }
    }
}
