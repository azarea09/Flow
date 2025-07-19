using Raylib_cs;

namespace Flow
{
    public static class Monitor
    {
        /// <summary> Raylibのウィンドウが表示されるモニターを変更する </summary>
        public static void SetWindowMonitor(int monitor) => Raylib.SetWindowMonitor(monitor);

        /// <summary> Raylibのウィンドウが表示されるメインモニター </summary>
        public static int GetCurrentMonitor() => Raylib.GetCurrentMonitor();

        /// <summary> モニターの数 </summary>
        public static int GetMonitorCount() => Raylib.GetMonitorCount();

        /// <summary> モニターのリフレッシュレート </summary>
        public static int GetMonitorRefreshRate(int monitor) => Raylib.GetMonitorRefreshRate(monitor);

        /// <summary> モニターの位置 (px) </summary>
        public static Vector2i GetMonitorPosition(int monitor) => Raylib.GetMonitorPosition(monitor);

        /// <summary> モニターの解像度 (px) </summary>
        public static Vector2i GetMonitorSize(int monitor) => new(Raylib.GetMonitorWidth(monitor), Raylib.GetMonitorHeight(monitor));

        /// <summary> モニターの解像度 (px) </summary>
        public static Vector2i Size
        {
            get
            {
                int monitor = GetCurrentMonitor();
                return GetMonitorSize(monitor);
            }
        }

        /// <summary> モニターの幅 (px) </summary>
        public static int Width
        {
            get
            {
                int monitor = GetCurrentMonitor();
                return Raylib.GetMonitorWidth(monitor);
            }
        }

        /// <summary> モニターの高さ (px) </summary>
        public static int Height
        {
            get
            {
                int monitor = GetCurrentMonitor();
                return Raylib.GetMonitorHeight(monitor);
            }
        }
    }
}
