using Raylib_cs;
using System;
using System.Numerics;

namespace Flow.WIP
{
    public class MsdfTexture : IDisposable
    {
        public Texture2D Texture { get; private set; }
        public Vector2 Size { get; private set; }
        public Shader Shader { get; private set; }

        public float PxRange { get; set; } = 10.0f;

        public Vector4 ForegroundColor { get; set; } = new Vector4(0f, 0.5f, 1f, 1f);
        public Vector4 BackgroundColor { get; set; } = new Vector4(1f, 1f, 1f, 0f); // 透明背景に変更
        public Vector2 Scale { get; set; } = Vector2.One;
        public float Rotation { get; set; } = 0f;

        public bool IsValid => Texture.Id != 0;

        // シェーダーロケーションをキャッシュして最適化
        private int locPxRange;
        private int locTextureSize;
        private int locFgColor;
        private int locBgColor;

        public MsdfTexture(string imagePath, string fragmentShaderPath)
        {
            Image image = Raylib.LoadImage(imagePath);
            Raylib.ImageAlphaPremultiply(ref image);
            Texture = Raylib.LoadTextureFromImage(image);
            Raylib.SetTextureFilter(Texture, TextureFilter.Bilinear);
            Raylib.UnloadImage(image);

            Shader = Raylib.LoadShader(null, fragmentShaderPath);
            Size = new Vector2(Texture.Width, Texture.Height);

            // シェーダーロケーションを事前取得
            locPxRange = Raylib.GetShaderLocation(Shader, "pxRange");
            locTextureSize = Raylib.GetShaderLocation(Shader, "textureSize");
            locFgColor = Raylib.GetShaderLocation(Shader, "fgColor");
            locBgColor = Raylib.GetShaderLocation(Shader, "bgColor");
        }

        public void Draw(Vector2 position)
        {
            if (!IsValid) return;

            // シェーダーパラメータを設定
            Raylib.SetShaderValue(Shader, locPxRange, PxRange, ShaderUniformDataType.Float);
            Raylib.SetShaderValue(Shader, locTextureSize, Size, ShaderUniformDataType.Vec2);
            Raylib.SetShaderValue(Shader, locFgColor, ForegroundColor, ShaderUniformDataType.Vec4);
            Raylib.SetShaderValue(Shader, locBgColor, BackgroundColor, ShaderUniformDataType.Vec4);

            Raylib.BeginShaderMode(Shader);

            Rectangle source = new Rectangle(0, 0, Size.X, Size.Y);
            Rectangle dest = new Rectangle(position.X, position.Y, Size.X * Scale.X, Size.Y * Scale.Y);
            Vector2 origin = new Vector2(dest.Width / 2, dest.Height / 2);

            Raylib.DrawTexturePro(Texture, source, dest, origin, Rotation, Color.White);

            Raylib.EndShaderMode();
        }

        // デバッグ用：異なるpxRange値をテスト
        public void DrawWithPxRange(Vector2 position, float customPxRange)
        {
            float originalPxRange = PxRange;
            PxRange = customPxRange;
            Draw(position);
            PxRange = originalPxRange;
        }

        public void Dispose()
        {
            if (IsValid)
            {
                Raylib.UnloadTexture(Texture);
                Raylib.UnloadShader(Shader);
            }
            GC.SuppressFinalize(this);
        }
    }
}