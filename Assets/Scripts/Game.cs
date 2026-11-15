using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Game : MonoBehaviour
{
    public TextAsset[] levels;
    public BoardView view;
    public Player player;
    public Hud hud;
    public LevelSelect select;

    enum Mode { Title, Select, Play, End }

    struct Snapshot
    {
        public BoardState state;
        public Vector2Int cell;
        public int facing;
        public int shots;
    }

    const string SaveKey = "streets_lit";

    Mode mode = Mode.Title;
    Board board;
    int levelIndex;
    int shotsLeft;
    int clearedMask;
    int cursor;
    bool firing;
    bool cleared;
    float clearedAt;
    float holdTimer;
    string[] levelNames;
    readonly Stack<Snapshot> history = new Stack<Snapshot>();

    public Board Board { get { return board; } }
    public bool Cleared { get { return cleared; } }
    public bool Busy { get { return firing || player.Moving || hud.Fading; } }
    public int LevelCount { get { return levels.Length; } }

    void Start()
    {
        Application.targetFrameRate = 60;
        clearedMask = PlayerPrefs.GetInt(SaveKey, 0);
        levelNames = new string[levels.Length];
        for (int i = 0; i < levels.Length; i++) levelNames[i] = Board.Parse(levels[i].text).name;
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
                if (confirm) { Sfx.Play(Sfx.I.ui); OpenSelect(); }
                else if (escape) Application.Quit();
                break;

            case Mode.Select:
                UpdateSelect(confirm, escape);
                break;

            case Mode.Play:
                UpdatePlay(confirm, escape);
                break;

            case Mode.End:
                if (confirm || escape) { Sfx.Play(Sfx.I.ui); OpenSelect(); }
                break;
        }
    }


    bool IsCleared(int i)
    {
        return (clearedMask & (1 << i)) != 0;
    }

    bool IsUnlocked(int i)
    {
        return i < 2 || IsCleared(i - 1) || IsCleared(i - 2);
    }

    public void OpenSelect()
    {
        hud.FadeThrough(() =>
        {
            mode = Mode.Select;
            player.gameObject.SetActive(false);
            hud.Show(hud.selectPanel);
            var unlocked = new bool[levels.Length];
            var done = new bool[levels.Length];
            for (int i = 0; i < levels.Length; i++)
            {
                unlocked[i] = IsUnlocked(i);
                done[i] = IsCleared(i);
            }
            select.Refresh(unlocked, done);
            MoveCursor(cursor);
        });
    }

    void MoveCursor(int to)
    {
        cursor = Mathf.Clamp(to, 0, levels.Length - 1);
        select.SetCursor(cursor, IsUnlocked(cursor) ? levelNames[cursor] : "LOCKED");
    }

    void UpdateSelect(bool confirm, bool escape)
    {
        int move = 0;
        if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A)) move = -1;
        if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D)) move = 1;
        if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W)) move = -select.columns;
        if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S)) move = select.columns;
        if (move != 0 && cursor + move >= 0 && cursor + move < levels.Length)
        {
            MoveCursor(cursor + move);
            Sfx.Play(Sfx.I.ui);
        }

        if (confirm)
        {
            if (IsUnlocked(cursor)) { Sfx.Play(Sfx.I.send); OpenLevel(cursor); }
            else Sfx.Play(Sfx.I.empty);
        }
        else if (escape)
        {
            Sfx.Play(Sfx.I.undo);
            hud.FadeThrough(() => { mode = Mode.Title; hud.Show(hud.titlePanel); });
        }
    }


    public void OpenLevel(int index)
    {
        hud.FadeThrough(() =>
        {
            mode = Mode.Play;
            hud.Show(hud.playPanel);
            LoadLevel(index);
        });
    }

    void LoadLevel(int index)
    {
        StopAllCoroutines();
        levelIndex = index;
        cursor = index;
        board = Board.Parse(levels[index].text);
        shotsLeft = board.shots;
        firing = false;
        cleared = false;
        history.Clear();

        view.Build(board);
        player.gameObject.SetActive(true);
        player.Place(board.start, view.World(board.start));
        hud.SetLevel(index, board);
        hud.SetShots(shotsLeft, board.shots);
        RefreshClue();
    }

    void UpdatePlay(bool confirm, bool escape)
    {
        if (hud.LetterOpen && (confirm || escape)) { hud.ShowLetter(null); return; }
        if (escape) { Sfx.Play(Sfx.I.undo); OpenSelect(); return; }
        if (firing) return;

        if (Input.GetKeyDown(KeyCode.R)) { Sfx.Play(Sfx.I.undo); LoadLevel(levelIndex); return; }
        if (Input.GetKeyDown(KeyCode.Z) || Input.GetKeyDown(KeyCode.Backspace)) { Undo(); return; }
        if (hud.LetterOpen) return;

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
        RefreshClue();
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
        if (!alreadyFound && board.state.letterFound)
            hud.ShowLetter(board.secret.letter);

        if (board.Solved)
        {
            cleared = true;
            clearedAt = Time.time;
            clearedMask |= 1 << levelIndex;
            PlayerPrefs.SetInt(SaveKey, clearedMask);
            PlayerPrefs.Save();
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
        view.Refresh();
        player.Place(s.cell, view.World(s.cell));
        player.Face(s.facing);
        hud.SetShots(shotsLeft, board.shots);
        hud.ShowClear(false, false);
        hud.ShowOut(false);
        hud.ShowLetter(null);
        RefreshClue();
        Sfx.Play(Sfx.I.undo);
    }

    void RefreshClue()
    {
        var secret = board.secret;
        hud.ShowClue(secret != null && player.cell == secret.slip ? secret.clue : null);
    }

    void NextLevel()
    {
        Sfx.Play(Sfx.I.ui);
        if (levelIndex < levels.Length - 1)
        {
            OpenLevel(levelIndex + 1);
            return;
        }
        hud.FadeThrough(() =>
        {
            mode = Mode.End;
            player.gameObject.SetActive(false);
            hud.Show(hud.endPanel);
        });
    }
}
