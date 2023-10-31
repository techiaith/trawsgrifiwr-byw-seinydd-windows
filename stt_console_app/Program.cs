using coqui_stt_client;

namespace stt_console_app
{
    class Program
    {   
        //
        static void Main(string[] args)
        {
            RealtimeTranscriber transcriber = new RealtimeTranscriber();
           
            Console.Out.WriteLine("\nPwyswch 'Return' i mi ddechrau gwrando ar seinydd eich cyfrifiadur a thrawsgrifio'n fyw...");
            Console.In.ReadLine();

            while (true)
            {                     
                transcriber.StartRecording();
                while (true)
                {
                    if (transcriber.isListening())
                    {
                        continue;
                    } 
                    else 
                    {
                        String transcript = transcriber.GetTranscript();
                        if (!String.IsNullOrEmpty(transcript) )
                        {
                            Console.Out.WriteLine("Trawsgrifiad: " + transcript);
                        }
                        break;
                    }
                }            
            }
        }
    }
}