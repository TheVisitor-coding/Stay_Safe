using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class KidnapperAI : MonoBehaviour
{
    [Header("Timing")]
    [SerializeField] private float delayBetweenRounds = 5f;
    [SerializeField] private float forcingDuration = 8f;
    [SerializeField] private float minimumForcingDuration = 2f;
    [SerializeField] private float difficultyReductionPerRound = 0.5f;

    [Header("Déplacement & Audio")]
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float minimumTravelDuration = 2f;
    [SerializeField] private AudioSource footstepsSource;
    [SerializeField] private AudioSource forcingSource;

    public static event Action OnAccessBreached;
    public static event Action<BarricadePoint> OnAttackStarted;

    private BarricadePoint[] _accessPoints;
    private bool _isActive = false;
    private Coroutine _attackCoroutine;

    private NavMeshAgent agent;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.speed = moveSpeed;
    }

    void Update()
    {
        
    }

    public void Initialize(BarricadePoint[] accessPoints)
    {
        _accessPoints = accessPoints;
    }

    public void StartAttack()
    {
        if (_accessPoints == null || _accessPoints.Length == 0)
        {
            Debug.LogError("[KidnapperAI] Aucun point d'accès assigné. Appelle Initialize() d'abord.");
            return;
        }

        if (_isActive) return;

        _isActive = true;
        _attackCoroutine = StartCoroutine(AttackCoroutine());
    }

    public void StopAttack()
    {
        _isActive = false;
        StopAudio();

        if (_attackCoroutine != null)
        {
            StopCoroutine(_attackCoroutine);
            _attackCoroutine = null;
        }
    }

    private IEnumerator AttackCoroutine()
    {
        while (_isActive)
        {
            yield return new WaitForSeconds(delayBetweenRounds);

            BarricadePoint target = GetRandomOpenAccessPoint();

            OnAttackStarted?.Invoke(target);
            Debug.Log($"[Kidnapper] Cible : {target.name}");

            // Phase 1 : déplacement vers le point d'accès
            yield return MoveToTarget(target.transform.position);

            // Phase 2 : forçage de l'accès — la source audio est positionnée sur le point attaqué
            forcingSource.Play();

            yield return new WaitForSeconds(forcingDuration);
            forcingSource.Stop();

            // Phase 3 : résultat
            if (target.GetBarricadeState() == BarricadePoint.BarricadeState.Open)
            {
                _isActive = false;
                Debug.Log("[Kidnapper] Accès forcé — défaite");
                agent.isStopped = true;
                OnAccessBreached?.Invoke();
                yield break;
            }

            forcingDuration = Mathf.Max(minimumForcingDuration, forcingDuration - difficultyReductionPerRound);
            Debug.Log($"[Kidnapper] Repoussé — prochain forçage en {forcingDuration}s");
        }
    }

    private IEnumerator MoveToTarget(Vector3 destination)
    {
        footstepsSource.Play();
        Debug.Log($"[Kidnapper] Audio de déplacement activé", footstepsSource);

        agent.SetDestination(destination);
        while (agent.pathPending || agent.remainingDistance > agent.stoppingDistance)
        {
            yield return null;
        }
        footstepsSource.Stop();
    }

    private BarricadePoint GetRandomOpenAccessPoint()
    {
        List<BarricadePoint> openPoints = new List<BarricadePoint>();

        foreach (BarricadePoint point in _accessPoints)
        {
            if (point != null && point.GetBarricadeState() == BarricadePoint.BarricadeState.Open)
                openPoints.Add(point);
        }

        return openPoints.Count == 0 ? null : openPoints[UnityEngine.Random.Range(0, openPoints.Count)];
    }

    private void StopAudio()
    {
        Debug.Log("[Kidnapper] Arrêt des sons");
        if (footstepsSource != null && footstepsSource.isPlaying) footstepsSource.Stop();
        if (forcingSource != null && forcingSource.isPlaying) forcingSource.Stop();
    }
}
