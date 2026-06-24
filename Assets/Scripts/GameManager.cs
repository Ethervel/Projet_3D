using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public int   ItemsDeposited { get; private set; }
    public float ElapsedTime    { get; private set; }
    public bool  GameComplete   { get; private set; }
    public const int Total = 3;

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
        if (!GameComplete) ElapsedTime += Time.deltaTime;
    }

    public void RegisterDeposit()
    {
        if (GameComplete) return;
        ItemsDeposited++;
        GameUI.Instance?.UpdateCounter(ItemsDeposited);
        if (ItemsDeposited >= Total)
        {
            GameComplete = true;
            GameUI.Instance?.ShowVictory(ElapsedTime);
        }
    }
}
