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

    public SvgImageRenderer()
    {
      // Offer to do our own drawing so Android will actually call `Draw`.
      SetWillNotDraw(willNotDraw: false);
    }

        private SvgImage _formsControl {
			get {
				return Element as SvgImage;
			}
		}

    public override void Draw(Android.Graphics.Canvas canvas)
    {
      base.Draw(canvas);

      if (_formsControl != null)
      {
        // Redraw SVG to new size.
        // TODO: Put some shortcut logic to avoid this when rendered size won't change
        //       (e.g., displaying proportional to horizontal and vertical has grown).
        var originalSvgSize = _LoadedGraphic.Size;
        
        var outputSize = new Size(canvas.Width, canvas.Height);
        const double screenScale = 1.0; // Don't need to deal with screen scaling on Android.
        
        var finalCanvas = _formsControl.RenderSvgToCanvas(_LoadedGraphic, originalSvgSize, outputSize, screenScale, CreatePlatformImageCanvas);
        var image = (BitmapImage)finalCanvas.GetImage();
        
        Control.SetImageBitmap(image.Bitmap);
      }
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
            Invalidate();
          });
      }
    }

    protected override void OnElementPropertyChanged (object sender, PropertyChangedEventArgs e)
    {
      base.OnElementPropertyChanged (sender, e);

      if (e.PropertyName == SvgImage.SvgPathProperty.PropertyName
        || e.PropertyName == SvgImage.SvgAssemblyProperty.PropertyName) {
        LoadSvgFromResource();
        Invalidate();
      }
      else if (e.PropertyName == SvgImage.SvgStretchableInsetsProperty.PropertyName) {
        Invalidate();
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