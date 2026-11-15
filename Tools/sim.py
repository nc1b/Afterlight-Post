"""Board rules + BFS solver for Afterlight Post levels. Mirrors Board.cs."""
import sys, os, glob
from collections import deque

DIRS = [(0, -1), (1, 0), (0, 1), (-1, 0)]
NAMES = "NESW"


class Level:
    def __init__(self, rows):
        self.h = len(rows)
        self.w = max(len(r) for r in rows)
        self.g = [r.ljust(self.w, '#') for r in rows]
        self.hinge = {}
        self.goal = 0
        self.start = None
        for y, r in enumerate(self.g):
            for x, c in enumerate(r):
                if c == 'P':
                    self.start = (x, y)
                if c in '<>':
                    self.hinge[(x, y)] = len(self.hinge)
                if c in 'abcd':
                    self.goal |= 1 << (ord(c) - 97)
        assert self.start, "no player"

    def t(self, p):
        x, y = p
        if 0 <= x < self.w and 0 <= y < self.h:
            return self.g[y][x]
        return '#'

    def walkable(self, p, lit):
        c = self.t(p)
        if c in '.P?^':
            return True
        if c in 'ABCD':
            return bool(lit >> (ord(c) - 65) & 1)
        return False

    def region(self, pos, lit):
        seen = {pos}
        q = [pos]
        while q:
            x, y = q.pop()
            for dx, dy in DIRS:
                n = (x + dx, y + dy)
                if n not in seen and self.walkable(n, lit):
                    seen.add(n)
                    q.append(n)
        return seen

    def trace(self, pos, d, flips, lit):
        """returns (path, stop, new_flips, new_lit)"""
        path = [pos]
        seen = set()
        touched = 0
        p = pos
        while True:
            p = (p[0] + DIRS[d][0], p[1] + DIRS[d][1])
            c = self.t(p)
            if c in '#w':
                stop = 'wall'; break
            if c in 'ABCD' and not (lit >> (ord(c) - 65) & 1):
                stop = 'gate'; break
            if (p, d) in seen:
                stop = 'loop'; break
            seen.add((p, d))
            path.append(p)
            if c == 'O':
                stop = 'bell'; break
            if c in '/\\<>':
                slash = c in '/<'
                if p in self.hinge:
                    touched |= 1 << self.hinge[p]
                    if flips >> self.hinge[p] & 1:
                        slash = not slash
                d = ((1 if slash else 3) - d) % 4
        nl = lit
        if stop == 'bell':
            for q in path:
                c = self.t(q)
                if c in 'abcd':
                    nl |= 1 << (ord(c) - 97)
        return path, stop, flips ^ touched, nl

    def solve(self, limit=200000):
        def canon(pos, lit):
            return min(self.region(pos, lit))
        s0 = (canon(self.start, 0), 0, 0)
        prev = {s0: None}
        q = deque([s0])
        while q:
            s = q.popleft()
            pos, flips, lit = s
            if lit == self.goal:
                out = []
                while prev[s]:
                    s, mv = prev[s]
                    out.append(mv)
                return out[::-1], len(prev)
            if len(prev) > limit:
                return None, len(prev)
            done = set()
            for cell in sorted(self.region(pos, lit)):
                for d in range(4):
                    path, stop, nf, nl = self.trace(cell, d, flips, lit)
                    key = (nf, nl)
                    if key in done or (nf == flips and nl == lit):
                        continue
                    done.add(key)
                    ns = (canon(cell, nl), nf, nl)
                    if ns not in prev:
                        prev[ns] = (s, (cell, NAMES[d], stop))
                        q.append(ns)
        return None, len(prev)


def load(path):
    meta, rows, inmap = {}, [], False
    for line in open(path, encoding='utf-8').read().splitlines():
        if inmap:
            if line.strip():
                rows.append(line.rstrip())
        elif line.strip() == 'map:':
            inmap = True
        elif ':' in line:
            k, v = line.split(':', 1)
            meta[k.strip()] = v.strip()
    return meta, Level(rows)


if __name__ == '__main__':
    files = sys.argv[1:] or sorted(glob.glob(os.path.join(os.path.dirname(__file__), 'levels', '*.txt')))
    for f in files:
        meta, lv = load(f)
        sol, n = lv.solve()
        par = len(sol) if sol is not None else None
        flag = '' if str(par) == meta.get('par') else '   <-- par says ' + str(meta.get('par'))
        print(f"{os.path.basename(f):28s} {lv.w}x{lv.h} par={par} states={n}{flag}")
        if sol:
            for cell, d, stop in sol:
                print(f"      {cell} {d} -> {stop}")
