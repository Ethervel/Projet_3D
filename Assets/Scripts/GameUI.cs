using UnityEngine;
using UnityEngine.UI;

public class GameUI : MonoBehaviour
{
    public static GameUI Instance { get; private set; }

    // ── Couleurs ──────────────────────────────────────────────────────────────
    static readonly Color C_Panel   = new Color(0.04f, 0.14f, 0.04f, 0.88f);
    static readonly Color C_Timer   = new Color(1.00f, 0.88f, 0.30f, 1.00f);
    static readonly Color C_Done    = new Color(0.30f, 0.95f, 0.40f, 1.00f);
    static readonly Color C_Pending = new Color(0.55f, 0.55f, 0.55f, 0.55f);
    static readonly Color C_Prompt  = new Color(0.02f, 0.10f, 0.02f, 0.92f);
    static readonly Color C_White   = Color.white;
    static readonly Color C_Overlay = new Color(0.00f, 0.00f, 0.00f, 0.82f);
    static readonly Color C_Gold    = new Color(1.00f, 0.82f, 0.20f, 1.00f);

    // ── References ─────────────────────────────────────────────────────────────
    private Text    _timerText;
    private Image[] _dots        = new Image[3];
    private Text    _counterText;
    private GameObject _promptPanel;
    private Text    _promptText;
    private GameObject _victoryPanel;
    private Text    _vTimeText;
    private Font    _font;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(this); return; }
        Instance = this;
        _font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf")
             ?? Resources.GetBuiltinResource<Font>("Arial.ttf");
        Build();
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    void Update()
    {
        if (GameManager.Instance == null || GameManager.Instance.GameComplete) return;
        float t  = GameManager.Instance.ElapsedTime;
        int   mn = (int)(t / 60);
        int   sc = (int)(t % 60);
        _timerText.text = $"  {mn:00}:{sc:00}";
    }

    // ── Construction du Canvas ────────────────────────────────────────────────
    void Build()
    {
        var cgo    = new GameObject("GameCanvas");
        var canvas = cgo.AddComponent<Canvas>();
        canvas.renderMode  = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 50;
        var scaler = cgo.AddComponent<CanvasScaler>();
        scaler.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight  = 0.5f;
        cgo.AddComponent<GraphicRaycaster>();

        BuildTopBar(cgo.transform);
        BuildPrompt(cgo.transform);
        BuildVictory(cgo.transform);
    }

    // ── Barre du haut ─────────────────────────────────────────────────────────
    void BuildTopBar(Transform root)
    {
        var bar = Panel(root, "TopBar", C_Panel);
        var rt  = bar.GetComponent<RectTransform>();
        rt.anchorMin  = new Vector2(0, 1);
        rt.anchorMax  = Vector2.one;
        rt.offsetMin  = new Vector2(0, -80);
        rt.offsetMax  = Vector2.zero;

        // -- Timer (gauche)
        var tGO = Txt(bar.transform, "Timer", "  00:00", 38, C_Timer, TextAnchor.MiddleLeft, true);
        Anchor(tGO, 0f, 0f, 0.28f, 1f, 24, 0, -10, 0);
        _timerText = tGO.GetComponent<Text>();

        // -- Label "Anomalies" (centre)
        var lGO = Txt(bar.transform, "Label", "Trouve les anomalitees", 22, new Color(0.7f, 1f, 0.7f, 0.8f),
                      TextAnchor.MiddleCenter, false);
        Anchor(lGO, 0.30f, 0f, 0.62f, 1f, 0, 0, 0, 0);

        // -- Dots x3 (apres label)
        for (int i = 0; i < 3; i++)
        {
            var dot = new GameObject($"Dot{i}");
            dot.transform.SetParent(bar.transform, false);
            var img = dot.AddComponent<Image>();
            img.color = C_Pending;
            var drt = dot.GetComponent<RectTransform>();
            float xMin = 0.63f + i * 0.065f;
            float xMax = xMin  + 0.055f;
            drt.anchorMin = new Vector2(xMin, 0.18f);
            drt.anchorMax = new Vector2(xMax, 0.82f);
            drt.offsetMin = drt.offsetMax = Vector2.zero;
            _dots[i] = img;
        }

        // -- Compteur texte (droite)
        var cGO = Txt(bar.transform, "Counter", "0 / 3", 30, C_White, TextAnchor.MiddleLeft, true);
        Anchor(cGO, 0.855f, 0f, 1f, 1f, 8, 0, -10, 0);
        _counterText = cGO.GetComponent<Text>();
    }

    // ── Prompt bas de l ecran ─────────────────────────────────────────────────
    void BuildPrompt(Transform root)
    {
        _promptPanel = Panel(root, "Prompt", C_Prompt);
        var rt = _promptPanel.GetComponent<RectTransform>();
        rt.anchorMin        = new Vector2(0.5f, 0f);
        rt.anchorMax        = new Vector2(0.5f, 0f);
        rt.pivot            = new Vector2(0.5f, 0f);
        rt.anchoredPosition = new Vector2(0, 32);
        rt.sizeDelta        = new Vector2(620, 64);

        var ptGO = Txt(_promptPanel.transform, "PromptText", "", 30, C_White, TextAnchor.MiddleCenter, true);
        Anchor(ptGO, 0, 0, 1, 1, 20, 4, -20, -4);
        _promptText = ptGO.GetComponent<Text>();

        _promptPanel.SetActive(false);
    }

    // ── Ecran de victoire ─────────────────────────────────────────────────────
    void BuildVictory(Transform root)
    {
        _victoryPanel = Panel(root, "Victory", C_Overlay);
        Stretch(_victoryPanel.GetComponent<RectTransform>());

        // Carte centrale
        var card = Panel(_victoryPanel.transform, "Card", new Color(0.03f, 0.18f, 0.03f, 0.97f));
        var crt  = card.GetComponent<RectTransform>();
        crt.anchorMin        = crt.anchorMax = new Vector2(0.5f, 0.5f);
        crt.pivot            = new Vector2(0.5f, 0.5f);
        crt.anchoredPosition = Vector2.zero;
        crt.sizeDelta        = new Vector2(760, 460);

        // Bordure decorative (ligne verte)
        var border = Panel(card.transform, "Border", new Color(0.3f, 0.8f, 0.3f, 0.5f));
        var brt    = border.GetComponent<RectTransform>();
        brt.anchorMin = Vector2.zero; brt.anchorMax = Vector2.one;
        brt.offsetMin = new Vector2(6, 6); brt.offsetMax = new Vector2(-6, -6);
        var innerCard = Panel(border.transform, "Inner", new Color(0.03f, 0.18f, 0.03f, 0.97f));
        Stretch(innerCard.GetComponent<RectTransform>());
        innerCard.GetComponent<RectTransform>().offsetMin =
        innerCard.GetComponent<RectTransform>().offsetMax = new Vector2(3, 3);

        // Titre
        var titleGO = Txt(innerCard.transform, "Title", "FORET NETTOYEE", 54, C_Gold, TextAnchor.MiddleCenter, true);
        Anchor(titleGO, 0, 0.78f, 1, 0.98f, 20, 0, -20, 0);

        // Sous-titre
        var subGO = Txt(innerCard.transform, "Sub", "Mission accomplie !", 26,
                        new Color(0.7f, 1f, 0.7f, 0.9f), TextAnchor.MiddleCenter, false);
        Anchor(subGO, 0, 0.66f, 1, 0.78f, 0, 0, 0, 0);

        // Temps
        var vtGO = Txt(innerCard.transform, "VTime", "", 34, C_White, TextAnchor.MiddleCenter, true);
        Anchor(vtGO, 0, 0.52f, 1, 0.66f, 20, 0, -20, 0);
        _vTimeText = vtGO.GetComponent<Text>();

        // Score
        var vsGO = Txt(innerCard.transform, "VScore", "3 / 3 anomalies collectees", 26,
                       C_Done, TextAnchor.MiddleCenter, true);
        Anchor(vsGO, 0, 0.40f, 1, 0.52f, 0, 0, 0, 0);

        // Bas de carte
        var botGO = Txt(innerCard.transform, "Bot", "La foret est sauvee — Bravo !",
                        20, new Color(0.6f, 0.9f, 0.6f, 0.8f), TextAnchor.MiddleCenter, false);
        Anchor(botGO, 0, 0.30f, 1, 0.40f, 0, 0, 0, 0);

        // Boutons Recommencer / Quitter
        UiKit.EnsureEventSystem();
        var bSize = new Vector2(260, 70);
        var br = UiKit.Button(innerCard.transform, "Recommencer", bSize,
                              new Color(0.20f, 0.40f, 0.65f, 1f), C_White, GameActions.Restart);
        var bq = UiKit.Button(innerCard.transform, "Quitter", bSize,
                              new Color(0.60f, 0.22f, 0.20f, 1f), C_White, GameActions.Quit);
        PlaceButton(br.GetComponent<RectTransform>(), bSize, new Vector2(-150, 50));
        PlaceButton(bq.GetComponent<RectTransform>(), bSize, new Vector2( 150, 50));

        _victoryPanel.SetActive(false);
    }

    // Place un bouton ancre en bas-centre de la carte de victoire.
    void PlaceButton(RectTransform rt, Vector2 size, Vector2 anchoredPos)
    {
        rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0f);
        rt.sizeDelta        = size;
        rt.anchoredPosition = anchoredPos;
    }

    // ── API publique ──────────────────────────────────────────────────────────
    public void SetPrompt(string msg)
    {
        if (string.IsNullOrEmpty(msg)) { _promptPanel.SetActive(false); return; }
        _promptText.text = msg;
        _promptPanel.SetActive(true);
    }

    public void UpdateCounter(int count)
    {
        _counterText.text = $"{count} / {GameManager.Total}";
        for (int i = 0; i < _dots.Length; i++)
            _dots[i].color = i < count ? C_Done : C_Pending;
    }

    public void ShowVictory(float seconds)
    {
        SetPrompt("");
        int mn = (int)(seconds / 60);
        int sc = (int)(seconds % 60);
        _vTimeText.text = $"Temps  :  {mn:00} min  {sc:00} sec";
        _victoryPanel.SetActive(true);
        GameActions.ShowCursor(true); // curseur libre pour cliquer Recommencer / Quitter
    }

    // ── Helpers ───────────────────────────────────────────────────────────────
    GameObject Panel(Transform parent, string name, Color color)
    {
        var go  = new GameObject(name);
        go.transform.SetParent(parent, false);
        var img = go.AddComponent<Image>();
        img.color = color;
        return go;
    }

    GameObject Txt(Transform parent, string name, string text, int size,
                   Color color, TextAnchor align, bool bold)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var t = go.AddComponent<Text>();
        t.font         = _font;
        t.text         = text;
        t.fontSize     = size;
        t.color        = color;
        t.alignment    = align;
        t.fontStyle    = bold ? FontStyle.Bold : FontStyle.Normal;
        t.raycastTarget = false;
        var rt = go.GetComponent<RectTransform>();
        Stretch(rt);
        return go;
    }

    void Anchor(GameObject go, float xMin, float yMin, float xMax, float yMax,
                float offLX, float offLY, float offRX, float offRY)
    {
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(xMin, yMin);
        rt.anchorMax = new Vector2(xMax, yMax);
        rt.offsetMin = new Vector2(offLX, offLY);
        rt.offsetMax = new Vector2(offRX, offRY);
    }

    void Stretch(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
    }
}
