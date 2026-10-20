using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] Transform target;               // left empty: found in Awake
    [SerializeField] Vector3 offset = new Vector3(0f, 5f, -7f);
    [SerializeField] float sharpness = 4f;           // k in blend = 1 - e^(-k dt)
    [SerializeField] float lookAhead = 0.2f;         // seconds of velocity

    Rigidbody targetRb;
    Vector3 focus;                                   // the smoothed point we aim at

    void Awake()
    {
        if (target == null)
        {
            var player = FindAnyObjectByType<PlayerController>();
            if (player != null) target = player.transform;
        }
        if (target != null)
        {
            targetRb = target.GetComponent<Rigidbody>();
            focus = target.position;                 // start on the player, no swoop
        }
    }

    void LateUpdate()
    {
        if (target == null) return;

        Vector3 flatVel = targetRb != null
            ? new Vector3(targetRb.linearVelocity.x, 0f, targetRb.linearVelocity.z)
            : Vector3.zero;

        // where the player will be
        Vector3 aim = target.position + flatVel * lookAhead;

        // smooth the aim point itself, frame-rate independent
        float blend = 1f - Mathf.Exp(-sharpness * Time.deltaTime);
        focus = Vector3.Lerp(focus, aim, blend);

        // camera keeps a fixed offset from the smoothed point, so it never
        // swings its angle when the player starts or stops
        transform.position = focus + offset;
        transform.LookAt(focus);
    }
}
