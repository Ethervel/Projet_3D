using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Animator))]
public class GardeForestierPatrol : MonoBehaviour
{
    [Header("Anomalies")]
    public string[] anomalyNames = { "ArbreMalade", "Shiba", "garbage" };

    [Header("Parametres")]
    public float patrolSpeed      = 1.5f;
    public float waveStopDistance = 2.5f;

    private NavMeshAgent _agent;
    private Animator     _animator;
    private Transform    _player;
    private Transform[]  _anomalies;
    private int          _currentIndex = 0;
    private bool         _isWaving     = false;

    static readonly int IsWavingHash = Animator.StringToHash("isWaving");
    static readonly int SpeedHash    = Animator.StringToHash("Speed");

    void Start()
    {
        _agent   = GetComponent<NavMeshAgent>();
        _animator = GetComponent<Animator>();
        _agent.speed            = patrolSpeed;
        _agent.stoppingDistance = waveStopDistance;

        // Chercher le joueur par tag
        var playerGO = GameObject.FindWithTag("Player");
        if (playerGO != null) _player = playerGO.transform;
        else Debug.LogWarning("GardeForestierPatrol: aucun objet tague 'Player' trouve.");

        // Trouver les anomalies par nom
        var list = new List<Transform>();
        foreach (var n in anomalyNames)
        {
            var go = GameObject.Find(n);
            if (go != null) list.Add(go.transform);
            else Debug.LogWarning($"GardeForestierPatrol: '{n}' introuvable.");
        }
        _anomalies = list.ToArray();

        if (_anomalies.Length > 0) GoTo(0);
    }

    void Update()
    {
        if (_anomalies == null || _anomalies.Length == 0) return;

        _animator.SetFloat(SpeedHash, _agent.velocity.magnitude / Mathf.Max(_agent.speed, 0.01f));

        if (_isWaving)
        {
            // Arreter completement le deplacement
            _agent.isStopped = true;
            _agent.velocity  = Vector3.zero;

            // Tourner vers le JOUEUR
            if (_player != null)
            {
                Vector3 dir = _player.position - transform.position;
                dir.y = 0;
                if (dir.sqrMagnitude > 0.01f)
                    transform.rotation = Quaternion.Slerp(transform.rotation,
                        Quaternion.LookRotation(dir), Time.deltaTime * 5f);
            }

            // Passer a la suivante seulement si l objet a ete ramasse (desactive)
            if (!_anomalies[_currentIndex].gameObject.activeSelf)
                StopWave();
            return;
        }

        if (!_agent.pathPending && _agent.remainingDistance <= _agent.stoppingDistance)
            StartWave();
    }

    void GoTo(int index)
    {
        _currentIndex    = index;
        _agent.isStopped = false;
        _agent.SetDestination(_anomalies[index].position);
    }

    void StartWave()
    {
        _isWaving        = true;
        _agent.isStopped = true;
        _animator.SetBool(IsWavingHash, true);
    }

    void StopWave()
    {
        _isWaving = false;
        _animator.SetBool(IsWavingHash, false);
        GoTo((_currentIndex + 1) % _anomalies.Length);
    }

    void OnDrawGizmos()
    {
        if (_anomalies == null) return;
        Gizmos.color = Color.yellow;
        foreach (var t in _anomalies)
            if (t != null) Gizmos.DrawWireSphere(t.position, waveStopDistance);
    }
}
