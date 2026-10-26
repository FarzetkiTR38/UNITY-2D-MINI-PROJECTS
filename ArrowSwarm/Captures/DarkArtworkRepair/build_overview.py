"""Review sheet composed from actual Unity captures; game artwork is never altered."""
from pathlib import Path
from PIL import Image, ImageDraw, ImageFont

ROOT = Path(__file__).resolve().parent


def main():
    canvas = Image.new('RGB', (1200, 1170), '#101A2B')
    draw = ImageDraw.Draw(canvas)

    def text(x, y, value, size=22):
        draw.text((x, y), value, fill='#E6F0FF', font=ImageFont.truetype('C:/Windows/Fonts/arialbd.ttf', size))

    def put(path, x, y, width, height):
        image = Image.open(path).convert('RGB')
        image.thumbnail((width, height), Image.Resampling.LANCZOS)
        canvas.paste(image, (x, y))

    text(25, 22, 'ARROW SWARM / DARK ARTWORK REPAIR', 31)
    text(25, 70, 'Actual Unity screenshots / original Light artwork preserved', 20)
    for col, theme in enumerate(('light', 'dark')):
        x = 25 + col*600
        text(x, 120, theme.upper()+' / clean stat plaques')
        put(ROOT/'Hud'/('HUD-'+theme+'.png'), x, 157, 550, 110)
        text(x, 300, theme.upper()+' / mode selection')
        put(ROOT/'Cards'/('Modal-'+theme+'.png'), x, 341, 550, 700)
    text(25, 1080, 'Paired artwork, original icon highlights, live themes and pointer feedback verified.', 20)
    text(25, 1123, 'Leaderboard concepts remain preview-only and are separate from the game.', 20)
    canvas.save(ROOT/'Overview.jpg', quality=94)


if __name__ == '__main__':
    main()
