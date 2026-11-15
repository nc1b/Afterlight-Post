using System.Collections;
using UnityEngine;

public class MirrorView : MonoBehaviour
{
    bool slash = true;
    Coroutine spin;

    public void Set(bool isSlash)
    {
        if (spin != null) StopCoroutine(spin);
        spin = null;
        slash = isSlash;
        transform.localEulerAngles = new Vector3(0, 0, slash ? 0 : 90);
        transform.localScale = Vector3.one;
    }

    public void FlipTo(bool isSlash)
    {
        if (spin != null) StopCoroutine(spin);
        spin = StartCoroutine(Spin(isSlash));
    }

    IEnumerator Spin(bool isSlash)
    {
        float from = slash ? 0 : 90;
        float to = isSlash ? 0 : 90;
        slash = isSlash;
        const float time = 0.16f;
        for (float t = 0; t < time; t += Time.deltaTime)
        {
            float k = t / time;
            float ease = 1 - (1 - k) * (1 - k);
            transform.localEulerAngles = new Vector3(0, 0, Mathf.Lerp(from, to, ease));
            transform.localScale = Vector3.one * (1 + 0.3f * Mathf.Sin(k * Mathf.PI));
            yield return null;
        }
        transform.localEulerAngles = new Vector3(0, 0, to);
        transform.localScale = Vector3.one;
        spin = null;
    }
}
