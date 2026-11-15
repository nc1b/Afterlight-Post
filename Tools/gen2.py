"""Template search: objects go on water cells of a template, then get pruned to the essentials."""
import random, sys
from sim import Level

BS = chr(92)
TEMPLATES = {
 'quay': [
  "#############",
  "#~~~~~~~~~~~#",
  "#~~~~~~~~~~~#",
  "#~~~~~~~~~~~#",
  "#.~~~~~~~~~~#",
  "#.~~~~~~~~~~#",
  "#.~~~~~~~~~~#",
  "#P..........#",
  "#############"],
 'island': [
  "#############",
  "#~~~~~~~~~~~#",
  "#~~~~~~~~~~~#",
  "#~~~~...~~~~#",
  "#~~~~.P.~~~~#",
  "#~~~~...~~~~#",
  "#~~~~~~~~~~~#",
  "#~~~~~~~~~~~#",
  "#############"],
 'ring': [
  "#############",
  "#P..........#",
  "#.~~~~~~~~~.#",
  "#.~~~~~~~~~.#",
  "#.~~~~~~~~~.#",
  "#.~~~~~~~~~.#",
  "#.~~~~~~~~~.#",
  "#...........#",
  "#############"],
 'piers': [
  "#############",
  "#~~~~~~~~~~~#",
  "#~~.~~~~~.~~#",
  "#~~.~~~~~.~~#",
  "#~~.~~~~~.~~#",
  "#~~.~~~~~.~~#",
  "#~~.~~~~~.~~#",
  "#P....G.....#",
  "#############"],
 'cross': [
  "#############",
  "#~~~~~.~~~~~#",
  "#~~~~~.~~~~~#",
  "#~~~~~.~~~~~#",
  "#.....P.....#",
  "#~~~~~.~~~~~#",
  "#~~~~~.~~~~~#",
  "#~~~~~.~~~~~#",
  "#############"],
 'split': [
  "#############",
  "#~~~~~~~~~~~#",
  "#~~~~~~~~~~~#",
  "#~~~~~~~~~~~#",
  "#~~~~~~~~~~~#",
  "#~~~~~~~~~~~#",
  "#~~~~~~~~~~~#",
  "#P...G.....G#",
  "#############"],
}

def make(rng, tpl, n_h, n_m, n_l, n_b, n_wall):
    g = [list(r) for r in TEMPLATES[tpl]]
    water = [(x, y) for y, r in enumerate(g) for x, c in enumerate(r) if c == '~']
    rng.shuffle(water)
    gates = [(x, y) for y, r in enumerate(g) for x, c in enumerate(r) if c == 'G']
    def put(c):
        x, y = water.pop(); g[y][x] = c
    for _ in range(n_b): put('O')
    for i in range(n_l): put('abcd'[i])
    for _ in range(n_h): put(rng.choice('<>'))
    for _ in range(n_m): put(rng.choice(['/', BS]))
    for _ in range(n_wall): put('#')
    for i, (x, y) in enumerate(gates): g[y][x] = 'ABCD'[i] if i < n_l else '.'
    return [''.join(r) for r in g]

def par(rows):
    sol, n = Level(rows).solve(limit=30000)
    return (len(sol), sol) if sol else (0, None)

def prune(rows, base, tpl):
    rows = [list(r) for r in rows]
    changed = True
    while changed:
        changed = False
        for y in range(1, len(rows) - 1):
            for x in range(1, len(rows[0]) - 1):
                c = rows[y][x]
                if c in '<>/#O' + BS + 'ABCD':
                    rows[y][x] = '.' if TEMPLATES[tpl][y][x] in '.G' else '~'
                    p, _ = par([''.join(r) for r in rows])
                    if p >= base: changed = True; base = p
                    else: rows[y][x] = c
    return [''.join(r) for r in rows], base

if __name__ == '__main__':
    tpl, seed, tries = sys.argv[1], int(sys.argv[2]), int(sys.argv[3])
    params = [int(v) for v in sys.argv[4:9]]
    minpar = int(sys.argv[9])
    rng = random.Random(seed)
    out = []
    for i in range(tries):
        rows = make(rng, tpl, *params)
        p, sol = par(rows)
        if p < minpar: continue
        rows, p = prune(rows, p, tpl)
        p, sol = par(rows)
        objs = sum(c in '<>/#O' + BS for r in rows[1:-1] for c in r[1:-1])
        stands = len({c for c, d, s in sol})
        out.append((p * 10 - objs * 3 + stands * 2, rows, sol))
    seen = set()
    for s, rows, sol in sorted(out, key=lambda b: -b[0])[:5]:
        k = '\n'.join(rows)
        if k in seen: continue
        seen.add(k)
        print('score', s, 'par', len(sol)); print(k)
        for m in sol: print('   ', m)
