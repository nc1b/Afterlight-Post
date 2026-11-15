using System.Collections;
using UnityEngine;

public class Player : MonoBehaviour
{
    public SpriteRenderer body;
    public Sprite front, frontStep, back, backStep;
    public float stepTime = 0.1f;

    [HideInInspector] public Vector2Int cell;
    [HideInInspector] public int facing = 2;
    public bool Moving { get; private set; }

    bool step;

    public void Place(Vector2Int c, Vector3 world)
    {
        StopAllCoroutines();
        Moving = false;
        cell = c;
        transform.position = world;
        body.transform.localPosition = Vector3.zero;
        body.transform.localScale = Vector3.one;
        step = false;
        Face(2);
    }

    public void Face(int dir)
    {
        facing = dir;
        Refresh();
    }

    void Refresh()
    {
        if (facing == 0) body.sprite = step ? backStep : back;
        else body.sprite = step ? frontStep : front;
        body.flipX = facing == 3;
    }

    public void WalkTo(Vector2Int c, Vector3 world)
    {
        cell = c;
        StartCoroutine(Walk(world));
    }

    IEnumerator Walk(Vector3 target)
    {
        Moving = true;
        step = !step;
        Refresh();
        Vector3 from = transform.position;
        for (float t = 0; t < stepTime; t += Time.deltaTime)
        {
            transform.position = Vector3.Lerp(from, target, t / stepTime);
            yield return null;
        }
        transform.position = target;
        Moving = false;
    }

    public void Bump(int dir)
    {
        StartCoroutine(BumpRoutine(dir));
    }

    IEnumerator BumpRoutine(int dir)
    {
        Moving = true;
        Vector3 push = new Vector3(Board.Dirs[dir].x, -Board.Dirs[dir].y, 0) * 0.15f;
        const float time = 0.09f;
        for (float t = 0; t < time; t += Time.deltaTime)
        {
            body.transform.localPosition = push * Mathf.Sin(t / time * Mathf.PI);
            yield return null;
        }
        body.transform.localPosition = Vector3.zero;
        Moving = false;
    }

    public void Hop()
    {
        StartCoroutine(HopRoutine());
    }

    IEnumerator HopRoutine()
    {
        const float time = 0.18f;
        for (float t = 0; t < time; t += Time.deltaTime)
        {
            float k = Mathf.Sin(t / time * Mathf.PI);
            body.transform.localScale = new Vector3(1 + 0.2f * k, 1 - 0.2f * k, 1);
            yield return null;
        }
        body.transform.localScale = Vector3.one;
    }
}
