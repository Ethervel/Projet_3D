using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;

// Helpers UI partages (construction par code, style commun au jeu).
public static class UiKit
{
    public static Font Font =>
        Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf")
        ?? Resources.GetBuiltinResource<Font>("Arial.ttf");

    // ── EventSystem (indispensable pour cliquer les boutons) ───────────────────
    public static void EnsureEventSystem()
    {
        if (EventSystem.current != null) return;
        if (Object.FindObjectOfType<EventSystem>() != null) return;
        var go = new GameObject("EventSystem");
        go.AddComponent<EventSystem>();
        go.AddComponent<StandaloneInputModule>(); // activeInputHandler = Both -> OK
    }

    // ── Canvas plein ecran ─────────────────────────────────────────────────────
    public static GameObject Canvas(string name, int sortingOrder)
    {
        var go     = new GameObject(name);
        var canvas = go.AddComponent<Canvas>();
        canvas.renderMode   = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = sortingOrder;
        var scaler = go.AddComponent<CanvasScaler>();
        scaler.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight  = 0.5f;
        go.AddComponent<GraphicRaycaster>();
        return go;
    }

    // ── Panneau (Image colore) ─────────────────────────────────────────────────
    public static Image Panel(Transform parent, string name, Color color, bool blockRaycast = false)
    {
        var go  = new GameObject(name);
        go.transform.SetParent(parent, false);
        var img = go.AddComponent<Image>();
        img.color         = color;
        img.raycastTarget = blockRaycast;
        Stretch(img.rectTransform);
        return img;
    }

    // ── Texte ──────────────────────────────────────────────────────────────────
    public static Text Label(Transform parent, string name, string text, int size,
                             Color color, TextAnchor align, bool bold)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var t = go.AddComponent<Text>();
        t.font          = Font;
        t.text          = text;
        t.fontSize      = size;
        t.color         = color;
        t.alignment     = align;
        t.fontStyle     = bold ? FontStyle.Bold : FontStyle.Normal;
        t.raycastTarget = false;
        t.horizontalOverflow = HorizontalWrapMode.Overflow;
        t.verticalOverflow   = VerticalWrapMode.Overflow;
        Stretch(t.rectTransform);
        return t;
    }

    // ── Bouton cliquable ───────────────────────────────────────────────────────
    public static Button Button(Transform parent, string label, Vector2 size,
                                Color bg, Color fg, UnityAction onClick)
    {
        var go  = new GameObject("Btn_" + label);
        go.transform.SetParent(parent, false);
        var img = go.AddComponent<Image>();
        img.color = bg;

        var btn = go.AddComponent<Button>();
        btn.targetGraphic = img;
        var cb = btn.colors;
        cb.normalColor      = bg;
        cb.highlightedColor = Shade(bg, 0.18f);
        cb.pressedColor     = Shade(bg, -0.18f);
        cb.selectedColor    = Shade(bg, 0.10f);
        cb.fadeDuration     = 0.08f;
        btn.colors = cb;
        btn.onClick.AddListener(onClick);

        go.GetComponent<RectTransform>().sizeDelta = size;

        var t = Label(go.transform, "Text", label, 28, fg, TextAnchor.MiddleCenter, true);
        Stretch(t.rectTransform);
        return btn;
    }

    // ── Utils ──────────────────────────────────────────────────────────────────
    public static void Stretch(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
    }

    public static void Center(RectTransform rt, Vector2 size, Vector2 pos)
    {
        rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta        = size;
        rt.anchoredPosition = pos;
    }

    static Color Shade(Color c, float amount)
    {
        if (amount >= 0) return new Color(
            Mathf.Lerp(c.r, 1f, amount), Mathf.Lerp(c.g, 1f, amount),
            Mathf.Lerp(c.b, 1f, amount), c.a);
        return new Color(
            c.r * (1f + amount), c.g * (1f + amount), c.b * (1f + amount), c.a);
    }
}
