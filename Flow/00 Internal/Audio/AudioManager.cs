using Un4seen.Bass;
using Un4seen.Bass.AddOn.Mix;
using Un4seen.BassWasapi;

namespace Flow.Internal.Audio
{
    /// <summary>
    /// オーディオシステムの中央管理 - BASS/WASAPI の初期化と全体制御
    /// </summary>
    public static class AudioManager
    {
        private const int INVALID_HANDLE = 0;
        private const string BASS_REGISTRATION_EMAIL = "dtx2013@gmail.com";
        private const string BASS_REGISTRATION_KEY = "2X9181017152222";

        private static int _mixer = INVALID_HANDLE; // メインミキサーハンドル
        private static int _outputMixer = INVALID_HANDLE; // 出力用ミキサーハンドル
        private static WASAPIPROC _wasapiCallback; // WASAPIコールバック関数
        private static readonly List<Flow.Audio> _registeredAudios = new(); // 登録済みAudioオブジェクト
        private static bool _isInitialized = false;

        /// <summary>メインミキサーハンドル</summary>
        public static int Mixer => _mixer;

        /// <summary>初期化済みかどうか</summary>
        public static bool IsInitialized => _isInitialized;

        /// <summary>オーディオシステムの初期化</summary>
        /// <param name="exclusive">排他モードで初期化するか</param>
        /// <param name="sampleRate">サンプルレート</param>
        /// <param name="bufferLength">バッファ長</param>
        /// <param name="period">更新間隔</param>
        public static void Init(bool exclusive = false, int sampleRate = 44100, float bufferLength = 0, float period = 0)
        {
            if (_isInitialized) return;

            try
            {
                Console.WriteLine("=== AudioManager初期化開始 ===");
                InitializeBass(sampleRate);
                InitializeWasapi(exclusive, bufferLength, period);
                CreateMixers();
                StartAudioProcessing();

                _isInitialized = true;
                Console.WriteLine("AudioManager初期化完了");
            }
            catch
            {
                Free();
                throw;
            }
        }

        /// <summary>全体更新処理（毎フレーム呼び出し）</summary>
        public static void Update()
        {
            if (!_isInitialized) return;

            // 登録されたAudioオブジェクトを安全に更新
            for (int i = _registeredAudios.Count - 1; i >= 0; i--)
            {
                if (i < _registeredAudios.Count)
                    _registeredAudios[i].Update();
            }
        }

        /// <summary>マスターボリューム設定</summary>
        public static void SetMasterVolume(float volume)
        {
            volume = Math.Clamp(volume, 0f, 1f);

            if (IsValidHandle(_mixer))
            {
                Bass.BASS_ChannelSetAttribute(_mixer, BASSAttribute.BASS_ATTRIB_VOL, volume);
                Bass.BASS_SetConfig(BASSConfig.BASS_CONFIG_GVOL_STREAM, (int)(volume * 10000f));
                Console.WriteLine($"マスターボリューム設定: {volume:F2}");
            }
        }

        /// <summary>全ての音声を停止</summary>
        public static void StopAll(TimeSpan? fadeOut = null)
        {
            foreach (var audio in _registeredAudios.ToList())
                audio.Stop(fadeOut);
        }

        /// <summary>Audioオブジェクトの登録</summary>
        public static void Register(Flow.Audio audio)
        {
            if (audio != null && !_registeredAudios.Contains(audio))
                _registeredAudios.Add(audio);
        }

        /// <summary>Audioオブジェクトの登録解除</summary>
        public static void Unregister(Flow.Audio audio)
        {
            _registeredAudios.Remove(audio);
        }

        /// <summary>システム全体のリソース解放</summary>
        public static void Free()
        {
            Console.WriteLine("=== AudioManager解放開始 ===");

            // 全ての登録済みAudioを破棄
            foreach (var audio in _registeredAudios.ToList())
                audio.Dispose();
            _registeredAudios.Clear();

            // ミキサーの解放
            FreeMixers();

            // WASAPI/BASSの解放
            FreeAudioSystems();

            _isInitialized = false;
            Console.WriteLine("AudioManager解放完了");
        }

        /// <summary>BASS音声ライブラリの初期化</summary>
        private static void InitializeBass(int sampleRate)
        {
            // ライセンス登録
            BassNet.Registration(BASS_REGISTRATION_EMAIL, BASS_REGISTRATION_KEY);

            // BASS設定
            Bass.BASS_SetConfig(BASSConfig.BASS_CONFIG_UPDATEPERIOD, 0);
            Bass.BASS_SetConfig(BASSConfig.BASS_CONFIG_UPDATETHREADS, 0);

            // BASS初期化
            if (!Bass.BASS_Init(-1, sampleRate, BASSInit.BASS_DEVICE_LATENCY, IntPtr.Zero))
                throw new InvalidOperationException($"BASS初期化失敗: {Bass.BASS_ErrorGetCode()}");

            // 音量カーブ設定
            Bass.BASS_SetConfig(BASSConfig.BASS_CONFIG_CURVE_VOL, newvalue: true);

            Console.WriteLine($"BASS初期化完了 - サンプルレート: {sampleRate}Hz");
        }

        /// <summary>WASAPI音声出力の初期化</summary>
        private static void InitializeWasapi(bool exclusive, float bufferLength = 0, float period = 0)
        {
            var flags = exclusive
                ? BASSWASAPIInit.BASS_WASAPI_EXCLUSIVE | BASSWASAPIInit.BASS_WASAPI_BUFFER
                : BASSWASAPIInit.BASS_WASAPI_BUFFER;

            _wasapiCallback = ProcessWasapiCallback;

            // デフォルトデバイス番号取得
            int defaultDevice = -1;
            BASS_WASAPI_DEVICEINFO deviceInfo = null;
            for (int device = 0; (deviceInfo = BassWasapi.BASS_WASAPI_GetDeviceInfo(device)) != null; device++)
            {
                if (deviceInfo.IsDefault && deviceInfo.IsEnabled)
                {
                    defaultDevice = device;
                    break;
                }
            }
            if (defaultDevice == -1)
                throw new InvalidOperationException("WASAPIのデフォルト出力デバイスが見つかりません");

            // デバイスの最小処理間隔(minperiod)取得
            float minPeriodSec = deviceInfo.minperiod; // 秒
            float defPeriodSec = deviceInfo.defperiod; // 秒

            // 自動計算
            // 更新間隔(period)はminperiodを採用
            float autoPeriod = minPeriodSec > 0 ? minPeriodSec : 0.005f; // 安全に5msデフォルト
                                                                         // バッファ長は period の倍くらい
            float autoBuffer = autoPeriod * 2;
            if (autoBuffer < autoPeriod + 0.001f)
                autoBuffer = autoPeriod + 0.001f;

            // 引数が0以下なら自動値で上書き
            if (period <= 0) period = autoPeriod;
            if (bufferLength <= 0) bufferLength = autoBuffer;

            // 初期化実行
            if (!BassWasapi.BASS_WASAPI_Init(defaultDevice, 0, 0, flags, bufferLength, period, _wasapiCallback, IntPtr.Zero))
                throw new InvalidOperationException($"WASAPI初期化失敗: {Bass.BASS_ErrorGetCode()}");

            Console.WriteLine($"WASAPI初期化完了 - 排他モード: {exclusive}");
            Console.WriteLine($"使用デバイス: {deviceInfo.name}");
            Console.WriteLine($"minperiod: {minPeriodSec * 1000:F2} ms, defperiod: {defPeriodSec * 1000:F2} ms");
            Console.WriteLine($"設定 period: {period * 1000:F2} ms, buffer: {bufferLength * 1000:F2} ms");
        }


        /// <summary>ミキサーストリームの作成</summary>
        private static void CreateMixers()
        {
            var wasapiInfo = BassWasapi.BASS_WASAPI_GetInfo();
            var mixerFlags = BASSFlag.BASS_SAMPLE_FLOAT | BASSFlag.BASS_STREAM_DECODE;

            // メインミキサー作成
            _mixer = BassMix.BASS_Mixer_StreamCreate(wasapiInfo.freq, wasapiInfo.chans, mixerFlags);

            // 出力ミキサー作成
            _outputMixer = BassMix.BASS_Mixer_StreamCreate(wasapiInfo.freq, wasapiInfo.chans, mixerFlags);

            Console.WriteLine($"メインミキサー: {_mixer}, 出力ミキサー: {_outputMixer}");

            if (!IsValidHandle(_mixer) || !IsValidHandle(_outputMixer))
                throw new InvalidOperationException($"ミキサー作成失敗: {Bass.BASS_ErrorGetCode()}");

            // ミキサー間の接続
            if (!BassMix.BASS_Mixer_StreamAddChannel(_outputMixer, _mixer, BASSFlag.BASS_DEFAULT))
                throw new InvalidOperationException($"ミキサー接続失敗: {Bass.BASS_ErrorGetCode()}");

            Console.WriteLine("ミキサー作成・接続完了");
        }

        /// <summary>音声処理の開始</summary>
        private static void StartAudioProcessing()
        {
            if (!BassWasapi.BASS_WASAPI_Start())
                throw new InvalidOperationException($"WASAPI開始失敗: {Bass.BASS_ErrorGetCode()}");

            Console.WriteLine("音声処理開始");
        }

        /// <summary>ミキサーリソースの解放</summary>
        private static void FreeMixers()
        {
            if (IsValidHandle(_outputMixer))
            {
                Bass.BASS_StreamFree(_outputMixer);
                _outputMixer = INVALID_HANDLE;
                Console.WriteLine("出力ミキサー解放");
            }

            if (IsValidHandle(_mixer))
            {
                Bass.BASS_StreamFree(_mixer);
                _mixer = INVALID_HANDLE;
                Console.WriteLine("メインミキサー解放");
            }
        }

        /// <summary>音声システムの解放</summary>
        private static void FreeAudioSystems()
        {
            BassWasapi.BASS_WASAPI_Stop(true);
            BassWasapi.BASS_WASAPI_Free();
            Bass.BASS_Free();

            Console.WriteLine("WASAPI/BASS解放完了");
        }

        /// <summary>WASAPIコールバック処理</summary>
        private static int ProcessWasapiCallback(IntPtr buffer, int length, IntPtr user)
        {
            if (!IsValidHandle(_outputMixer)) return 0;

            var bytesRead = Bass.BASS_ChannelGetData(_outputMixer, buffer, length);
            return bytesRead == -1 ? 0 : bytesRead;
        }

        /// <summary>ハンドルの有効性チェック</summary>
        private static bool IsValidHandle(int handle) => handle != INVALID_HANDLE;
    }
}