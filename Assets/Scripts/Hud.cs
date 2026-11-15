using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Hud : MonoBehaviour
{
    [Header("Screens")]
    public GameObject titlePanel;
    public GameObject selectPanel;
    public GameObject playPanel;
    public GameObject endPanel;

    [Header("Play")]
    public PixelText levelName;
    public PixelText tip;
    public Image[] pips;
    public Sprite pipFull;
    public Sprite pipEmpty;
    public GameObject clearBox;
    public PixelText clearPrompt;
    public GameObject outBox;

    [Header("Other")]
    public PixelText pressStart;
    public Image fade;

    public bool Fading { get; private set; }

    public void Show(GameObject panel)
    {
        titlePanel.SetActive(panel == titlePanel);
        selectPanel.SetActive(panel == selectPanel);
        playPanel.SetActive(panel == playPanel);
        endPanel.SetActive(panel == endPanel);
    }

    public void SetLevel(int index, Board board)
    {
        levelName.Text = (index + 1).ToString("00") + "  " + board.name;
        tip.Text = string.Join("\n", board.tips.ToArray());
        clearBox.SetActive(false);
        outBox.SetActive(false);
        tip.gameObject.SetActive(true);
    }

    public void SetShots(int left, int total)
    {
        int hidden = pips.Length - total;
        for (int i = 0; i < pips.Length; i++)
        {
            pips[i].gameObject.SetActive(i >= hidden);
            pips[i].sprite = i - hidden < left ? pipFull : pipEmpty;
        }
    }

    public void ShowClear(bool show, bool lastLevel)
    {
        clearBox.SetActive(show);
        outBox.SetActive(false);
        tip.gameObject.SetActive(!show);
        clearPrompt.Text = lastLevel ? "ALL LIT!   SPACE - HEAD HOME" : "STREET LIT!   SPACE - NEXT STREET";
    }

    public void ShowOut(bool show)
    {
        outBox.SetActive(show);
        tip.gameObject.SetActive(!show);
    }

    void Update()
    {
        if (titlePanel.activeSelf) pressStart.enabled = Time.time % 1.1f < 0.7f;
    }

    public void FadeThrough(System.Action change)
    {
        StartCoroutine(FadeRoutine(change));
    }

    IEnumerator FadeRoutine(System.Action change)
    {
        Fading = true;
        yield return FadeTo(1f, 0.16f);
        change();
        yield return FadeTo(0f, 0.2f);
        Fading = false;
    }

    IEnumerator FadeTo(float alpha, float time)
    {
        Color c = fade.color;
        float from = c.a;
        fade.enabled = true;
        for (float t = 0; t < time; t += Time.unscaledDeltaTime)
        {
            c.a = Mathf.Lerp(from, alpha, t / time);
            fade.color = c;
            yield return null;
        }
        c.a = alpha;
        fade.color = c;
        fade.enabled = alpha > 0.01f;
    }
}
