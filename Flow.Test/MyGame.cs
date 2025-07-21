namespace Flow.Test
{
    public class MyGame : Game
    {

        protected override void Init()
        {
            Flow.MaxFPS = 0;
        }

        protected override void Load()
        {

        }

        protected override void Update()
        {
            if (Raylib_cs.Raylib.IsKeyPressed(Raylib_cs.KeyboardKey.A))
            {
                Window.Size = new Vector2i(1920, 720);
            }
        }

        protected override void Draw()
        {
            Raylib_cs.Raylib.DrawFPS(10, 10);
            Raylib_cs.Raylib.DrawText($"DeltaTime {Flow.DeltaTime}", 10, 30, 20, Raylib_cs.Color.Red);
            Raylib_cs.Raylib.DrawText($"Time {Flow.Time}", 10, 50, 20, Raylib_cs.Color.Red);
        }

        protected override void End()
        {

        }
    }
}
