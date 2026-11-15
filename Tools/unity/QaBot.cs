using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class QaBot : MonoBehaviour
{
    string output;
    Game game;
    readonly List<string> log = new List<string>();
    bool failed;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Boot()
    {
        string[] args = Environment.GetCommandLineArgs();
        int flag = Array.IndexOf(args, "-qa");
        if (flag < 0 || flag + 1 == args.Length) return;

        var bot = new GameObject("QaBot").AddComponent<QaBot>();
        bot.output = args[flag + 1];
    }

    void Check(bool condition, string message)
    {
        string entry = (condition ? "PASS " : "FAIL ") + message;
        log.Add(entry);
        Debug.Log("QA " + entry);
        failed |= !condition;
    }

    IEnumerator Idle()
    {
        yield return null;
        while (game.Busy) yield return null;
    }

    IEnumerator Replay(string commands)
    {
        const string walk = "WDSA";
        const string shoot = "^>v<";
        foreach (char command in commands)
        {
            int direction = walk.IndexOf(command);
            if (direction >= 0)
                Check(game.Step(direction), "walk " + command + " to " + game.player.cell);
            else
                game.Fire(shoot.IndexOf(command));
            yield return Idle();
            if (failed) yield break;
        }
    }

    IEnumerator Route(string[] rooms, bool withLetter)
    {
        game.OpenLevel(0);
        yield return Idle();

        for (int room = 0; room < rooms.Length && !failed; room++)
        {
            if (withLetter && room == 1)
            {
                yield return Replay(new string('D', 11));
                Check(game.Board.At(game.player.cell) == '?', "reached the torn slip");
                yield return Replay("AA^");
                Check(game.Board.state.letterFound && game.hud.LetterOpen, "opened the mailbox");
                game.SendMessage("Undo");
                Check(!game.Board.state.letterFound && !game.hud.LetterOpen, "undo closed the mailbox");
                yield return Replay("^");
                Check(game.Board.state.letterFound && game.hud.LetterOpen, "replayed the mailbox shot");
                game.hud.ShowLetter(false);
                yield return Replay(new string('A', 9));
            }

            yield return Replay(rooms[room]);
            Check(game.Cleared, "cleared " + game.Board.name);
            if (failed) yield break;
            game.SendMessage("NextLevel");
            yield return Idle();
        }

        Check(game.hud.endPanel.activeSelf, "reached the ending");
        Check(game.hud.recoveredLetter.gameObject.activeSelf == withLetter, "ending letter matches the run");
        yield return new WaitForEndOfFrame();
        ScreenCapture.CaptureScreenshot(Path.Combine(output, withLetter ? "ending-with-letter.png" : "ending.png"));
        yield return null;
    }

    IEnumerator Start()
    {
        Directory.CreateDirectory(output);
        AudioListener.volume = 0;
        game = FindFirstObjectByType<Game>();
        string[] rooms = File.ReadAllLines(Path.Combine(output, "solutions.txt"));
        yield return null;
        Check(rooms.Length == game.LevelCount, "solution file matches the campaign");
        if (!failed) yield return Route(rooms, false);
        if (!failed) yield return Route(rooms, true);
        File.WriteAllLines(Path.Combine(output, failed ? "FAIL.txt" : "PASS.txt"), log);
        Application.Quit(failed ? 1 : 0);
    }
}
