using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public int   ItemsDeposited { get; private set; }
    public float TimeRemaining  { get; private set; }
    public bool  GameComplete   { get; private set; }
    public bool  Win            { get; private set; }
    public const int   Total     = 3;
    public const float TimeLimit = 240f;   // 4 minutes

    void Start()
    {
        TimeRemaining = TimeLimit;
    }

    // S execute une seule fois au lancement : cree le controller et s assure
    // qu il est recree a CHAQUE chargement de scene (ex : Recommencer).
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Bootstrap()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;
        EnsureController();
    }

    static void OnSceneLoaded(Scene scene, LoadSceneMode mode) => EnsureController();

    static void EnsureController()
    {
        if (Instance != null) return;
        if (FindObjectOfType<GameManager>() != null) return;
        var go = new GameObject("GameController");
        go.AddComponent<GameManager>();
        go.AddComponent<GameUI>();
        go.AddComponent<StartMenu>();
        go.AddComponent<PauseMenu>();
    }

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    void Update()
    {
        if (GameComplete) return;
        TimeRemaining -= Time.deltaTime;
        if (TimeRemaining <= 0f)
        {
            TimeRemaining = 0f;
            EndGame(false);   // temps ecoule
        }
    }

    public void RegisterDeposit()
    {
        if (GameComplete) return;
        ItemsDeposited++;
        GameUI.Instance?.UpdateCounter(ItemsDeposited);
        if (ItemsDeposited >= Total)
            EndGame(true);    // victoire
    }

    void EndGame(bool win)
    {
        if (GameComplete) return;
        GameComplete = true;
        Win = win;
        GameUI.Instance?.ShowEnd(win, ItemsDeposited, TimeLimit - TimeRemaining);
    }
}
