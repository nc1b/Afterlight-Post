using UnityEngine;
using UnityEngine.UI;

public class LevelSelect : MonoBehaviour
{
    public Image[] stamps;
    public PixelText[] numbers;
    public Image[] locks;
    public RectTransform cursor;
    public PixelText caption;
    public PixelText progress;

    public Color lockedColour = new Color(0.2f, 0.25f, 0.36f);
    public Color openColour = new Color(0.91f, 0.87f, 0.78f);
    public Color clearedColour = new Color(1f, 0.81f, 0.36f);
    public int columns = 5;

    public void Refresh(bool[] unlocked, bool[] cleared)
    {
        int done = 0;
        for (int i = 0; i < stamps.Length; i++)
        {
            stamps[i].color = cleared[i] ? clearedColour : unlocked[i] ? openColour : lockedColour;
            numbers[i].gameObject.SetActive(unlocked[i]);
            locks[i].gameObject.SetActive(!unlocked[i]);
            if (cleared[i]) done++;
        }
        progress.Text = done + " / " + stamps.Length + " STREETS LIT";
    }

    public void SetCursor(int index, string label)
    {
        cursor.anchoredPosition = stamps[index].rectTransform.anchoredPosition;
        caption.Text = label;
    }

    void Update()
    {
        float s = 1f + 0.06f * Mathf.Sin(Time.unscaledTime * 6f);
        cursor.localScale = new Vector3(s, s, 1);
    }
}
