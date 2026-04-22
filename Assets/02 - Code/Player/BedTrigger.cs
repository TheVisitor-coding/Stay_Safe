using System;
using UnityEngine;

public class BedTrigger : MonoBehaviour
{
    public static event Action OnPlayerWokeUp;

    [SerializeField] private GameObject keypressHint;
    [SerializeField] private Transform bedSleepPosition;
    [SerializeField] private Transform wakeUpPosition;
    [SerializeField] private float interactionDistance = 2f;

    [SerializeField] private Transform player;
    [SerializeField] private Transform playerCamera;
    [SerializeField] private FirstPersonController firstPersonController;

    private enum BedState { Idle, Sleeping, WaitingToWakeUp }
    private BedState _state = BedState.Idle;

    void OnEnable()
    {
        GameManager.OnWakeUpPhase += HandleWakeUpPhase;
    }

    void OnDisable()
    {
        GameManager.OnWakeUpPhase -= HandleWakeUpPhase;
    }

    void Update()
    {
        switch (_state)
        {
            case BedState.Idle:
                HandleIdle();
                break;
            case BedState.WaitingToWakeUp:
                HandleWaitingToWakeUp();
                break;
        }
    }

    private void HandleIdle()
    {
        Ray ray = new Ray(transform.position, transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, interactionDistance))
        {
            if (hit.collider.CompareTag("Bed"))
            {
                keypressHint.SetActive(true);
                if (Input.GetKeyDown(KeyCode.E))
                    GoToSleep();
                return;
            }
        }
        keypressHint.SetActive(false);
    }

    private void GoToSleep()
    {
        keypressHint.SetActive(false);
        _state = BedState.Sleeping;

        firstPersonController.playerCanMove = false;
        firstPersonController.cameraCanMove = false;
        player.GetComponent<Rigidbody>().isKinematic = true;
        player.position = new Vector3(bedSleepPosition.position.x, 0, bedSleepPosition.position.z);
        playerCamera.localRotation = Quaternion.Euler(-90f, 0f, 0f);

        GameManager.Instance.StartTutorial();
    }

    private void HandleWakeUpPhase()
    {
        _state = BedState.WaitingToWakeUp;
        keypressHint.SetActive(true);
    }

    private void HandleWaitingToWakeUp()
    {
        if (Input.GetKeyDown(KeyCode.E))
            WakeUp();
    }

    private void WakeUp()
    {
        keypressHint.SetActive(false);
        _state = BedState.Idle;

        player.position = wakeUpPosition.position;
        playerCamera.localRotation = Quaternion.identity;

        player.GetComponent<Rigidbody>().isKinematic = false;
        firstPersonController.playerCanMove = true;
        firstPersonController.cameraCanMove = true;

        OnPlayerWokeUp?.Invoke();
    }
}
