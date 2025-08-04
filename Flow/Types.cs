using System.Globalization;
using System.Numerics;

namespace Flow
{
    public struct Vector2i
    {
        public int X;
        public int Y;

        public Vector2i(int x, int y)
        {
            X = x;
            Y = y;
        }

        public Vector2i(int i)
        {
            X = i;
            Y = i;
        }

        public static Vector2i Zero => new Vector2i(0, 0);

        public static Vector2i One => new Vector2i(1, 1);

        public static Vector2i UnitX => new Vector2i(1, 0);

        public static Vector2i UnitY => new Vector2i(0, 1);

        public static implicit operator Vector2i((int x, int y) tuple) => new Vector2i(tuple.x, tuple.y);
        public static implicit operator (int x, int y)(Vector2i v) => (v.X, v.Y);

        public static bool operator ==(Vector2i left, Vector2i right)
        {
            return left.X == right.X && left.Y == right.Y;
        }

        public static bool operator !=(Vector2i left, Vector2i right)
        {
            return !(left == right);
        }

        public static Vector2i operator +(Vector2i left, Vector2i right)
        {
            return new Vector2i(left.X + right.X, left.Y + right.Y);
        }

        public static Vector2i operator -(Vector2i left, Vector2i right)
        {
            return new Vector2i(left.X - right.X, left.Y - right.Y);
        }

        public static Vector2i operator *(Vector2i left, int right)
        {
            return new Vector2i(left.X * right, left.Y * right);
        }

        public static Vector2i operator /(Vector2i left, int right)
        {
            if (right == 0)
                throw new DivideByZeroException("Division by zero in Vector2i division.");
            return new Vector2i(left.X / right, left.Y / right);
        }

        public override string ToString()
        {
            return $"({X}, {Y})";
        }

        public override bool Equals(object obj)
        {
            if (obj is Vector2i other)
            {
                return this == other;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(X, Y);
        }

        public static Vector2i Min(Vector2i a, Vector2i b)
        {
            return new Vector2i(Math.Min(a.X, b.X), Math.Min(a.Y, b.Y));
        }

        public static Vector2i Max(Vector2i a, Vector2i b)
        {
            return new Vector2i(Math.Max(a.X, b.X), Math.Max(a.Y, b.Y));
        }

        public static Vector2i Clamp(Vector2i value, Vector2i min, Vector2i max)
        {
            return new Vector2i(
                Math.Clamp(value.X, min.X, max.X),
                Math.Clamp(value.Y, min.Y, max.Y)
            );
        }

        public static Vector2i Lerp(Vector2i a, Vector2i b, float t)
        {
            t = Math.Clamp(t, 0f, 1f);
            return new Vector2i(
                (int)(a.X + (b.X - a.X) * t),
                (int)(a.Y + (b.Y - a.Y) * t)
            );
        }

        // ----------------------------
        // 変換
        // ----------------------------
        public static implicit operator Vector2i(Vector2 v)
        {
            return new Vector2i((int)v.X, (int)v.Y);
        }

        public static implicit operator Vector2i(Vector2d v)
        {
            return new Vector2i((int)v.X, (int)v.Y);
        }
    }

    public struct Vector2d
    {
        public double X;
        public double Y;

        public Vector2d(double x, double y)
        {
            X = x;
            Y = y;
        }

        public Vector2d(double i)
        {
            X = i;
            Y = i;
        }

        public static Vector2d Zero => new Vector2d(0, 0);

        public static Vector2d One => new Vector2d(1, 1);

        public static Vector2d UnitX => new Vector2d(1, 0);

        public static Vector2d UnitY => new Vector2d(0, 1);

        public static implicit operator Vector2d((double x, double y) tuple) => new Vector2d(tuple.x, tuple.y);
        public static implicit operator (double x, double y)(Vector2d v) => (v.X, v.Y);

        public static bool operator ==(Vector2d left, Vector2d right)
        {
            return left.X == right.X && left.Y == right.Y;
        }

        public static bool operator !=(Vector2d left, Vector2d right)
        {
            return !(left == right);
        }

        public static Vector2d operator -(Vector2d v)
        {
            return new Vector2d(-v.X, -v.Y);
        }

        public static Vector2d operator +(Vector2d left, Vector2d right)
        {
            return new Vector2d(left.X + right.X, left.Y + right.Y);
        }

        public static Vector2d operator -(Vector2d left, Vector2d right)
        {
            return new Vector2d(left.X - right.X, left.Y - right.Y);
        }

        public static Vector2d operator *(Vector2d left, Vector2d right)
        {
            return new Vector2d(left.X * right.X, left.Y * right.Y);
        }

        public static Vector2d operator /(Vector2d left, Vector2d right)
        {
            if (right.X == 0 || right.Y == 0)
                throw new DivideByZeroException("Division by zero in component-wise Vector2d division.");

            return new Vector2d(left.X / right.X, left.Y / right.Y);
        }

        public static Vector2d operator *(Vector2d left, double right)
        {
            return new Vector2d(left.X * right, left.Y * right);
        }

        public static Vector2d operator /(Vector2d left, double right)
        {
            if (right == 0)
                throw new DivideByZeroException("Division by zero in Vector2d division.");
            return new Vector2d(left.X / right, left.Y / right);
        }

        public override string ToString()
        {
            return $"({X}, {Y})";
        }

        public override bool Equals(object obj)
        {
            if (obj is Vector2d other)
            {
                return this == other;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(X, Y);
        }

        public static Vector2d Min(Vector2d a, Vector2d b)
        {
            return new Vector2d(Math.Min(a.X, b.X), Math.Min(a.Y, b.Y));
        }

        public static Vector2d Max(Vector2d a, Vector2d b)
        {
            return new Vector2d(Math.Max(a.X, b.X), Math.Max(a.Y, b.Y));
        }

        public static Vector2d Clamp(Vector2d value, Vector2d min, Vector2d max)
        {
            return new Vector2d(
                Math.Clamp(value.X, min.X, max.X),
                Math.Clamp(value.Y, min.Y, max.Y)
            );
        }

        public static Vector2d Lerp(Vector2d a, Vector2d b, float t)
        {
            t = Math.Clamp(t, 0f, 1f);
            return new Vector2d(
                (double)(a.X + (b.X - a.X) * t),
                (double)(a.Y + (b.Y - a.Y) * t)
            );
        }

        // ----------------------------
        // 変換
        // ----------------------------
        public static implicit operator Vector2d(Vector2 v)
        {
            return new Vector2d((double)v.X, (double)v.Y);
        }

        public static implicit operator Vector2(Vector2d v)
        {
            return new Vector2((float)v.X, (float)v.Y);
        }

        public static implicit operator Vector2d(Vector2i v)
        {
            return new Vector2d((double)v.X, (double)v.Y);
        }
    }

    public struct Color4
    {
        public double R;
        public double G;
        public double B;
        public double A;
        public Color4(double r, double g, double b, double a = 255)
        {
            R = r;
            G = g;
            B = b;
            A = a;
        }
        public static implicit operator Color4((double r, double g, double b, double a) tuple) => new Color4(tuple.r, tuple.g, tuple.b, tuple.a);
        public static implicit operator (double r, double g, double b, double a)(Color4 c) => (c.R, c.G, c.B, c.A);

        public static bool operator ==(Color4 left, Color4 right)
        {
            return left.R == right.R &&
                   left.G == right.G &&
                   left.B == right.B &&
                   left.A == right.A;
        }

        public static bool operator !=(Color4 left, Color4 right)
        {
            return !(left == right);
        }

        public override bool Equals(object obj)
        {
            if (obj is Color4 other)
            {
                return this == other;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(R, G, B, A);
        }

        /// <summary> HTMLカラーコード (#RGB, #RGBA, #RRGGBB, #RRGGBBAA) から変換 </summary>
        public static Color4 FromHtml(string html)
        {
            if (string.IsNullOrWhiteSpace(html))
                throw new ArgumentException("HTML color string is null or empty.");

            if (html[0] == '#')
                html = html.Substring(1);

            switch (html.Length)
            {
                case 3: // #RGB
                    return new Color4(
                        Convert.ToInt32(new string(html[0], 2), 16),
                        Convert.ToInt32(new string(html[1], 2), 16),
                        Convert.ToInt32(new string(html[2], 2), 16),
                        255
                    );
                case 4: // #RGBA
                    return new Color4(
                        Convert.ToInt32(new string(html[0], 2), 16),
                        Convert.ToInt32(new string(html[1], 2), 16),
                        Convert.ToInt32(new string(html[2], 2), 16),
                        Convert.ToInt32(new string(html[3], 2), 16)
                    );
                case 6: // #RRGGBB
                    return new Color4(
                        int.Parse(html.Substring(0, 2), NumberStyles.HexNumber),
                        int.Parse(html.Substring(2, 2), NumberStyles.HexNumber),
                        int.Parse(html.Substring(4, 2), NumberStyles.HexNumber),
                        255
                    );
                case 8: // #RRGGBBAA
                    return new Color4(
                        int.Parse(html.Substring(0, 2), NumberStyles.HexNumber),
                        int.Parse(html.Substring(2, 2), NumberStyles.HexNumber),
                        int.Parse(html.Substring(4, 2), NumberStyles.HexNumber),
                        int.Parse(html.Substring(6, 2), NumberStyles.HexNumber)
                    );
                default:
                    throw new FormatException("Invalid HTML color format.");
            }
        }

        // ----------------------------
        // 変換
        // ----------------------------
        public static implicit operator Color4(Color3 c)
        {
            return new Color4(c.R, c.G, c.B);
        }

        public static implicit operator Color4(Raylib_cs.Color c)
        {
            return new Color4(c.R, c.G, c.B, c.A);
        }

        public static implicit operator Color4(System.Drawing.Color c)
        {
            return new Color4(c.R, c.G, c.B, c.A);
        }

        public static implicit operator Raylib_cs.Color(Color4 c)
        {
            return new Raylib_cs.Color((byte)c.R, (byte)c.G, (byte)c.B, (byte)c.A);
        }

        public static implicit operator System.Drawing.Color(Color4 c)
        {
            return System.Drawing.Color.FromArgb((int)c.A, (int)c.R, (int)c.G, (int)c.B);
        }

        // ----------------------------
        // 色のプリセット
        // ----------------------------
        public static readonly Color4 Red = new Color4(255, 0, 0);
        public static readonly Color4 Green = new Color4(0, 255, 0);
        public static readonly Color4 Blue = new Color4(0, 0, 255);
        public static readonly Color4 White = new Color4(255, 255, 255);
        public static readonly Color4 OffWhite = new Color4(250, 250, 250);
        public static readonly Color4 WhiteSmoke = new Color4(245, 245, 245);
        public static readonly Color4 Black = new Color4(0, 0, 0);
        public static readonly Color4 Transparent = new Color4(0, 0, 0, 0);
        public static readonly Color4 Gray = new Color4(128, 128, 128);
        public static readonly Color4 Yellow = new Color4(255, 255, 0);
        public static readonly Color4 Cyan = new Color4(0, 255, 255);
        public static readonly Color4 Magenta = new Color4(255, 0, 255);
        public static readonly Color4 Orange = new Color4(255, 165, 0);
        public static readonly Color4 Purple = new Color4(128, 0, 128);
        public static readonly Color4 Pink = new Color4(255, 192, 203);
        public static readonly Color4 Brown = new Color4(165, 42, 42);
    }

    public struct Color3
    {
        public double R;
        public double G;
        public double B;
        public Color3(double r, double g, double b)
        {
            R = r;
            G = g;
            B = b;
        }
        public static implicit operator Color3((double r, double g, double b) tuple) => new Color3(tuple.r, tuple.g, tuple.b);
        public static implicit operator (double r, double g, double b)(Color3 c) => (c.R, c.G, c.B);

        public static bool operator ==(Color3 left, Color3 right)
        {
            return left.R == right.R &&
                   left.G == right.G &&
                   left.B == right.B;
        }

        public static bool operator !=(Color3 left, Color3 right)
        {
            return !(left == right);
        }

        public override bool Equals(object obj)
        {
            if (obj is Color3 other)
            {
                return this == other;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(R, G, B);
        }

        /// <summary> HTMLカラーコード (#RGB, #RGBA, #RRGGBB, #RRGGBBAA) から変換 </summary>
        public static Color3 FromHtml(string html)
        {
            if (string.IsNullOrWhiteSpace(html))
                throw new ArgumentException("HTML color string is null or empty.");

            if (html[0] == '#')
                html = html.Substring(1);

            switch (html.Length)
            {
                case 3: // #RGB
                    return new Color3(
                        Convert.ToInt32(new string(html[0], 2), 16),
                        Convert.ToInt32(new string(html[1], 2), 16),
                        Convert.ToInt32(new string(html[2], 2), 16)
                    );
                case 4: // #RGBA
                    return new Color3(
                        Convert.ToInt32(new string(html[0], 2), 16),
                        Convert.ToInt32(new string(html[1], 2), 16),
                        Convert.ToInt32(new string(html[2], 2), 16)
                    );
                case 6: // #RRGGBB
                    return new Color3(
                        int.Parse(html.Substring(0, 2), NumberStyles.HexNumber),
                        int.Parse(html.Substring(2, 2), NumberStyles.HexNumber),
                        int.Parse(html.Substring(4, 2), NumberStyles.HexNumber)
                    );
                case 8: // #RRGGBBAA
                    return new Color3(
                        int.Parse(html.Substring(0, 2), NumberStyles.HexNumber),
                        int.Parse(html.Substring(2, 2), NumberStyles.HexNumber),
                        int.Parse(html.Substring(4, 2), NumberStyles.HexNumber)
                        );
                default:
                    throw new FormatException("Invalid HTML color format.");
            }
        }

        // ----------------------------
        // 変換
        // ----------------------------
        public static implicit operator Color3(Color4 c)
        {
            return new Color3(c.R, c.G, c.B);
        }

        public static implicit operator Color3(Raylib_cs.Color c)
        {
            return new Color3(c.R, c.G, c.B);
        }

        public static implicit operator Color3(System.Drawing.Color c)
        {
            return new Color3(c.R, c.G, c.B);
        }

        public static implicit operator Raylib_cs.Color(Color3 c)
        {
            return new Raylib_cs.Color((byte)c.R, (byte)c.G, (byte)c.B);
        }

        public static implicit operator System.Drawing.Color(Color3 c)
        {
            return System.Drawing.Color.FromArgb((int)c.R, (int)c.G, (int)c.B);
        }

        public static Raylib_cs.Color ToRaylibColorWithOpacity(Color3 c, double opacity)
        {
            return new Raylib_cs.Color((byte)c.R, (byte)c.G, (byte)c.B, (byte)opacity);
        }

        // ----------------------------
        // 色のプリセット
        // ----------------------------
        public static readonly Color3 Red = new Color3(255, 0, 0);
        public static readonly Color3 Green = new Color3(0, 255, 0);
        public static readonly Color3 Blue = new Color3(0, 0, 255);
        public static readonly Color3 White = new Color3(255, 255, 255);
        public static readonly Color3 OffWhite = new Color3(250, 250, 250);
        public static readonly Color3 WhiteSmoke = new Color3(245, 245, 245);
        public static readonly Color3 Black = new Color3(0, 0, 0);
        public static readonly Color3 Gray = new Color3(128, 128, 128);
        public static readonly Color3 Yellow = new Color3(255, 255, 0);
        public static readonly Color3 Cyan = new Color3(0, 255, 255);
        public static readonly Color3 Magenta = new Color3(255, 0, 255);
        public static readonly Color3 Orange = new Color3(255, 165, 0);
        public static readonly Color3 Purple = new Color3(128, 0, 128);
        public static readonly Color3 Pink = new Color3(255, 192, 203);
        public static readonly Color3 Brown = new Color3(165, 42, 42);
    }

    public struct Recti
    {
        public int X;
        public int Y;
        public int Width;
        public int Height;
        public Recti(int x, int y, int width, int height)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }
        public static implicit operator Recti((int x, int y, int width, int height) tuple) => new Recti(tuple.x, tuple.y, tuple.width, tuple.height);
        public static implicit operator (int x, int y, int width, int height)(Recti r) => (r.X, r.Y, r.Width, r.Height);
        public override string ToString()
        {
            return $"({X}, {Y}, {Width}, {Height})";
        }
    }

    public struct Rectd
    {
        public double X;
        public double Y;
        public double Width;
        public double Height;
        public Rectd(double x, double y, double width, double height)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }
        public static implicit operator Rectd((double x, double y, double width, double height) tuple) => new Rectd(tuple.x, tuple.y, tuple.width, tuple.height);
        public static implicit operator (double x, double y, double width, double height)(Rectd r) => (r.X, r.Y, r.Width, r.Height);
        public override string ToString()
        {
            return $"({X}, {Y}, {Width}, {Height})";
        }
    }

    public enum BlendState
    {
        Alpha,
        Additive,
        Subtract,
        Multiply,
        Screen
    }

    public enum Anchor
    {
        TopLeft,
        TopCenter,
        TopRight,
        CenterLeft,
        Center,
        CenterRight,
        BottomLeft,
        BottomCenter,
        BottomRight
    }

    public enum Filter
    {
        Nearest = 0,
        Bilinear = 1,
        Trilinear = 2,
    }
}