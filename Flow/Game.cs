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

            Init(); // Window初期化前の処理
            backend.Init(); // Windowの初期化
            Load(); // リソースのロード

            while (!backend.ShouldClose())
            {
                backend.Update();
                Update();

                backend.Draw(() => Draw());
            }

            End(); // ゲーム終了時の処理
            backend.Shutdown(); // Windowのシャットダウン
        }
    }
}
