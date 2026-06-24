using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

// Actions globales partagees par le menu debut, le menu pause et l ecran de fin.
public static class GameActions
{
    // Relance la scene courante depuis zero.
    public static void Restart()
    {
        Time.timeScale = 1f;
        ShowCursor(false);
        var scene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(scene.name);
    }

    // Quitte le jeu (ou sort du Play Mode dans l editeur).
    public static void Quit()
    {
        Time.timeScale = 1f;
        Debug.Log("[GameActions] Quitter.");
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    // Affiche / cache le curseur (libre pour les menus, verrouille en jeu).
    public static void ShowCursor(bool visible)
    {
        Cursor.visible   = visible;
        Cursor.lockState = visible ? CursorLockMode.None : CursorLockMode.Locked;
    }

    // ── Gel du joueur (mouvement + camera Invector) pour les menus ─────────────
    static readonly List<Behaviour> _frozen = new List<Behaviour>();

    public static void FreezePlayer(bool freeze)
    {
        if (freeze)
        {
            _frozen.Clear();
            foreach (var mb in Object.FindObjectsOfType<MonoBehaviour>())
            {
                string n = mb.GetType().Name;
                if (n == "vThirdPersonInput" || n == "vThirdPersonCamera")
                    if (mb.enabled) { mb.enabled = false; _frozen.Add(mb); }
            }
        }
        else
        {
            foreach (var mb in _frozen)
                if (mb != null) mb.enabled = true;
            _frozen.Clear();
        }
    }
}
