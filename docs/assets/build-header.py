"""Rebuild the README GIF from approved repository artwork; requires Pillow."""
from pathlib import Path
import math
from PIL import Image, ImageDraw, ImageFont

ROOT = Path(__file__).resolve().parents[2]
OUT = ROOT / "docs/assets"
OUT.mkdir(parents=True, exist_ok=True)
source = Image.open(ROOT / "Assets/FlyioNav/simulation.png").convert("RGBA")
# Existing white drone only, excluding the surrounding monitor/VR artwork.
drone = source.crop((480, 64, 606, 186))
drone.thumbnail((76, 76), Image.Resampling.LANCZOS)
W,H = 960,240
bg = Image.new("RGB", (W,H), "#161c26")
d = ImageDraw.Draw(bg)
for x in range(0,W,40): d.line((x,0,x,H), fill="#1e2836")
for y in range(0,H,40): d.line((0,y,W,y), fill="#1e2836")
d.line((0,104,W,104), fill="#35404e")
d.line((40,147,88,147), fill="#f56600", width=3)
fontroot = Path("/usr/share/fonts/truetype/dejavu")
title = ImageFont.truetype(str(fontroot/"DejaVuSans-Bold.ttf"), 30)
small = ImageFont.truetype(str(fontroot/"DejaVuSans.ttf"), 13)
d.text((40,165), "Fly.io GCS", font=title, fill="#f2f5f8")
d.text((355,179), "UAV / UGV   |   MISSION PLANNING   |   TELEMETRY", font=small, fill="#b4bfcd")
frames=[]
palette = Image.new('P', (1, 1))
colors = [(round(22+(255-22)*i/127), round(28+(255-28)*i/127), round(38+(255-38)*i/127)) for i in range(128)]
colors += [(round(22+(245-22)*i/63), round(28+(102-28)*i/63), round(38*(1-i/63))) for i in range(64)]
colors += [(30,40,54),(53,64,78),(180,191,205),(242,245,248)]
colors += [(22,28,38)]*(256-len(colors))
palette.putpalette([v for color in colors for v in color])
for i in range(240):
    frame=bg.copy()
    x=round(-90+(W+180)*i/240)
    y=round(65+7*math.sin(2*math.pi*i/240))
    frame.paste(drone,(x,y),drone)
    frames.append(frame.quantize(palette=palette, dither=Image.Dither.NONE))
path=OUT/"flyio-drone-header.gif"
frames[0].save(path,save_all=True,append_images=frames[1:],duration=50,loop=0,optimize=True,disposal=1)
with Image.open(path) as gif:
    assert gif.n_frames > 200 and gif.info["loop"] == 0
    count = gif.n_frames
    duration=0
    for i in range(gif.n_frames):
        gif.seek(i)
        duration += gif.info["duration"]
    assert duration == 12000
    gif.seek(120)
    gif.convert("RGB").save(OUT/"flyio-drone-header-preview.png")
print(f"{path}: {path.stat().st_size} bytes; 960x240; {count} encoded frames; 12 seconds; infinite loop")
