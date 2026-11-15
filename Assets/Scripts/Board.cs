using System.Collections.Generic;
using UnityEngine;

public enum Tile { Floor, Water, Wall, Bell, Mirror, Hinge, Lamp, Gate, Counter, Mailbox }
public enum StopKind { Wall, Gate, Bell, Loop }

public struct BoardState
{
    public int flips;
    public int lit;
    public bool letterFound;
}

public class Shot
{
    public List<Vector2Int> path = new List<Vector2Int>();
    public StopKind stop;
    public int endDir;
    public bool Charged { get { return stop == StopKind.Bell; } }
}

public class Board
{
    public static readonly Vector2Int[] Dirs =
    {
        new Vector2Int(0, -1), new Vector2Int(1, 0), new Vector2Int(0, 1), new Vector2Int(-1, 0)
    };

    public string name = "";
    public int shots = 3;
    public List<string> tips = new List<string>();
    public int width, height;
    public Vector2Int start;
    public int goal;
    public BoardState state;

    char[,] cells;
    int[,] hinge;

    public static Board Parse(string text)
    {
        var board = new Board();
        var rows = new List<string>();
        bool inMap = false;
        foreach (string raw in text.Replace("\r", "").Split('\n'))
        {
            if (inMap)
            {
                if (raw.Trim().Length > 0) rows.Add(raw.TrimEnd());
                continue;
            }
            string line = raw.Trim();
            if (line == "map:") { inMap = true; continue; }
            int colon = line.IndexOf(':');
            if (colon < 0) continue;
            string key = line.Substring(0, colon).Trim();
            string value = line.Substring(colon + 1).Trim();
            if (key == "name") board.name = value;
            else if (key == "shots") board.shots = int.Parse(value);
            else if (key == "tip") board.tips.Add(value);
        }

        board.height = rows.Count;
        foreach (string row in rows) board.width = Mathf.Max(board.width, row.Length);
        board.cells = new char[board.width, board.height];
        board.hinge = new int[board.width, board.height];
        int hinges = 0;
        for (int y = 0; y < board.height; y++)
        {
            for (int x = 0; x < board.width; x++)
            {
                char c = x < rows[y].Length ? rows[y][x] : '#';
                board.hinge[x, y] = -1;
                if (c == 'P') { board.start = new Vector2Int(x, y); c = '.'; }
                if (c == '<' || c == '>') board.hinge[x, y] = hinges++;
                if (c >= 'a' && c <= 'd') board.goal |= 1 << (c - 'a');
                board.cells[x, y] = c;
            }
        }
        return board;
    }

    public bool Inside(Vector2Int p)
    {
        return p.x >= 0 && p.y >= 0 && p.x < width && p.y < height;
    }

    public char At(Vector2Int p)
    {
        return Inside(p) ? cells[p.x, p.y] : '#';
    }

    public Tile TileAt(Vector2Int p)
    {
        char c = At(p);
        if (c == '#' || c == 'w') return Tile.Wall;
        if (c == ':' || c == ',') return Tile.Counter;
        if (c == 'm') return Tile.Mailbox;
        if (c == '~') return Tile.Water;
        if (c == 'O') return Tile.Bell;
        if (c == '/' || c == '\\') return Tile.Mirror;
        if (c == '<' || c == '>') return Tile.Hinge;
        if (c >= 'a' && c <= 'd') return Tile.Lamp;
        if (c >= 'A' && c <= 'D') return Tile.Gate;
        return Tile.Floor;
    }

    public int ColourAt(Vector2Int p)
    {
        char c = At(p);
        if (c >= 'a' && c <= 'd') return c - 'a';
        if (c >= 'A' && c <= 'D') return c - 'A';
        return -1;
    }

    public int HingeAt(Vector2Int p)
    {
        return Inside(p) ? hinge[p.x, p.y] : -1;
    }

    public bool IsSlash(Vector2Int p)
    {
        char c = At(p);
        bool slash = c == '/' || c == '<';
        int h = HingeAt(p);
        if (h >= 0 && (state.flips & (1 << h)) != 0) slash = !slash;
        return slash;
    }

    public bool IsLit(int colour)
    {
        return (state.lit & (1 << colour)) != 0;
    }

    public bool Solved { get { return state.lit == goal; } }

    public bool Walkable(Vector2Int p)
    {
        Tile t = TileAt(p);
        if (t == Tile.Floor) return true;
        if (t == Tile.Gate) return IsLit(ColourAt(p));
        return false;
    }

    public Shot Trace(Vector2Int from, int dir)
    {
        var shot = new Shot();
        var seen = new HashSet<int>();
        Vector2Int p = from;
        shot.path.Add(p);
        while (true)
        {
            Vector2Int next = p + Dirs[dir];
            Tile t = TileAt(next);
            if (t == Tile.Wall) { shot.stop = StopKind.Wall; break; }
            if (t == Tile.Gate && !IsLit(ColourAt(next))) { shot.stop = StopKind.Gate; break; }
            if (!seen.Add((next.y * width + next.x) * 4 + dir)) { shot.stop = StopKind.Loop; break; }
            p = next;
            shot.path.Add(p);
            if (t == Tile.Bell) { shot.stop = StopKind.Bell; break; }
            if (t == Tile.Mirror || t == Tile.Hinge)
            {
                dir = ((IsSlash(p) ? 1 : 3) - dir + 4) % 4;
            }
        }
        shot.endDir = dir;
        return shot;
    }
}
