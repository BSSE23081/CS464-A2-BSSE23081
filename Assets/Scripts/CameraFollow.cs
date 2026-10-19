using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] Transform target;               // left empty: found in Awake
    [SerializeField] Vector3 offset = new Vector3(0f, 5f, -7f);
    [SerializeField] float sharpness = 5f;           // k in blend = 1 - e^(-k dt)
    [SerializeField] float lookAhead = 0.35f;        // seconds of velocity

    Rigidbody targetRb;

    void Awake()
    {
        if (target == null)
        {
            var player = FindAnyObjectByType<PlayerController>();
            if (player != null) target = player.transform;
        }
        if (target != null) targetRb = target.GetComponent<Rigidbody>();
    }

    void LateUpdate()
    {
        if (target == null) return;

        Vector3 flatVel = targetRb != null
            ? new Vector3(targetRb.linearVelocity.x, 0f, targetRb.linearVelocity.z)
            : Vector3.zero;

        Vector3 aim = target.position + flatVel * lookAhead;   // where the player will be
        Vector3 wanted = aim + offset;

        float blend = 1f - Mathf.Exp(-sharpness * Time.deltaTime);
        transform.position = Vector3.Lerp(transform.position, wanted, blend);

        transform.LookAt(aim);                                  // look at the aim point, not the player
    }
}
