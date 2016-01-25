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
    }
    public override void LayoutSubviews()
    {
      base.LayoutSubviews();

      // Redraw SVG to new size.
      // TODO: Put some shortcut logic to avoid this when rendered size won't change
      //       (e.g., displaying proportional to horizontal and vertical has grown).
      var originalSvgSize = _LoadedGraphic.Size;

      var width = _formsControl.WidthRequest <= 0 ? 100 : _formsControl.WidthRequest;
      var height = _formsControl.HeightRequest <= 0 ? 100 : _formsControl.HeightRequest;
      var outputSize = new Size(width, height);
      var screenScaleFactor = UIScreen.MainScreen.Scale;

      var finalCanvas = RenderSvgToCanvas(_LoadedGraphic, originalSvgSize, outputSize, screenScaleFactor, CreatePlatformImageCanvas);

      var image = finalCanvas.GetImage();
      var uiImage = image.GetUIImage();
      Control.Image = uiImage;
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
        SetNeedsLayout();
      }
      else if (e.PropertyName == SvgImage.SvgStretchableInsetsProperty.PropertyName) {
        SetNeedsLayout();
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
  }
}