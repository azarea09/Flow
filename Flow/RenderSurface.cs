namespace Flow
{
    public static class RenderSurface
    {
        /// <summary> レンダーサーフェスのサイズ (px) </summary>
        public static Vector2i Size { get; set; } = new Vector2i(1280, 720);

        /// <summary> レンダーサーフェスの幅 (px) </summary>
        public static int Width
        {
            get => Size.X;
            set => Size = new Vector2i(value, Size.Y);
        }

        /// <summary> レンダーサーフェスの高さ (px) </summary>
        public static int Height
        {
            get => Size.Y;
            set => Size = new Vector2i(Size.X, value);
        }

        /// <summary> レンダーサーフェスのフィルタリングモード </summary>
        public static RenderSurfaceFilter Filter { get; set; } = RenderSurfaceFilter.Bilinear;

        /// <summary> レンダーサーフェスを使用するかどうか </summary>
        public static bool UseRenderSurface { get; set; } = true;

        /// <summary>
        /// レンダーサーフェスを指定したサイズの解像度に設定します (ウィンドウの初期化前にのみ使用出来ます)
        /// </summary>
        public static void Resize(int width, int height)
        {
            Width = width;
            Height = height;
        }
    }

    public enum RenderSurfaceFilter
    {
        Nearest = 0,
        Bilinear = 1,
        Trilinear = 2,
    }
}
