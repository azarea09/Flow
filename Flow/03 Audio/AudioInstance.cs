using Flow.Internal.Audio;
using Un4seen.Bass;
using Un4seen.Bass.AddOn.Fx;
using Un4seen.Bass.AddOn.Mix;

namespace Flow
{
    /// <summary>
    /// 音声の個別インスタンス - 単一音声ファイルの再生制御を担当
    /// </summary>
    public class AudioInstance : IDisposable
    {
        private const int INVALID_HANDLE = 0;
        private const double FADE_THRESHOLD = 0.001;
        private const double VOLUME_CURVE_EXPONENT = 0.2; // 人間の聴覚特性に基づく対数カーブ

        private int _handle = INVALID_HANDLE;
        private readonly float _baseFrequency; // 元の音声ファイルの周波数
        private readonly long _streamLength; // ストリームの総バイト数

        // フェード制御
        private double _fadeVolume = 1.0;
        private double _fadeTarget = 1.0;
        private double _fadeSpeed = 0.0;
        private bool _stopAfterFade = false;

        // 状態管理
        private double _volume = 1.0;
        private bool _disposed = false;

        /// <summary>デバッグ用ハンドル値</summary>
        public int Handle => _handle;

        /// <summary>ハンドルが有効かどうか</summary>
        public bool IsHandleValid => _handle != INVALID_HANDLE;

        /// <summary>現在再生中かどうか</summary>
        public bool IsPlaying
        {
            get
            {
                if (!IsHandleValid)
                {
                    return false;
                }

                var status = BassMix.BASS_Mixer_ChannelIsActive(_handle);
                return status == BASSActive.BASS_ACTIVE_PLAYING;
            }
        }

        /// <summary>フェード処理中かどうか</summary>
        public bool IsFading => Math.Abs(_fadeVolume - _fadeTarget) > FADE_THRESHOLD;

        /// <summa
        /// ry>破棄済みかどうか</summary>
        public bool IsDisposed => _disposed;

        /// <summary>フェード音量</summary>
        public double FadeVolume { get => _fadeVolume; }

        /// <summary>音量 (0.0～1.0)</summary>
        public double Volume
        {
            get => _volume;
            set
            {
                _volume = Math.Max(0.0, value);
                UpdateVolumeAttribute();
            }
        }

        /// <summary>パン (-1.0～1.0)</summary>
        public double Pan
        {
            get => GetAttribute(BASSAttribute.BASS_ATTRIB_PAN);
            set => SetAttribute(BASSAttribute.BASS_ATTRIB_PAN, (float)Math.Clamp(value, -1.0, 1.0));
        }

        /// <summary>ピッチ</summary>
        public double Pitch
        {
            get => GetAttribute(BASSAttribute.BASS_ATTRIB_TEMPO_PITCH);
            set => SetAttribute(BASSAttribute.BASS_ATTRIB_TEMPO_PITCH, (float)value);
        }

        /// <summary>再生速度 (0.1～10.0)</summary>
        public double Speed
        {
            get => IsHandleValid ? GetAttribute(BASSAttribute.BASS_ATTRIB_FREQ) / _baseFrequency : 1.0;
            set => SetAttribute(BASSAttribute.BASS_ATTRIB_FREQ, _baseFrequency * (float)Math.Clamp(value, 0.1, 10.0));
        }

        /// <summary>再生位置 (秒)</summary>
        public double Position
        {
            get => IsHandleValid ? Bass.BASS_ChannelBytes2Seconds(_handle, BassMix.BASS_Mixer_ChannelGetPosition(_handle)) : 0;
            set
            {
                if (IsHandleValid)
                {
                    var bytes = Bass.BASS_ChannelSeconds2Bytes(_handle, Math.Max(0, value));
                    BassMix.BASS_Mixer_ChannelSetPosition(_handle, bytes);
                }
            }
        }

        internal AudioInstance(string filePath, float baseFreq, long bytes, bool loop)
        {
            _baseFrequency = baseFreq;
            _streamLength = bytes;
            LoadAudioStream(filePath, loop);
        }

        /// <summary>再生開始</summary>
        public void Play()
        {
            Console.WriteLine("=== 再生開始 ===");

            if (_disposed)
            {
                Console.WriteLine("インスタンスは既に破棄されています");
                return;
            }

            if (!IsHandleValid)
            {
                Console.WriteLine("無効なハンドル");
                return;
            }

            Console.WriteLine($"ハンドル: {_handle}, ミキサー: {AudioManager.Mixer}");

            Position = 0; // 先頭に戻す

            if (BassMix.BASS_Mixer_ChannelPlay(_handle))
            {
                LogPlaybackStatus();
                Console.WriteLine("再生開始成功");
            }
            else
            {
                var error = Bass.BASS_ErrorGetCode();
                Console.WriteLine($"再生開始失敗: {error}");
            }
        }

        /// <summary>停止</summary>
        public void Stop(TimeSpan? fadeOut = null)
        {
            if (fadeOut.HasValue)
            {
                FadeOut(fadeOut.Value);
            }
            else if (IsHandleValid)
            {
                Console.WriteLine("=== 再生停止 ===");
                Console.WriteLine($"ハンドル: {_handle}");
                if (!BassMix.BASS_Mixer_ChannelPause(_handle))
                {
                    Console.WriteLine($"停止エラー: {Bass.BASS_ErrorGetCode()}");
                }
            }
        }

        /// <summary>一時停止</summary>
        public void Pause()
        {
            if (IsHandleValid)
                BassMix.BASS_Mixer_ChannelPause(_handle);
        }

        /// <summary>再開</summary>
        public void Resume()
        {
            if (IsHandleValid)
                BassMix.BASS_Mixer_ChannelPlay(_handle);
        }

        /// <summary>フェードイン</summary>
        public void FadeIn(TimeSpan duration, double targetVolume = 1.0)
        {
            if (duration.TotalSeconds <= 0) return;

            _fadeVolume = 0.0;
            _fadeTarget = Math.Clamp(targetVolume, 0.0, 1.0);
            _fadeSpeed = _fadeTarget / duration.TotalSeconds;
            _stopAfterFade = false;

            Console.WriteLine($"フェードイン開始: 速度={_fadeSpeed:F3}");
        }

        /// <summary>フェードアウト</summary>
        public void FadeOut(TimeSpan duration)
        {
            if (duration.TotalSeconds <= 0)
            {
                Stop();
                return;
            }

            _fadeTarget = 0.0;
            _fadeSpeed = _fadeVolume / duration.TotalSeconds;
            _stopAfterFade = true;

            Console.WriteLine($"フェードアウト開始: 速度={_fadeSpeed:F3}");
        }

        /// <summary>指定音量までフェード</summary>
        public void FadeTo(double targetVolume, TimeSpan duration)
        {
            if (duration.TotalSeconds <= 0)
            {
                _fadeVolume = _fadeTarget = Math.Clamp(targetVolume, 0.0, 1.0);
                UpdateVolumeAttribute();
                return;
            }

            _fadeTarget = Math.Clamp(targetVolume, 0.0, 1.0);
            _fadeSpeed = Math.Abs(_fadeTarget - _fadeVolume) / duration.TotalSeconds;
            _stopAfterFade = false;
        }

        /// <summary>フレーム更新処理</summary>
        public void Update()
        {
            if (_disposed) return;

            ProcessFading();
        }

        /// <summary>詳細ステータス情報（デバッグ用）</summary>
        public string GetDetailedStatus()
        {
            if (_disposed) return "Disposed";
            if (!IsHandleValid) return "Invalid Handle";

            var mixerStatus = BassMix.BASS_Mixer_ChannelIsActive(_handle);
            var channelInfo = Bass.BASS_ChannelGetInfo(_handle);
            var position = BassMix.BASS_Mixer_ChannelGetPosition(_handle);

            return $"Handle:{_handle}, Mixer:{mixerStatus}, Pos:{position}/{_streamLength}, Type:{channelInfo.ctype}";
        }

        /// <summary>リソース解放</summary>
        public void Dispose()
        {
            if (_disposed) return;

            Console.WriteLine($"AudioInstance破棄 - ハンドル: {_handle}");

            if (IsHandleValid)
            {
                // ミキサーから削除
                var removeResult = BassMix.BASS_Mixer_ChannelRemove(_handle);
                Console.WriteLine($"ミキサーから削除: {removeResult}");

                // ストリーム解放
                var freeResult = Bass.BASS_StreamFree(_handle);
                Console.WriteLine($"ストリーム解放: {freeResult}");

                _handle = INVALID_HANDLE;
            }

            _disposed = true;
        }
        /// <summary>音声ストリームの読み込みと初期化</summary>
        private void LoadAudioStream(string filePath, bool loop)
        {
            Console.WriteLine($"=== 音声読み込み開始: {filePath} ===");

            // 基本デコードストリーム作成
            var baseStream = Bass.BASS_StreamCreateFile(filePath, 0L, 0L,
                BASSFlag.BASS_SAMPLE_FLOAT | BASSFlag.BASS_STREAM_DECODE);

            if (baseStream == INVALID_HANDLE)
                throw new InvalidOperationException($"音声ストリーム作成失敗: {Bass.BASS_ErrorGetCode()}");

            try
            {
                // テンポエフェクト付きストリーム作成
                _handle = BassFx.BASS_FX_TempoCreate(baseStream, BASSFlag.BASS_STREAM_DECODE);

                if (_handle == INVALID_HANDLE)
                    throw new InvalidOperationException($"テンポエフェクト作成失敗: {Bass.BASS_ErrorGetCode()}");

                ConfigureStream(loop);
                AddToMixer();

                Console.WriteLine("音声読み込み完了");
            }
            catch
            {
                Bass.BASS_StreamFree(baseStream);
                throw;
            }
        }

        /// <summary>ストリーム設定</summary>
        private void ConfigureStream(bool loop)
        {
            if (loop)
                Bass.BASS_ChannelFlags(_handle, BASSFlag.BASS_SAMPLE_LOOP, BASSFlag.BASS_SAMPLE_LOOP);

            LogChannelInfo();
        }

        /// <summary>ミキサーに追加</summary>
        private void AddToMixer()
        {
            Console.WriteLine($"ミキサーID: {AudioManager.Mixer}");

            if (!BassMix.BASS_Mixer_StreamAddChannel(AudioManager.Mixer, _handle, BASSFlag.BASS_SPEAKER_FRONT))
            {
                var error = Bass.BASS_ErrorGetCode();
                Bass.BASS_StreamFree(_handle);
                _handle = INVALID_HANDLE;
                throw new InvalidOperationException($"ミキサーへの追加失敗: {error}");
            }

            // 初期状態で一時停止
            BassMix.BASS_Mixer_ChannelPause(_handle);
            Console.WriteLine("ミキサーに追加完了");
        }

        /// <summary>チャンネル属性の設定</summary>
        private void SetAttribute(BASSAttribute attr, float value)
        {
            if (!IsHandleValid || !IsValidFloat(value)) return;

            if (!Bass.BASS_ChannelSetAttribute(_handle, attr, value))
            {
                Console.WriteLine($"属性設定失敗 {attr}={value}: {Bass.BASS_ErrorGetCode()}");
            }
        }

        /// <summary>チャンネル属性の取得</summary>
        private float GetAttribute(BASSAttribute attr)
        {
            if (!IsHandleValid) return 0f;

            float value = 0f;
            if (!Bass.BASS_ChannelGetAttribute(_handle, attr, ref value))
            {
                Console.WriteLine($"属性取得失敗 {attr}: {Bass.BASS_ErrorGetCode()}");
            }
            return value;
        }

        /// <summary>音量属性の更新</summary>
        private void UpdateVolumeAttribute()
        {
            // 聴覚特性を考慮した対数カーブ適用
            var actualVolume = (float)Math.Pow(_volume * _fadeVolume, VOLUME_CURVE_EXPONENT);
            Console.WriteLine($"音量更新: ベース音量={_volume:F2}, 現在の音量(見かけ上)={_fadeVolume:F2}, 現在の音量(実際の値)={actualVolume:F2}");
            SetAttribute(BASSAttribute.BASS_ATTRIB_VOL, actualVolume);
        }

        /// <summary>フェード処理</summary>
        private void ProcessFading()
        {
            if (!IsFading) return;

            var fadeStep = _fadeSpeed * Engine.DeltaTime;

            _fadeVolume = _fadeVolume < _fadeTarget
                ? Math.Min(_fadeTarget, _fadeVolume + fadeStep)
                : Math.Max(_fadeTarget, _fadeVolume - fadeStep);

            UpdateVolumeAttribute();

            // フェード完了後の処理
            if (!IsFading && _stopAfterFade)
                Stop();
        }

        /// <summary>浮動小数点数の有効性チェック</summary>
        private static bool IsValidFloat(float value) => !float.IsNaN(value) && !float.IsInfinity(value);

        /// <summary>再生状態ログ出力</summary>
        private void LogPlaybackStatus()
        {
            var status = BassMix.BASS_Mixer_ChannelIsActive(_handle);
            var position = BassMix.BASS_Mixer_ChannelGetPosition(_handle);
            float vol = 0f;

            Bass.BASS_ChannelGetAttribute(_handle, BASSAttribute.BASS_ATTRIB_VOL, ref vol);

            Console.WriteLine($"再生状態: {status}, 位置: {position}, 音量: {vol:F2}");
        }

        /// <summary>チャンネル情報ログ出力</summary>
        private void LogChannelInfo()
        {
            var info = Bass.BASS_ChannelGetInfo(_handle);
            Console.WriteLine($"チャンネル情報: 周波数={info.freq}Hz, チャンネル={info.chans}ch, タイプ={info.ctype}");
        }
    }
}