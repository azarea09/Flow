using Flow.Internal.Audio;
using Flow.Internal.Core;

namespace Flow
{
    /// <summary>
    /// ゲームの初期化、更新、描画を行うクラス。
    /// </summary>
    public abstract class Game
    {
        protected abstract void Init();
        protected abstract void Load();
        protected abstract void Update();
        protected abstract void Draw();
        protected abstract void End();
        public void Run()
        {
            var backend = new RaylibBackend();

            // Bass初期化
            Init(); // Window初期化前の処理
            backend.Init(); // Windowの初期化
            AudioManager.Init();
            Load(); // リソースのロード

            while (!backend.ShouldClose())
            {
                backend.Update();
                AudioManager.Update(); // フェード処理と区間ループ処理
                Update();

                backend.Draw(() => Draw());
            }

            End(); // ゲーム終了時の処理
            AudioManager.Free(); // オーディオシステムのクリーンアップ
            backend.Shutdown(); // Windowのシャットダウン
        }
    }
}
