using System.Xml.Linq;

namespace Flow
{
    /// <summary>
    /// 太鼓の達人のアトラスフォントを扱うクラス。
    /// </summary>
    public class TaikoAtlasFont
    {
        private Texture _fontTexture;
        private Dictionary<int, GlyphInfo> GlyphData = new Dictionary<int, GlyphInfo>();

        // 幅高さ計算キャッシュ
        private readonly Dictionary<(string, double, double, bool, double), (double width, double height)> DimensionCache
            = new();

        // 文字 → 白文字キャッシュ（色なし・アウトラインなし・スケール1.0）
        private readonly Dictionary<string, Texture> _whiteTextCache = new();

        // (文字, edge, backColor, foreColor) → アウトライン込みキャッシュ（スケール1.0）
        private readonly Dictionary<(string text, double edge, Color3 back, Color3 fore), Texture> _outlinedTextCache = new();


        public TaikoAtlasFont(string texturePath, string xmlPath)
        {
            _fontTexture = new Texture(texturePath);
            Raylib.SetTextureFilter(_fontTexture.RayTexture, TextureFilter.Bilinear);

            XElement root = XElement.Load(xmlPath);
            var fontElement = root.Element("font")
                ?? throw new Exception($"XMLフォーマットが不正です: {xmlPath}");

            var glyphDict = new Dictionary<int, GlyphInfo>();
            float maxHeight = 0;

            // 並列処理でグリフ読み込み
            var lockObject = new object();
            Parallel.ForEach(fontElement.Elements("glyph"), glyph =>
            {
                int index = int.Parse(glyph.Attribute("index")?.Value ?? "0");
                int height = int.Parse(glyph.Attribute("height")?.Value ?? "0");

                var glyphInfo = new GlyphInfo
                {
                    OffsetU = int.Parse(glyph.Attribute("offsetU")?.Value ?? "0"),
                    OffsetV = int.Parse(glyph.Attribute("offsetV")?.Value ?? "0"),
                    Width = int.Parse(glyph.Attribute("width")?.Value ?? "0"),
                    Height = height
                };

                lock (lockObject)
                {
                    glyphDict[index] = glyphInfo;
                    maxHeight = Math.Max(maxHeight, height);
                }
            });

            GlyphData = glyphDict;
        }

        /// <summary>
        /// フォントを描画
        /// </summary>
        public void Draw(double x, double y, string text, Vector2d? scale = null, double opacity = 255, Anchor anchor = Anchor.TopLeft, double edge = 0.0, Color3? backColor = null, Color3? foreColor = null)
        {
            if (string.IsNullOrEmpty(text)) return;

            scale ??= Vector2d.One;
            backColor ??= Color3.Black;
            foreColor ??= Color3.White;

            // ----------------------------
            // キャッシュを初回描画時に両方用意
            // ----------------------------
            var whiteTex = GetOrCreateWhiteTextCache(text);
            Texture? outlinedTex = null;
            if (edge > 0.0)
            {
                outlinedTex = GetOrCreateOutlinedCache(text, edge, backColor.Value, foreColor.Value);
            }

            // ----------------------------
            // 描画
            // ----------------------------

            // 不透明時：アウトライン込みのキャッシュを一回で描画（位置ズレ防止のためパディング込みで生成済み）
            if (edge > 0.0 && opacity >= 255.0)
            {
                var t = outlinedTex!;
                t.Color = Color4.White;　// 既に色は焼き込みなのでそのまま
                t.Opacity = 255.0;
                t.Anchor = anchor;
                t.Origin = anchor;
                t.Scale = new Vector2d(scale.Value.X, scale.Value.Y);
                t.Draw(x, y);
                return;
            }

            // 半透明時：白文字キャッシュを使ってアウトラインを重ねる
            if (edge > 0.0)
            {
                for (int i = 0; i < 16; i++)
                {
                    double angle = i / 8.0 * Math.PI;
                    double x1 = Math.Cos(angle) * edge;
                    double y1 = Math.Sin(angle) * edge;

                    // スケール適用
                    x1 *= scale.Value.X;
                    y1 *= scale.Value.Y;

                    double countervalue = opacity >= 255.0 ? 255.0 : opacity < 0.0 ? 0.0 : opacity;
                    double edge_opacity = Easing.EaseIn(countervalue, 0.0, 255.0, Easing.CalcType.Quart);

                    whiteTex.Color = backColor.Value;
                    whiteTex.Opacity = edge_opacity;
                    whiteTex.Anchor = anchor;
                    whiteTex.Origin = anchor;
                    whiteTex.Scale = new Vector2d(scale.Value.X, scale.Value.Y);
                    whiteTex.Draw(x + x1, y + y1);
                }
            }

            whiteTex.Color = foreColor.Value;
            whiteTex.Opacity = opacity;
            whiteTex.Anchor = anchor;
            whiteTex.Origin = anchor;
            whiteTex.Scale = new Vector2d(scale.Value.X, scale.Value.Y);
            whiteTex.Draw(x, y);
        }

        /// <summary>
        /// 文字列全体の幅と高さを計算する関数
        /// </summary>
        public (double width, double height) GetTextDimensions(string text, double scaleX = 1.0, double scaleY = 1.0, bool outline = false, double edge = 0.0)
        {
            var key = (text ?? "", scaleX, scaleY, outline, edge);
            if (DimensionCache.TryGetValue(key, out var cachedResult))
            {
                return cachedResult;
            }

            double totalWidth = 0;
            double maxGlyphHeight = 0;

            if (!string.IsNullOrEmpty(text))
            {
                var glyphDict = GlyphData;
                foreach (char c in text)
                {
                    if (!glyphDict.TryGetValue(c, out var glyph)) continue;
                    totalWidth += glyph.Width * scaleX;
                    maxGlyphHeight = Math.Max(maxGlyphHeight, glyph.Height);
                }

                if (outline)
                {
                    totalWidth += edge * 2 * scaleX;
                    maxGlyphHeight += edge * 2 * scaleY;
                }
            }

            var result = (totalWidth, maxGlyphHeight);
            DimensionCache[key] = result;
            return result;
        }

        #region [private]

        /// <summary>
        /// 白文字をアトラスから描画
        /// </summary>
        private void DrawWhiteText(double x, double y, string text, Color3? color = null, double scaleX = 1.0, double scaleY = 1.0, double opacity = 255)
        {
            var glyphDict = GlyphData;
            if (string.IsNullOrEmpty(text)) return;

            foreach (char c in text)
            {
                int unicode = c;
                if (!glyphDict.TryGetValue(unicode, out var glyph)) continue;

                _fontTexture.Color = color ?? Color3.White;
                _fontTexture.Opacity = opacity;
                _fontTexture.Scale = new Vector2d(scaleX, scaleY);
                _fontTexture.Draw(x, y, new Rectd(glyph.OffsetU, glyph.OffsetV, glyph.Width, glyph.Height));

                x += glyph.Width * scaleX;
            }
        }

        // 既存の白文字キャッシュ（1枚物）
        private Texture GetOrCreateWhiteTextCache(string text)
        {
            if (_whiteTextCache.TryGetValue(text, out var tex))
                return tex;

            var (width, height) = GetTextDimensions(text, 1.0, 1.0, false, 0.0);
            RenderTexture2D screen = Raylib.LoadRenderTexture((int)Math.Ceiling(width), (int)Math.Ceiling(height));

            Raylib.BeginTextureMode(screen);
            Raylib.ClearBackground(Color4.Transparent);
            DrawWhiteText(0, 0, text, Color3.White);
            Raylib.EndTextureMode();

            tex = new Texture(screen);
            Raylib.SetTextureFilter(tex.RayTexture, TextureFilter.Bilinear);
            _whiteTextCache[text] = tex;
            return tex;
        }

        // アウトライン込みキャッシュ（左右上下に edge の透明パディングを含める）
        private Texture GetOrCreateOutlinedCache(string text, double edge, Color3 backColor3, Color3 foreColor3)
        {
            var key = (text, edge, backColor3, foreColor3);
            if (_outlinedTextCache.TryGetValue(key, out var tex))
                return tex;

            var (Width, Height) = GetTextDimensions(text, 1.0, 1.0, true, edge);

            RenderTexture2D screen = Raylib.LoadRenderTexture((int)Width, (int)Height);
            Raylib.BeginTextureMode(screen);
            Raylib.ClearBackground(new Raylib_cs.Color(0, 0, 0, 0));


            // アウトライン（従来の16方向と同じオフセットを edge 基準で）
            for (int i = 0; i < 16; i++)
            {
                double angle = i / 8.0 * Math.PI;
                double x1 = Math.Cos(angle) * edge;
                double y1 = Math.Sin(angle) * edge;
                DrawWhiteText(edge + x1, edge + y1, text, backColor3);
            }

            // 本体
            DrawWhiteText(edge, edge, text, foreColor3);

            Raylib.EndTextureMode();

            tex = new Texture(screen);
            Raylib.SetTextureFilter(tex.RayTexture, TextureFilter.Bilinear);
            _outlinedTextCache[key] = tex;
            return tex;
        }
        #endregion
    }
}

public class GlyphInfo
{
    public int OffsetU { get; set; }
    public int OffsetV { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
}
