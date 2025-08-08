using System.Globalization;

namespace Flow
{
    public struct Color3
    {
        public double R, G, B;

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
    }
}
