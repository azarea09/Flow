using Raylib_cs;

namespace Flow.Test
{
    public class MyGame : Game
    {
        private Audio audio;
        private TaikoAtlasFont font;

        protected override void Init()
        {
            Engine.MaxFPS = 0;
            Window.IsUseDarkMode = true;
            RenderSurface.Size = new Vector2i(1920, 1080);
        }

        protected override void Load()
        {
            audio = new Audio("1・2・さんしのでドンドカッカッ！.ogg");
            font = new TaikoAtlasFont("font\\jp_64.png", "font\\jp_64.xml");
        }

        protected override void Update()
        {
            if (Raylib_cs.Raylib.IsKeyPressed(Raylib_cs.KeyboardKey.Space))
            {
                audio.Play(TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(10));
            }

            if (Raylib_cs.Raylib.IsKeyPressed(Raylib_cs.KeyboardKey.S))
            {
                audio.Stop(TimeSpan.FromSeconds(5));
            }

            if (Raylib_cs.Raylib.IsKeyPressed(Raylib_cs.KeyboardKey.Right))
            {
                audio.Speed += 0.1;
            }

            if (Raylib_cs.Raylib.IsKeyPressed(Raylib_cs.KeyboardKey.Left))
            {
                audio.Speed -= 0.1;
            }

            if (Raylib_cs.Raylib.IsKeyPressed(Raylib_cs.KeyboardKey.Up))
            {
                audio.Volume += 0.1;
            }

            if (Raylib_cs.Raylib.IsKeyPressed(Raylib_cs.KeyboardKey.Down))
            {
                audio.Volume -= 0.1;
            }
        }

        protected override void Draw()
        {
            int y = 10;

            Raylib_cs.Raylib.DrawFPS(10, y);
            y += 20;
            Raylib_cs.Raylib.DrawText($"DeltaTime {Engine.DeltaTime}", 10, y += 20, 20, Raylib_cs.Color.Violet);
            Raylib_cs.Raylib.DrawText($"Time {Engine.Time}", 10, y += 20, 20, Raylib_cs.Color.Violet);
            y += 20;
            Raylib_cs.Raylib.DrawText($"audio.IsPlaying {audio.IsPlaying}", 10, y += 20, 20, Raylib_cs.Color.Magenta);
            Raylib_cs.Raylib.DrawText($"audio.Speed {audio.Speed}", 10, y += 20, 20, Raylib_cs.Color.Magenta);
            Raylib_cs.Raylib.DrawText($"audio.Volume {audio.Volume}", 10, y += 20, 20, Raylib_cs.Color.Magenta);
            Raylib_cs.Raylib.DrawText($"audio.Duration {audio.Duration}", 10, y += 20, 20, Raylib_cs.Color.Magenta);
            Raylib_cs.Raylib.DrawText($"audio.Position {audio.Position}", 10, y += 20, 20, Raylib_cs.Color.Magenta);
            Raylib_cs.Raylib.DrawText($"audio.ActiveInstanceCount {audio.ActiveInstanceCount}", 10, y += 20, 20, Raylib_cs.Color.Magenta);

            font.Draw(0, 0, "あいうえお・かきくけこ♡♪★™", Vector2d.One, 255, Anchor.Center, 6, Color4.Purple, Color4.White);
            font.Draw(0, 0, "あいうえお・かきくけこ♡♪★™", Vector2d.One, 125, Anchor.Center, 0, Color4.Black, Color4.Yellow);
        }

        protected override void End()
        {

        }
    }
}
