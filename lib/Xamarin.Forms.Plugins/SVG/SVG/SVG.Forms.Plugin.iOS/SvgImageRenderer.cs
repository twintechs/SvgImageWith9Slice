﻿using Xamarin.Forms;
using SVG.Forms.Plugin.iOS;
using Xamarin.Forms.Platform.iOS;
using SVG.Forms.Plugin.Abstractions;
using NGraphics;
using UIKit;
using System;
using CoreGraphics;
using Foundation;
using Size = NGraphics.Size;

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
    public new static void Init()
    {
      var temp = DateTime.Now;
    }

    private SvgImage _formsControl {
      get { return Element as SvgImage; }
    }

    static double ScreenScale = UIScreen.MainScreen.Scale;

    public override void Draw(CGRect rect)
    {
      base.Draw(rect);

      if (_formsControl != null)
      {
        using (CGContext context = UIGraphics.GetCurrentContext())
        {
          context.SetAllowsAntialiasing(true);
          context.SetShouldAntialias(true);
          context.SetShouldSmoothFonts(true);

          var currentGraphic = _formsControl.LoadedGraphic;
          var outputSize = new Size(rect.Width, rect.Height);
          var finalCanvas = _formsControl.RenderSvgToCanvas(currentGraphic, currentGraphic.Size, outputSize, ScreenScale, CreatePlatformImageCanvas);
          var image = finalCanvas.GetImage();
          var uiImage = image.GetUIImage();
          Control.Image = uiImage;
        }
      }
    }

    static Func<Size, double, IImageCanvas> CreatePlatformImageCanvas = (size, scale) => new ApplePlatform().CreateImageCanvas(size, scale);

    protected override void OnElementChanged(ElementChangedEventArgs<Image> e)
    {
      base.OnElementChanged(e);

      if (e.OldElement != null)
      {
        (e.OldElement as SvgImage).OnInvalidate -= HandleInvalidate;
      }

      if (e.NewElement != null)
      {
        (e.NewElement as SvgImage).OnInvalidate += HandleInvalidate;
      }

      SetNeedsDisplay();
    }

    /// <summary>
    /// Handles view invalidate.
    /// </summary>
    void HandleInvalidate(object sender, System.EventArgs args)
    {
      SetNeedsDisplay();
    }
  }
}