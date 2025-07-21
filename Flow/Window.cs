using Raylib_cs;
using System.Numerics;

namespace Flow
{
    public static class Window
    {
        // ----------------------------
        // フィールド
        // ----------------------------

        private static Vector2i _size = new Vector2i(1280, 720);
        private static Vector2i _position = new Vector2i(1280, 720);
        private static bool _isResizeOnlyProp = false;
        private static bool _isMoveOnlyProp = false;


        // ----------------------------
        // プロパティ
        // ----------------------------

        public static event Action? OnResized;
        public static event Action? OnMoved;


        /// <summary> ウィンドウのタイトル </summary>
        public static String Title { get; set; } = "Flow";

        /// <summary> ウィンドウのサイズ (px) </summary>
        public static Vector2i Size
        {
            get => _size;
            set
            {
                _size = value;
                if (!_isResizeOnlyProp)
                {
                    Raylib.SetWindowSize(_size.X, _size.Y);
                    OnResized?.Invoke(); // 通知
                }
            }
        }

        /// <summary> ウィンドウの幅 (px) </summary>
        public static int Width
        {
            get => Size.X;
            set
            {
                Size = new Vector2i(value, Size.Y);
                Raylib.SetWindowSize(Size.X, Size.Y);
            }
        }

        /// <summary> ウィンドウの高さ (px) </summary>
        public static int Height
        {
            get => Size.Y;
            set
            {
                Size = new Vector2i(Size.X, value);
                Raylib.SetWindowSize(Size.X, Size.Y);
            }
        }

        /// <summary> ウィンドウの位置 (px) </summary>
        public static Vector2i Position
        {
            get => _position;
            set
            {
                _position = value;
                if (!_isMoveOnlyProp)
                {
                    Raylib.SetWindowPosition(_position.X, _position.Y);
                    OnMoved?.Invoke(); // 通知
                }  
            }
        }

        /// <summary> フルスクリーンかどうか </summary>
        public static bool IsFullScreen { get; set; } = false;

        /// <summary> リサイズ可能かどうか </summary>
        public static bool IsResizable { get; set; } = true;

        /// <summary> タイトルバーなしのウィンドウにするかどうか </summary>
        public static bool IsUndecoratedWindow { get; set; } = false;

        /// <summary> ウィンドウがアクティブかどうか </summary>
        public static bool IsFocused
        {
            get
            {
                return Raylib.IsWindowFocused();
            }
        }

        /// <summary> マウスカーソルを表示するかどうか </summary>
        public static bool IsCursorVisible { get; set; } = true;

        /// <summary>
        /// ウィンドウサイズを指定された幅と高さに変更します。
        /// </summary>
        public static void Resize(int width, int height)
        {
            Size = new Vector2i(width, height);
        }

        internal static void ResizeOnlyProp(int width, int height)
        {
            _isResizeOnlyProp = true;
            Size = new Vector2i(width, height);
            _isResizeOnlyProp = false;
        }


        internal static void MoveOnlyProp(Vector2i pos)
        {
            _isMoveOnlyProp = true;
            Position = new Vector2i((int)pos.X, (int)pos.Y);
            _isMoveOnlyProp = false;
        }
    }
}
