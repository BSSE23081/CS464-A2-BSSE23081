using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
public class PlayerController : MonoBehaviour
{
    [Header("Move")]
    [SerializeField] float speed = 5f;

    [Header("Jump")]
    [SerializeField] float jumpHeight = 1.8f;     // metres
    [SerializeField] float fallMultiplier = 2.2f;
    [SerializeField] float coyoteTime = 0.1f;     // seconds
    [SerializeField] float jumpBuffer = 0.12f;    // seconds

    [Header("Ground check (CheckSphere on Ground layer only)")]
    [SerializeField] Transform feet;              // child at the bottom of the capsule
    [SerializeField] float checkRadius = 0.2f;
    [SerializeField] LayerMask groundMask;

    [Header("Juice: squash on landing")]
    [SerializeField] Transform body;              // child mesh, NO collider
    [SerializeField] float squashDuration = 0.25f;
    [SerializeField] float squashY = 0.7f;

    Rigidbody rb;
    Transform cam;
    Vector3 move;
    bool isGrounded, wasGrounded, jumpQueued;
    float lastGroundedTime = -10f, lastPressedTime = -10f, airTime;
    Coroutine squashRoutine;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        cam = Camera.main != null ? Camera.main.transform : null;
    }

    void Update()
    {
        // --- input, relative to the camera ---
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        if (cam == null && Camera.main != null) cam = Camera.main.transform;

        Vector3 forward = cam != null ? cam.forward : Vector3.forward;
        Vector3 right = cam != null ? cam.right : Vector3.right;
        forward.y = 0f; forward.Normalize();
        right.y = 0f;   right.Normalize();
        move = Vector3.ClampMagnitude(forward * v + right * h, 1f);

        // --- ground ---
        isGrounded = Physics.CheckSphere(feet.position, checkRadius, groundMask);

        // stamp only when not rising and no jump queued
        if (isGrounded && !jumpQueued && rb.linearVelocity.y <= 0f)
            lastGroundedTime = Time.time;
        if (Input.GetKeyDown(KeyCode.Space))
            lastPressedTime = Time.time;

        bool canJump = Time.time - lastGroundedTime <= coyoteTime;
        bool wantJump = Time.time - lastPressedTime <= jumpBuffer;
        if (canJump && wantJump && !jumpQueued)
        {
            jumpQueued = true;
            lastGroundedTime = -1f;   // use up both windows
            lastPressedTime = -1f;
        }

        // --- juice: once per real landing ---
        if (isGrounded)
        {
            if (!wasGrounded && airTime > 0.1f)
            {
                if (squashRoutine != null) StopCoroutine(squashRoutine);
                squashRoutine = StartCoroutine(Squash());
            }
            airTime = 0f;
        }
        else airTime += Time.deltaTime;
        wasGrounded = isGrounded;
    }

    void FixedUpdate()
    {
        rb.linearVelocity = new Vector3(move.x * speed, rb.linearVelocity.y, move.z * speed);

        if (jumpQueued)
        {
            Vector3 vel = rb.linearVelocity;
            vel.y = 0f;                                   // so a jump while falling isn't short
            rb.linearVelocity = vel;

            float g = -Physics.gravity.y;
            float v0 = Mathf.Sqrt(2f * g * jumpHeight);
            rb.AddForce(Vector3.up * rb.mass * v0, ForceMode.Impulse);
            jumpQueued = false;
        }
        else if (rb.linearVelocity.y < 0f)
        {
            rb.AddForce(Physics.gravity * (fallMultiplier - 1f), ForceMode.Acceleration);
        }
    }

    IEnumerator Squash()
    {
        for (float elapsed = 0f; elapsed < squashDuration; elapsed += Time.deltaTime)
        {
            float p = Ease.OutBack(elapsed / squashDuration);   // from Ease.cs in the lecture
            float y = Mathf.LerpUnclamped(squashY, 1f, p);      // unclamped so the overshoot survives
            float xz = 1f / Mathf.Sqrt(y);                      // keep volume
            body.localScale = new Vector3(xz, y, xz);
            yield return null;
        }
        body.localScale = Vector3.one;                          // exactly at rest
        squashRoutine = null;
    }

    void OnDrawGizmosSelected()
    {
        if (feet == null) return;
        Gizmos.color = isGrounded ? Color.green : Color.red;
        Gizmos.DrawWireSphere(feet.position, checkRadius);
    }
}
