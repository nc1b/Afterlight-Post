using System.Collections;
using UnityEngine;

public class GateView : MonoBehaviour
{
    public SpriteRenderer body;
    public Sprite closedSprite;
    public Sprite openSprite;

    public void SetColour(Color c)
    {
        body.color = c;
    }

    public void SetOpen(bool open, bool animate)
    {
        StopAllCoroutines();
        transform.localScale = Vector3.one;
        body.sprite = open ? openSprite : closedSprite;
        body.sortingOrder = open ? 1 : 3;
        if (open && animate) StartCoroutine(Drop());
    }

    IEnumerator Drop()
    {
        const float time = 0.2f;
        for (float t = 0; t < time; t += Time.deltaTime)
        {
            float k = 1 - t / time;
            transform.localScale = new Vector3(1 + 0.25f * k, 1 - 0.3f * k, 1);
            yield return null;
        }
        transform.localScale = Vector3.one;
    }
}
