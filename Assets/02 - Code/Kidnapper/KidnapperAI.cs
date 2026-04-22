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
    [SerializeField] private PoliceTimer timerGlitch;

    [Header("Déplacement & Audio")]
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private AudioSource footstepsSource;
    [SerializeField] private AudioSource forcingSource;

    public static event Action OnAccessBreached;
    public static event Action<BarricadePoint> OnAttackStarted;
    public static event Action OnTutorialAttackStarted;
    public static event Action OnTutorialCompleted;

    private BarricadePoint[] _accessPoints;
    [SerializeField] private BarricadePoint firstAccessPoint;
    private Coroutine _attackCoroutine;

    private NavMeshAgent agent;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.speed = moveSpeed;
    }

    public void Initialize(BarricadePoint[] accessPoints)
    {
        _accessPoints = accessPoints;
    }

    public void StartTutorialAttack()
    {
        if (firstAccessPoint == null)
        {
            Debug.LogError("[KidnapperAI] Aucun point d'accès assigné pour le tutoriel.");
            return;
        }

        if (_attackCoroutine != null) return;

        _attackCoroutine = StartCoroutine(TutorialApproachCoroutine());
    }

    public void StartTutorialForcing()
    {
        _attackCoroutine = StartCoroutine(TutorialForcingCoroutine());
    }

    public void StartAttack()
    {
        if (_accessPoints == null || _accessPoints.Length == 0)
        {
            Debug.LogError("[KidnapperAI] Aucun point d'accès assigné. Appelle Initialize() d'abord.");
            return;
        }

        if (_attackCoroutine != null) return;

        _attackCoroutine = StartCoroutine(AttackCoroutine());
    }

    public void StopAttack()
    {
        StopAudio();

        if (_attackCoroutine != null)
        {
            StopCoroutine(_attackCoroutine);
            _attackCoroutine = null;
        }
    }

    private IEnumerator AttackCoroutine()
    {
        yield return new WaitForSeconds(delayBetweenRounds);

        BarricadePoint target = GetRandomOpenAccessPoint();

        if (target == null)
        {
            _attackCoroutine = null;
            yield break;
        }

        OnAttackStarted?.Invoke(target);
        Debug.Log($"[Kidnapper] Cible : {target.name}");

        yield return MoveToTarget(target.transform.position);

        forcingSource.Play();
        timerGlitch.TriggerGlitchEffect(forcingDuration);

        yield return new WaitForSeconds(forcingDuration);
        forcingSource.Stop();

        if (target.GetBarricadeState() == BarricadePoint.BarricadeState.Open)
        {
            _attackCoroutine = null;
            agent.isStopped = true;
            Debug.Log("[Kidnapper] Accès forcé — défaite");
            OnAccessBreached?.Invoke();
            yield break;
        }

        forcingDuration = Mathf.Max(minimumForcingDuration, forcingDuration - difficultyReductionPerRound);
        Debug.Log($"[Kidnapper] Repoussé — prochain forçage en {forcingDuration}s");

        _attackCoroutine = StartCoroutine(AttackCoroutine());
    }

    private IEnumerator TutorialApproachCoroutine()
    {
        OnAttackStarted?.Invoke(firstAccessPoint);
        Debug.Log($"[Kidnapper] Cible tutoriel : {firstAccessPoint.name}");

        yield return MoveToTarget(firstAccessPoint.transform.position);

        _attackCoroutine = null;
        forcingSource.Play();
        OnTutorialAttackStarted?.Invoke();
        DialogueManager.Instance.Enqueue(
            DialogueManager.Instance.Database.onFirstAttack
        );
    }

    private IEnumerator TutorialForcingCoroutine()
    {
        while (firstAccessPoint.GetBarricadeState() == BarricadePoint.BarricadeState.Open)
        {
            yield return null;
        }

        forcingSource.Stop();
        _attackCoroutine = null;
        Debug.Log("[Kidnapper] Repoussé au tutoriel");
        OnTutorialCompleted?.Invoke();
    }

    private IEnumerator MoveToTarget(Vector3 destination)
    {
        footstepsSource.Play();

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
        if (footstepsSource != null && footstepsSource.isPlaying) footstepsSource.Stop();
        if (forcingSource != null && forcingSource.isPlaying) forcingSource.Stop();
    }
}
