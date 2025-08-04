using System.Reflection.Metadata.Ecma335;

namespace Flow
{
    public class Texture : IDisposable
    {
        /// <summary> RaylibのTexture2D </summary>
        public Texture2D RayTexture { get; private set; }
        /// <summary> テクスチャが有効かどうか </summary>
        public bool IsEnable { get; private set; } = false;
        /// <summary> テクスチャのファイル名 </summary>
        public string FileName { get; private set; } = string.Empty;
        /// <summary> テクスチャのサイズ (px) </summary>
        public Vector2d Size { get; private set; } = Vector2d.Zero;

        /// <summary> 描画の拡大率 (X, Y) </summary>
        public Vector2d Scale = Vector2d.One;
        /// <summary> 描画の回転角度 (ラジアン) </summary>
        public double Rotation = 0.0;
        /// <summary> 描画の透明度 (0-255) </summary>
        public double Opacity = 255;
        /// <summary> 描画色 </summary>
        public Color3 Color = Color3.White;
        /// <summary> 描画のブレンド状態 </summary>
        public BlendState BlendState = BlendState.Alpha;
        /// <summary> 画面上に配置する基準 </summary>
        public Anchor Anchor = Anchor.TopLeft;
        /// <summary> テクスチャの描画エリアの基準 </summary>
        public Anchor Origin = Anchor.TopLeft;
        /// <summary> テクスチャの補完方法 </summary>
        private Filter _filter = Filter.Nearest;
        public Filter Filter
        {
            get => _filter;
            set
            {
                if (IsEnable) 
                {
                    _filter = value;
                    Raylib.SetTextureFilter(RayTexture, (TextureFilter)value);
                }
            }
        }

        /// <summary>
        /// テクスチャを指定したパスから読み込む
        /// </summary>
        /// <param name="path">画像のパス</param>
        /// <param name="filter">描画時の補完方法 (デフォルト: NearestFilter)</param>
        /// <exception cref="Exception"></exception>
        public Texture(string path, Filter filter = Filter.Nearest)
        {
            // 初期化処理
            var image = Raylib.LoadImage(path);
            Raylib.ImageAlphaPremultiply(ref image);
            RayTexture = Raylib.LoadTextureFromImage(image);
            Raylib.SetTextureFilter(RayTexture, (TextureFilter)filter);
            Raylib.UnloadImage(image);
            if (RayTexture.Id == 0)
            {
                throw new Exception($"Failed to load texture from {path}");
            }

            Console.WriteLine($"Texture loaded: {path} ({RayTexture.Width}x{RayTexture.Height})");

            IsEnable = true;
            FileName = path;
            Size = new Vector2d(RayTexture.Width, RayTexture.Height);
        }

        public void Draw(double x, double y, Rectd? drawArea = null)
        {
            if (!IsEnable) return;

            if (!drawArea.HasValue)
            {
                drawArea = new Rectd(0, 0, Size.X, Size.Y);
            }

            Vector2d drawAreaSize = new Vector2d((drawArea.Value.Width - drawArea.Value.X), drawArea.Value.Height - drawArea.Value.Y);
            Vector2d screenSize = RenderSurface.UseRenderSurface ? RenderSurface.Size : Window.Size;

            // テクスチャから描画に使う範囲
            Rectangle sourceRect = new Rectangle((float)drawArea.Value.X, (float)drawArea.Value.Y, (float)drawArea.Value.Width, (float)drawArea.Value.Height);
            // 描画先の矩形
            Rectangle destRect = new Rectangle((float)x, (float)y, (float)(drawAreaSize.X * Scale.X), (float)(drawAreaSize.Y * Scale.Y));

            SetBlend(BlendState);

            Raylib.DrawTexturePro
            (
                RayTexture,
                sourceRect,
                destRect,
                -GetAnchorOffset(screenSize) + GetOriginOffset(drawAreaSize * Scale),
                (float)Rotation,
                Color3.ToRaylibColorWithOpacity(Color, Opacity)
            );

            Rlgl.SetBlendMode(BlendMode.Alpha);
        }

        public void Dispose()
        {
            Raylib.UnloadTexture(RayTexture);
            IsEnable = false;
            GC.SuppressFinalize(this);
        }

        ~Texture()
        {
            Dispose();
        }

        private void SetBlend(BlendState blendState)
        {
            switch (blendState)
            {
                case BlendState.Alpha:
                    Rlgl.SetBlendMode(BlendMode.AlphaPremultiply);
                    break;
                case BlendState.Additive:
                    Rlgl.SetBlendFactorsSeparate(Rlgl.ONE, Rlgl.ONE, Rlgl.ONE, Rlgl.ONE, Rlgl.FUNC_ADD, Rlgl.FUNC_ADD);
                    Rlgl.SetBlendMode(BlendMode.CustomSeparate); 
                    break;
                case BlendState.Subtract:
                    Rlgl.SetBlendFactorsSeparate(Rlgl.ONE, Rlgl.ONE, Rlgl.ONE, Rlgl.ONE, Rlgl.FUNC_REVERSE_SUBTRACT, Rlgl.FUNC_ADD);
                    Rlgl.SetBlendMode(BlendMode.CustomSeparate);
                    break;
                case BlendState.Multiply:
                    Rlgl.SetBlendFactorsSeparate(Rlgl.DST_COLOR, Rlgl.ZERO, Rlgl.ONE, Rlgl.ONE, Rlgl.FUNC_ADD, Rlgl.FUNC_ADD);
                    Rlgl.SetBlendMode(BlendMode.CustomSeparate);
                    break;
                case BlendState.Screen:
                    Rlgl.SetBlendFactorsSeparate(Rlgl.ONE, Rlgl.ONE_MINUS_SRC_COLOR, Rlgl.ONE, Rlgl.ONE, Rlgl.FUNC_ADD, Rlgl.FUNC_ADD);
                    Rlgl.SetBlendMode(BlendMode.CustomSeparate);
                    break;
                default:
                    break;
            }
        }   

        private Vector2d GetAnchorOffset(Vector2d screenSize)
        {
            return Anchor switch
            {
                Anchor.TopLeft => Vector2d.Zero,
                Anchor.TopCenter => new Vector2d(screenSize.X * 0.5, 0),
                Anchor.TopRight => new Vector2d(screenSize.X, 0),
                Anchor.CenterLeft => new Vector2d(0, screenSize.Y * 0.5),
                Anchor.Center => new Vector2d(screenSize.X * 0.5, screenSize.Y * 0.5),
                Anchor.CenterRight => new Vector2d(screenSize.X, screenSize.Y * 0.5),
                Anchor.BottomLeft => new Vector2d(0, screenSize.Y),
                Anchor.BottomCenter => new Vector2d(screenSize.X * 0.5, screenSize.Y),
                Anchor.BottomRight => new Vector2d(screenSize.X, screenSize.Y),
                _ => Vector2d.Zero,
            };
        }

        private Vector2d GetOriginOffset(Vector2d drawAreaSize)

        {
            return Origin switch
            {
                Anchor.TopLeft =>       Vector2d.Zero,
                Anchor.TopCenter =>     new Vector2d(drawAreaSize.X * 0.5, 0),
                Anchor.TopRight =>      new Vector2d(drawAreaSize.X, 0),
                Anchor.CenterLeft =>    new Vector2d(0, drawAreaSize.Y * 0.5),
                Anchor.Center =>        new Vector2d(drawAreaSize.X * 0.5, drawAreaSize.Y * 0.5),
                Anchor.CenterRight =>   new Vector2d(drawAreaSize.X, drawAreaSize.Y * 0.5),
                Anchor.BottomLeft =>    new Vector2d(0, drawAreaSize.Y),
                Anchor.BottomCenter =>  new Vector2d(drawAreaSize.X * 0.5, drawAreaSize.Y),
                Anchor.BottomRight =>   new Vector2d(drawAreaSize.X, drawAreaSize.Y),
                _ => Vector2d.Zero,
            };
        }
    }
}
