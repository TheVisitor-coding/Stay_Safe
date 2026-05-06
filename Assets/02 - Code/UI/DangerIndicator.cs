using UnityEngine;
using UnityEngine.UI;

public class DangerIndicator : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private Image leftVignette;
    [SerializeField] private Image rightVignette;
    [SerializeField] private float maxAlpha = 0.35f;
    [SerializeField] private float deadZoneAngle = 20f;
    [SerializeField] private float smoothSpeed = 4f;
    [SerializeField] private float appearDelay = 1.5f;

    private BarricadePoint _currentTarget;
    private float _attackStartTime;

    void Awake()
    {
        KidnapperAI.OnAttackStarted += OnAttackStarted;
        KidnapperAI.OnAccessBreached += ClearTarget;
        BarricadePoint.OnBarricaded += OnBarricaded;
        GameManager.OnGameStateChanged += OnGameStateChanged;
    }

    void OnDestroy()
    {
        KidnapperAI.OnAttackStarted -= OnAttackStarted;
        KidnapperAI.OnAccessBreached -= ClearTarget;
        BarricadePoint.OnBarricaded -= OnBarricaded;
        GameManager.OnGameStateChanged -= OnGameStateChanged;
    }

    void Update()
    {
        float targetLeft = 0f;
        float targetRight = 0f;

        bool active = _currentTarget != null
            && GameManager.Instance.GetGameState() == GameManager.GameState.Playing
            && Time.time - _attackStartTime >= appearDelay;

        if (active)
        {
            Vector3 toTarget = _currentTarget.transform.position - player.position;
            toTarget.y = 0f;
            float angle = Vector3.SignedAngle(player.forward, toTarget, Vector3.up);

            if (Mathf.Abs(angle) > deadZoneAngle)
            {
                float intensity = Mathf.Abs(angle) / 180f * maxAlpha;
                if (angle > 0) targetLeft = intensity;
                else targetRight = intensity;
            }
        }

        float t = Time.deltaTime * smoothSpeed;
        SetAlpha(leftVignette, Mathf.Lerp(leftVignette.color.a, targetLeft, t));
        SetAlpha(rightVignette, Mathf.Lerp(rightVignette.color.a, targetRight, t));
    }

    private void OnAttackStarted(BarricadePoint target)
    {
        _currentTarget = target;
        _attackStartTime = Time.time;
    }

    private void OnBarricaded(BarricadePoint point)
    {
        if (point == _currentTarget) ClearTarget();
    }

    private void OnGameStateChanged(GameManager.GameState state)
    {
        if (state != GameManager.GameState.Playing) ClearTarget();
    }

    private void ClearTarget() => _currentTarget = null;

    private void SetAlpha(Image image, float alpha)
    {
        Color color = image.color;
        color.a = alpha;
        image.color = color;
    }
}
