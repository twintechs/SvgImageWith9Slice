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
        var finalCanvas = new ApplePlatform().CreateImageCanvas(graphics.Size, scale*scaleFactor);

        // TEMP: Fill for canvas visiblity.
        finalCanvas.DrawRectangle(new Rect(finalCanvas.Size), new NGraphics.Custom.Models.Pen(Brushes.LightGray.Color), Brushes.LightGray);

        graphics.ViewBox = new Rect(0, 0, originalSvgSize.Width, originalSvgSize.Height);
        var partialSize1 = new Size(graphics.Size.Width / 2.0, graphics.Size.Height / 2.0);
        var partialCanvas1 = new ApplePlatform().CreateImageCanvas(partialSize1, scale*scaleFactor);
        graphics.Draw(partialCanvas1);
        finalCanvas.DrawImage(partialCanvas1.GetImage(), new Rect(0, 0, originalSvgSize.Width / 2.0, originalSvgSize.Height / 2.0));

        graphics.ViewBox = new Rect(originalSvgSize.Width / 2.0, originalSvgSize.Height / 2.0, originalSvgSize.Width, originalSvgSize.Height);
        var partialSize2 = new Size(graphics.Size.Width / 2.0, graphics.Size.Height / 2.0);
        var partialCanvas2 = new ApplePlatform().CreateImageCanvas(partialSize2, scale*scaleFactor);
        graphics.Draw(partialCanvas2);
        finalCanvas.DrawImage(partialCanvas2.GetImage(), new Rect(originalSvgSize.Width / 2.0, originalSvgSize.Height / 2.0, originalSvgSize.Width / 2.0, originalSvgSize.Height / 2.0));

//          var canvas = new ApplePlatform().CreateImageCanvas(graphics.Size, scale*scaleFactor);
//          graphics.Draw(canvas);
        var image = finalCanvas.GetImage();

          var uiImage = image.GetUIImage();
          Control.Image = uiImage;
        }
      }
    }
}