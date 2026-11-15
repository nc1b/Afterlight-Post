using UnityEngine;

public class WaterTile : MonoBehaviour
{
    public SpriteRenderer sr;
    public Sprite[] frames;
    public float speed = 1.4f;

    float offset;

    void Start()
    {
        Vector3 p = transform.position;
        offset = Mathf.Abs(p.x * 0.37f + p.y * 0.61f);
    }

    void Update()
    {
        sr.sprite = frames[(int)(Time.time * speed + offset) % frames.Length];
    }
}
