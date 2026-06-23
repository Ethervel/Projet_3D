using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerPickup : MonoBehaviour
{
    [Header("Parametres ramassage")]
    public float   pickupRadius    = 5f;
    public KeyCode pickupKey       = KeyCode.E;
    [Tooltip("Taille monde de l objet tenu (en metres)")]
    public float   objectWorldSize = 0.25f;
    [Tooltip("Secondes avant que la main touche l objet")]
    public float   attachDelay     = 1f;

    [Header("Poubelle")]
    public string binName         = "trashcan";
    public float  depositRadius   = 3f;

    [Header("Debug")]
    public string nearbyInfo    = "";
    public string rightHandInfo = "";

    // ── Refs ──────────────────────────────────────────────────────────────────
    private Animator             _animator;
    private Transform            _rightHand;
    private Rigidbody            _rb;
    private MonoBehaviour        _inputComp;
    private RigidbodyConstraints _originalConstraints;

    // ── Etat ──────────────────────────────────────────────────────────────────
    private bool         _isPickingUp = false;
    private GameObject   _heldObject  = null;   // null = rien en main
    private Collider[]   _heldColliders;
    private GameObject   _binObject;

    // ── Cache scene ───────────────────────────────────────────────────────────
    private List<GameObject> _anomalyObjects = new List<GameObject>();
    private readonly string[] _anomalyNames  = { "ArbreMalade", "Shiba", "garbage" };

    static readonly int PickupHash = Animator.StringToHash("Pickup");

    // ─────────────────────────────────────────────────────────────────────────
    void Start()
    {
        _animator = GetComponent<Animator>();
        _rb       = GetComponent<Rigidbody>();
        if (_rb != null) _originalConstraints = _rb.constraints;
        _inputComp = GetComponent("vThirdPersonInput") as MonoBehaviour;

        // Main droite
        if (_animator != null)
            _rightHand = _animator.GetBoneTransform(HumanBodyBones.RightHand);
        if (_rightHand == null)
        {
            string[] cands = { "mixamorig:RightHand", "RightHand", "Hand_R", "Bip001 R Hand" };
            foreach (var t in GetComponentsInChildren<Transform>(true))
                foreach (var n in cands)
                    if (t.name == n) { _rightHand = t; break; }
        }
        rightHandInfo = _rightHand != null ? $"OK: {_rightHand.name}" : "INTROUVABLE";
        Debug.Log($"[PlayerPickup] {rightHandInfo}");

        // Anomalies (actives ET inactives)
        foreach (var name in _anomalyNames)
        {
            var go = GameObject.Find(name);
            if (go == null)
                foreach (var c in Resources.FindObjectsOfTypeAll<GameObject>())
                    if (c.name == name && c.scene.IsValid()) { go = c; break; }
            if (go != null) _anomalyObjects.Add(go);
            else Debug.LogWarning($"[PlayerPickup] '{name}' introuvable.");
        }

        // Poubelle — chercher par le nom configure, puis par noms alternatifs connus
        string[] binCandidats = { binName, "trashcan", "TrashCan", "Trashcan", "Poubelle",
                                  "Trash", "Bin", "trash_can", "trash" };
        foreach (var n in binCandidats)
        {
            _binObject = GameObject.Find(n);
            if (_binObject != null) { binName = n; break; }
        }
        if (_binObject == null)
            Debug.LogWarning($"[PlayerPickup] Poubelle introuvable. Noms testes: {string.Join(", ", binCandidats)}");
        else
        {
            Debug.Log($"[PlayerPickup] Poubelle: '{_binObject.name}'");
            EnsureCollider(_binObject);
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    void Update()
    {
        // ── En train de ramasser : attendre fin d animation
        if (_isPickingUp)
        {
            if (_animator != null)
            {
                var info = _animator.GetCurrentAnimatorStateInfo(0);
                if (info.IsName("Lifting") && info.normalizedTime >= 0.95f)
                    FinishPickup();
            }
            return;
        }

        // ── Transport : objet en main, chercher la poubelle
        if (_heldObject != null)
        {
            bool nearBin = false;
            if (_binObject != null)
            {
                var binRend = _binObject.GetComponentInChildren<Renderer>();
                float d = binRend != null
                    ? Vector3.Distance(transform.position, binRend.bounds.ClosestPoint(transform.position))
                    : Vector3.Distance(transform.position, _binObject.transform.position);
                nearBin = d <= depositRadius;
            }

            GameUI.Instance?.SetPrompt(nearBin
                ? $"[E]  Deposer dans la poubelle"
                : $"  Transport : {_heldObject.name}");

            nearbyInfo = nearBin ? "Pres de la poubelle" : $"En transport : {_heldObject.name}";

            if (nearBin && Input.GetKeyDown(pickupKey))
                Deposit();
            return;
        }

        // ── Idle : chercher l anomalie la plus proche
        GameObject nearest = null;
        float minDist = pickupRadius;

        foreach (var go in _anomalyObjects)
        {
            if (go == null || !go.activeSelf) continue;
            var rend = go.GetComponentInChildren<Renderer>();
            float d = rend != null
                ? Vector3.Distance(transform.position, rend.bounds.ClosestPoint(transform.position))
                : Vector3.Distance(transform.position, go.transform.position);
            if (d < minDist) { minDist = d; nearest = go; }
        }

        nearbyInfo = nearest != null ? $"{nearest.name} {minDist:F1}m" : "";

        if (nearest != null)
        {
            GameUI.Instance?.SetPrompt($"[E]  Ramasser : {nearest.name}");
            if (Input.GetKeyDown(pickupKey)) StartPickup(nearest);
        }
        else
        {
            GameUI.Instance?.SetPrompt("");
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    void StartPickup(GameObject obj)
    {
        _isPickingUp = true;
        _heldObject  = obj;
        GameUI.Instance?.SetPrompt("");

        FreezePlayer(true);
        if (_animator != null) _animator.SetTrigger(PickupHash);

        // Figer la physique de l objet
        var objRb = obj.GetComponent<Rigidbody>();
        if (objRb != null) { objRb.linearVelocity = objRb.angularVelocity = Vector3.zero; objRb.isKinematic = true; }

        // Geler son animation propre + LOD
        foreach (var a in obj.GetComponentsInChildren<Animator>()) a.enabled = false;
        var lod = obj.GetComponentInChildren<LODGroup>();
        if (lod != null) lod.enabled = false;
        foreach (var r in obj.GetComponentsInChildren<Renderer>()) r.enabled = true;

        _heldColliders = obj.GetComponentsInChildren<Collider>(true);
        foreach (var c in _heldColliders) c.enabled = false;

        StartCoroutine(AttachAfterDelay(obj, attachDelay));
    }

    IEnumerator AttachAfterDelay(GameObject obj, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (obj == null) yield break;

        if (_rightHand != null)
        {
            foreach (var r in obj.GetComponentsInChildren<Renderer>(true)) r.enabled = true;

            // Mesurer les bounds monde AVANT reparentage pour obtenir la vraie taille visuelle
            // (les FBX exportes en cm ont des vertices a ~0.01 unites, scale scene compense — il faut en tenir compte)
            Bounds preB = new Bounds(); bool preFirst = true;
            foreach (var r in obj.GetComponentsInChildren<Renderer>(true))
            { if (preFirst) { preB = r.bounds; preFirst = false; } else preB.Encapsulate(r.bounds); }

            float boneScale = Mathf.Max(_rightHand.lossyScale.x, 0.0001f);
            float ls;
            if (!preFirst)
            {
                float currentWorldSize = Mathf.Max(preB.size.x, preB.size.y, preB.size.z, 0.0001f);
                float currentObjScale  = Mathf.Max(obj.transform.lossyScale.x, 0.0001f);
                ls = currentObjScale * (objectWorldSize / currentWorldSize) / boneScale;
            }
            else
            {
                ls = objectWorldSize / boneScale;
            }

            // 1. Parenter avec root a l origine de la main
            obj.transform.SetParent(_rightHand, worldPositionStays: false);
            obj.transform.localPosition = Vector3.zero;
            obj.transform.localRotation = Quaternion.identity;
            obj.transform.localScale    = Vector3.one * ls;

            // 2. Recentrer les bounds sur la main (corrige le decalage pivot)
            Bounds b = new Bounds(); bool first = true;
            foreach (var r in obj.GetComponentsInChildren<Renderer>(true))
            { if (first) { b = r.bounds; first = false; } else b.Encapsulate(r.bounds); }
            if (!first) obj.transform.position += _rightHand.position - b.center;

            Debug.Log($"[PlayerPickup] '{obj.name}' attache (ls={ls:F2})");
        }
        else
        {
            obj.transform.SetParent(transform, false);
            obj.transform.localPosition = new Vector3(0.3f, 1.3f, 0.5f);
            obj.transform.localScale    = Vector3.one * objectWorldSize;
        }
    }

    // Fin d animation : joueur peut bouger, objet reste en main
    void FinishPickup()
    {
        _isPickingUp = false;
        FreezePlayer(false);
        // _heldObject reste non-null : joueur transporte l objet
    }

    // Depot dans la poubelle
    void Deposit()
    {
        if (_heldObject == null) return;
        _heldObject.SetActive(false);
        _heldObject    = null;
        _heldColliders = null;
        GameManager.Instance?.RegisterDeposit();
        GameUI.Instance?.SetPrompt("");
        Debug.Log("[PlayerPickup] Objet depose dans la poubelle.");
    }

    // ─────────────────────────────────────────────────────────────────────────
    void FreezePlayer(bool freeze)
    {
        if (_inputComp != null) _inputComp.enabled = !freeze;
        if (_rb == null) return;
        if (freeze)
        {
            _rb.linearVelocity  = Vector3.zero;
            _rb.angularVelocity = Vector3.zero;
            _rb.constraints     = RigidbodyConstraints.FreezeAll;
        }
        else
        {
            _rb.constraints = _originalConstraints;
        }
    }

    // Ajoute un BoxCollider calibre sur le mesh si l objet n en a pas
    void EnsureCollider(GameObject go)
    {
        if (go.GetComponentInChildren<Collider>(true) != null) return;

        var rend = go.GetComponentInChildren<Renderer>();
        var col  = go.AddComponent<BoxCollider>();

        if (rend != null)
        {
            // Convertir les bounds monde en espace local du GO
            Vector3 wCenter = rend.bounds.center;
            Vector3 wSize   = rend.bounds.size;
            Vector3 ls      = go.transform.lossyScale;

            col.center = go.transform.InverseTransformPoint(wCenter);
            col.size   = new Vector3(
                wSize.x / Mathf.Max(Mathf.Abs(ls.x), 0.0001f),
                wSize.y / Mathf.Max(Mathf.Abs(ls.y), 0.0001f),
                wSize.z / Mathf.Max(Mathf.Abs(ls.z), 0.0001f));
        }

        Debug.Log($"[PlayerPickup] BoxCollider ajoute sur '{go.name}'.");
    }

    void OnDrawGizmos()
    {
        Gizmos.color = new Color(0f, 1f, 0f, 0.2f);
        Gizmos.DrawWireSphere(transform.position, pickupRadius);
        if (_binObject != null)
        {
            Gizmos.color = new Color(1f, 0.5f, 0f, 0.35f);
            Gizmos.DrawWireSphere(_binObject.transform.position, depositRadius);
        }
    }
}
