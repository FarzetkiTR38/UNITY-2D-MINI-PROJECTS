"""Compose a review sheet from actual Unity screenshots without changing game artwork."""
from pathlib import Path
from PIL import Image, ImageDraw, ImageFont

ROOT = Path(__file__).resolve().parent
PHONE = ROOT / 'Release' / 'Phone'
FONT = 'C:/Windows/Fonts/arial.ttf'
BOLD = 'C:/Windows/Fonts/arialbd.ttf'


def put_image(canvas, path, box, crop=None):
    image = Image.open(path).convert('RGB')
    if crop is not None:
        image = image.crop(crop)
    image.thumbnail((box[2], box[3]), Image.Resampling.LANCZOS)
    canvas.paste(image, (box[0] + (box[2]-image.width)//2, box[1]))


def main():
    canvas = Image.new('RGB', (1080, 1470), '#101a2b')
    draw = ImageDraw.Draw(canvas)

    def label(position, text, size=21, bold=False, color='#E6F0FF'):
        draw.text(position, text, font=ImageFont.truetype(BOLD if bold else FONT, size), fill=color)

    label((30, 22), 'ARROW SWARM / MENU REVISION', 32, True)
    label((30, 66), 'Actual Unity captures / Light + Dark / all three modes', 20, color='#91CDFF')
    label((30, 112), 'Clean top HUD', 23, True)
    put_image(canvas, PHONE/'Dark-Shapes.png', (30, 150, 1020, 160), (90, 75, 995, 215))

    label((30, 320), 'Selected mode artwork', 23, True)
    for row, theme in enumerate(('Light', 'Dark')):
        for col, mode in enumerate(('Defense', 'Shapes', 'ColorShift')):
            x, y = 30 + col*342, 363 + row*146
            label((x, y), theme+' / '+mode, 17, True)
            put_image(canvas, PHONE/(theme+'-'+mode+'.png'), (x, y+30, 328, 100), (140, 1180, 940, 1380))

    label((30, 670), 'Scene transition and full menu', 23, True)
    cards = (
        ('Light transition', ROOT/'Transitions'/'Light-closed.png'),
        ('Dark transition', ROOT/'Transitions'/'Dark-closed.png'),
        ('Dark / Color Shift', PHONE/'Dark-ColorShift.png'),
    )
    for col, (name, path) in enumerate(cards):
        x = 30 + col*342
        label((x, 714), name, 19, True)
        put_image(canvas, path, (x, 749, 320, 590))

    label((30, 1370), 'Phone: 1080 x 1920    Tablet: 1536 x 2048', 20, color='#91CDFF')
    label((30, 1408), 'Mode selection, pointer feedback, theme switching and scene routes verified.', 18)
    canvas.save(ROOT/'Overview.jpg', quality=94)


if __name__ == '__main__':
    main()
