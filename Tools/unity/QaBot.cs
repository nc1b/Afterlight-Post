using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class QaBot : MonoBehaviour
{
    string dir;
    Game game;
    readonly List<string> log = new List<string>();
    bool failed;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Boot()
    {
        string[] args = Environment.GetCommandLineArgs();
        int flag = Array.IndexOf(args, "-qa");
        if (flag < 0) return;
        var bot = new GameObject("QaBot").AddComponent<QaBot>();
        bot.dir = args[flag + 1];
        DontDestroyOnLoad(bot.gameObject);
    }

    void Note(string s) { log.Add(s); Debug.Log("QA " + s); }
    void Fail(string s) { failed = true; Note("FAIL " + s); }

    bool recording;

    IEnumerator Record(string folder)
    {
        recording = true;
        Directory.CreateDirectory(Path.Combine(dir, folder));
        int n = 0;
        while (recording)
        {
            yield return new WaitForEndOfFrame();
            ScreenCapture.CaptureScreenshot(Path.Combine(dir, folder, n.ToString("0000") + ".png"));
            n++;
            yield return new WaitForSeconds(0.05f);
        }
    }

    IEnumerator Snap(string name)
    {
        yield return new WaitForEndOfFrame();
        ScreenCapture.CaptureScreenshot(Path.Combine(dir, name + ".png"));
        yield return null;
        yield return null;
    }

    IEnumerator Idle(float extra = 0.3f)
    {
        yield return null;
        while (game.Busy) yield return null;
        yield return new WaitForSeconds(extra);
    }

    IEnumerator WalkTo(Vector2Int target)
    {
        Board b = game.Board;
        var prev = new Dictionary<Vector2Int, Vector2Int>();
        var queue = new Queue<Vector2Int>();
        Vector2Int from = game.player.cell;
        queue.Enqueue(from);
        prev[from] = from;
        while (queue.Count > 0)
        {
            Vector2Int c = queue.Dequeue();
            foreach (Vector2Int d in Board.Dirs)
            {
                Vector2Int n = c + d;
                if (prev.ContainsKey(n) || !b.Walkable(n)) continue;
                prev[n] = c;
                queue.Enqueue(n);
            }
        }
        if (!prev.ContainsKey(target)) { Fail("cannot walk to " + target); yield break; }
        var steps = new List<Vector2Int>();
        for (Vector2Int c = target; c != from; c = prev[c]) steps.Add(c);
        steps.Reverse();
        foreach (Vector2Int s in steps)
        {
            int dirIndex = Array.IndexOf(Board.Dirs, s - game.player.cell);
            while (game.Busy) yield return null;
            if (!game.Step(dirIndex)) { Fail("step refused at " + game.player.cell); yield break; }
            yield return null;
        }
        while (game.Busy) yield return null;
    }

    IEnumerator Start()
    {
        Directory.CreateDirectory(dir);
        PlayerPrefs.DeleteKey("streets_lit");
        AudioListener.volume = 0;
        game = FindFirstObjectByType<Game>();
        string[] solutions = File.ReadAllLines(Path.Combine(dir, "solutions.txt"));
        yield return new WaitForSeconds(1f);
        while (Time.time % 1.1f > 0.5f) yield return null;
        yield return Snap("00-title");

        game.OpenSelect();
        yield return Idle();
        yield return Snap("01-select-fresh");

        for (int i = 0; i < game.LevelCount; i++)
        {
            game.OpenLevel(i);
            yield return Idle(0.4f);
            yield return Snap(string.Format("L{0:00}-a-start", i + 1));
            string[] moves = solutions[i].Split(';');
            if (i == 2 || i == 6) StartCoroutine(Record("frames" + (i + 1)));
            for (int m = 0; m < moves.Length; m++)
            {
                string[] p = moves[m].Split(',');
                yield return WalkTo(new Vector2Int(int.Parse(p[0]), int.Parse(p[1])));
                game.Fire(int.Parse(p[2]));
                if (m == moves.Length - 1)
                {
                    yield return new WaitForSeconds(0.45f);
                    yield return Snap(string.Format("L{0:00}-b-shot", i + 1));
                }
                yield return Idle(0.25f);
            }
            yield return new WaitForSeconds(0.5f);
            if (!game.Cleared) Fail("level " + (i + 1) + " not cleared by solver route");
            else Note("level " + (i + 1) + " cleared");
            yield return Snap(string.Format("L{0:00}-c-clear", i + 1));
            recording = false;
        }

        game.OpenLevel(4);
        yield return Idle();
        game.Fire(1);
        yield return Idle();
        game.Fire(2);
        yield return Idle(0.5f);
        yield return Snap("90-out-of-light");
        game.SendMessage("Undo");
        yield return Idle();
        yield return Snap("91-after-undo");

        game.OpenSelect();
        yield return Idle();
        yield return Snap("92-select-done");

        game.OpenLevel(game.LevelCount - 1);
        yield return Idle();
        string[] last = solutions[game.LevelCount - 1].Split(';');
        foreach (string mv in last)
        {
            string[] p = mv.Split(',');
            yield return WalkTo(new Vector2Int(int.Parse(p[0]), int.Parse(p[1])));
            game.Fire(int.Parse(p[2]));
            yield return Idle(0.25f);
        }
        yield return new WaitForSeconds(0.6f);
        game.SendMessage("NextLevel");
        yield return Idle(0.6f);
        yield return Snap("93-ending");

        File.WriteAllLines(Path.Combine(dir, failed ? "FAIL.txt" : "PASS.txt"), log.ToArray());
        PlayerPrefs.DeleteKey("streets_lit");
        PlayerPrefs.Save();
        Application.Quit(failed ? 1 : 0);
    }
}
