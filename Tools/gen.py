"""Random search for interesting boards. Prints the best candidates."""
import random, sys
from sim import Level

def make(rng, w, h, n_h, n_m, n_l, n_g, n_b, n_water, n_wall):
    g = [['#'] * w] + [['#'] + ['.'] * (w - 2) + ['#'] for _ in range(h - 2)] + [['#'] * w]
    cells = [(x, y) for y in range(1, h - 1) for x in range(1, w - 1)]
    rng.shuffle(cells)
    def put(c):
        x, y = cells.pop(); g[y][x] = c
    put('P')
    for _ in range(n_b): put('O')
    for i in range(n_l): put('abcd'[i])
    for i in range(n_g): put('ABCD'[i])
    for _ in range(n_h): put(rng.choice('<>'))
    for _ in range(n_m): put(rng.choice(['/', chr(92)]))
    for _ in range(n_water): put('~')
    for _ in range(n_wall): put('#')
    return [''.join(r) for r in g]

def score(rows):
    lv = Level(rows)
    sol, n = lv.solve(limit=20000)
    if not sol: return 0, None
    stands = len({c for c, d, s in sol})
    blanks = sum(1 for c, d, s in sol if s != 'bell')
    return len(sol) * 10 + stands * 3 + blanks * 4, sol

if __name__ == '__main__':
    seed, tries = int(sys.argv[1]), int(sys.argv[2])
    w, h = int(sys.argv[3]), int(sys.argv[4])
    params = [int(v) for v in sys.argv[5:12]]
    rng = random.Random(seed)
    best = []
    for i in range(tries):
        rows = make(rng, w, h, *params)
        s, sol = score(rows)
        if s: best.append((s, rows, sol))
        best = sorted(best, key=lambda b: -b[0])[:4]
    for s, rows, sol in best:
        print('score', s, 'par', len(sol))
        print('\n'.join(rows))
        for m in sol: print('   ', m)
