using Flow.Internal.Audio;
using Un4seen.Bass;

namespace Flow
{
    /// <summary>
    /// 音声ファイルの管理と再生制御 - 複数インスタンスの同時再生に対応
    /// </summary>
    public class Audio : IDisposable
    {
        private readonly List<AudioInstance> _instances = new();
        private readonly float _baseFrequency; // 元の音声ファイルの周波数
        private readonly long _streamLength; // ストリームの総バイト数
        private readonly double _duration; // 再生時間（秒）
        private double _initialPan = 0.0;
        private double _initialPitch = 0.0;
        private double _initialVolume = 1.0;
        private double _initialSpeed = 1.0;
        private bool _disposed = false;

        public string FilePath { get; }
        public bool Loop { get; set; }
        public TimeSpan? LoopBegin { get; }
        public TimeSpan? LoopEnd { get; }

        /// <summary>再生中のインスタンスが存在するか</summary>
        public bool IsPlaying => _instances.Any(i => i.IsPlaying);

        /// <summary>アクティブなインスタンス数（再生中またはフェード中）</summary>
        public int ActiveInstanceCount => _instances.Count(i => i.IsPlaying || i.IsFading);

        /// <summary>全インスタンスの音量制御</summary>
        public double Volume
        {
            get
            {
                // 最後に登録されたのインスタンスの音量を返す。
                // フェード中の場合はフェード音量を優先
                var first = _instances.LastOrDefault();
                return first?.IsFading == true
                    ? first?.FadeVolume ?? _initialVolume
                    : first?.Volume ?? _initialVolume;
            }
            set
            {
                _initialVolume = value;
                _instances.ForEach(instance => instance.Volume = value);
            }
        }

        /// <summary>全インスタンスのパン制御</summary>
        public double Pan
        {
            get => _instances.LastOrDefault()?.Pan ?? _initialPan;
            set
            {
                _initialPan = value;
                _instances.ForEach(instance => instance.Pan = value);
            }
        }

        /// <summary>全インスタンスのピッチ制御</summary>
        public double Pitch
        {
            get => _instances.LastOrDefault()?.Pitch ?? _initialPitch;
            set
            {
                _initialPitch = value;
                _instances.ForEach(instance => instance.Pitch = value);
            }
        }

        /// <summary>全インスタンスの再生速度制御</summary>
        public double Speed
        {
            get => _instances.LastOrDefault()?.Speed ?? _initialSpeed;
            set
            {
                _initialSpeed = value;
                _instances.ForEach(instance => instance.Speed = value);
            }
        }

        /// <summary>メインインスタンスの再生位置（秒）</summary>
        public double Position
        {
            get => _instances.LastOrDefault()?.Position ?? 0.0;
            set
            {
                if (_instances.Count > 0)
                    _instances[0].Position = value;
            }
        }

        /// <summary>総再生時間（秒）</summary>
        public double Duration => _duration;

        /// <summary> Audioクラスのコンストラクタ</summary>
        public Audio(string filePath, bool loop = false, TimeSpan? loopBegin = null, TimeSpan? loopEnd = null)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException("ファイルパスが無効です", nameof(filePath));

            FilePath = filePath.Replace("\\", "/");
            Loop = loop;
            LoopBegin = loopBegin;
            LoopEnd = loopEnd;

            (_baseFrequency, _streamLength, _duration) = LoadAudioMetadata(filePath);
            AudioManager.Register(this);
        }

        public AudioInstance? GetInstanceByIndex(int index)
        {
            if (index < 0 || index >= _instances.Count)
                return null;

            return _instances[index];
        }

        /// <summary>新しいインスタンスで重複再生</summary>
        public AudioInstance Play(TimeSpan? fadeIn = null, TimeSpan? startPosition = null)
        {
            ThrowIfDisposed();

            var instance = CreateInstance();

            instance.Play();

            if (startPosition.HasValue)
                instance.Position = startPosition!.Value.TotalSeconds;

            if (fadeIn.HasValue)
                instance.FadeIn(fadeIn.Value);

            return instance;
        }

        /// <summary>単独再生（既存のインスタンスを停止してから再生）</summary>
        public AudioInstance PlayOnce(TimeSpan? fadeIn = null, TimeSpan? startPosition = null, TimeSpan? fadeOut = null)
        {
            ThrowIfDisposed();

            Stop(fadeOut);
            return Play(fadeIn, startPosition);
        }

        /// <summary>指定時間後に自動停止する再生</summary>
        public AudioInstance PlayWithDuration(TimeSpan duration, TimeSpan? fadeIn = null, TimeSpan? fadeOut = null)
        {
            var instance = Play(fadeIn);

            // 自動停止のスケジューリング
            var stopDelay = duration - (fadeOut ?? TimeSpan.Zero);
            Task.Delay(stopDelay).ContinueWith(_ =>
            {
                if (!instance.IsDisposed)
                    instance.Stop(fadeOut);
            }, TaskScheduler.Default);

            return instance;
        }

        /// <summary>全インスタンスの停止</summary>
        public void Stop(TimeSpan? fadeOut = null)
        {
            if (fadeOut.HasValue)
            {
                foreach (var instance in _instances.ToList())
                    instance.FadeOut(fadeOut.Value);
            }
            else
            {
                StopAllInstances();
            }
        }

        /// <summary>特定のインスタンスを停止</summary>
        public void StopInstance(AudioInstance instance, TimeSpan? fadeOut = null)
        {
            if (instance == null || !_instances.Contains(instance)) return;

            if (fadeOut.HasValue)
                instance.FadeOut(fadeOut.Value);
            else
                instance.Stop();
        }

        /// <summary>リソース解放</summary>
        public void Dispose()
        {
            if (_disposed) return;

            StopAllInstances();
            AudioManager.Unregister(this);
            _disposed = true;
        }

        /// <summary>フレーム更新処理（区間ループとインスタンス管理）</summary>
        internal void Update()
        {
            if (_disposed) return;

            ProcessInstances();
        }

        /// <summary>音声メタデータの読み込み</summary>
        private static (float frequency, long bytes, double duration) LoadAudioMetadata(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException($"音声ファイルが見つかりません: {filePath}");

            var stream = Bass.BASS_StreamCreateFile(filePath, 0L, 0L,
                BASSFlag.BASS_SAMPLE_FLOAT | BASSFlag.BASS_STREAM_DECODE);

            if (stream == 0)
                throw new InvalidOperationException($"音声ファイルの読み込みに失敗: {Bass.BASS_ErrorGetCode()}");

            try
            {
                var bytes = Bass.BASS_ChannelGetLength(stream);
                var frequency = 0f;
                Bass.BASS_ChannelGetAttribute(stream, BASSAttribute.BASS_ATTRIB_FREQ, ref frequency);
                var duration = Bass.BASS_ChannelBytes2Seconds(stream, bytes);

                return (frequency, bytes, duration);
            }
            finally
            {
                Bass.BASS_StreamFree(stream);
            }
        }

        /// <summary>新しいオーディオインスタンスの作成</summary>
        private AudioInstance CreateInstance()
        {
            var instance = new AudioInstance(FilePath, _baseFrequency, _streamLength, Loop)
            {
                Pan = _initialPan,
                Pitch = _initialPitch,
                Volume = _initialVolume,
                Speed = _initialSpeed
            };
            _instances.Add(instance);
            return instance;
        }

        /// <summary>インスタンスの状態処理</summary>
        private void ProcessInstances()
        {
            for (int i = _instances.Count - 1; i >= 0; i--)
            {
                var instance = _instances[i];
                instance.Update();

                // 非アクティブなインスタンスを削除
                if (!instance.IsPlaying && !instance.IsFading)
                {
                    instance.Dispose();
                    _instances.RemoveAt(i);
                }
                // 区間ループの処理
                else if (ShouldLoopSection(instance))
                {
                    instance.Position = LoopBegin!.Value.TotalSeconds;
                }
            }
        }

        /// <summary>区間ループの必要性判定</summary>
        private bool ShouldLoopSection(AudioInstance instance)
        {
            return Loop &&
                   LoopBegin.HasValue &&
                   LoopEnd.HasValue &&
                   instance.Position >= LoopEnd.Value.TotalSeconds;
        }

        /// <summary>全インスタンスの即座停止と破棄</summary>
        private void StopAllInstances()
        {
            foreach (var instance in _instances.ToList())
            {
                instance.Stop();
                instance.Dispose();
            }
            _instances.Clear();
        }

        private void ThrowIfDisposed()
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(Audio));
        }
    }
}