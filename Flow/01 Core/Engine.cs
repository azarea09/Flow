namespace Flow
{
    public static class Engine
    {
        /// <summary> 現在FPS </summary>
        public static int CurrentFPS { get; internal set; }
        /// <summary> 前フレームからの経過時間 (秒) </summary>
        public static double DeltaTime { get; internal set; }

        /// <summary> 起動してからの経過時間 (秒) </summary>
        public static double Time { get; internal set; }

        /// <summary> 最大FPS </summary>
        public static int MaxFPS { get; set; } = 1000;

        /// <summary> VSyncが有効かどうか </summary>
        public static bool Vsync { get; set; } = false;

        /// <summary> 背景色 </summary>
        public static Color4 BackgroundColor { get; set; } = Color4.WhiteSmoke;

        /// <summary> コンソールログが有効かどうか </summary>
        public static bool IsLogEnabled { get; set; } = false;
    }
}
