namespace Flow.Internal.Core
{
    internal interface IBackend
    {
        void Init();
        void Update();
        void Draw(Action drawCallback);
        bool ShouldClose();
        void Shutdown();
    }
}
