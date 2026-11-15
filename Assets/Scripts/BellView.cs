using System.Collections;
using UnityEngine;

public class BellView : MonoBehaviour
{
    public void Ring()
    {
        StopAllCoroutines();
        StartCoroutine(Swing());
    }

    IEnumerator Swing()
    {
        const float time = 0.7f;
        for (float t = 0; t < time; t += Time.deltaTime)
        {
            float k = 1 - t / time;
            transform.localEulerAngles = new Vector3(0, 0, Mathf.Sin(t * 38f) * 22f * k);
            transform.localScale = Vector3.one * (1 + 0.25f * k * k);
            yield return null;
        }
        transform.localEulerAngles = Vector3.zero;
        transform.localScale = Vector3.one;
    }
}
