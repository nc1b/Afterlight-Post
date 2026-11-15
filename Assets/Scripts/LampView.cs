using System.Collections;
using UnityEngine;

public class LampView : MonoBehaviour
{
    public SpriteRenderer glass;
    public SpriteRenderer glow;

    static readonly Color unlitTint = new Color(0.10f, 0.13f, 0.22f);
    Color colour = Color.white;
    bool lit;
    float seed;

    void Awake()
    {
        seed = Random.value * 10f;
    }

    public void SetColour(Color c)
    {
        colour = c;
        Apply();
    }

    public void SetLit(bool on, bool animate)
    {
        lit = on;
        Apply();
        if (on && animate) StartCoroutine(Pop());
    }

    void Apply()
    {
        glass.color = lit ? Color.Lerp(colour, Color.white, 0.35f) : Color.Lerp(colour, unlitTint, 0.55f);
        glow.enabled = lit;
        glow.color = new Color(colour.r, colour.g, colour.b, 0.55f);
    }

    void Update()
    {
        if (!lit) return;
        float s = 1f + 0.06f * Mathf.Sin(Time.time * 2.3f + seed);
        glow.transform.localScale = new Vector3(s, s, 1);
    }

    IEnumerator Pop()
    {
        const float time = 0.25f;
        for (float t = 0; t < time; t += Time.deltaTime)
        {
            float k = 1 - t / time;
            transform.localScale = Vector3.one * (1 + 0.5f * k * k);
            yield return null;
        }
        transform.localScale = Vector3.one;
    }
}
