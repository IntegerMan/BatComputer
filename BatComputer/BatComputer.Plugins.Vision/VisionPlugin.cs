using System.ComponentModel;
using System.Text;
using Azure;
using Azure.AI.Vision.Common;
using Azure.AI.Vision.ImageAnalysis;
using MattEland.BatComputer.Abstractions;
using Microsoft.SemanticKernel;

namespace MattEland.BatComputer.Plugins.Vision;

public class VisionPlugin
{
    private readonly IAppKernel _kernel;
    private readonly VisionServiceOptions _visionOptions;

    public VisionPlugin(IAppKernel kernel, string endpoint, string apiKey)
    {
        _kernel = kernel;
        AzureKeyCredential credentials = new(apiKey);
        _visionOptions = new VisionServiceOptions(endpoint, credentials);
    }

    [SKFunction,
     Description("Analyzes an image from its path on disk and returns a list of detected object representing what's in the image. The resulting string will need to be summarized.")]
    public async Task<string> AnalyzeDiskImageAsync(string filePath)
    {
        if (!File.Exists(filePath))
        {
            return $"I couldn't find an image at file '{filePath}'";
        }

        try
        {
            using VisionSource source = VisionSource.FromFile(filePath);

            ImageAnalysisOptions analysisOptions = new()
            {
                Features = ImageAnalysisFeature.Caption | ImageAnalysisFeature.Objects | ImageAnalysisFeature.Text
            };

            using ImageAnalyzer analyzer = new(_visionOptions, source, analysisOptions);

            ImageAnalysisResult result = await analyzer.AnalyzeAsync();

            switch (result.Reason)
            {
                case ImageAnalysisResultReason.Error:
                    ImageAnalysisErrorDetails error = ImageAnalysisErrorDetails.FromResult(result);
                    return $"There was a problem analyzing the image: {error.Message}";

                case ImageAnalysisResultReason.Analyzed:
                    StringBuilder sb = new();

                    sb.AppendLine($"Analyzed an image described as: {result.Caption.Content}.");
                    if (result.Objects != null && result.Objects.Any())
                    {
                        sb.AppendLine($"In this image the following objects were detected: {string.Join(", ", result.Objects.Select(o => o.Name))}.");
                    }

                    if (result.DenseCaptions != null && result.DenseCaptions.Any())
                    {
                        sb.AppendLine($"Additional descriptions: {string.Join(", ", result.DenseCaptions.Select(dc => dc.Content).Distinct())}.");
                    }

                    if (result.Text != null && result.Text.Lines.Any())
                    {
                        sb.AppendLine($"Text in the image: {string.Join(' ', result.Text.Lines.Select(l => l.Content))}.");
                    }

                    return sb.ToString();

                default:
                    return $"The image analysis result returned an unexpected status: {result.Reason}";
            }
        }
        catch (ApplicationException)
        {
            return "A problem reading the image occurred. The file may not exist or may be invalid.";
        }
    }
}
