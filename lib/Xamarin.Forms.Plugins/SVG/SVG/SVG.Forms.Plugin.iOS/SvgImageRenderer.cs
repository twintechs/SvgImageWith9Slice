using Xamarin.Forms;
using SVG.Forms.Plugin.iOS;
using Xamarin.Forms.Platform.iOS;
using System.IO;
using SVG.Forms.Plugin.Abstractions;
using NGraphics;
using UIKit;
using System;
using CoreGraphics;
using Foundation;
using NGraphics.Custom.Parsers;
using NGraphics.iOS.Custom;
using Color = NGraphics.Custom.Models.Color;
using Size = NGraphics.Custom.Models.Size;
using Rect = NGraphics.Custom.Models.Rect;
using Point = NGraphics.Custom.Models.Point;
using NGraphics.Custom.Codes;
using NGraphics.Custom.Interfaces;
using NGraphics.Custom;
using NGraphics.Custom.Models.Brushes;

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

    protected override void OnElementChanged(ElementChangedEventArgs<Image> e)
    {
      base.OnElementChanged(e);

      if (_formsControl != null)
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

        var width = _formsControl.WidthRequest <= 0 ? 100 : _formsControl.WidthRequest;
        var height = _formsControl.HeightRequest <= 0 ? 100 : _formsControl.HeightRequest;

        var scale = 1.0;

        if (height >= width)
        {
          scale = height/graphics.Size.Height;
        }
        else
        {
          scale = width/graphics.Size.Width;
        }


        var scaleFactor = UIScreen.MainScreen.Scale;
        var outputSize = originalSvgSize;
        var finalCanvas = RenderSvgToCanvas(graphics, originalSvgSize, outputSize, scale * scaleFactor, CreatePlatformImageCanvas);

        var image = finalCanvas.GetImage();

        var uiImage = image.GetUIImage();
        Control.Image = uiImage;
      }
    }

    static Func<Size, double, IImageCanvas> CreatePlatformImageCanvas = (size, scale) => new ApplePlatform().CreateImageCanvas(size, scale);
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

        var sliceFramePairs = new[] {
          // Upper left
          Tuple.Create(
            new Rect(Point.Zero, originalSvgSize),
            new Rect(Point.Zero, new Size(_formsControl.Svg9SliceInsets.Left, _formsControl.Svg9SliceInsets.Top))),
          // Upper middle
          Tuple.Create(
            new Rect(new Point(_formsControl.Svg9SliceInsets.Left, 0), originalSvgSize),
            new Rect(new Point(_formsControl.Svg9SliceInsets.Left, 0), new Size(originalSvgSize.Width - _formsControl.Svg9SliceInsets.Right - _formsControl.Svg9SliceInsets.Left, _formsControl.Svg9SliceInsets.Top))),
          // Upper right
          Tuple.Create(
            new Rect(new Point(originalSvgSize.Width - _formsControl.Svg9SliceInsets.Right, 0), originalSvgSize),
            new Rect(new Point(originalSvgSize.Width - _formsControl.Svg9SliceInsets.Right, 0), new Size(originalSvgSize.Width - _formsControl.Svg9SliceInsets.Right, _formsControl.Svg9SliceInsets.Top))),
          // Middle left
          Tuple.Create(
            new Rect(new Point(0, _formsControl.Svg9SliceInsets.Top), originalSvgSize),
            new Rect(new Point(0, _formsControl.Svg9SliceInsets.Top), new Size(_formsControl.Svg9SliceInsets.Right, originalSvgSize.Height - _formsControl.Svg9SliceInsets.Bottom - _formsControl.Svg9SliceInsets.Top))),
          // Center
          Tuple.Create(
            new Rect(new Point(_formsControl.Svg9SliceInsets.Left, _formsControl.Svg9SliceInsets.Top), originalSvgSize),
            new Rect(new Point(_formsControl.Svg9SliceInsets.Left, _formsControl.Svg9SliceInsets.Top), new Size(originalSvgSize.Width - _formsControl.Svg9SliceInsets.Right - _formsControl.Svg9SliceInsets.Left, originalSvgSize.Height - _formsControl.Svg9SliceInsets.Bottom - _formsControl.Svg9SliceInsets.Top))),
          // Middle right
          Tuple.Create(
            new Rect(new Point(originalSvgSize.Width - _formsControl.Svg9SliceInsets.Right, _formsControl.Svg9SliceInsets.Top), originalSvgSize),
            new Rect(new Point(originalSvgSize.Width - _formsControl.Svg9SliceInsets.Right, _formsControl.Svg9SliceInsets.Top), new Size(_formsControl.Svg9SliceInsets.Right, originalSvgSize.Height - _formsControl.Svg9SliceInsets.Bottom - _formsControl.Svg9SliceInsets.Top))),
          // Lower left
          Tuple.Create(
            new Rect(new Point(0, originalSvgSize.Height - _formsControl.Svg9SliceInsets.Bottom), originalSvgSize),
            new Rect(new Point(0, originalSvgSize.Height - _formsControl.Svg9SliceInsets.Bottom), new Size(_formsControl.Svg9SliceInsets.Right, _formsControl.Svg9SliceInsets.Bottom))),
          // Lower middle
          Tuple.Create(
            new Rect(new Point(_formsControl.Svg9SliceInsets.Left, originalSvgSize.Height - _formsControl.Svg9SliceInsets.Bottom), originalSvgSize),
            new Rect(new Point(_formsControl.Svg9SliceInsets.Left, originalSvgSize.Height - _formsControl.Svg9SliceInsets.Bottom), new Size(originalSvgSize.Width - _formsControl.Svg9SliceInsets.Right - _formsControl.Svg9SliceInsets.Left, _formsControl.Svg9SliceInsets.Bottom))),
          // Lower right
          Tuple.Create(
            new Rect(new Point(originalSvgSize.Width - _formsControl.Svg9SliceInsets.Right, originalSvgSize.Height - _formsControl.Svg9SliceInsets.Bottom), originalSvgSize),
            new Rect(new Point(originalSvgSize.Width - _formsControl.Svg9SliceInsets.Right, originalSvgSize.Height - _formsControl.Svg9SliceInsets.Bottom), new Size(_formsControl.Svg9SliceInsets.Right, _formsControl.Svg9SliceInsets.Bottom))),
        };

        foreach (var sliceFramePair in sliceFramePairs) {
          var upperLeftImage = RenderSectionToImage(graphics, sliceFramePair.Item1, sliceFramePair.Item2, finalScale, CreatePlatformImageCanvas);
          finalCanvas.DrawImage(upperLeftImage, sliceFramePair.Item2);
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
  }
}