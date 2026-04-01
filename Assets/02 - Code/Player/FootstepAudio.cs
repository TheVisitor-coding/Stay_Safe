using UnityEngine;

public class FootstepAudio : MonoBehaviour
{
    public AudioSource footstepsSource;
    public AudioClip homeFootstepClip;

    RaycastHit hit;
    public Transform RayStart;
    public float range;
    public LayerMask layerMask;

    public void Footsteps()
    {
        if (Physics.Raycast(RayStart.position, RayStart.transform.up * -1, out hit, range, layerMask))
        {
            if (hit.collider.CompareTag("Home"))
            {
                footstepsSource.clip = homeFootstepClip;
                footstepsSource.pitch = Random.Range(0.8f, 1.2f);
                footstepsSource.PlayOneShot(footstepsSource.clip);
            }
            else
            {
                if (footstepsSource.isPlaying) footstepsSource.Stop();
            }
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Debug.DrawRay(RayStart.position, RayStart.transform.up * -range, Color.red);
    }
}
