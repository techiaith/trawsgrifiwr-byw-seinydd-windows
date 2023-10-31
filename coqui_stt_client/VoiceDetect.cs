using WebRtcVadSharp;

namespace coqui_stt_client
{
    public class VoiceDetect
    {
        public static bool DoesFrameContainSpeech(byte[] audioFrame)
        {
            using var vad = new WebRtcVad();
            return vad.HasSpeech(audioFrame, SampleRate.Is16kHz, FrameLength.Is10ms);
        }
    }
}
