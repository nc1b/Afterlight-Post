import sys, itertools
from sim import Level, load
meta, lv = load(sys.argv[1])
rows = lv.g
hs = [(x, y) for y, r in enumerate(rows) for x, c in enumerate(r) if c in '<>']
for combo in itertools.product('<>', repeat=len(hs)):
    g = [list(r) for r in rows]
    for (x, y), c in zip(hs, combo): g[y][x] = c
    sol, n = Level([''.join(r) for r in g]).solve()
    print(''.join(combo), 'par', len(sol) if sol else None, ' '.join(f"{c[0]},{c[1]}{d}{'*' if s=='bell' else ''}" for c, d, s in sol) if sol else '')
