using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using PixelFormat = System.Drawing.Imaging.PixelFormat;

namespace Fork.Logic.Utils;

public class ImageUtils
{
    /// <summary>
    ///     Resize the image to the specified width and height.
    /// </summary>
    /// <param name="image">The image to resize.</param>
    /// <param name="width">The width to resize to.</param>
    /// <param name="height">The height to resize to.</param>
    /// <returns>The resized image.</returns>
    public static Bitmap ResizeImage(Image image, int width, int height)
    {
        Rectangle destRect = new Rectangle(0, 0, width, height);
        Bitmap destImage = new Bitmap(width, height, PixelFormat.Format32bppArgb);

        destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

        using (Graphics graphics = Graphics.FromImage(destImage))
        {
            graphics.CompositingMode = CompositingMode.SourceCopy;
            graphics.CompositingQuality = CompositingQuality.HighQuality;
            graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
            graphics.SmoothingMode = SmoothingMode.HighQuality;
            graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

            using (ImageAttributes wrapMode = new ImageAttributes())
            {
                wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
            }
        }

        return destImage;
    }

    public static ImageSource BitmapToImageSource(Bitmap src)
    {
        return Imaging.CreateBitmapSourceFromHIcon(src.GetHicon(), Int32Rect.Empty,
            BitmapSizeOptions.FromEmptyOptions());
        MemoryStream ms = new();
        src.Save(ms, ImageFormat.Bmp);
        BitmapImage image = new();
        image.BeginInit();
        ms.Seek(0, SeekOrigin.Begin);
        image.StreamSource = ms;
        image.EndInit();
        return image;
    }
}