using Xamarin.Forms;
using SVG.Forms.Plugin.iOS;
using Xamarin.Forms.Platform.iOS;
using System.IO;
using SVG.Forms.Plugin.Abstractions;
using NGraphics;
using UIKit;
using System;
using System.Linq;
using CoreGraphics;
using Foundation;
using Color = NGraphics.Color;
using Size = NGraphics.Size;
using Rect = NGraphics.Rect;
using Point = NGraphics.Point;
using System.ComponentModel;

[assembly: ExportRenderer(typeof(SVG.Forms.Plugin.Abstractions.SvgImage), typeof(SvgImageRenderer))]
namespace SVG.Forms.Plugin.iOS
{
  /// <summary>
  /// SVG Renderer
  /// </summary>
  [Preserve(AllMembers = true)]
  public class SvgImageRenderer : ImageRenderer
  {
    /// <summary>
    ///   Used for registration with dependency service
    /// </summary>
    public static void Init()
    {
      var temp = DateTime.Now;
    }

    private SvgImage _formsControl
    {
      get { return Element as SvgImage; }
    }

    public override void Draw(CGRect rect)
    {
      base.Draw(rect);

      if (_formsControl != null)
      {
        using (CGContext context = UIGraphics.GetCurrentContext ()) 
        {
          context.SetAllowsAntialiasing(true);
          context.SetShouldAntialias(true);
          context.SetShouldSmoothFonts(true);
          
          var finalCanvas = _formsControl.RenderSvgToCanvas(_LoadedGraphic, _LoadedGraphic.Size, new Size(rect.Width, rect.Height), UIScreen.MainScreen.Scale, CreatePlatformImageCanvas);
          var image = finalCanvas.GetImage();
          var uiImage = image.GetUIImage();
          Control.Image = uiImage;
        }
      }
    }

    protected override void OnElementChanged(ElementChangedEventArgs<Image> e)
    {
      base.OnElementChanged(e);

      if (_formsControl != null)
      {
        LoadSvgFromResource();
      }
    }

    protected override void OnElementPropertyChanged (object sender, PropertyChangedEventArgs e)
    {
      base.OnElementPropertyChanged (sender, e);

      if (e.PropertyName == SvgImage.SvgPathProperty.PropertyName
        || e.PropertyName == SvgImage.SvgAssemblyProperty.PropertyName) {
        LoadSvgFromResource();
        SetNeedsDisplay();
      }
      else if (e.PropertyName == SvgImage.SvgStretchableInsetsProperty.PropertyName) {
        SetNeedsDisplay();
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

    static Func<Size, double, IImageCanvas> CreatePlatformImageCanvas = (size, scale) => new ApplePlatform().CreateImageCanvas(size, scale);
  }
}