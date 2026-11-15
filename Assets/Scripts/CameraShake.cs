using UnityEngine;

public class CameraShake : MonoBehaviour
{
    Vector3 home;
    float amount, timeLeft, duration;

    void Awake()
    {
        home = transform.localPosition;
    }

    public void Shake(float strength, float time)
    {
        if (strength < amount && timeLeft > 0) return;
        amount = strength;
        duration = timeLeft = time;
    }

    void LateUpdate()
    {
        if (timeLeft <= 0)
        {
            amount = 0;
            transform.localPosition = home;
            return;
        }
        timeLeft -= Time.deltaTime;
        float k = amount * Mathf.Clamp01(timeLeft / duration);
        float x = Mathf.Round(Random.Range(-k, k) * 16f) / 16f;
        float y = Mathf.Round(Random.Range(-k, k) * 16f) / 16f;
        transform.localPosition = home + new Vector3(x, y, 0);
    }
}
