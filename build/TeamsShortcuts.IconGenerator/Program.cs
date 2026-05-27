using SkiaSharp;
using Svg.Skia;

if (args.Length != 1)
{
    Console.Error.WriteLine("Usage: TeamsShortcuts.IconGenerator <image-directory>");
    return 1;
}

var imageDirectory = args[0];
if (!Directory.Exists(imageDirectory))
{
    Console.Error.WriteLine($"Image directory does not exist: {imageDirectory}");
    return 1;
}

foreach (var svgPath in Directory.EnumerateFiles(imageDirectory, "*.svg"))
{
    Render(svgPath, Path.ChangeExtension(svgPath, ".png"), 72);
    Render(svgPath, Path.Combine(
        Path.GetDirectoryName(svgPath)!,
        Path.GetFileNameWithoutExtension(svgPath) + "@2x.png"), 144);
    File.Delete(svgPath);
}

return 0;

static void Render(string svgPath, string pngPath, int pixels)
{
    var svg = new SKSvg();
    var picture = svg.Load(svgPath);
    if (picture is null)
    {
        throw new InvalidOperationException($"Could not load SVG: {svgPath}");
    }

    var source = picture.CullRect;
    var scale = Math.Min(pixels / source.Width, pixels / source.Height);
    var x = (pixels - (source.Width * scale)) / 2f;
    var y = (pixels - (source.Height * scale)) / 2f;

    using var bitmap = new SKBitmap(pixels, pixels, SKColorType.Rgba8888, SKAlphaType.Premul);
    using var canvas = new SKCanvas(bitmap);
    canvas.Clear(SKColors.Transparent);
    canvas.Translate(x, y);
    canvas.Scale(scale);
    canvas.DrawPicture(picture);
    canvas.Flush();

    using var image = SKImage.FromBitmap(bitmap);
    using var data = image.Encode(SKEncodedImageFormat.Png, 100);
    using var stream = File.Open(pngPath, FileMode.Create, FileAccess.Write);
    data.SaveTo(stream);
}
