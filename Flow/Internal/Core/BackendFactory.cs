namespace Flow.Internal.Core
{
    internal static class BackendFactory
    {
        public static IBackend Create(BackendType type)
        {
            return type switch
            {
                BackendType.Raylib => new RaylibBackend(),
                _ => throw new NotSupportedException()
            };
        }

        public static BackendType GetBackendType()
        {
            if (OperatingSystem.IsWindows())
            {
                return BackendSettings.WindowsBackend;
            }
            else if (OperatingSystem.IsLinux())
            {
                throw new NotSupportedException("Linux backend is not implemented yet.");
            }
            else if (OperatingSystem.IsMacOS())
            {
                throw new NotSupportedException("macOS backend is not implemented yet.");
            }

            return BackendType.Raylib; // デフォルトはRaylib
        }
    }
}
