using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Game : MonoBehaviour
{
    public TextAsset[] levels;
    public BoardView view;
    public Player player;
    public Hud hud;

    enum Mode { Title, Play, End }
    struct Snapshot
    {
        public BoardState state;
        public Vector2Int cell;
        public int facing;
        public int shots;
    }

    Mode mode = Mode.Title;
    Board board;
    int levelIndex;
    int shotsLeft;
    bool firing;
    bool cleared;
    bool letterRecovered;
    float clearedAt;
    float holdTimer;
    readonly Stack<Snapshot> history = new Stack<Snapshot>();

    public Board Board { get { return board; } }
    public bool Cleared { get { return cleared; } }
    public bool Busy { get { return firing || player.Moving || hud.Fading; } }
    public int LevelCount { get { return levels.Length; } }

    void Start()
    {
        Application.targetFrameRate = 60;
        player.gameObject.SetActive(false);
        hud.Show(hud.titlePanel);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.M)) Sfx.ToggleMute();
        if (Input.GetKeyDown(KeyCode.F11)) Screen.fullScreen = !Screen.fullScreen;
        if (hud.Fading) return;

        bool confirm = Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return);
        bool escape = Input.GetKeyDown(KeyCode.Escape);
        switch (mode)
        {
            case Mode.Title:
                if (confirm)
                {
                    letterRecovered = false;
                    Sfx.Play(Sfx.I.ui);
                    OpenLevel(0);
                }
                else if (escape) Application.Quit();
                break;
            case Mode.Play:
                UpdatePlay(confirm, escape);
                break;
            case Mode.End:
                if (confirm || escape) ReturnToTitle();
                break;
        }
    }

    void ReturnToTitle()
    {
        Sfx.Play(Sfx.I.ui);
        hud.FadeThrough(() =>
        {
            mode = Mode.Title;
            view.gameObject.SetActive(false);
            player.gameObject.SetActive(false);
            hud.Show(hud.titlePanel);
        });
    }

    public void OpenLevel(int index)
    {
        hud.FadeThrough(() =>
        {
            mode = Mode.Play;
            hud.Show(hud.playPanel);
            view.gameObject.SetActive(true);
            LoadLevel(index);
        });
    }

    void LoadLevel(int index)
    {
        StopAllCoroutines();
        levelIndex = index;
        board = Board.Parse(levels[index].text);
        shotsLeft = board.shots;
        firing = false;
        cleared = false;
        holdTimer = 0;
        history.Clear();

        view.Build(board);
        player.gameObject.SetActive(true);
        player.Place(board.start, view.World(board.start));
        hud.SetLevel(index, levels.Length, board);
        hud.SetShots(shotsLeft, board.shots);
    }

    void UpdatePlay(bool confirm, bool escape)
    {
        if (firing) return;
        if (Input.GetKeyDown(KeyCode.R)) { Sfx.Play(Sfx.I.undo); LoadLevel(levelIndex); return; }
        if (Input.GetKeyDown(KeyCode.Z) || Input.GetKeyDown(KeyCode.Backspace)) { Undo(); return; }

        if (hud.LetterOpen)
        {
            if (confirm || escape) hud.ShowLetter(false);
            return;
        }
        if (escape) { ReturnToTitle(); return; }

        if (cleared)
        {
            if (confirm && Time.time - clearedAt > 0.4f) NextLevel();
            return;
        }

        if (Input.GetKeyDown(KeyCode.UpArrow)) Fire(0);
        else if (Input.GetKeyDown(KeyCode.RightArrow)) Fire(1);
        else if (Input.GetKeyDown(KeyCode.DownArrow)) Fire(2);
        else if (Input.GetKeyDown(KeyCode.LeftArrow)) Fire(3);
        if (firing) return;

        int dir = -1;
        if (Input.GetKey(KeyCode.W)) dir = 0;
        else if (Input.GetKey(KeyCode.D)) dir = 1;
        else if (Input.GetKey(KeyCode.S)) dir = 2;
        else if (Input.GetKey(KeyCode.A)) dir = 3;
        if (!player.Moving) hud.ShowSlip(board.At(player.cell) == '?');
        if (dir < 0) { holdTimer = 0; return; }
        holdTimer -= Time.deltaTime;
        if (!player.Moving && holdTimer <= 0) Step(dir);
    }

    public bool Step(int dir)
    {
        if (firing || cleared || player.Moving) return false;
        Vector2Int target = player.cell + Board.Dirs[dir];
        player.Face(dir);
        if (!board.Walkable(target))
        {
            player.Bump(dir);
            holdTimer = 0.2f;
            return false;
        }
        player.WalkTo(target, view.World(target));
        holdTimer = 0.14f;
        Sfx.Play(Sfx.I.step, Random.Range(0.9f, 1.1f), 0.6f);
        return true;
    }

    public void Fire(int dir)
    {
        if (firing || cleared || player.Moving) return;
        if (shotsLeft <= 0)
        {
            Sfx.Play(Sfx.I.empty);
            hud.ShowOut(true);
            return;
        }

        history.Push(new Snapshot { state = board.state, cell = player.cell, facing = player.facing, shots = shotsLeft });
        shotsLeft--;
        hud.SetShots(shotsLeft, board.shots);
        player.Face(dir);
        Sfx.Play(Sfx.I.send);
        StartCoroutine(FireRoutine(dir));
    }

    IEnumerator FireRoutine(int dir)
    {
        firing = true;
        Shot shot = board.Trace(player.cell, dir);
        bool alreadyFound = board.state.letterFound;
        yield return view.PlayShot(shot);
        player.Hop();
        firing = false;
        if (board.state.letterFound && !alreadyFound) hud.ShowLetter(true);

        if (board.Solved)
        {
            cleared = true;
            clearedAt = Time.time;
            view.Celebrate();
            Sfx.Play(Sfx.I.clear);
            hud.ShowClear(true, levelIndex == levels.Length - 1);
        }
        else if (shotsLeft == 0)
        {
            Sfx.Play(Sfx.I.empty);
            hud.ShowOut(true);
        }
    }

    void Undo()
    {
        if (history.Count == 0) return;
        Snapshot s = history.Pop();
        board.state = s.state;
        shotsLeft = s.shots;
        cleared = false;
        holdTimer = 0;
        view.Refresh();
        player.Place(s.cell, view.World(s.cell));
        player.Face(s.facing);
        hud.SetShots(shotsLeft, board.shots);
        hud.ShowClear(false, false);
        hud.ShowLetter(false);
        hud.ShowSlip(board.At(player.cell) == '?');
        Sfx.Play(Sfx.I.undo);
    }

    void NextLevel()
    {
        letterRecovered |= board.state.letterFound;
        Sfx.Play(Sfx.I.ui);
        if (levelIndex < levels.Length - 1)
        {
            OpenLevel(levelIndex + 1);
            return;
        }
        hud.FadeThrough(() =>
        {
            mode = Mode.End;
            view.gameObject.SetActive(false);
            player.gameObject.SetActive(false);
            hud.ShowRecoveredLetter(letterRecovered);
            hud.Show(hud.endPanel);
        });
    }
}
