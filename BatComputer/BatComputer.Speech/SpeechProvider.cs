
using Microsoft.CognitiveServices.Speech;

namespace BatComputer.Speech;

public class SpeechProvider : IDisposable
{
    private readonly SpeechSynthesizer _synth;
    private readonly SpeechRecognizer _recognizer;

    public SpeechProvider(string region, string apiKey, string voiceName = "en-GB-AlfieNeural")
    {
        SpeechConfig config = SpeechConfig.FromSubscription(apiKey, region);
        config.SpeechSynthesisVoiceName = voiceName;

        _synth = new SpeechSynthesizer(config);
        _recognizer = new SpeechRecognizer(config);
    }

    public async Task<bool> SpeakAsync(string message)
    {
        SpeechSynthesisResult result = await _synth.SpeakTextAsync(message);

        if (result.Reason == ResultReason.Canceled)
        {
            var cancellation = SpeechSynthesisCancellationDetails.FromResult(result);
            return false;
        }

        return result.Reason == ResultReason.SynthesizingAudioCompleted;
    }

    public async Task<string?> RecognizeAsync()
    {
        SpeechRecognitionResult result = await _recognizer.RecognizeOnceAsync();

        if (result.Reason != ResultReason.RecognizedSpeech)
        {
            return null;
        }

        return result.Text;
    }

    public void Dispose()
    {
        _synth.Dispose();
        _recognizer.Dispose();
    }
}
