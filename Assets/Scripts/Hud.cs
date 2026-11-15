using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Hud : MonoBehaviour
{
    [Header("Screens")]
    public GameObject titlePanel;
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
    public GameObject letterBox;

    [Header("Other")]
    public PixelText recoveredLetter;
    public Image fade;

    string roomTip;
    public bool LetterOpen { get { return letterBox.activeSelf; } }
    public bool Fading { get; private set; }

    public void Show(GameObject panel)
    {
        titlePanel.SetActive(panel == titlePanel);
        playPanel.SetActive(panel == playPanel);
        endPanel.SetActive(panel == endPanel);
    }

    public void SetLevel(int index, int count, Board board)
    {
        levelName.Text = (index + 1).ToString("00") + "/" + count.ToString("00") + "  " + board.name;
        roomTip = string.Join("\n", board.tips);
        ShowSlip(false);
        clearBox.SetActive(false);
        outBox.SetActive(false);
        ShowLetter(false);
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
        clearPrompt.Text = lastLevel ? "ALL LIT!   SPACE - OPEN UP" : "ROOM LIT!   SPACE - NEXT ROOM";
    }

    public void ShowOut(bool show)
    {
        outBox.SetActive(show);
        tip.gameObject.SetActive(!show);
    }

    public void ShowSlip(bool show)
    {
        tip.Text = show
            ? "TORN SLIP - SEND IT BACK THE WAY IT CAME.\nTHE DEAD BOX OPENS ON THE RETURN FROM A BELL."
            : roomTip;
    }

    public void ShowLetter(bool show)
    {
        letterBox.SetActive(show);
    }

    public void ShowRecoveredLetter(bool show)
    {
        recoveredLetter.gameObject.SetActive(show);
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
