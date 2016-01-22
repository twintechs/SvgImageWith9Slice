﻿using Xamarin.Forms;
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
        var outputSize = new Size(width, height);
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
        // Partition into 9 segments, based on _formsControl.Svg9SliceInsets, storing both original and scaled sizes.
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