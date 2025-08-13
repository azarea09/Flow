using Flow.Internal.Platform.Windows;

namespace Flow.Internal.Core
{
    internal class RaylibBackend : IBackend
    {
        private static RenderTexture2D _target;
        private const double TitleUpdateInterval = 1.0; // 1秒

        // キャッシュ用変数
        private static bool _isDirectDraw = false;
        private static Rectangle _sourceRect;
        private static Rectangle _destRect;
        private static Vector2i _lastNonFullscreenSize;
        private double _lastTitleUpdateTime = 0;

        public unsafe void Init()
        {
            // ----------------------------
            // ConfigFlags設定
            // ----------------------------
            if (Window.IsResizable)
            {
                Raylib.SetConfigFlags(ConfigFlags.ResizableWindow);
            }
            if (Window.IsUndecoratedWindow)
            {
                Raylib.SetConfigFlags(ConfigFlags.UndecoratedWindow);
            }
            if (Engine.MaxFPS > 0)
            {
                Raylib.SetTargetFPS(Engine.MaxFPS);
            }
            if (Engine.Vsync)
            {
                Raylib.SetConfigFlags(ConfigFlags.VSyncHint);
            }
            if (!Engine.IsLogEnabled)
            {
                Raylib.SetTraceLogLevel(TraceLogLevel.None);
            }

            Raylib.InitWindow(Window.Size.X, Window.Size.Y, Window.ShowInfoTitle ? RenderSurface.UseRenderSurface ? $"{Window.Title} | FPS {Raylib.GetFPS()} | W {Window.Size.X}x{Window.Size.Y} | RS {RenderSurface.Size.X}x{RenderSurface.Size.Y}" : $"{Window.Title} | FPS {Raylib.GetFPS()} | W {Window.Size.X}x{Window.Size.Y}" : $"{Window.Title}");
            Window.Position = Raylib.GetWindowPosition();
            _lastNonFullscreenSize = Window.Size;

            // ----------------------------
            // レンダーサーフェス設定
            // ----------------------------
            _target = Raylib.LoadRenderTexture(RenderSurface.Size.X, RenderSurface.Size.Y);
            Raylib.SetTextureFilter(_target.Texture, (TextureFilter)RenderSurface.Filter);

            // ----------------------------
            // 描画モードを事前計算
            // ----------------------------
            CalculateRenderMode();

            // ----------------------------
            // ダークモード適応 
            // ----------------------------
            if (Window.IsUseDarkMode && OperatingSystem.IsWindows())
            {
                nint hwnd = (nint)Raylib.GetWindowHandle();
                Win32Api.SetDarkModeTitleBar(hwnd, true);
                Win32Api.RefreshWindowLayout(hwnd);
            }

            // ----------------------------
            // イベント登録
            // ----------------------------

            Window.OnResized += () =>
            {
                _lastNonFullscreenSize = Window.Size;
                CalculateRenderMode();
                SetTitleWithFPS();
            };

            Window.OnMoved += () =>
            {
                _lastNonFullscreenSize = Window.Size;
                CalculateRenderMode();
                SetTitleWithFPS();
            };
        }

        public void Update()
        {
            Engine.CurrentFPS = Raylib.GetFPS();
            Engine.DeltaTime = Raylib.GetFrameTime();
            Engine.Time = Raylib.GetTime();

            // ----------------------------
            // ウィンドウサイズ変更の検出と同期
            // ----------------------------
            if (Raylib.IsWindowResized())
            {
                Window.ResizeOnlyProp(Raylib.GetScreenWidth(), Raylib.GetScreenHeight());
                Window.MoveOnlyProp(Raylib.GetWindowPosition());
                _lastNonFullscreenSize = Window.Size;
                CalculateRenderMode();
                SetTitleWithFPS();
            }

            // ----------------------------
            // フルスクリーン切り替え
            // ----------------------------
            if (Raylib.IsKeyPressed(KeyboardKey.F11) || Raylib.IsKeyPressed(KeyboardKey.Enter) && (Raylib.IsKeyDown(KeyboardKey.LeftAlt) || Raylib.IsKeyDown(KeyboardKey.RightAlt)))
            {
                ToggleFullScreen();
            }

            // ----------------------------
            // タイトル更新（1秒に1回）
            // ----------------------------
            if (Engine.Time - _lastTitleUpdateTime >= TitleUpdateInterval)
            {
                SetTitleWithFPS();
                _lastTitleUpdateTime = Engine.Time;
            }
        }

        public void Draw(Action drawCallback)
        {
            // ----------------------------
            // ゲームの基本解像度とモニターのサイズが一緒かつ、フルスクリーンの場合は仮想画面を使わずに直接描画する。
            // それ以外の場合は仮想画面に一旦描画してからスケーリングしてウィンドウに描画する。
            // ----------------------------
            if (_isDirectDraw || !RenderSurface.UseRenderSurface)
            {
                Raylib.BeginDrawing();
                Raylib.ClearBackground(Engine.BackgroundColor);
                drawCallback.Invoke();
                Raylib.EndDrawing();
                return;
            }

            Raylib.BeginTextureMode(_target);
            Raylib.ClearBackground(Engine.BackgroundColor);
            drawCallback.Invoke();
            Raylib.EndTextureMode();

            Raylib.BeginDrawing();
            DrawWindow();
            Raylib.EndDrawing();
        }

        public bool ShouldClose()
        {
            return Raylib.WindowShouldClose();
        }

        public void Shutdown()
        {
            Raylib.UnloadRenderTexture(_target);
            Raylib.CloseWindow();
        }

        #region [private]

        /// <summary>
        /// フルスクリーンモードを切り替えます。
        /// </summary>
        private static unsafe void ToggleFullScreen()
        {
            if (Window.IsFullScreen)
            {
                Raylib.ToggleBorderlessWindowed();
                Window.ResizeOnlyProp(_lastNonFullscreenSize.X, _lastNonFullscreenSize.Y);
                Window.MoveOnlyProp(Raylib.GetWindowPosition());

                // タイトルバーなしの場合ちゃんと戻す
                if (Window.IsUndecoratedWindow) Raylib.SetWindowState(ConfigFlags.UndecoratedWindow);
            }
            else
            {
                Raylib.ToggleBorderlessWindowed();
                Window.ResizeOnlyProp(Monitor.Width, Monitor.Height);
                Window.MoveOnlyProp(Raylib.GetWindowPosition());

                // TopMostを解除
                if (OperatingSystem.IsWindows())
                {
                    nint hwnd = (nint)Raylib.GetWindowHandle();
                    Win32Api.RemoveTopMost(hwnd);
                }
            }

            Window.IsFullScreen = !Window.IsFullScreen;
            CalculateRenderMode();
        }

        /// <summary>
        /// 描画モードを事前計算してキャッシュします。
        /// </summary>
        private static void CalculateRenderMode()
        {
            // 直接描画可能かどうかの判定をキャッシュ
            _isDirectDraw = (Window.IsFullScreen && Monitor.Width == RenderSurface.Width && Monitor.Height == RenderSurface.Height) ||
                (!Window.IsFullScreen && _lastNonFullscreenSize.X == RenderSurface.Width && _lastNonFullscreenSize.Y == RenderSurface.Height) ||
                !RenderSurface.UseRenderSurface;

            if (!_isDirectDraw)
            {
                // スケーリング描画用の値を事前計算
                float scale = Math.Min((float)Window.Width / RenderSurface.Width,
                                (float)Window.Height / RenderSurface.Height);

                _sourceRect = new Rectangle(0, 0, RenderSurface.Width, -RenderSurface.Height);
                _destRect = new Rectangle(
                    (float)Math.Floor((Window.Width - RenderSurface.Width * scale) * 0.5f),
                    (float)Math.Floor((Window.Height - RenderSurface.Height * scale) * 0.5f),
                    (float)Math.Floor(RenderSurface.Width * scale),
                    (float)Math.Floor(RenderSurface.Height * scale)
                );
            }
        }

        /// <summary>
        /// 仮想画面を実画面に描画します。
        /// </summary>
        private static void DrawWindow()
        {
            Raylib.DrawTexturePro(_target.Texture, _sourceRect, _destRect, Vector2.Zero, 0, Color.White);
        }

        private static void SetTitleWithFPS()
        {
            if (!Window.ShowInfoTitle) return;
            Raylib.SetWindowTitle(RenderSurface.UseRenderSurface ? $"{Window.Title} | FPS {Engine.CurrentFPS} | W {Window.Size.X}x{Window.Size.Y} | RS {RenderSurface.Size.X}x{RenderSurface.Size.Y}" : $"{Window.Title} | FPS {Engine.CurrentFPS} | W {Window.Size.X}x{Window.Size.Y}");
        }
        #endregion
    }
}
