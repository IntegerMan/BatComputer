using System.ComponentModel;
using FlashCap;
using MattEland.BatComputer.Abstractions;
using MattEland.BatComputer.Abstractions.Widgets;
using Microsoft.SemanticKernel;

namespace MattEland.BatComputer.Plugins.Camera;

public class CameraPlugin
{
    private readonly IAppKernel _kernel;

    public CameraPlugin(IAppKernel kernel)
    {
        _kernel = kernel;
    }
    
    [SKFunction, Description("Gets an image from the user's camera, saves it to disk, and returns the path of that image")]
    public async Task<string> GetImageAsync()
    {
        // Find a suitable camera
        CaptureDevices devices = new();
        List<CaptureDeviceDescriptor> cameras = devices.EnumerateDescriptors().ToList();
        CaptureDeviceDescriptor? camera = cameras.FirstOrDefault(c => string.Equals(c.Name, "Default", StringComparison.OrdinalIgnoreCase));
        camera ??= cameras.FirstOrDefault();
        if (camera == null)
        {
            throw new InvalidOperationException("No camera was detected");
        }

        // Take the image frame
        byte[] imageData = await camera.TakeOneShotAsync(camera.Characteristics[0]);

        // Save to file
        string tempPath = Path.GetTempPath();
        string fileName = Path.Combine(tempPath, Path.ChangeExtension(Path.GetRandomFileName(), ".png"));
        await File.WriteAllBytesAsync(fileName, imageData);

        // Register and return it
        _kernel.AddWidget(new ImageFileWidget(fileName));
        return fileName;
    }

}
