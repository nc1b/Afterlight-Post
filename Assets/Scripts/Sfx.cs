using UnityEngine;

public class Sfx : MonoBehaviour
{
    public static Sfx I;

    public AudioSource source;
    public AudioSource music;

    public AudioClip step, send, tick, bell, thud, lamp, flip, gate, undo, ui, empty, clear;

    void Awake()
    {
        I = this;
        AudioListener.volume = PlayerPrefs.GetInt("muted", 0) == 1 ? 0 : 1;
    }

    public static void Play(AudioClip clip, float pitch = 1f, float volume = 1f)
    {
        if (I == null || clip == null) return;
        I.source.pitch = pitch;
        I.source.PlayOneShot(clip, volume);
    }

    public static void ToggleMute()
    {
        bool mute = AudioListener.volume > 0.5f;
        AudioListener.volume = mute ? 0 : 1;
        PlayerPrefs.SetInt("muted", mute ? 1 : 0);
    }
}
