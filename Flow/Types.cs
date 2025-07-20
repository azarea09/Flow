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

    public enum BackendType
    {
        Raylib,
    }
}