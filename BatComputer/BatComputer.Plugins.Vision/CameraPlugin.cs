using System.ComponentModel;
using FlashCap;
using MattEland.BatComputer.Abstractions;
using MattEland.BatComputer.Abstractions.Widgets;
using Microsoft.SemanticKernel;

namespace BatComputer.Plugins.Vision;




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
        // Take only one image, given the image characteristics:
        CaptureDevices devices = new();
        CaptureDeviceDescriptor camera = devices.EnumerateDescriptors().ElementAt(0);

        byte[] imageData = await camera.TakeOneShotAsync(camera.Characteristics[0]);

        // Save to file
        string fileName = Path.GetTempFileName();
        await File.WriteAllBytesAsync(fileName, imageData);

        // Register and return it
        _kernel.AddWidget(new ImageFileWidget(fileName));
        return fileName;
    }

}
