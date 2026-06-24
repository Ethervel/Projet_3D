using UnityEngine;
using UnityEngine.UI;

// Menu de debut : ecran d accueil avec Commencer / Quitter.
// Le jeu est gele tant que le joueur n a pas clique sur Commencer.
public class StartMenu : MonoBehaviour
{
    public static StartMenu Instance { get; private set; }
    public static bool IsActive { get; private set; }

    static readonly Color C_Overlay = new Color(0.01f, 0.06f, 0.01f, 0.92f);
    static readonly Color C_Card    = new Color(0.03f, 0.16f, 0.03f, 0.98f);
    static readonly Color C_Border  = new Color(0.30f, 0.80f, 0.30f, 0.55f);
    static readonly Color C_Gold    = new Color(1.00f, 0.82f, 0.20f, 1.00f);
    static readonly Color C_Start   = new Color(0.18f, 0.55f, 0.22f, 1f);
    static readonly Color C_Quit    = new Color(0.60f, 0.22f, 0.20f, 1f);

    GameObject _root;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(this); return; }
        Instance = this;
        UiKit.EnsureEventSystem();
        Build();
        Open();
    }

    public void Open()
    {
        IsActive       = true;
        Time.timeScale = 0f;
        _root.SetActive(true);
        GameActions.ShowCursor(true);
        GameActions.FreezePlayer(true);
    }

    public void StartGame()
    {
        IsActive       = false;
        Time.timeScale = 1f;
        _root.SetActive(false);
        GameActions.ShowCursor(false);
        GameActions.FreezePlayer(false);
    }

    // ── Construction de l UI ───────────────────────────────────────────────────
    void Build()
    {
        var canvas = UiKit.Canvas("StartCanvas", 150);
        _root = canvas;

        var overlay = UiKit.Panel(canvas.transform, "Overlay", C_Overlay, blockRaycast: true);
        UiKit.Stretch(overlay.rectTransform);

        var card = UiKit.Panel(overlay.transform, "Card", C_Card, blockRaycast: true);
        UiKit.Center(card.rectTransform, new Vector2(620, 540), Vector2.zero);

        var border = UiKit.Panel(card.transform, "Border", C_Border);
        border.rectTransform.offsetMin = new Vector2(6, 6);
        border.rectTransform.offsetMax = new Vector2(-6, -6);
        var inner = UiKit.Panel(border.transform, "Inner", C_Card);
        inner.rectTransform.offsetMin = new Vector2(3, 3);
        inner.rectTransform.offsetMax = new Vector2(-3, -3);

        // Titre
        var title = UiKit.Label(inner.transform, "Title", "NETTOYAGE\nDE LA FORET", 56, C_Gold,
                                TextAnchor.MiddleCenter, true);
        title.rectTransform.anchorMin = new Vector2(0, 0.62f);
        title.rectTransform.anchorMax = new Vector2(1, 0.96f);
        title.rectTransform.offsetMin = title.rectTransform.offsetMax = Vector2.zero;

        // Sous-titre
        var sub = UiKit.Label(inner.transform, "Sub", "Trouve et ramasse les 3 anomalies", 24,
                              new Color(0.7f, 1f, 0.7f, 0.9f), TextAnchor.MiddleCenter, false);
        sub.rectTransform.anchorMin = new Vector2(0, 0.50f);
        sub.rectTransform.anchorMax = new Vector2(1, 0.62f);
        sub.rectTransform.offsetMin = sub.rectTransform.offsetMax = Vector2.zero;

        // Boutons
        var size = new Vector2(400, 82);
        var bStart = UiKit.Button(inner.transform, "Commencer", size, C_Start, Color.white, StartGame);
        var bQuit  = UiKit.Button(inner.transform, "Quitter",   size, C_Quit,  Color.white, GameActions.Quit);
        UiKit.Center(bStart.GetComponent<RectTransform>(), size, new Vector2(0,  10));
        UiKit.Center(bQuit.GetComponent<RectTransform>(),  size, new Vector2(0, -95));

        // Astuce
        var hint = UiKit.Label(inner.transform, "Hint", "E : ramasser     Echap : pause", 20,
                               new Color(0.6f, 0.85f, 0.6f, 0.8f), TextAnchor.MiddleCenter, false);
        hint.rectTransform.anchorMin = new Vector2(0, 0.02f);
        hint.rectTransform.anchorMax = new Vector2(1, 0.12f);
        hint.rectTransform.offsetMin = hint.rectTransform.offsetMax = Vector2.zero;
    }

    void OnDestroy()
    {
        if (Instance == this) { Instance = null; IsActive = false; Time.timeScale = 1f; }
    }
}
