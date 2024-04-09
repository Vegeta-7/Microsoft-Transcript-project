using System.Text;
using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;

string azureKey = "dd40a49e14c14e0fa292c73e612eff0f";
string azureLocation = "eastus";
string textFile = "Shakespeare.txt";
string waveFile = "Shakespeare.wav";

try
{
    FileInfo fileInfo = new FileInfo(waveFile);
    if (fileInfo.Exists)
    {
        var speechConfig = SpeechConfig.FromSubscription(azureKey, azureLocation);
        using var audioConfig = AudioConfig.FromWavFileInput(fileInfo.FullName);
        using var speechRecognizer = new SpeechRecognizer(speechConfig, audioConfig);
        var stopRecognition = new TaskCompletionSource<int>();

        FileStream fileStream = File.OpenWrite(textFile);
        StreamWriter streamWriter = new StreamWriter(fileStream, Encoding.UTF8);

        speechRecognizer.Recognized += (s, e) =>
        {
            switch(e.Result.Reason)
            {
                case ResultReason.RecognizedSpeech:
                    streamWriter.WriteLine(e.Result.Text);
                    break;
                case ResultReason.NoMatch:
                    Console.WriteLine("Speech could not be recognized.");
                    break;
            }
        };

        speechRecognizer.Canceled += (s, e) =>
        {
            if (e.Reason != CancellationReason.EndOfStream)
            {
                Console.WriteLine("Speech recognition canceled.");
            }
            stopRecognition.TrySetResult(0);
            streamWriter.Close();
        };

        speechRecognizer.SessionStopped += (s, e) =>
        {
            Console.WriteLine("Speech recognition stopped.");
            stopRecognition.TrySetResult(0);
            streamWriter.Close();
        };

        Console.WriteLine("Speech recognition started.");
        await speechRecognizer.StartContinuousRecognitionAsync();
        Task.WaitAny(new[] { stopRecognition.Task });
        await speechRecognizer.StopContinuousRecognitionAsync();
    }
}
catch (Exception ex)
{
    Console.WriteLine(ex.Message);
}