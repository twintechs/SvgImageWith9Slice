using System;
using System.Linq;
using System.IO;
using Android.Widget;
using SVG.Forms.Plugin.Abstractions;
using SVG.Forms.Plugin.Droid;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;
using Android.Runtime;
using NGraphics;
using Color = NGraphics.Color;
using Size = NGraphics.Size;
using Point = NGraphics.Point;
using Rect = NGraphics.Rect;
using System.ComponentModel;

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

    protected override void OnLayout(bool changed, int l, int t, int r, int b)
    {
      base.OnLayout(changed, l, t, r, b);

      // Redraw SVG to new size.
      // TODO: Put some shortcut logic to avoid this when rendered size won't change
      //       (e.g., displaying proportional to horizontal and vertical has grown).
      var originalSvgSize = _LoadedGraphic.Size;

      var width = _formsControl.WidthRequest <= 0 ? 100 : _formsControl.WidthRequest;
      var height = _formsControl.HeightRequest <= 0 ? 100 : _formsControl.HeightRequest;
      var outputSize = new Size(width, height);
      var screenScale = 1.0; // Don't deal with screen scaling on Android.

      var finalCanvas = RenderSvgToCanvas(_LoadedGraphic, originalSvgSize, outputSize, screenScale, CreatePlatformImageCanvas);
      var image = (BitmapImage)finalCanvas.GetImage();

      Control.SetImageBitmap(image.Bitmap);
    }

    protected override void OnElementChanged(ElementChangedEventArgs<SvgImage> e)
    {
      base.OnElementChanged(e);

      if (_formsControl != null)
      {
        LoadSvgFromResource();
        Device.BeginInvokeOnMainThread(() =>
          {
            var imageView = new ImageView(Context);

            imageView.SetScaleType(ImageView.ScaleType.FitXy);

            // TODO: ?Reuse existing Control instead?
            SetNativeControl(imageView);
          });
      }
    }

    protected override void OnElementPropertyChanged (object sender, PropertyChangedEventArgs e)
    {
      base.OnElementPropertyChanged (sender, e);

      if (e.PropertyName == SvgImage.SvgPathProperty.PropertyName
        || e.PropertyName == SvgImage.SvgAssemblyProperty.PropertyName) {
        LoadSvgFromResource();
        RequestLayout();
      }
      else if (e.PropertyName == SvgImage.SvgStretchableInsetsProperty.PropertyName) {
        RequestLayout();
      }
    }

    Graphic _LoadedGraphic { get; set; }
    void LoadSvgFromResource() {
      var svgStream = _formsControl.SvgAssembly.GetManifestResourceStream(_formsControl.SvgPath);

      if (svgStream == null)
      {
        throw new Exception(string.Format("Error retrieving {0} make sure Build Action is Embedded Resource",
          _formsControl.SvgPath));
      }

      var r = new SvgReader(new StreamReader(svgStream));

      _LoadedGraphic = r.Graphic;
    }

    public override SizeRequest GetDesiredSize (int widthConstraint, int heightConstraint)
    {
      return new SizeRequest (new Xamarin.Forms.Size (_formsControl.WidthRequest, _formsControl.WidthRequest));
    }
    
    static Func<Size, double, IImageCanvas> CreatePlatformImageCanvas = (size, scale) => new AndroidPlatform().CreateImageCanvas(size, scale);
    IImageCanvas RenderSvgToCanvas(Graphic graphics, Size originalSvgSize, Size outputSize, double finalScale, Func<Size, double, IImageCanvas> createPlatformImageCanvas)
    {
      var finalCanvas = createPlatformImageCanvas(outputSize, finalScale);

      if (_formsControl.SvgStretchableInsets != ResizableSvgInsets.Zero)
      {
        // Doing a stretchy 9-slice manipulation on the original SVG.
        // Partition into 9 segments, based on _formsControl.Svg9SliceInsets, storing both original and scaled sizes.
        var sliceInsets = _formsControl.SvgStretchableInsets;
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
            sliceInsets.ScaleSection(outputSize, section));
        }).ToArray();

        foreach (var sliceFramePair in sliceFramePairs) {
          var sliceImage = RenderSectionToImage(graphics, sliceFramePair.Item1, sliceFramePair.Item2, finalScale, CreatePlatformImageCanvas);
          finalCanvas.DrawImage(sliceImage, sliceFramePair.Item2);
        }
      }
      else
      {
        // Typical approach to rendering an SVG; just draw it to the canvas.
        double proportionalOutputScale;
        if (outputSize.Height >= outputSize.Width)
        {
          proportionalOutputScale = outputSize.Width/_LoadedGraphic.Size.Width;
        }
        else
        {
          proportionalOutputScale = outputSize.Height/_LoadedGraphic.Size.Height;
        }

        // Make sure ViewBox is reset to a proportionally-scaled default in case it was previous set by slicing.
        graphics.ViewBox = new Rect(Point.Zero, graphics.Size / proportionalOutputScale);
        graphics.Draw(finalCanvas);
      }
      return finalCanvas;
    }

    static IImage RenderSectionToImage(/*this*/ Graphic graphics, Rect sourceFrame, Rect outputFrame, double finalScale, Func<Size, double, IImageCanvas> createPlatformImageCanvas)
    {
      var originalSize = graphics.Size;
      var sectionCanvas = createPlatformImageCanvas(outputFrame.Size, finalScale);

      // Redraw into final version with any scaling between the original and the output slice.
      var sliceVerticalScale = outputFrame.Height / sourceFrame.Height;
      var sliceHorizontalScale = outputFrame.Width / sourceFrame.Width;
      // Potentially setting ViewBox size smaller to enlarge result.
      graphics.ViewBox = new Rect(sourceFrame.Position, new Size(originalSize.Width / sliceHorizontalScale, originalSize.Height / sliceVerticalScale));

      graphics.Draw(sectionCanvas);
      return sectionCanvas.GetImage();
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