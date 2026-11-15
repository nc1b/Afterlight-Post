"""Pixel art for Afterlight Post. Run: python gen_art.py <Assets dir>"""
import sys, os, random, math
from PIL import Image

OUT = sys.argv[1]
S = os.path.join(OUT, 'Sprites')

def hexc(h, a=255):
    h = h.lstrip('#')
    return (int(h[0:2], 16), int(h[2:4], 16), int(h[4:6], 16), a)

K = hexc('15121f')
NAVY = hexc('0e1626')
PAL = {
    '.': (0, 0, 0, 0), 'K': K,
    'G': hexc('e8ad45'), 'L': hexc('ffe391'), 'D': hexc('a56a2a'),
    'W': hexc('ffffff'), 'w': hexc('c9d1dd'), 'I': hexc('59637a'), 'i': hexc('3b4357'),
    'B': hexc('3d6fc2'), 'b': hexc('2c4f94'), 'Y': hexc('ffd35a'), 'S': hexc('f1c39b'), 's': hexc('c98f6c'),
    'C': hexc('31508f'), 'c': hexc('243a6b'), 'R': hexc('c2473f'), 'N': hexc('3a2a2a'), 'A': hexc('ffcf5c'),
    'H': hexc('6b4a35'), 'M': hexc('b9d3e6'), 'm': hexc('7f9bb3'), 'X': hexc('eaf6ff'),
    'O': hexc('ffb84a'), 'o': hexc('5a3518'), 'P': hexc('e9dfc8'), 'p': hexc('b8ab8d'), 'E': hexc('8a8f9c'),
}

def grid(rows, pal=PAL):
    h, w = len(rows), len(rows[0])
    im = Image.new('RGBA', (w, h), (0, 0, 0, 0))
    for y, r in enumerate(rows):
        assert len(r) == w, (r, len(r), w)
        for x, c in enumerate(r):
            im.putpixel((x, y), pal[c])
    return im

def save(im, *path):
    p = os.path.join(S, *path)
    os.makedirs(os.path.dirname(p), exist_ok=True)
    im.save(p)
    return im

def floor(seed):
    rng = random.Random(seed)
    base, line, hi, lo = hexc('39465a'), hexc('2c3648'), hexc('445369'), hexc('333f52')
    im = Image.new('RGBA', (16, 16), base)
    for y in (7, 15):
        for x in range(16): im.putpixel((x, y), line)
    for x, ys in ((5, range(0, 7)), (13, range(0, 7)), (1, range(8, 15)), (9, range(8, 15))):
        for y in ys: im.putpixel((x, y), line)
    for _ in range(7):
        x, y = rng.randrange(16), rng.randrange(16)
        if im.getpixel((x, y)) == base: im.putpixel((x, y), rng.choice((hi, lo)))
    for x in range(16):
        if im.getpixel((x, 0)) == base and rng.random() < .5: im.putpixel((x, 0), hi)
        if im.getpixel((x, 8)) == base and rng.random() < .5: im.putpixel((x, 8), hi)
    return im

def water(frame):
    base, hi, deep = hexc('13304b'), hexc('21506f'), hexc('102941')
    im = Image.new('RGBA', (16, 16), base)
    marks = [(2, 3, 4), (10, 6, 3), (5, 11, 4), (12, 14, 2), (0, 8, 2)]
    for x, y, n in marks:
        for i in range(n):
            im.putpixel(((x + i + frame * 2) % 16, y), hi)
        im.putpixel(((x + 1 + frame * 2) % 16, y + 1), deep)
    return im

def wall(seed):
    rng = random.Random(seed)
    top, hi, front, mortar = hexc('27314a'), hexc('3a4766'), hexc('181f31'), hexc('1d2539')
    im = Image.new('RGBA', (16, 16), top)
    for x in range(16): im.putpixel((x, 0), hi)
    for y in range(11, 16):
        for x in range(16): im.putpixel((x, y), front)
    for x in range(16): im.putpixel((x, 10), mortar)
    for x in (3, 11):
        for y in range(11, 13): im.putpixel((x, y), mortar)
    for x in (7, 15):
        for y in range(14, 16): im.putpixel((x, y), mortar)
    for x in range(16): im.putpixel((x, 13), mortar)
    for _ in range(4):
        im.putpixel((rng.randrange(16), rng.randrange(2, 9)), hi if rng.random() < .5 else mortar)
    return im

save(floor(1), 'Tiles', 'floor_a.png'); save(floor(2), 'Tiles', 'floor_b.png')
save(water(0), 'Tiles', 'water_0.png'); save(water(1), 'Tiles', 'water_1.png')
save(wall(3), 'Tiles', 'wall_a.png'); save(wall(8), 'Tiles', 'wall_b.png')

save(grid([
    "................",
    ".......KK.......",
    "......KLLK......",
    ".....KLGGGK.....",
    "....KLGGGGDK....",
    "....KLGGGGDK....",
    "....KLGGGGDK....",
    "...KLLGGGGGDK...",
    "...KLGGGGGGDK...",
    "..KLLGGGGGGGDK..",
    "..KGGGGGGGGDDK..",
    "..KKKKKKKKKKKK..",
    "......KDDK......",
    "......KGGK......",
    ".......KK.......",
    "................"]), 'Tiles', 'bell.png')

save(grid([
    "................",
    "................",
    "......KKKK......",
    ".....K....K.....",
    "....K......K....",
    "....K......K....",
    "....K......K....",
    ".....K....K.....",
    "......KKKK......",
    ".......KI.......",
    ".......KI.......",
    ".......KI.......",
    ".......KI.......",
    "......KIIK......",
    ".....KIiiIK.....",
    ".....KKKKKK....."]), 'Tiles', 'lamp_post.png')
save(grid([
    "................",
    "................",
    "................",
    "......wWWw......",
    ".....wWWWWw.....",
    ".....WWWWWW.....",
    ".....wWWWWw.....",
    "......wwww......",
    "................",
    "................",
    "................",
    "................",
    "................",
    "................",
    "................",
    "................"]), 'Tiles', 'lamp_glass.png')

save(grid([
    "................",
    ".KKKKKKKKKKKKKK.",
    ".KWWKWWKKWWKWWK.",
    ".KWwKWwKKWwKWwK.",
    ".KWwKWwKKWwKWwK.",
    ".KKKKKKKKKKKKKK.",
    ".KWwKWwKKWwKWwK.",
    ".KWwKWwKKWwKWwK.",
    ".KWwKWwKKWwKWwK.",
    ".KWwKWwKKWwKWwK.",
    ".KKKKKKKKKKKKKK.",
    ".KWwKWwKKWwKWwK.",
    ".KWwKWwKKWwKWwK.",
    ".KwwKwwKKwwKwwK.",
    ".KKKKKKKKKKKKKK.",
    "................"]), 'Tiles', 'gate_closed.png')
save(grid([
    "................",
    "................",
    "................",
    "................",
    "................",
    "................",
    "................",
    "................",
    "................",
    "................",
    ".KKK........KKK.",
    ".KWK........KWK.",
    ".KWK........KWK.",
    ".KwK.KK..KK.KwK.",
    ".KKKKKKKKKKKKKK.",
    "................"]), 'Tiles', 'gate_open.png')

def mirror(hinged):
    im = Image.new('RGBA', (16, 16), (0, 0, 0, 0))
    for i in range(2, 14):
        x, y = i, 15 - i
        for dx, dy in ((0, 0), (1, 0), (0, 1), (-1, 0), (0, -1), (1, 1), (-1, -1), (1, -1), (-1, 1)):
            px, py = x + dx, y + dy
            if 0 <= px < 16 and 0 <= py < 16 and im.getpixel((px, py))[3] == 0:
                im.putpixel((px, py), K)
    for i in range(2, 14):
        x, y = i, 15 - i
        if hinged:
            c = PAL['O'] if (i // 2) % 2 == 0 else PAL['o']
            im.putpixel((x, y), c)
            if x + 1 < 15: im.putpixel((x + 1, y), c)
        else:
            im.putpixel((x, y), PAL['X'])
            if x + 1 < 15: im.putpixel((x + 1, y), PAL['M'])
            im.putpixel((x, y + 1), PAL['m']) if y + 1 < 14 and i > 2 else None
    if hinged:
        for px, py in ((7, 7), (8, 7), (7, 8), (8, 8)): im.putpixel((px, py), PAL['W'])
        for px, py in ((6, 7), (6, 8), (9, 7), (9, 8), (7, 6), (8, 6), (7, 9), (8, 9)): im.putpixel((px, py), K)
    return im

save(mirror(False), 'Tiles', 'mirror.png')
save(mirror(True), 'Tiles', 'mirror_hinge.png')

front = [
    "................",
    ".....KKKKKK.....",
    "....KBBBBBBK....",
    "....KBBYYBBK....",
    "...KKbbbbbbKK...",
    "....KSSSSSSK....",
    "....KSKSSKSK....",
    "....KSSSSSsK....",
    ".....KKssKK.....",
    "....KCCRCCCK....",
    "...KCCCCRCCCK...",
    "...KSKCCCRKSKA..",
    "....KKCCCCKKAAA.",
    ".....KccccK.KAK.",
    ".....KNKKNK.....",
    ".....KK..KK....."]
back = [
    "................",
    ".....KKKKKK.....",
    "....KBBBBBBK....",
    "....KBBBBBBK....",
    "....KbbbbbbK....",
    "....KHHHHHHK....",
    "....KHHHHHHK....",
    "....KSHHHHSK....",
    ".....KKssKK.....",
    "....KCCCRCCK....",
    "...KCCCRCCCCK...",
    "...KSKRCCCKSK...",
    "....KKPPPCKK....",
    ".....KPPPcK.....",
    ".....KNKKNK.....",
    ".....KK..KK....."]
save(grid(front), 'Tiles', 'postie_front.png')
save(grid(back), 'Tiles', 'postie_back.png')
step = list(front); step[14] = ".....KNK.KK....."; step[15] = ".....KK........."
save(grid(step), 'Tiles', 'postie_front_step.png')
stepb = list(back); stepb[14] = ".....KK.KNK....."; stepb[15] = ".........KK....."
save(grid(stepb), 'Tiles', 'postie_back_step.png')

def radial(size, power, alpha):
    im = Image.new('RGBA', (size, size), (0, 0, 0, 0))
    c = (size - 1) / 2
    for y in range(size):
        for x in range(size):
            d = math.hypot(x - c, y - c) / c
            a = max(0.0, 1 - d) ** power
            a = math.floor(a * 5) / 5
            im.putpixel((x, y), (255, 255, 255, int(255 * alpha * a)))
    return im

save(radial(48, 1.4, .55), 'Tiles', 'glow.png')
save(grid([
    "..WWWW..",
    ".WWWWWW.",
    "WWWWWWWW",
    "WWWWWWWW",
    "WWWWWWWW",
    "WWWWWWWW",
    ".WWWWWW.",
    "..WWWW.."]), 'Tiles', 'pulse.png')
beam = Image.new('RGBA', (16, 4), (255, 255, 255, 90))
for x in range(16):
    beam.putpixel((x, 1), (255, 255, 255, 255)); beam.putpixel((x, 2), (255, 255, 255, 255))
save(beam, 'Tiles', 'beam.png')
save(grid(["WW", "WW"]), 'Tiles', 'spark.png')
shadow = Image.new('RGBA', (16, 16), (0, 0, 0, 0))
for y in range(12, 16):
    for x in range(3, 13):
        if (x in (3, 12)) and y in (12, 15): continue
        shadow.putpixel((x, y), (0, 0, 0, 90))
save(shadow, 'Tiles', 'shadow.png')

F = {
 'A': ".###.#...##...#######...##...##...#", 'B': "####.#...##...#####.#...##...#####.",
 'C': ".###.#...##....#....#....#...#.###.", 'D': "####.#...##...##...##...##...#####.",
 'E': "######....#....####.#....#....#####", 'F': "######....#....####.#....#....#....",
 'G': ".###.#...##....#.####...##...#.###.", 'H': "#...##...##...#######...##...##...#",
 'I': ".###...#....#....#....#....#...###.", 'J': "..###...#....#....#....#.#..#..##..",
 'K': "#...##..#.#.#..##...#.#..#..#.#...#", 'L': "#....#....#....#....#....#....#####",
 'M': "#...###.###.#.##.#.##...##...##...#", 'N': "#...###..##.#.##..###...##...##...#",
 'O': ".###.#...##...##...##...##...#.###.", 'P': "####.#...##...#####.#....#....#....",
 'Q': ".###.#...##...##...##.#.##..#..##.#", 'R': "####.#...##...#####.#.#..#..#.#...#",
 'S': ".#####....#.....###.....#....#####.", 'T': "#####..#....#....#....#....#....#..",
 'U': "#...##...##...##...##...##...#.###.", 'V': "#...##...##...##...##...#.#.#...#..",
 'W': "#...##...##...##.#.##.#.###.###...#", 'X': "#...##...#.#.#...#...#.#.#...##...#",
 'Y': "#...##...#.#.#...#....#....#....#..", 'Z': "#####....#...#...#...#...#....#####",
 '0': ".###.#...##..###.#.###..##...#.###.", '1': "..#...##....#....#....#....#...###.",
 '2': ".###.#...#....#...#...#...#...#####", '3': "####.....#....#.###.....#....#####.",
 '4': "...#...##..#.#.#..#.#####...#....#.", '5': "######....####.....#....##...#.###.",
 '6': ".###.#....#....####.#...##...#.###.", '7': "#####....#...#...#....#....#....#..",
 '8': ".###.#...##...#.###.#...##...#.###.", '9': ".###.#...##...#.####....#....#.###.",
 ' ': "." * 35, '.': "." * 25 + ".##...##..", ',': "." * 25 + "..#...#...",
 '!': "..#....#....#....#....#.........#..", '?': ".###.#...#....#...#...#.........#..",
 "'": "..#....#...#......................."[:35], ':': ".....##...##........##...##........"[:35],
 '-': "." * 15 + ".###." + "." * 15, '/': "....#....#...#...#...#...#....#....",
 '+': "." * 5 + "..#....#..#####..#....#.." + "." * 5, '(': "...#...#...#....#....#.....#.....#.",
 ')': ".#.....#.....#....#....#...#...#...", '*': "." * 5 + "#.#.#.###.#####.###.#.#.#" + "." * 5,
 '<': "...#...#...#...#.....#.....#.....#.", '>': ".#.....#.....#.....#...#...#...#...",
 '=': "." * 10 + "#####" + "." * 5 + "#####" + "." * 10, '"': ".#.#..#.#.#.#.#" + "." * 20,
 '#': ".#.#..#.#.#####.#.#.#####.#.#..#.#.", '%': "##..###..#...#...#...#...#..###..##",
 '&': ".##..#..#.#.#...#...#.#.##..#..##.#", '_': "." * 30 + "#####",
 '[': ".###..#....#....#....#....#....###.", ']': ".###....#....#....#....#....#..###.",
}
for k, v in F.items():
    assert len(v) == 35, (k, len(v))
font = Image.new('RGBA', (96, 36), (0, 0, 0, 0))
for code in range(32, 96):
    ch = chr(code)
    g = F.get(ch)
    if not g: continue
    cx, cy = (code - 32) % 16 * 6, (code - 32) // 16 * 9
    for i, c in enumerate(g):
        if c == '#': font.putpixel((cx + i % 5, cy + 1 + i // 5), (255, 255, 255, 255))
os.makedirs(os.path.join(OUT, 'Fonts'), exist_ok=True)
font.save(os.path.join(OUT, 'Fonts', 'postfont.png'))

def text(im, s, x, y, col, scale=1, shadow=None):
    for i, ch in enumerate(s.upper()):
        g = F.get(ch, F['?'])
        for j, c in enumerate(g):
            if c != '#': continue
            px, py = x + (i * 6 + j % 5) * scale, y + (j // 5) * scale
            for dx in range(scale):
                for dy in range(scale):
                    if shadow:
                        sx, sy = px + dx + scale, py + dy + scale
                        if 0 <= sx < im.width and 0 <= sy < im.height and im.getpixel((sx, sy)) != col: im.putpixel((sx, sy), shadow)
                    if 0 <= px + dx < im.width and 0 <= py + dy < im.height: im.putpixel((px + dx, py + dy), col)

save(grid([
    ".KKKKK.",
    ".KLLLK.",
    "KKKKKKK",
    "KYYYYYK",
    "KYLYYYK",
    "KYLYYYK",
    "KYYYYDK",
    "KDDDDDK",
    "KKKKKKK"]), 'UI', 'pip_full.png')
save(grid([
    ".iiiii.",
    ".i...i.",
    "iiiiiii",
    "i.....i",
    "i.....i",
    "i.....i",
    "i.....i",
    "i.....i",
    "iiiiiii"]), 'UI', 'pip_empty.png')

def panel():
    im = Image.new('RGBA', (16, 16), hexc('101a2e', 235))
    edge, hi = hexc('e9dfc8'), hexc('2a3a5c')
    for i in range(16):
        for p in ((i, 0), (i, 15), (0, i), (15, i)): im.putpixel(p, (0, 0, 0, 0))
    for i in range(2, 14):
        for p in ((i, 1), (i, 14), (1, i), (14, i)): im.putpixel(p, edge)
    for p in ((1, 1), (14, 1), (1, 14), (14, 14)): im.putpixel(p, (0, 0, 0, 0))
    for i in range(3, 13): im.putpixel((i, 13), hi) if False else None
    return im
save(panel(), 'UI', 'panel.png')

def stamp():
    im = Image.new('RGBA', (26, 26), (0, 0, 0, 0))
    for y in range(1, 25):
        for x in range(1, 25): im.putpixel((x, y), (255, 255, 255, 255))
    for i in range(2, 25, 4):
        for p in ((i, 1), (i, 24), (1, i), (24, i)): im.putpixel(p, (0, 0, 0, 0))
    for i in range(4, 22):
        for p in ((i, 4), (i, 21), (4, i), (21, i)): im.putpixel(p, (200, 200, 200, 255))
    return im
save(stamp(), 'UI', 'stamp.png')

def cursor():
    im = Image.new('RGBA', (32, 32), (0, 0, 0, 0))
    for i in range(0, 9):
        for a, b in ((i, 0), (0, i), (31 - i, 0), (31, i), (i, 31), (0, 31 - i), (31 - i, 31), (31, 31 - i)):
            im.putpixel((a, b), (255, 255, 255, 255))
        for a, b in ((i, 1), (1, i), (31 - i, 1), (30, i), (i, 30), (1, 31 - i), (31 - i, 30), (30, 31 - i)):
            im.putpixel((a, b), (255, 255, 255, 255))
    return im
save(cursor(), 'UI', 'cursor.png')
save(grid([
    "..KKK..",
    ".K...K.",
    ".K...K.",
    "KKKKKKK",
    "KKK.KKK",
    "KKK.KKK",
    "KKKKKKK"], {'.': (0, 0, 0, 0), 'K': (255, 255, 255, 255)}), 'UI', 'lock.png')
save(Image.new('RGBA', (4, 4), (255, 255, 255, 255)), 'UI', 'white.png')

def lerp(a, b, t): return tuple(int(a[i] + (b[i] - a[i]) * t) for i in range(4))

def sea_bg():
    rng = random.Random(11)
    im = Image.new('RGBA', (320, 180), hexc('0b1422'))
    for _ in range(170):
        x, y, n = rng.randrange(320), rng.randrange(180), rng.randrange(2, 6)
        d = math.hypot((x - 160) / 160, (y - 90) / 90)
        col = lerp(hexc('13243a'), hexc('0b1422'), min(1, d * .8))
        for i in range(n):
            if x + i < 320: im.putpixel((x + i, y), col)
    return im
save(sea_bg(), 'Backgrounds', 'sea.png')

def town(lit):
    rng = random.Random(47)
    im = Image.new('RGBA', (320, 180), (0, 0, 0, 255))
    top, mid = (hexc('090d1f'), hexc('1a2a4e')) if not lit else (hexc('0c1230'), hexc('2b3f73'))
    bands = 14
    for y in range(118):
        t = math.floor(y / 118 * bands) / bands
        for x in range(320): im.putpixel((x, y), lerp(top, mid, t))
    for _ in range(90):
        x, y = rng.randrange(320), rng.randrange(100)
        im.putpixel((x, y), hexc('e9dfc8') if rng.random() < .3 else hexc('7d8cb3'))
    for y in range(-13, 14):
        for x in range(-13, 14):
            if x * x + y * y <= 169 and (x - 5) ** 2 + (y + 3) ** 2 > 130:
                im.putpixel((296 + x, 24 + y), hexc('f3ecd2'))
    sea_a, sea_b = hexc('0d1d33'), hexc('132b47')
    for y in range(118, 180):
        for x in range(320): im.putpixel((x, y), sea_a)
    for _ in range(130):
        x, y, n = rng.randrange(320), rng.randrange(120, 180), rng.randrange(3, 12)
        for i in range(n):
            if x + i < 320: im.putpixel((x + i, y), sea_b)
    hill = hexc('0a1120')
    def ground(x): return int(118 - 54 * max(0, (x - 130) / 190) ** 1.5 - (6 if x > 250 else 0))
    for x in range(100, 320):
        for y in range(ground(x), 124): im.putpixel((x, y), hill)
    win_off, win_on = hexc('18233a'), hexc('ffcf5c')
    wall_c, roof_c = (hexc('121b2f'), hexc('0d1424')) if not lit else (hexc('1c2946'), hexc('131c33'))
    windows = []
    x = 138
    while x < 312:
        w, h = rng.randrange(12, 20), rng.randrange(10, 20)
        gy = ground(x + w // 2)
        for yy in range(gy - h, gy + 3):
            for xx in range(x, x + w): im.putpixel((xx, yy), wall_c)
        for i in range(w // 2 + 2):
            for xx in range(x - 1 + i, x + w + 1 - i):
                if 0 <= xx < 320: im.putpixel((xx, gy - h - 1 - i), roof_c)
        for wy in range(gy - h + 3, gy - 3, 6):
            for wx in range(x + 3, x + w - 3, 5):
                windows.append((wx, wy))
        x += w + rng.randrange(1, 5)
    for i, (wx, wy) in enumerate(windows):
        on = lit and rng.random() < .85
        for dx in range(2):
            for dy in range(3): im.putpixel((wx + dx, wy + dy), win_on if on else win_off)
        if on:
            for yy in range(126, 176, 3 + (i % 3)):
                if rng.random() < .25: im.putpixel((wx, yy), hexc('7a6a3d'))
    px, py = 112, 118
    for yy in range(py - 16, py + 2):
        for xx in range(px, px + 26): im.putpixel((xx, yy), hexc('1e2b49'))
    for i in range(8):
        for xx in range(px - 2 + i * 2, px + 28 - i * 2): im.putpixel((xx, py - 17 - i), hexc('141d35'))
    for xx in range(px + 4, px + 10):
        for yy in range(py - 11, py - 5): im.putpixel((xx, yy), win_on)
    for xx in range(px + 15, px + 21):
        for yy in range(py - 9, py + 2): im.putpixel((xx, yy), hexc('0d1424'))
    for yy in range(126, 170, 4): im.putpixel((px + 6 + (yy % 3) - 1, yy), hexc('a88f45'))
    for xx in range(0, 112):
        for yy in range(119, 122): im.putpixel((xx, yy), hexc('0a1120'))
    for xx in range(8, 110, 17):
        for yy in range(122, 132): im.putpixel((xx, yy), hexc('0a1120'))
    for i, xx in enumerate(range(20, 110, 30)):
        for yy in range(106, 119): im.putpixel((xx, yy), hexc('0a1120'))
        col = win_on if lit else hexc('27324d')
        for dx in (-1, 0, 1):
            for dy in (0, 1): im.putpixel((xx + dx, 104 + dy), col)
        if lit:
            for yy in range(124, 160, 5): im.putpixel((xx, yy), hexc('a88f45'))
    return im
save(town(False), 'Backgrounds', 'town_dark.png')
save(town(True), 'Backgrounds', 'town_lit.png')

def logo():
    im = Image.new('RGBA', (244, 74), (0, 0, 0, 0))
    text(im, "AFTERLIGHT", 4, 4, hexc('2a1c3a'), 4)
    text(im, "POST", 4, 42, hexc('2a1c3a'), 4)
    text(im, "AFTERLIGHT", 2, 2, hexc('f3ecd2'), 4)
    text(im, "POST", 2, 40, hexc('ffcf5c'), 4)
    return im
save(logo(), 'UI', 'logo.png')

def icon():
    im = Image.new('RGBA', (32, 32), hexc('14223b'))
    for i in range(32):
        for p in ((i, 0), (i, 31), (0, i), (31, i)): im.putpixel(p, hexc('e9dfc8'))
    g = radial(28, 1.2, .5)
    tint = Image.new('RGBA', (28, 28), hexc('ffcf5c'))
    tint.putalpha(g.split()[3])
    im.alpha_composite(tint, (2, 2))
    bell = Image.open(os.path.join(S, 'Tiles', 'bell.png'))
    im.alpha_composite(bell, (8, 8))
    return im.resize((256, 256), Image.NEAREST)
save(icon(), 'UI', 'icon.png')

sheet = Image.new('RGBA', (16 * 12 * 6, 16 * 6 * 2 + 36 * 4), hexc('13304b'))
names = ['floor_a', 'floor_b', 'water_0', 'wall_a', 'bell', 'lamp_post', 'gate_closed', 'gate_open', 'mirror', 'mirror_hinge', 'postie_front', 'postie_back']
for i, n in enumerate(names):
    t = Image.open(os.path.join(S, 'Tiles', n + '.png'))
    if n == 'lamp_post':
        t = t.copy(); t.alpha_composite(Image.open(os.path.join(S, 'Tiles', 'lamp_glass.png')))
    fl = Image.open(os.path.join(S, 'Tiles', 'floor_a.png')).copy(); fl.alpha_composite(t)
    sheet.alpha_composite(t.resize((96, 96), Image.NEAREST), (i * 96, 0))
    sheet.alpha_composite(fl.resize((96, 96), Image.NEAREST), (i * 96, 96))
sheet.alpha_composite(font.resize((96 * 4, 36 * 4), Image.NEAREST), (0, 192))
sheet.save(os.path.join(os.path.dirname(__file__), 'sheet.png'))
