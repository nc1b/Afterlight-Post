"""Pixel art for Afterlight Post. Run: python gen_art.py <Assets dir>"""
import sys, os, random, math
from PIL import Image, ImageDraw

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
    base = hexc('6b5961' if seed == 1 else '67565e')
    seam, edge = hexc('51434e'), hexc('77656a')
    im = Image.new('RGBA', (16, 16), base)
    draw = ImageDraw.Draw(im)
    draw.line((0, 7, 15, 7), seam)
    draw.line((0, 15, 15, 15), seam)
    split = 10 if seed == 1 else 3
    draw.line((split, 0, split, 6), seam)
    draw.line((15 - split, 8, 15 - split, 14), seam)
    draw.line((1, 8, 4, 8), edge)
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
    im = Image.new('RGBA', (16, 16), hexc('47465a'))
    draw = ImageDraw.Draw(im)
    draw.line((0, 0, 15, 0), hexc('626176'))
    draw.polygon([(0, 8), (4, 8), (4, 9), (12, 9), (12, 8), (15, 8),
                  (15, 15), (0, 15)], hexc('302e43'))
    joint = 5 if seed == 3 else 11
    draw.line((joint, 10, joint, 14), hexc('454457'))
    draw.line((1, 4, 3, 4) if seed == 3 else (9, 5, 12, 5), hexc('525166'))
    return im

save(floor(1), 'Tiles', 'floor_a.png'); save(floor(2), 'Tiles', 'floor_b.png')
save(water(0), 'Tiles', 'water_0.png'); save(water(1), 'Tiles', 'water_1.png')
save(wall(3), 'Tiles', 'wall_a.png'); save(wall(8), 'Tiles', 'wall_b.png')

def counter(parcels):
    im = Image.new('RGBA', (16, 16), hexc('4d3d45'))
    draw = ImageDraw.Draw(im)
    draw.line((0, 1, 13, 1), hexc('695051'))
    draw.line((0, 15, 15, 15), hexc('312735'))
    draw.line((15, 2, 15, 15), hexc('382b37'))
    if parcels:
        draw.polygon([(1, 7), (2, 6), (8, 6), (8, 11), (1, 11)], fill=K)
        draw.rectangle((2, 7, 7, 10), fill=hexc('a07861'))
        draw.line((4, 7, 4, 10), hexc('d1af82'))
        draw.polygon([(8, 4), (13, 3), (14, 4), (14, 12), (8, 12)], fill=K)
        draw.rectangle((9, 5, 13, 11), fill=hexc('7f6255'))
        draw.line((10, 5, 10, 10), hexc('c39870'))
        draw.point((12, 8), hexc('d6bda0'))
    return im

def office_window(lit):
    im = Image.new('RGBA', (16, 16))
    draw = ImageDraw.Draw(im)
    draw.polygon([(2, 2), (12, 1), (13, 2), (13, 11), (2, 11)], fill=K)
    glass = hexc('ffcf5c' if lit else '292f46')
    draw.rectangle((3, 3, 12, 10), fill=glass)
    draw.line((7, 3, 7, 10), K)
    draw.line((3, 7, 12, 7), K)
    draw.line((2, 11, 13, 11), hexc('817065'))
    return im

save(counter(False), 'Tiles', 'counter.png')
save(counter(True), 'Tiles', 'parcels.png')
save(office_window(False), 'Tiles', 'window_dark.png')
save(office_window(True), 'Tiles', 'window_lit.png')

def mailbox(lit):
    im = Image.new('RGBA', (16, 16))
    draw = ImageDraw.Draw(im)
    draw.polygon([(2, 6), (4, 4), (6, 3), (12, 3), (14, 6), (14, 11), (2, 11)], fill=K)
    draw.polygon([(3, 6), (5, 4), (12, 4), (13, 6), (13, 10), (3, 10)],
                 fill=hexc('b88055' if lit else '5e6f73'))
    draw.line((5, 6, 11, 6), K)
    draw.line((10, 2, 10, 5), hexc('fa9e66' if lit else '886356'))
    draw.rectangle((10, 2, 12, 3), fill=hexc('fa9e66' if lit else '886356'))
    draw.line((7, 12, 7, 14), hexc('403441'), width=2)
    if lit:
        draw.polygon([(0, 4), (7, 4), (7, 9), (1, 10), (0, 9)], fill=PAL['L'], outline=K)
        draw.line([(1, 5), (3, 7), (6, 5)], hexc('a46a41'))
    return im

save(mailbox(False), 'Tiles', 'mailbox_dark.png')
save(mailbox(True), 'Tiles', 'mailbox_open.png')

slip = Image.new('RGBA', (16, 16))
draw = ImageDraw.Draw(slip)
draw.polygon([(10, 4), (14, 5), (14, 10), (12, 9), (11, 11), (9, 10)],
             fill=hexc('bfa98b'), outline=K)
draw.line((11, 6, 13, 6), hexc('665462'))
draw.point((11, 8), hexc('665462'))
save(slip, 'Tiles', 'return_slip.png')

scratch = Image.new('RGBA', (16, 16))
draw = ImageDraw.Draw(scratch)
draw.line([(3, 8), (7, 4), (10, 7)], hexc('b49d86'))
draw.line([(7, 5), (7, 8), (6, 11), (6, 13)], hexc('b49d86'))
draw.point((11, 9), hexc('6e6262'))
save(scratch, 'Tiles', 'return_arrow.png')

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
    sky = hexc('292c43' if lit else '171c31')
    ground = hexc('151827')
    sea = hexc('263b4e' if lit else '1c2a3e')
    warm = hexc('ffcf73')
    im = Image.new('RGBA', (320, 180), sky)
    draw = ImageDraw.Draw(im)
    draw.ellipse((283, 12, 301, 30), fill=hexc('e0c89a'))
    draw.ellipse((288, 9, 305, 25), fill=sky)
    for x, y in [(268, 18), (305, 45), (247, 41), (116, 78), (156, 68)]:
        draw.point((x, y), fill=hexc('c3bac0'))
    draw.rectangle((0, 129, 319, 179), fill=sea)
    draw.polygon([(145, 132), (178, 119), (223, 118), (265, 101), (319, 102),
                  (319, 142), (146, 142)], fill=ground)
    draw.rectangle((0, 141, 226, 145), fill=ground)
    for x in range(5, 180, 27):
        draw.line((x, 144, x, 153 - (x % 3)), ground, width=2)
    for x, y, length in [(7, 159, 25), (48, 151, 12), (87, 162, 30), (118, 152, 14),
                         (159, 161, 18), (207, 154, 22), (269, 163, 38)]:
        draw.line((x, y, x + length, y), hexc('374c5b'))
    for x, y in [(285, 91), (304, 85)]:
        draw.rectangle((x, y, x + 13, y + 21), fill=hexc('32364b'))
        draw.polygon([(x - 2, y), (x + 6, y - 7), (x + 15, y)], fill=ground)
        draw.rectangle((x + 4, y + 4, x + 6, y + 8), fill=warm if lit else hexc('242536'))

    draw.polygon([(190, 81), (274, 80), (275, 137), (189, 139)], fill=hexc('5f4a54'), outline=ground)
    draw.polygon([(181, 88), (198, 68), (258, 67), (282, 85), (283, 89), (181, 92)],
                 fill=hexc('39435a'), outline=ground)
    draw.line([(183, 90), (233, 89), (280, 86)], hexc('8b6260'), width=2)
    draw.line((198, 74, 231, 73), hexc('4d5c73'), width=2)
    draw.line((255, 59, 255, 69), ground, width=2)
    draw.polygon([(255, 59), (267, 60), (263, 65), (255, 64)], fill=hexc('a65957'))
    draw.rectangle((214, 93, 247, 105), fill=hexc('cfb58d'), outline=ground)
    text(im, 'POST', 219, 96, hexc('3e2930'))
    for x, y in [(198, 110), (251, 109)]:
        draw.rectangle((x - 1, y - 1, x + 13, y + 13), fill=ground)
        draw.rectangle((x, y, x + 12, y + 11), fill=warm if lit else hexc('303449'))
        draw.line((x + 6, y, x + 6, y + 11), ground)
        draw.line((x, y + 6, x + 12, y + 6), ground)
    draw.rectangle((219, 111, 241, 137), fill=ground)
    if lit:
        draw.rectangle((222, 113, 237, 137), fill=hexc('e9aa66'))
        draw.polygon([(223, 138), (236, 137), (246, 146), (214, 148)], fill=hexc('bb805e'))
        draw.line((197, 148, 207, 148), warm)
        draw.line((251, 152, 263, 152), warm)
    else:
        draw.polygon([(223, 113), (238, 114), (238, 136), (223, 136)], fill=hexc('353b52'))
        draw.point((235, 125), fill=hexc('b39677'))
    for x, y in [(178, 131), (182, 135), (273, 133)]:
        draw.rectangle((x, y, x + 7, y + 5), fill=hexc('91705b'), outline=ground)
        draw.point((x + 3, y + 1), fill=hexc('e5c3a0'))
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
