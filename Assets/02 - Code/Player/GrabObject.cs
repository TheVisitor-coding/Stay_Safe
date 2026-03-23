using UnityEngine;

public class GrabObject : MonoBehaviour
{
    [SerializeField] private Camera playerCamera;
    [SerializeField] private float grabDistance = 5f;
    [SerializeField] private Transform holdPosition;

    private GameObject _grabbedObject;
    private BarricadePoint _currentBarricadePoint;

    void Update()
    {
        Vector3 rayOrigin = playerCamera.transform.position;
        Vector3 rayDirection = playerCamera.transform.forward;

        Debug.DrawRay(rayOrigin, rayDirection * grabDistance, Color.green);

        if (Input.GetKeyDown(KeyCode.E))
        {
            if (_grabbedObject == null)
                PickUpObject(rayOrigin, rayDirection);
            else
                DropObject();
        }
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
    }

    private void DropObject()
    {
        if (_currentBarricadePoint != null)
        {
            _currentBarricadePoint.Barricade(_grabbedObject);
        }
        else
        {
            Rigidbody rb = _grabbedObject.GetComponent<Rigidbody>();
            rb.isKinematic = false;
            _grabbedObject.transform.SetParent(null);
            _grabbedObject.GetComponent<Collider>().enabled = true;
        }

        _grabbedObject = null;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("BarricadePoint"))
            _currentBarricadePoint = other.GetComponent<BarricadePoint>();
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("BarricadePoint") && other.GetComponent<BarricadePoint>() == _currentBarricadePoint)
            _currentBarricadePoint = null;
    }
}
