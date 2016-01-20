using System;
using System.IO;
using Android.Widget;
using SVG.Forms.Plugin.Abstractions;
using SVG.Forms.Plugin.Droid;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;
using System.Threading.Tasks;
using Android.Runtime;
using NGraphics.Android.Custom;
using NGraphics.Custom.Parsers;
using Color = NGraphics.Custom.Models.Color;
using Size = NGraphics.Custom.Models.Size;
using Rect = NGraphics.Custom.Models.Rect;
using NGraphics.Custom.Codes;

[assembly: ExportRenderer (typeof(SvgImage), typeof(SvgImageRenderer))]
namespace SVG.Forms.Plugin.Droid
{
    [Preserve(AllMembers = true)]
    public class SvgImageRenderer : ViewRenderer<SvgImage,ImageView>
	{
		public static void Init ()
		{
            var temp = DateTime.Now;
        }

        private SvgImage _formsControl {
			get {
        BitmapCanvas x;
				return Element as SvgImage;
			}
		}

		protected override async void OnElementChanged (ElementChangedEventArgs<SvgImage> e)
		{
			base.OnElementChanged (e);

		  if (_formsControl != null)
		  {
        await Task.Run(async () =>
        {
          var svgStream = _formsControl.SvgAssembly.GetManifestResourceStream(_formsControl.SvgPath);

          if (svgStream == null)
          {
            throw new Exception(string.Format("Error retrieving {0} make sure Build Action is Embedded Resource",
              _formsControl.SvgPath));
          }

          var r = new SvgReader(new StreamReader(svgStream), new StylesParser(new ValuesParser()), new ValuesParser());

          var graphics = r.Graphic;

            var originalSvgSize = graphics.Size;

          var width = PixelToDP((int)_formsControl.WidthRequest <= 0 ? 100 : (int)_formsControl.WidthRequest);
          var height = PixelToDP((int)_formsControl.HeightRequest <= 0 ? 100 : (int)_formsControl.HeightRequest);

          var scale = 1.0;

          if (height >= width)
          {
            scale = height / graphics.Size.Height;
          }
          else
          {
            scale = width / graphics.Size.Width;
          }

            var finalCanvas = new AndroidPlatform().CreateImageCanvas(graphics.Size, scale);

            // TEMP: Fill for canvas visiblity.
            // TODO: Remove this.
            finalCanvas.DrawRectangle(new Rect(finalCanvas.Size), new NGraphics.Custom.Models.Pen(Brushes.LightGray.Color), Brushes.LightGray);
            if (_formsControl.Svg9SliceInsets != ResizableSvgInsets.Zero)
            {
              // Doing a stretchy 9-slice manipulation on the original SVG.

              graphics.ViewBox = new Rect(0, 0, originalSvgSize.Width, originalSvgSize.Height);
              // Fails, but feels right to me.
              //        graphics.ViewBox = new Rect(0, 0, originalSvgSize.Width / 2.0, originalSvgSize.Height / 2.0);
              var partialSize1 = new Size(graphics.Size.Width / 2.0, graphics.Size.Height / 2.0);
              var partialCanvas1 = new AndroidPlatform().CreateImageCanvas(partialSize1, scale);
              graphics.Draw(partialCanvas1);
              finalCanvas.DrawImage(partialCanvas1.GetImage(), new Rect(0, 0, originalSvgSize.Width / 2.0, originalSvgSize.Height / 2.0));

              graphics.ViewBox = new Rect(originalSvgSize.Width / 2.0, originalSvgSize.Height / 2.0, originalSvgSize.Width, originalSvgSize.Height);
              // Fails, but feels right to me.
              //        graphics.ViewBox = new Rect(originalSvgSize.Width / 2.0, originalSvgSize.Height / 2.0, originalSvgSize.Width / 2.0, originalSvgSize.Height / 2.0);
              var partialSize2 = new Size(graphics.Size.Width / 2.0, graphics.Size.Height / 2.0);
              var partialCanvas2 = new AndroidPlatform().CreateImageCanvas(partialSize2, scale);
              graphics.Draw(partialCanvas2);
              finalCanvas.DrawImage(partialCanvas2.GetImage(), new Rect(originalSvgSize.Width / 2.0, originalSvgSize.Height / 2.0, originalSvgSize.Width / 2.0, originalSvgSize.Height / 2.0));
            }
            else
            {
              // Typical approach to rendering an SVG; just draw it to the canvas.
              graphics.Draw(finalCanvas);
            }

            var image = (BitmapImage)finalCanvas.GetImage();
          return image;
        }).ContinueWith(taskResult =>
        {
          Device.BeginInvokeOnMainThread(() =>
          {
            var imageView = new ImageView(Context);

            imageView.SetScaleType(ImageView.ScaleType.FitXy);
            imageView.SetImageBitmap(taskResult.Result.Bitmap);

            SetNativeControl(imageView);
          });

        });
		  }
		}

		public override SizeRequest GetDesiredSize (int widthConstraint, int heightConstraint)
		{
			return new SizeRequest (new Xamarin.Forms.Size (_formsControl.WidthRequest, _formsControl.WidthRequest));
		}

    /// <summary>
    /// http://stackoverflow.com/questions/24465513/how-to-get-detect-screen-size-in-xamarin-forms
    /// </summary>
    /// <param name="pixel"></param>
    /// <returns></returns>
    private int PixelToDP(int pixel) {
      var scale =Resources.DisplayMetrics.Density;
      return (int) ((pixel * scale) + 0.5f);
    }
	}
}