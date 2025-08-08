namespace Flow.Test
{
    public class MyGame : Game
    {
        private Texture texture;

        protected override void Init()
        {
            Flow.MaxFPS = 0;
            RenderSurface.Size = new Vector2i(1920, 1080);
        }

        protected override void Load()
        {
            texture = new Texture("Shaders/MSDFTexture_CretaIcon.png");
        }

        protected override void Update()
        {
            if (Raylib_cs.Raylib.IsKeyPressed(Raylib_cs.KeyboardKey.A))
            {
                texture.Filter = Filter.Bilinear;
            }
        }

        protected override void Draw()
        {
            texture.Scale = new Vector2d(1);
            texture.Anchor = Anchor.Center;
            texture.Origin = Anchor.Center;
            texture.Draw(400, 0);

            Raylib_cs.Raylib.DrawFPS(10, 10);
            Raylib_cs.Raylib.DrawText($"DeltaTime {Flow.DeltaTime}", 10, 30, 20, Raylib_cs.Color.Red);
            Raylib_cs.Raylib.DrawText($"Time {Flow.Time}", 10, 50, 20, Raylib_cs.Color.Red);
        }

        protected override void End()
        {

        }
    }
}
