using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardView : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject floorPrefab;
    public GameObject waterPrefab;
    public GameObject wallPrefab;
    public GameObject bellPrefab;
    public GameObject mirrorPrefab;
    public GameObject hingePrefab;
    public GameObject lampPrefab;
    public GameObject gatePrefab;
    public GameObject beamPrefab;

    [Header("Variants")]
    public Sprite[] floorSprites;
    public Sprite[] wallSprites;
    public Sprite counterSprite;
    public Sprite parcelsSprite;
    public Sprite windowDark;
    public Sprite windowLit;
    public Sprite slipSprite;
    public Sprite scratchSprite;
    public Sprite mailboxDark;
    public Sprite mailboxOpen;

    [Header("Scene")]
    public Transform tiles;
    public Transform beams;
    public SpriteRenderer pulse;
    public SpriteRenderer pulseGlow;
    public ParticleSystem sparks;
    public CameraShake shake;

    [Header("Colours")]
    public Color[] lampColours;
    public Color outColour = new Color(0.75f, 0.9f, 1f);
    public Color chargedColour = new Color(1f, 0.81f, 0.36f);
    public Color deadColour = new Color(0.42f, 0.5f, 0.62f);

    [Header("Timing")]
    public float outSpeed = 34f;     
    public float backSpeed = 24f;
    public float boardYOffset = 0.25f;

    Board board;
    Vector3 origin;
    SpriteRenderer mailbox;
    readonly Dictionary<Vector2Int, MirrorView> mirrors = new Dictionary<Vector2Int, MirrorView>();
    readonly Dictionary<Vector2Int, LampView> lamps = new Dictionary<Vector2Int, LampView>();
    readonly Dictionary<Vector2Int, GateView> gates = new Dictionary<Vector2Int, GateView>();
    readonly Dictionary<Vector2Int, BellView> bells = new Dictionary<Vector2Int, BellView>();
    readonly List<SpriteRenderer> segments = new List<SpriteRenderer>();
    readonly List<SpriteRenderer> surfaces = new List<SpriteRenderer>();
    readonly List<SpriteRenderer> windows = new List<SpriteRenderer>();

    int flippedThisShot;
    int lampsThisShot;

    public Vector3 World(Vector2Int cell)
    {
        return origin + new Vector3(cell.x, -cell.y, 0);
    }

    public void Build(Board b)
    {
        board = b;
        StopAllCoroutines();
        for (int i = tiles.childCount - 1; i >= 0; i--) Destroy(tiles.GetChild(i).gameObject);
        mirrors.Clear();
        lamps.Clear();
        gates.Clear();
        bells.Clear();
        surfaces.Clear();
        windows.Clear();
        mailbox = null;
        ClearBeam();
        pulse.gameObject.SetActive(false);

        origin = new Vector3(-(b.width - 1) / 2f, (b.height - 1) / 2f + boardYOffset, 0);
        origin.x = Mathf.Round(origin.x * 16f) / 16f;

        for (int y = 0; y < b.height; y++)
        {
            for (int x = 0; x < b.width; x++)
            {
                var cell = new Vector2Int(x, y);
                Tile tile = b.TileAt(cell);
                char mark = b.At(cell);
                Vector3 pos = World(cell);

                if (tile == Tile.Wall)
                {
                    var wall = Instantiate(wallPrefab, pos, Quaternion.identity, tiles);
                    var renderer = wall.GetComponent<SpriteRenderer>();
                    renderer.sprite = wallSprites[(x * 7 + y * 3) % wallSprites.Length];
                    surfaces.Add(renderer);
                    if (mark == 'w') windows.Add(AddProp("Office window", windowDark, pos));
                    continue;
                }

                if (tile == Tile.Water || StandsInWater(cell, tile))
                {
                    Instantiate(waterPrefab, pos, Quaternion.identity, tiles);
                }
                else
                {
                    var floor = Instantiate(floorPrefab, pos, Quaternion.identity, tiles);
                    var floorRenderer = floor.GetComponent<SpriteRenderer>();
                    if (tile == Tile.Counter)
                        floorRenderer.sprite = mark == ',' ? parcelsSprite : counterSprite;
                    else
                        floorRenderer.sprite = floorSprites[(x + y * 2) % floorSprites.Length];
                    surfaces.Add(floorRenderer);
                }

                switch (tile)
                {
                    case Tile.Bell:
                        bells[cell] = Instantiate(bellPrefab, pos, Quaternion.identity, tiles).GetComponent<BellView>();
                        break;
                    case Tile.Mirror:
                        mirrors[cell] = Instantiate(mirrorPrefab, pos, Quaternion.identity, tiles).GetComponent<MirrorView>();
                        break;
                    case Tile.Hinge:
                        mirrors[cell] = Instantiate(hingePrefab, pos, Quaternion.identity, tiles).GetComponent<MirrorView>();
                        break;
                    case Tile.Lamp:
                        var lamp = Instantiate(lampPrefab, pos, Quaternion.identity, tiles).GetComponent<LampView>();
                        lamp.SetColour(lampColours[b.ColourAt(cell)]);
                        lamps[cell] = lamp;
                        break;
                    case Tile.Gate:
                        var gate = Instantiate(gatePrefab, pos, Quaternion.identity, tiles).GetComponent<GateView>();
                        gate.SetColour(lampColours[b.ColourAt(cell)]);
                        gates[cell] = gate;
                        break;
                    case Tile.Mailbox:
                        mailbox = AddProp("Returns mailbox", mailboxDark, pos);
                        break;
                }

                if (mark == '?') AddProp("Torn delivery slip", slipSprite, pos);
                else if (mark == '^') AddProp("Scratched arrow", scratchSprite, pos);
            }
        }
        Refresh();
    }

    SpriteRenderer AddProp(string label, Sprite sprite, Vector3 position)
    {
        var prop = new GameObject(label);
        prop.transform.SetParent(tiles);
        prop.transform.position = position;
        var renderer = prop.AddComponent<SpriteRenderer>();
        renderer.sprite = sprite;
        renderer.sortingOrder = 2;
        return renderer;
    }

    bool StandsInWater(Vector2Int cell, Tile tile)
    {
        if (tile == Tile.Floor) return false;
        bool water = false, ground = false;
        foreach (Vector2Int d in Board.Dirs)
        {
            Tile n = board.TileAt(cell + d);
            if (n == Tile.Water) water = true;
            if (n == Tile.Floor) ground = true;
        }
        if (tile == Tile.Gate && ground) return false;
        return water;
    }

    public void Refresh()
    {
        StopAllCoroutines();
        ClearBeam();
        pulse.gameObject.SetActive(false);
        sparks.Clear();
        if (mailbox != null) mailbox.sprite = board.state.letterFound ? mailboxOpen : mailboxDark;
        foreach (var pair in mirrors) pair.Value.Set(board.IsSlash(pair.Key));
        foreach (var pair in lamps) pair.Value.SetLit(board.IsLit(board.ColourAt(pair.Key)), false);
        foreach (var pair in gates) pair.Value.SetOpen(board.IsLit(board.ColourAt(pair.Key)), false);
        RefreshRoomLight();
    }

    void RefreshRoomLight()
    {
        int lit = 0;
        foreach (var cell in lamps.Keys)
            if (board.IsLit(board.ColourAt(cell))) lit++;

        float brightness = lamps.Count > 0 ? (float)lit / lamps.Count : 0;
        Color tint = Color.Lerp(new Color(0.76f, 0.79f, 0.94f), new Color(1f, 0.96f, 0.76f), brightness);
        foreach (var surface in surfaces) surface.color = tint;
        foreach (var window in windows) window.sprite = board.Solved ? windowLit : windowDark;
    }

    public IEnumerator PlayShot(Shot shot)
    {
        ClearBeam();
        List<Vector2Int> path = shot.path;
        int last = path.Count - 1;
        flippedThisShot = 0;
        lampsThisShot = 0;

        pulse.gameObject.SetActive(true);
        SetPulseColour(outColour);
        pulse.transform.position = World(path[0]);

        for (int i = 1; i <= last; i++)
        {
            yield return Glide(World(path[i - 1]), World(path[i]), 1f / outSpeed);
            AddSegment(path[i - 1], path[i]);
            Tile t = board.TileAt(path[i]);
            if (t == Tile.Mirror || t == Tile.Hinge)
            {
                Sfx.Play(Sfx.I.tick, 1f + i * 0.015f);
                Burst(World(path[i]), outColour, 3);
            }
        }

        Vector3 end = World(path[last]);
        if (shot.Charged)
        {
            bells[path[last]].Ring();
            Sfx.Play(Sfx.I.bell);
            shake.Shake(0.14f, 0.25f);
            Burst(end, chargedColour, 16);
            Recolour(chargedColour);
            yield return new WaitForSeconds(0.22f);
        }
        else
        {
            Vector3 nudge = Vector3.zero;
            if (shot.stop != StopKind.Loop)
            {
                Vector2Int d = Board.Dirs[shot.endDir];
                nudge = new Vector3(d.x, -d.y, 0) * 0.4f;
                yield return Glide(end, end + nudge, 0.4f / outSpeed);
            }
            Sfx.Play(Sfx.I.thud);
            shake.Shake(0.07f, 0.12f);
            Burst(end + nudge, deadColour, 8);
            Recolour(deadColour);
            if (shot.stop != StopKind.Loop) yield return Glide(end + nudge, end, 0.4f / backSpeed);
            yield return new WaitForSeconds(0.1f);
        }

        for (int i = last; i >= 1; i--)
        {
            Touch(path[i], shot.Charged);
            segments[i - 1].enabled = false;
            yield return Glide(World(path[i]), World(path[i - 1]), 1f / backSpeed);
        }

        Burst(World(path[0]), shot.Charged ? chargedColour : deadColour, 6);
        pulse.gameObject.SetActive(false);
        ClearBeam();
    }

    void Touch(Vector2Int cell, bool charged)
    {
        int h = board.HingeAt(cell);
        if (h >= 0 && (flippedThisShot & (1 << h)) == 0)
        {
            flippedThisShot |= 1 << h;
            board.state.flips ^= 1 << h;
            mirrors[cell].FlipTo(board.IsSlash(cell));
            Sfx.Play(Sfx.I.flip);
            Burst(World(cell), new Color(1f, 0.72f, 0.29f), 5);
        }

        if (!charged) return;
        Tile tile = board.TileAt(cell);
        if (tile == Tile.Mailbox && !board.state.letterFound)
        {
            board.state.letterFound = true;
            mailbox.sprite = mailboxOpen;
            Sfx.Play(Sfx.I.lamp, 1.45f);
            Burst(World(cell), chargedColour, 18);
        }
        if (tile != Tile.Lamp) return;
        int colour = board.ColourAt(cell);
        if (board.IsLit(colour)) return;

        board.state.lit |= 1 << colour;
        lamps[cell].SetLit(true, true);
        RefreshRoomLight();
        Sfx.Play(Sfx.I.lamp, 1f + 0.12f * lampsThisShot);
        lampsThisShot++;
        Burst(World(cell), lampColours[colour], 12);
        shake.Shake(0.05f, 0.1f);

        foreach (var pair in gates)
        {
            if (board.ColourAt(pair.Key) != colour) continue;
            pair.Value.SetOpen(true, true);
            Sfx.Play(Sfx.I.gate);
            Burst(World(pair.Key), lampColours[colour], 8);
        }
    }

    public void Celebrate()
    {
        foreach (var pair in lamps)
        {
            pair.Value.SetLit(true, true);
            Burst(World(pair.Key), lampColours[board.ColourAt(pair.Key)], 10);
        }
    }

    IEnumerator Glide(Vector3 from, Vector3 to, float time)
    {
        for (float t = 0; t < time; t += Time.deltaTime)
        {
            pulse.transform.position = Vector3.Lerp(from, to, t / time);
            yield return null;
        }
        pulse.transform.position = to;
    }

    void SetPulseColour(Color c)
    {
        pulse.color = Color.Lerp(c, Color.white, 0.5f);
        pulseGlow.color = new Color(c.r, c.g, c.b, 0.45f);
    }

    void AddSegment(Vector2Int a, Vector2Int b)
    {
        Vector3 mid = (World(a) + World(b)) / 2f;
        Quaternion rot = a.x == b.x ? Quaternion.Euler(0, 0, 90) : Quaternion.identity;
        var seg = Instantiate(beamPrefab, mid, rot, beams).GetComponent<SpriteRenderer>();
        seg.color = outColour;
        segments.Add(seg);
    }

    void Recolour(Color c)
    {
        foreach (SpriteRenderer seg in segments) seg.color = c;
        SetPulseColour(c);
    }

    void ClearBeam()
    {
        foreach (SpriteRenderer seg in segments)
            if (seg != null) Destroy(seg.gameObject);
        segments.Clear();
    }

    void Burst(Vector3 at, Color c, int count)
    {
        var emit = new ParticleSystem.EmitParams();
        emit.position = at;
        emit.startColor = c;
        emit.applyShapeToPosition = true;
        sparks.Emit(emit, count);
    }
}
