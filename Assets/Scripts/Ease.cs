using UnityEngine;

// Easing curves from Lecture 05. t runs 0..1 in, progress p comes out.
// Use with Mathf.LerpUnclamped / Vector3.LerpUnclamped so OutBack's overshoot survives.
public static class Ease
{
    public static float Linear(float t) => t;

    public static float OutQuad(float t) => 1f - (1f - t) * (1f - t);

    public static float InOutCubic(float t) =>
        t < 0.5f ? 4f * t * t * t : 1f - Mathf.Pow(-2f * t + 2f, 3f) / 2f;

    public static float OutBack(float t)
    {
        const float c1 = 1.70158f, c3 = c1 + 1f;
        float u = t - 1f;
        return 1f + c3 * u * u * u + c1 * u * u;
    }

    public static float OutElastic(float t)
    {
        if (t <= 0f) return 0f;
        if (t >= 1f) return 1f;
        const float c4 = (2f * Mathf.PI) / 3f;
        return Mathf.Pow(2f, -10f * t) * Mathf.Sin((t * 10f - 0.75f) * c4) + 1f;
    }

    public static float OutBounce(float t)
    {
        const float n1 = 7.5625f, d1 = 2.75f;
        if (t < 1f / d1) return n1 * t * t;
        if (t < 2f / d1) { t -= 1.5f / d1;   return n1 * t * t + 0.75f; }
        if (t < 2.5f / d1) { t -= 2.25f / d1;  return n1 * t * t + 0.9375f; }
        t -= 2.625f / d1;
        return n1 * t * t + 0.984375f;
    }
}
