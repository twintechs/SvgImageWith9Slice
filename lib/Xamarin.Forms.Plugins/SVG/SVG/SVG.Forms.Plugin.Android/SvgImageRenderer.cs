using System;
using System.Linq;
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
using Point = NGraphics.Custom.Models.Point;
using Rect = NGraphics.Custom.Models.Rect;
using NGraphics.Custom.Codes;
using NGraphics.Custom.Models.Brushes;
using NGraphics.Custom.Interfaces;
using NGraphics.Custom;

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

            var outputSize = originalSvgSize;
            var finalCanvas = RenderSvgToCanvas(graphics, originalSvgSize, outputSize, scale, CreatePlatformImageCanvas);

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

    static Func<Size, double, IImageCanvas> CreatePlatformImageCanvas = (size, scale) => new AndroidPlatform().CreateImageCanvas(size, scale);
    IImageCanvas RenderSvgToCanvas(Graphic graphics, Size originalSvgSize, Size outputSize, double finalScale, Func<Size, double, IImageCanvas> createPlatformImageCanvas)
    {
      var finalCanvas = createPlatformImageCanvas(outputSize, finalScale);
      // TEMP: Fill for canvas visiblity.
      // TODO: Remove this.
      finalCanvas.DrawRectangle(new Rect(finalCanvas.Size), new NGraphics.Custom.Models.Pen(Brushes.LightGray.Color), Brushes.LightGray);

      if (_formsControl.Svg9SliceInsets != ResizableSvgInsets.Zero)
      {
        // Doing a stretchy 9-slice manipulation on the original SVG.
        // [x] Partition into 9 segments, based on _formsControl.Svg9SliceInsets
        // TODO: Redraw into final version with proportions based on translation between the original and the [potentially-stretched] segments

        var sliceInsets = _formsControl.Svg9SliceInsets;
        var sliceFramePairs = new[] {
          ResizableSvgSection.TopLeft,
          ResizableSvgSection.TopCenter,
          ResizableSvgSection.TopRight,
          ResizableSvgSection.CenterLeft,
          ResizableSvgSection.CenterCenter,
          ResizableSvgSection.CenterRight,
          ResizableSvgSection.BottomLeft,
          ResizableSvgSection.BottomCenter,
          ResizableSvgSection.BottomRight,
        }.Select(section => {
          return Tuple.Create(
            sliceInsets.GetSection(originalSvgSize, section),
            sliceInsets.ScaleSection(originalSvgSize, outputSize, section));
        }).ToArray();

        foreach (var sliceFramePair in sliceFramePairs) {
          var sliceImage = RenderSectionToImage(graphics, sliceFramePair.Item1, sliceFramePair.Item2, finalScale, CreatePlatformImageCanvas);
          finalCanvas.DrawImage(sliceImage, sliceFramePair.Item2);
        }
      }
      else
      {
        // Typical approach to rendering an SVG; just draw it to the canvas.
        graphics.Draw(finalCanvas);
      }
      return finalCanvas;
    }

    static IImage RenderSectionToImage(/*this*/ Graphic graphics, Rect sourceFrame, Rect outputFrame, double finalScale, Func<Size, double, IImageCanvas> createPlatformImageCanvas)
    {
      graphics.ViewBox = sourceFrame;
      var sectionCanvas = createPlatformImageCanvas(outputFrame.Size, finalScale);

      // TODO: Remove (debug helper section shading)
      var debugBrush = GetDebugBrush();
      sectionCanvas.DrawRectangle(new Rect(Point.Zero, outputFrame.Size), new NGraphics.Custom.Models.Pen(debugBrush.Color), debugBrush);

      graphics.Draw(sectionCanvas);
      return sectionCanvas.GetImage();
    }

    static int currentDebugBrushIndex = 0;
    static readonly SolidBrush[] debugBrushes = new[] { Brushes.Red, Brushes.Green, Brushes.Blue, Brushes.Yellow };
    static SolidBrush GetDebugBrush() {
      return debugBrushes[currentDebugBrushIndex++ % debugBrushes.Length];
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