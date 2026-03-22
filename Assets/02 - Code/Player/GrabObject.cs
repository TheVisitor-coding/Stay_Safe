using UnityEngine;

public class GrabObject : MonoBehaviour
{

    public Camera playerCamera;
    public float grabDistance = 5f;
    private GameObject grabbedObject;
    public Transform holdPosition;
    private BarricadePoint currentBarricadePoint;

    void Update()
    {
        Vector3 rayOrigin = playerCamera.transform.position;
        Vector3 rayDirection = playerCamera.transform.forward;

        Debug.DrawRay(rayOrigin, rayDirection * grabDistance, Color.green);

        if (Input.GetKeyDown(KeyCode.E))
        {
            if (grabbedObject == null)
            {
                PickUpObject(rayOrigin, rayDirection);
            } else 
             {
                DropObject();
             }
        }
    }


    void PickUpObject(Vector3 rayOrigin, Vector3 rayDirection)
    {
        RaycastHit hit;
        bool hasHit = Physics.Raycast(rayOrigin, rayDirection, out hit, grabDistance, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore);

        if (hasHit)
        {
            if (hit.transform.CompareTag("canPickUp"))
            {
                grabbedObject = hit.transform.gameObject;
                Rigidbody rb = grabbedObject.GetComponent<Rigidbody>();

                if (rb == null)
                {
                    grabbedObject = null;
                    return;
                }

                rb.isKinematic = true;
                grabbedObject.transform.SetParent(holdPosition);
                grabbedObject.transform.localPosition = Vector3.zero;
                grabbedObject.transform.localRotation = Quaternion.identity;
                grabbedObject.GetComponent<Collider>().enabled = false;
            }
        }
    }

    void DropObject()
    {
        if (currentBarricadePoint != null)
        {
            currentBarricadePoint.Barricade(grabbedObject);
            grabbedObject = null;
        } else
        {
            Rigidbody rb = grabbedObject.GetComponent<Rigidbody>();
            rb.isKinematic = false;
            grabbedObject.transform.SetParent(null);
            grabbedObject.GetComponent<Collider>().enabled = true;
            grabbedObject = null;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("BarricadePoint"))
        {
            currentBarricadePoint = other.GetComponent<BarricadePoint>();
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("BarricadePoint"))        
        {
            if (currentBarricadePoint != null && other.GetComponent<BarricadePoint>() == currentBarricadePoint)
            {                
                currentBarricadePoint = null;
            }
        }
    }
}
