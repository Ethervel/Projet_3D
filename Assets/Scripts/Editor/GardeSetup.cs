using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;

public static class GardeSetup
{
    [MenuItem("Tools/Setup Garde Forestier")]
    public static void Run()
    {
        ConfigureWavingFBX();
        ConfigureLiftingFBX();
        ConfigureGardeController();
        ConfigurePlayerController();
        AddPickupToPlayer();
    }

    // ── Waving.fbx ──────────────────────────────────────────────────
    static void ConfigureWavingFBX()
    {
        string path = FindFBX("Waving.fbx");
        if (path == null) { Debug.LogWarning("[GardeSetup] Waving.fbx non trouve."); return; }
        var imp = AssetImporter.GetAtPath(path) as ModelImporter;
        if (imp == null) return;
        imp.animationType = ModelImporterAnimationType.Generic;
        imp.avatarSetup   = ModelImporterAvatarSetup.CreateFromThisModel;
        var clips = imp.defaultClipAnimations;
        for (int i = 0; i < clips.Length; i++) { clips[i].loopTime = true; clips[i].loopPose = true; }
        imp.clipAnimations = clips;
        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
        Debug.Log("[GardeSetup] Waving.fbx: OK.");
    }

    // ── Lifting.fbx ──────────────────────────────────────────────────
    static void ConfigureLiftingFBX()
    {
        string path = FindFBX("Lifting.fbx");
        if (path == null) { Debug.LogWarning("[GardeSetup] Lifting.fbx non trouve."); return; }
        var imp = AssetImporter.GetAtPath(path) as ModelImporter;
        if (imp == null) return;
        imp.animationType = ModelImporterAnimationType.Human;
        imp.avatarSetup   = ModelImporterAvatarSetup.CreateFromThisModel;
        var clips = imp.defaultClipAnimations;
        for (int i = 0; i < clips.Length; i++) { clips[i].loopTime = false; }
        imp.clipAnimations = clips;
        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
        Debug.Log("[GardeSetup] Lifting.fbx: Humanoid, no loop OK.");
    }

    // ── GardeForestier_Controller : Wave ─────────────────────────────
    static void ConfigureGardeController()
    {
        var ctrl = AssetDatabase.LoadAssetAtPath<AnimatorController>("Assets/GardeForestier_Controller.controller");
        if (ctrl == null) { Debug.LogError("[GardeSetup] GardeForestier_Controller introuvable!"); return; }

        bool hasIsWaving = false, hasSpeed = false;
        foreach (var p in ctrl.parameters)
        { if (p.name == "isWaving") hasIsWaving = true; if (p.name == "Speed") hasSpeed = true; }
        if (!hasIsWaving) ctrl.AddParameter("isWaving", AnimatorControllerParameterType.Bool);
        if (!hasSpeed)    ctrl.AddParameter("Speed",    AnimatorControllerParameterType.Float);

        var sm = ctrl.layers[0].stateMachine;
        AnimatorState walkState = null, waveState = null;
        foreach (var s in sm.states)
        { if (s.state.name == "Walking") walkState = s.state; if (s.state.name == "Wave") waveState = s.state; }

        AnimationClip waveClip = FindClipInFBX("Waving.fbx");
        if (waveClip == null) { Debug.LogWarning("[GardeSetup] Clip Waving introuvable."); return; }

        if (waveState == null) waveState = sm.AddState("Wave");
        waveState.motion = waveClip;

        if (walkState != null)
        {
            bool hasToWave = false, hasToWalk = false;
            foreach (var t in walkState.transitions) if (t.destinationState == waveState) hasToWave = true;
            foreach (var t in waveState.transitions) if (t.destinationState == walkState) hasToWalk = true;
            if (!hasToWave) { var t = walkState.AddTransition(waveState); t.hasExitTime = false; t.duration = 0.15f; t.AddCondition(AnimatorConditionMode.If, 0, "isWaving"); }
            if (!hasToWalk) { var t = waveState.AddTransition(walkState); t.hasExitTime = false; t.duration = 0.15f; t.AddCondition(AnimatorConditionMode.IfNot, 0, "isWaving"); }
        }
        AssetDatabase.SaveAssets();
        Debug.Log("[GardeSetup] GardeForestier_Controller: Wave OK.");
    }

    // ── Invector@BasicLocomotion : Pickup + Lifting ───────────────────
    static void ConfigurePlayerController()
    {
        string ctrlPath = "Assets/Invector-3rdPersonController_LITE/Animator/Invector@BasicLocomotion.controller";
        var ctrl = AssetDatabase.LoadAssetAtPath<AnimatorController>(ctrlPath);
        if (ctrl == null) { Debug.LogError("[GardeSetup] Invector@BasicLocomotion introuvable!"); return; }

        // Parametre Pickup (trigger)
        bool hasPickup = false;
        foreach (var p in ctrl.parameters) if (p.name == "Pickup") hasPickup = true;
        if (!hasPickup) ctrl.AddParameter("Pickup", AnimatorControllerParameterType.Trigger);

        var sm = ctrl.layers[0].stateMachine;
        AnimatorState liftState = null;
        foreach (var s in sm.states) if (s.state.name == "Lifting") liftState = s.state;

        AnimationClip liftClip = FindClipInFBX("Lifting.fbx");
        if (liftClip == null) { Debug.LogWarning("[GardeSetup] Clip Lifting introuvable."); return; }

        if (liftState == null) liftState = sm.AddState("Lifting");
        liftState.motion = liftClip;

        // AnyState -> Lifting quand Pickup trigger
        bool hasAnyToLift = false;
        foreach (var t in sm.anyStateTransitions) if (t.destinationState == liftState) hasAnyToLift = true;
        if (!hasAnyToLift)
        {
            var t = sm.AddAnyStateTransition(liftState);
            t.hasExitTime = false;
            t.duration    = 0.1f;
            t.AddCondition(AnimatorConditionMode.If, 0, "Pickup");
        }

        // Lifting -> default state apres fin animation
        bool hasLiftExit = false;
        foreach (var t in liftState.transitions) hasLiftExit = true;
        if (!hasLiftExit)
        {
            var t = liftState.AddExitTransition();
            t.hasExitTime = true;
            t.exitTime    = 1f;
            t.duration    = 0.2f;
        }

        AssetDatabase.SaveAssets();
        Debug.Log("[GardeSetup] Invector controller: Pickup trigger + Lifting OK.");
    }

    // ── Ajouter PlayerPickup sur le joueur ───────────────────────────
    static void AddPickupToPlayer()
    {
        var playerGO = GameObject.FindWithTag("Player");
        if (playerGO == null) { Debug.LogWarning("[GardeSetup] Pas de GameObject tague 'Player'."); return; }
        if (playerGO.GetComponent<PlayerPickup>() == null)
        {
            playerGO.AddComponent<PlayerPickup>();
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
                UnityEngine.SceneManagement.SceneManager.GetActiveScene());
            Debug.Log("[GardeSetup] PlayerPickup ajoute sur le joueur.");
        }
        else Debug.Log("[GardeSetup] PlayerPickup deja present.");
    }

    // ── Utilitaires ──────────────────────────────────────────────────
    static AnimationClip FindClipInFBX(string fileName)
    {
        string path = FindFBX(fileName);
        if (path == null) return null;
        foreach (var a in AssetDatabase.LoadAllAssetsAtPath(path))
            if (a is AnimationClip c && !c.name.StartsWith("__")) return c;
        return null;
    }

    static string FindFBX(string fileName)
    {
        foreach (var guid in AssetDatabase.FindAssets(System.IO.Path.GetFileNameWithoutExtension(fileName)))
        {
            var p = AssetDatabase.GUIDToAssetPath(guid);
            if (p.EndsWith(fileName)) return p;
        }
        return null;
    }
}
