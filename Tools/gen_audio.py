"""Chip-style sound effects and the music loop. Run: python gen_audio.py <Assets/Audio dir>"""
import sys, os, wave
import numpy as np

OUT = sys.argv[1]
SR = 22050
rng = np.random.default_rng(5)

def t(n): return np.arange(int(SR * n)) / SR
def sq(f, n, duty=.5): return np.where((t(n) * f) % 1 < duty, 1.0, -1.0)
def tri(f, n): return 2 * np.abs(2 * ((t(n) * f) % 1) - 1) - 1
def sine(f, n): return np.sin(2 * np.pi * f * t(n))
def env(x, a=.005, r=.05):
    n = len(x); e = np.ones(n)
    na, nr = max(1, int(SR * a)), max(1, int(SR * r))
    e[:na] = np.linspace(0, 1, na); e[-nr:] *= np.linspace(1, 0, nr)
    return x * e
def decay(x, k): return x * np.exp(-k * np.arange(len(x)) / SR)
def sweep(f0, f1, n, wave_fn='sq'):
    f = np.linspace(f0, f1, int(SR * n)); ph = np.cumsum(f) / SR
    return np.where(ph % 1 < .5, 1.0, -1.0) if wave_fn == 'sq' else 2 * np.abs(2 * (ph % 1) - 1) - 1
def lowpass(x, k):
    y = np.zeros_like(x); acc = 0.0
    for i, v in enumerate(x):
        acc += (v - acc) * k; y[i] = acc
    return y
def write(name, x, vol=.6):
    x = np.clip(x * vol, -1, 1)
    with wave.open(os.path.join(OUT, name + '.wav'), 'wb') as w:
        w.setnchannels(1); w.setsampwidth(2); w.setframerate(SR)
        w.writeframes((x * 32000).astype('<i2').tobytes())
def seq(*parts): return np.concatenate(parts)
def mix(*parts):
    n = max(len(p) for p in parts); out = np.zeros(n)
    for p in parts: out[:len(p)] += p
    return out

write('step', env(decay(lowpass(rng.uniform(-1, 1, int(SR * .05)), .25), 60), .001, .02), .5)
write('send', env(sweep(330, 880, .11), .002, .04), .32)
write('tick', env(decay(sq(1320, .05, .25), 50), .001, .02), .28)
bell = mix(decay(sine(880, .9), 5), decay(sine(880 * 2.76, .9), 9) * .45, decay(sine(880 * 5.4, .9), 16) * .25, decay(tri(440, .9), 7) * .3)
write('bell', env(bell, .002, .1), .5)
write('thud', env(mix(decay(sweep(150, 50, .22, 'tri'), 14), decay(lowpass(rng.uniform(-1, 1, int(SR * .22)), .1), 22)), .002, .05), .75)
write('lamp', env(seq(decay(tri(784, .07), 8), decay(tri(1175, .22), 10)), .002, .06), .5)
write('flip', seq(env(decay(sq(240, .035, .3), 40), .001, .01), np.zeros(int(SR * .03)), env(decay(sq(360, .05, .3), 40), .001, .02)) , .3)
write('gate', env(mix(sweep(90, 180, .32, 'tri'), lowpass(rng.uniform(-1, 1, int(SR * .32)), .08) * .5), .01, .1), .55)
write('undo', env(sweep(520, 300, .09, 'tri'), .002, .03), .45)
write('ui', env(decay(sq(660, .05, .25), 30), .001, .02), .25)
write('empty', env(seq(tri(294, .12), tri(233, .12), decay(tri(175, .3), 5)), .004, .08), .45)
notes = [523.25, 659.25, 783.99, 1046.5, 1318.5]
clear = np.zeros(int(SR * 1.3))
for i, f in enumerate(notes):
    n = decay(tri(f, .8), 5) * .6 + decay(sq(f, .8, .25), 9) * .2
    s = int(SR * .085 * i); clear[s:s + len(n)] += n[:len(clear) - s]
write('clear', env(clear, .002, .2), .4)

bpm = 72; beat = 60 / bpm; bars = 8
total = int(SR * beat * 4 * bars)
music = np.zeros(total)
def hz(semi): return 220.0 * 2 ** (semi / 12)
def add(x, at, vol):
    s = int(SR * at); e = min(total, s + len(x)); music[s:e] += x[:e - s] * vol
chords = [(0, 3, 7), (-4, 0, 3), (3, 7, 10), (-2, 2, 5), (0, 3, 7), (-4, 0, 3), (5, 8, 12), (-5, -1, 2)]
lead = [[12, None, 10, 7], [8, None, 7, 3], [10, None, 7, 3], [5, None, 2, None], [12, 15, 14, 10], [8, None, 12, 8], [8, 5, None, 8], [7, None, 4, None]]
for b, ch in enumerate(chords):
    t0 = b * 4 * beat
    root = hz(ch[0] - 12)
    add(env(decay(tri(root, beat * 3.6), .7), .02, .5), t0, .30)
    for i in range(8):
        n = ch[i % 3] + (12 if i % 6 >= 3 else 0)
        add(env(decay(sq(hz(n), beat * .45, .25), 6), .004, .05), t0 + i * beat / 2, .055)
    for i, n in enumerate(lead[b]):
        if n is None: continue
        add(env(decay(tri(hz(n + 12), beat * .95), 2.5), .01, .12), t0 + i * beat, .13)
music = lowpass(music, .35)
noise = lowpass(rng.uniform(-1, 1, total), .02) * (.55 + .45 * np.sin(2 * np.pi * np.arange(total) / total * 4))
music += noise * .22
write('night_shift', music, .9)
print('ok')
