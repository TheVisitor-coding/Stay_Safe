using UnityEngine;
using UnityEngine.UI;

public class GrabObject : MonoBehaviour
{
    [SerializeField] private Camera playerCamera;
    [SerializeField] private float grabDistance = 5f;
    [SerializeField] private Transform holdPosition;
    [SerializeField] private Slider barricadeProgressBar;
    [SerializeField] private GameObject keypressHint;
    [SerializeField] private float barricadeTime = 2f;

    [Header("Audio")]
    [SerializeField] private AudioSource barricadeAudioSource;
    [SerializeField] private AudioSource pickupAudioSource;

    private GameObject _grabbedObject;
    private BarricadePoint _currentBarricadePoint;
    private float _barricadeProgress = 0f;

    void Update()
    {
        Vector3 rayOrigin = playerCamera.transform.position;
        Vector3 rayDirection = playerCamera.transform.forward;

        bool canBarricade = _grabbedObject != null && _currentBarricadePoint != null && GameManager.Instance.GetGameState() == GameManager.GameState.Playing;

        // Cas 1 : hold E pour barricader
        if (canBarricade && Input.GetKey(KeyCode.E))
        {
            keypressHint.SetActive(false);
            _barricadeProgress += Time.deltaTime;
            barricadeProgressBar.gameObject.SetActive(true);
            barricadeProgressBar.value = _barricadeProgress / barricadeTime;

            if (_barricadeProgress >= barricadeTime)
            {
                barricadeAudioSource.Play();
                CompleteBarricade();
            }

            return;
        }

        // Cas 2 : reset de la progression (relâché ou sorti de la zone)
        if (_barricadeProgress > 0f)
        {
            _barricadeProgress = 0f;
            barricadeProgressBar.value = 0f;
            barricadeProgressBar.gameObject.SetActive(false);
        }

        // Cas 3 : appui simple E — ramasser ou poser librement
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (_grabbedObject == null)
                PickUpObject(rayOrigin, rayDirection);
            else
                DropObject();
        }
    }

    private void CompleteBarricade()
    {
        _currentBarricadePoint.Barricade(_grabbedObject);
        _grabbedObject = null;
        _barricadeProgress = 0f;
        barricadeProgressBar.value = 0f;
        barricadeProgressBar.gameObject.SetActive(false);
    }

    private void PickUpObject(Vector3 rayOrigin, Vector3 rayDirection)
    {
        RaycastHit hit;
        bool hasHit = Physics.Raycast(rayOrigin, rayDirection, out hit, grabDistance, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore);

        if (!hasHit || !hit.transform.CompareTag("canPickUp")) return;

        Rigidbody rb = hit.transform.GetComponent<Rigidbody>();
        if (rb == null) return;

        _grabbedObject = hit.transform.gameObject;
        rb.isKinematic = true;
        _grabbedObject.transform.SetParent(holdPosition);
        _grabbedObject.transform.localPosition = Vector3.zero;
        _grabbedObject.transform.localRotation = Quaternion.identity;
        _grabbedObject.GetComponent<Collider>().enabled = false;
        _grabbedObject.GetComponentInChildren<HintKey>()?.Hide();
        pickupAudioSource.Play();
    }

    private void DropObject()
    {
        Rigidbody rb = _grabbedObject.GetComponent<Rigidbody>();
        rb.isKinematic = false;
        _grabbedObject.transform.SetParent(null);
        _grabbedObject.GetComponent<Collider>().enabled = true;
        _grabbedObject = null;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("BarricadePoint"))
        {
            _currentBarricadePoint = other.GetComponent<BarricadePoint>();
            
            if (_grabbedObject != null)
            {
                keypressHint.SetActive(true);
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("BarricadePoint") && other.GetComponent<BarricadePoint>() == _currentBarricadePoint)
        {
            _currentBarricadePoint = null;
            keypressHint.SetActive(false);
        }
    }
}
