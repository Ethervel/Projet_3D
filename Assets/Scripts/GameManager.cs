using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public int   ItemsDeposited { get; private set; }
    public float ElapsedTime    { get; private set; }
    public bool  GameComplete   { get; private set; }
    public const int Total = 3;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void AutoCreate()
    {
        if (FindObjectOfType<GameManager>() != null) return;
        var go = new GameObject("GameController");
        go.AddComponent<GameManager>();
        go.AddComponent<GameUI>();
    }

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
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
