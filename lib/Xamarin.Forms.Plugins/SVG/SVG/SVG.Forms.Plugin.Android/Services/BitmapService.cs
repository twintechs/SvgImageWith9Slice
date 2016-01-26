using System.Threading.Tasks;
using Android.Graphics;
using System.IO;
using NGraphics;
using SVG.Forms.Plugin.Abstractions;

namespace SVG.Forms.Plugin.Droid
{
	public static class BitmapService
	{
		public static async Task<Bitmap> GetBitmapAsync (SvgImage svgImage, int width, int height)
		{
			Bitmap result = null;

			Stream svgStream = await SvgService.GetSvgStreamAsync (svgImage).ConfigureAwait (false);

      await Task.Run(() =>
      {
          var svgReader = new SvgReader(new StreamReader(svgStream));

        var graphics = svgReader.Graphic;

        var scale = 1.0;

        if (height >= width)
        {
          scale = height / graphics.Size.Height;
        }
        else
        {
          scale = width / graphics.Size.Width;
        }

        var canvas = new AndroidPlatform().CreateImageCanvas(graphics.Size, scale);
        graphics.Draw(canvas);
        var image = (BitmapImage)canvas.GetImage();
        result = image.Bitmap;
      });

			return result;
		}
	}
}