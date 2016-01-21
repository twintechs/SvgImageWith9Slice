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
        var finalCanvas = new ApplePlatform().CreateImageCanvas(graphics.Size, scale * scaleFactor);
        
        // TEMP: Fill for canvas visiblity.
        // TODO: Remove this.
        finalCanvas.DrawRectangle(new Rect(finalCanvas.Size), new NGraphics.Custom.Models.Pen(Brushes.LightGray.Color), Brushes.LightGray);
        
        if (_formsControl.Svg9SliceInsets != ResizableSvgInsets.Zero)
        {
          // Doing a stretchy 9-slice manipulation on the original SVG.
          // [x] Partition into 9 segments, based on _formsControl.Svg9SliceInsets

          // TODO: Redraw into final version with proportions based on translation between the original and the [potentially-stretched] segments

          var upperLeftLocation = Point.Zero;
          graphics.ViewBox = new Rect(upperLeftLocation, originalSvgSize);
          var upperLeftSize = new Size(_formsControl.Svg9SliceInsets.Left, _formsControl.Svg9SliceInsets.Top);
          var upperLeftCanvas = new ApplePlatform().CreateImageCanvas(upperLeftSize, scale * scaleFactor);
          upperLeftCanvas.DrawRectangle(new Rect(Point.Zero, upperLeftSize), new NGraphics.Custom.Models.Pen(Brushes.Red.Color), Brushes.Red);
          graphics.Draw(upperLeftCanvas);
          finalCanvas.DrawImage(upperLeftCanvas.GetImage(), new Rect(upperLeftLocation, upperLeftSize));

          var upperMiddleLocation = new Point(_formsControl.Svg9SliceInsets.Left, 0);
          graphics.ViewBox = new Rect(upperMiddleLocation, originalSvgSize);
          var upperMiddleSize = new Size(originalSvgSize.Width - _formsControl.Svg9SliceInsets.Right - _formsControl.Svg9SliceInsets.Left, _formsControl.Svg9SliceInsets.Top);
          var upperMiddleCanvas = new ApplePlatform().CreateImageCanvas(upperMiddleSize, scale * scaleFactor);
          upperMiddleCanvas.DrawRectangle(new Rect(Point.Zero, upperMiddleSize), new NGraphics.Custom.Models.Pen(Brushes.Green.Color), Brushes.Green);
          graphics.Draw(upperMiddleCanvas);
          finalCanvas.DrawImage(upperMiddleCanvas.GetImage(), new Rect(upperMiddleLocation, upperMiddleSize));

          var upperRightLocation = new Point(originalSvgSize.Width - _formsControl.Svg9SliceInsets.Right, 0);
          graphics.ViewBox = new Rect(upperRightLocation, originalSvgSize);
          var upperRightSize = new Size(originalSvgSize.Width - _formsControl.Svg9SliceInsets.Right, _formsControl.Svg9SliceInsets.Top);
          var upperRightCanvas = new ApplePlatform().CreateImageCanvas(upperRightSize, scale * scaleFactor);
          upperRightCanvas.DrawRectangle(new Rect(Point.Zero, upperRightSize), new NGraphics.Custom.Models.Pen(Brushes.Blue.Color), Brushes.Blue);
          graphics.Draw(upperRightCanvas);
          finalCanvas.DrawImage(upperRightCanvas.GetImage(), new Rect(upperRightLocation, upperRightSize));

          var middleLeftLocation = new Point(0, _formsControl.Svg9SliceInsets.Top);
          graphics.ViewBox = new Rect(middleLeftLocation, originalSvgSize);
          var middleLeftSize = new Size(_formsControl.Svg9SliceInsets.Right, originalSvgSize.Height - _formsControl.Svg9SliceInsets.Bottom - _formsControl.Svg9SliceInsets.Top);
          var middleLeftCanvas = new ApplePlatform().CreateImageCanvas(middleLeftSize, scale * scaleFactor);
          middleLeftCanvas.DrawRectangle(new Rect(Point.Zero, middleLeftSize), new NGraphics.Custom.Models.Pen(Brushes.Blue.Color), Brushes.Blue);
          graphics.Draw(middleLeftCanvas);
          finalCanvas.DrawImage(middleLeftCanvas.GetImage(), new Rect(middleLeftLocation, middleLeftSize));

          var centerLocation = new Point(_formsControl.Svg9SliceInsets.Left, _formsControl.Svg9SliceInsets.Top);
          graphics.ViewBox = new Rect(centerLocation, originalSvgSize);
          var centerSize = new Size(originalSvgSize.Width - _formsControl.Svg9SliceInsets.Right - _formsControl.Svg9SliceInsets.Left, originalSvgSize.Height - _formsControl.Svg9SliceInsets.Bottom - _formsControl.Svg9SliceInsets.Top);
          var centerCanvas = new ApplePlatform().CreateImageCanvas(centerSize, scale * scaleFactor);
          centerCanvas.DrawRectangle(new Rect(Point.Zero, centerSize), new NGraphics.Custom.Models.Pen(Brushes.Red.Color), Brushes.Red);
          graphics.Draw(centerCanvas);
          finalCanvas.DrawImage(centerCanvas.GetImage(), new Rect(centerLocation, centerSize));

          var middleRightLocation = new Point(originalSvgSize.Width - _formsControl.Svg9SliceInsets.Right, _formsControl.Svg9SliceInsets.Top);
          graphics.ViewBox = new Rect(middleRightLocation, originalSvgSize);
          var middleRightSize = new Size(_formsControl.Svg9SliceInsets.Right, originalSvgSize.Height - _formsControl.Svg9SliceInsets.Bottom - _formsControl.Svg9SliceInsets.Top);
          var middleRightCanvas = new ApplePlatform().CreateImageCanvas(middleRightSize, scale * scaleFactor);
          middleRightCanvas.DrawRectangle(new Rect(Point.Zero, middleRightSize), new NGraphics.Custom.Models.Pen(Brushes.Green.Color), Brushes.Green);
          graphics.Draw(middleRightCanvas);
          finalCanvas.DrawImage(middleRightCanvas.GetImage(), new Rect(middleRightLocation, middleRightSize));

          var bottomLeftLocation = new Point(0, originalSvgSize.Height - _formsControl.Svg9SliceInsets.Bottom);
          graphics.ViewBox = new Rect(bottomLeftLocation, originalSvgSize);
          var bottomLeftSize = new Size(_formsControl.Svg9SliceInsets.Right, _formsControl.Svg9SliceInsets.Bottom);
          var bottomLeftCanvas = new ApplePlatform().CreateImageCanvas(bottomLeftSize, scale * scaleFactor);
          bottomLeftCanvas.DrawRectangle(new Rect(Point.Zero, bottomLeftSize), new NGraphics.Custom.Models.Pen(Brushes.Green.Color), Brushes.Green);
          graphics.Draw(bottomLeftCanvas);
          finalCanvas.DrawImage(bottomLeftCanvas.GetImage(), new Rect(bottomLeftLocation, bottomLeftSize));

          var bottomMiddleLocation = new Point(_formsControl.Svg9SliceInsets.Left, originalSvgSize.Height - _formsControl.Svg9SliceInsets.Bottom);
          graphics.ViewBox = new Rect(bottomMiddleLocation, originalSvgSize);
          var bottomMiddleSize = new Size(originalSvgSize.Width - _formsControl.Svg9SliceInsets.Right - _formsControl.Svg9SliceInsets.Left, _formsControl.Svg9SliceInsets.Bottom);
          var bottomMiddleCanvas = new ApplePlatform().CreateImageCanvas(bottomMiddleSize, scale * scaleFactor);
          bottomMiddleCanvas.DrawRectangle(new Rect(Point.Zero, bottomMiddleSize), new NGraphics.Custom.Models.Pen(Brushes.Blue.Color), Brushes.Blue);
          graphics.Draw(bottomMiddleCanvas);
          finalCanvas.DrawImage(bottomMiddleCanvas.GetImage(), new Rect(bottomMiddleLocation, bottomMiddleSize));

          var bottomRightLocation = new Point(originalSvgSize.Width - _formsControl.Svg9SliceInsets.Right, originalSvgSize.Height - _formsControl.Svg9SliceInsets.Bottom);
          graphics.ViewBox = new Rect(bottomRightLocation, originalSvgSize);
          var bottomRightSize = new Size(_formsControl.Svg9SliceInsets.Right, _formsControl.Svg9SliceInsets.Bottom);
          var bottomRightCanvas = new ApplePlatform().CreateImageCanvas(bottomRightSize, scale * scaleFactor);
          bottomRightCanvas.DrawRectangle(new Rect(Point.Zero, bottomRightSize), new NGraphics.Custom.Models.Pen(Brushes.Red.Color), Brushes.Red);
          graphics.Draw(bottomRightCanvas);
          finalCanvas.DrawImage(bottomRightCanvas.GetImage(), new Rect(bottomRightLocation, bottomRightSize));


//          graphics.ViewBox = new Rect(0, 0, originalSvgSize.Width, originalSvgSize.Height);
//          // Fails, but feels right to me.
//          //        graphics.ViewBox = new Rect(0, 0, originalSvgSize.Width / 2.0, originalSvgSize.Height / 2.0);
//          var partialSize1 = new Size(graphics.Size.Width / 2.0, graphics.Size.Height / 2.0);
//          var partialCanvas1 = new ApplePlatform().CreateImageCanvas(partialSize1, scale * scaleFactor);
//          graphics.Draw(partialCanvas1);
//          finalCanvas.DrawImage(partialCanvas1.GetImage(), new Rect(0, 0, originalSvgSize.Width / 2.0, originalSvgSize.Height / 2.0));
//          
//          graphics.ViewBox = new Rect(originalSvgSize.Width / 2.0, originalSvgSize.Height / 2.0, originalSvgSize.Width, originalSvgSize.Height);
//          // Fails, but feels right to me.
//          //        graphics.ViewBox = new Rect(originalSvgSize.Width / 2.0, originalSvgSize.Height / 2.0, originalSvgSize.Width / 2.0, originalSvgSize.Height / 2.0);
//          var partialSize2 = new Size(graphics.Size.Width / 2.0, graphics.Size.Height / 2.0);
//          var partialCanvas2 = new ApplePlatform().CreateImageCanvas(partialSize2, scale * scaleFactor);
//          graphics.Draw(partialCanvas2);
//          finalCanvas.DrawImage(partialCanvas2.GetImage(), new Rect(originalSvgSize.Width / 2.0, originalSvgSize.Height / 2.0, originalSvgSize.Width / 2.0, originalSvgSize.Height / 2.0));
        }
        else
        {
          // Typical approach to rendering an SVG; just draw it to the canvas.
          graphics.Draw(finalCanvas);
        }

        var image = finalCanvas.GetImage();

          var uiImage = image.GetUIImage();
          Control.Image = uiImage;
        }
      }
    }
}