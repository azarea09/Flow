namespace Flow.Test
{
    public class MyGame : Game
    {
        protected override void Init()
        {
            Engine.MaxFPS = 0;
            Window.WindowTheme = WindowTheme.Dark;
            RenderSurface.Size = new Vector2i(1920, 1080);
        }

        protected override void Load()
        {
        }

        protected override void Update()
        {
        }

        protected override void Draw()
        {
        }

        protected override void End()
        {

        }
    }
}
