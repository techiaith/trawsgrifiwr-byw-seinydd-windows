using NAudio.Wave;
using NAudio.CoreAudioApi;

using STTClient.Models;


namespace coqui_stt_client
{

    public class RealtimeTranscriber
    {

        const String DEFAULT_MODEL = "models/techiaith_bangor_22.10.tflite";
        const String DEFAULT_KENLM_SCORER = "models/techiaith_bangor_transcription_22.10.scorer";

        private String tmpWavFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "tmpstt.wav");

        private static int NON_SPEECH_DATA_LIMIT = 1;
        private int _nonSpeechCount = 0;
       
        public String model { get; private set; }
        public String kenlm_scorer { get; private set; }

        private STTClient.Interfaces.ISTT _sttClient;
        private STTStream _sttStream;
        private String _transcript = String.Empty;

        //private WaveInEvent _waveSource;
        private WasapiLoopbackCapture _waveSource;
       
        private bool _isListening = false;
        private string _intermediateTranscription = String.Empty;

        //
        public RealtimeTranscriber(String model = DEFAULT_MODEL,
                                   String kenlm_scorer = DEFAULT_KENLM_SCORER)
        {           
            //
            InitializeModels(model, kenlm_scorer);

            //
            InitializeAudioDevices();
        }


        public String InitializeModels(String model, String kenlm_scorer)
        {
            Console.Out.WriteLine("Creating Speech Recognition Engine");

            this.model = String.IsNullOrEmpty(model) ? DEFAULT_MODEL : model;
            this.kenlm_scorer = String.IsNullOrEmpty(kenlm_scorer) ? DEFAULT_KENLM_SCORER : kenlm_scorer;

            String message = String.Empty;

            Tuple<string, bool> avx = avx_checks.isAvxSupported();
            if (avx.Item2 == false)
            {
                message =
                    "Methwyd creu'r peiriant DeepSpeech oherwydd ddiffyg yn CPU ("
                    + avx.Item1 + ") y cyfrifiadur.\n\n"
                    + "Mae angen cyfrifiadur sydd a fath diweddar o CPU (fel Intel Core i3/5/7/9) ac sy'n cynorthwyo AVX.\n\n"
                    + "Defnyddiwch raglen fel CoreInfo64 i ddysgu mwy am eich CPU.\n\n"
                    + "RHYBUDD: Am defnyddio peiriant DeepSpeech ar-lein fel ddarpariaeth amgen.";             

                Console.Out.WriteLine(message);
                Console.Out.WriteLine();
            }
            else
            {
                try
                {
                    _sttClient = new STTClient.STT(this.model);
                    _sttClient.EnableExternalScorer(this.kenlm_scorer);
                }
                catch (Exception exc)
                {
                    message = "Methwyd creu'r peiriant adnabod lleferydd am rheswm anhysbys.\n\n" + exc.Message;
                    message += "RHYBUDD: Am defnyddio peiriant ar-lein fel ddarpariaeth amgen.";
              
                    Console.Out.WriteLine(message);
                    Console.Out.WriteLine();

                }
            }

            return message;

        }

        private void InitializeAudioDevices()
        {
            MMDevice device = WasapiLoopbackCapture.GetDefaultLoopbackCaptureDevice();
            //Console.WriteLine(device.FriendlyName);
            _waveSource = new WasapiLoopbackCapture(); // new WaveInEvent();
            _waveSource.WaveFormat = new WaveFormat(16000, 1);
            _waveSource.DataAvailable += new EventHandler<WaveInEventArgs>(onWaveSource_DataAvailable);
            _waveSource.RecordingStopped += new EventHandler<StoppedEventArgs>(onWaveSource_StopRecording);
        }

        public void StartRecording()
        {
            if (_isListening == false)
            {
                //Console.Out.WriteLine("Yn recordio...");
                _transcript = String.Empty;
                _nonSpeechCount = 0;
                _isListening = true;
                _waveSource.StartRecording();
                _sttStream = _sttClient.CreateStream();
            }

           
        }


        public void StopRecording()
        {           
            _waveSource.StopRecording();         
        }


        private void onWaveSource_DataAvailable(object sender, WaveInEventArgs e)
        {           
            if (VoiceDetect.DoesFrameContainSpeech(e.Buffer))
            {
                //
                // https://stackoverflow.com/a/4060095
                //
                short[] sdata = new short[(int)Math.Ceiling((double)e.Buffer.Length / 2)];
                Buffer.BlockCopy(e.Buffer, 0, sdata, 0, e.Buffer.Length);

                //
                _sttClient.FeedAudioContent(_sttStream, sdata, Convert.ToUInt32(sdata.Length));
                _intermediateTranscription = _sttClient.IntermediateDecode(_sttStream);
                
                //
                Console.Out.Write("\r" + countersToString() + _intermediateTranscription);
            }
            else
            {
                _nonSpeechCount++;
                StopRecording();
                Console.Out.Write("\r" + String.Empty);           
            }
        }


        private string countersToString()
        {
            if (_isListening)
                return "Yn trawsgrifio.. " + (NON_SPEECH_DATA_LIMIT - _nonSpeechCount).ToString() + " ";
            else
                return String.Empty; 
        }


        private void onWaveSource_StopRecording(object sender, StoppedEventArgs e)
        {
            //Console.Out.WriteLine("stopped recording..");
            _transcript = _sttClient.FinishStream(_sttStream);           
            _sttStream.Dispose();
            _isListening = false;   
        }


        public bool isListening()
        {
            return _isListening;
        }


        public String GetTranscript()
        {
            return _transcript;            
        }

    }

}
