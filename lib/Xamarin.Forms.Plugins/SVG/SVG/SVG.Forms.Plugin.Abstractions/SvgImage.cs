using System.Reflection;
using Xamarin.Forms;
using System;
using System.Linq;
using NGraphics;
using Point = NGraphics.Point;
using Size = NGraphics.Size;

namespace SVG.Forms.Plugin.Abstractions
{
  public enum ResizableSvgSection {
    TopLeft,
    TopCenter,
    TopRight,
    CenterLeft,
    CenterCenter,
    CenterRight,
    BottomLeft,
    BottomCenter,
    BottomRight,
  }
  public struct ResizableSvgInsets : IEquatable<ResizableSvgInsets> {
    public int Top { get; set; }
    public int Right { get; set; }
    public int Bottom { get; set; }
    public int Left { get; set; }
    
    public static ResizableSvgInsets Zero = new ResizableSvgInsets(0, 0, 0, 0);
    public ResizableSvgInsets(int top, int right, int bottom, int left) {
      Top = top;
      Right = right;
      Bottom = bottom;
      Left = left;
    }
    public ResizableSvgInsets(int vertical, int horizontal) : this(vertical, horizontal, vertical, horizontal) { }
    public ResizableSvgInsets(int allSides) : this(allSides, allSides, allSides, allSides) { }

    // NOTE: Returns actual section from original SVG, not what is used for the ViewBox when slicing
    //       (which seems to require the original SVG size, or a proportional scale of it for resizing).
    public Rect GetSection(Size originalSvgSize, ResizableSvgSection section) {
      switch (section) {
        case ResizableSvgSection.TopLeft:
          return new Rect(Point.Zero, new Size(Left, Top));
        case ResizableSvgSection.TopCenter:
          return new Rect(new Point(Left, 0), new Size(originalSvgSize.Width - Right - Left, Top));
        case ResizableSvgSection.TopRight:
          return new Rect(new Point(originalSvgSize.Width - Right, 0), new Size(Right, Top));
        case ResizableSvgSection.CenterLeft:
          return new Rect(new Point(0, Top), new Size(Right, originalSvgSize.Height - Bottom - Top));
        case ResizableSvgSection.CenterCenter:
          return new Rect(new Point(Left, Top), new Size(originalSvgSize.Width - Right - Left, originalSvgSize.Height - Bottom - Top));
        case ResizableSvgSection.CenterRight:
          return new Rect(new Point(originalSvgSize.Width - Right, Top), new Size(Right, originalSvgSize.Height - Bottom - Top));
        case ResizableSvgSection.BottomLeft:
          return new Rect(new Point(0, originalSvgSize.Height - Bottom), new Size(Right, Bottom));
        case ResizableSvgSection.BottomCenter:
          return new Rect(new Point(Left, originalSvgSize.Height - Bottom), new Size(originalSvgSize.Width - Right - Left, Bottom));
        case ResizableSvgSection.BottomRight:
          return new Rect(new Point(originalSvgSize.Width - Right, originalSvgSize.Height - Bottom), new Size(Right, Bottom));
        default:
          throw new ArgumentOutOfRangeException("section", "Invalid resizable SVG section");
      }
    }

    public Rect ScaleSection(Size finalImageSize, ResizableSvgSection section) {
      // TODO: Decide if the corners scale proportionally (an option with SVGs) or if they always stay the same size (current) or if it is optional.
      // TODO: Factor in `scaleCorners`, if needed.
      //public Rect ScaleSection(Size originalSvgSize, Size finalImageSize, ResizableSvgSection section, bool scaleCorners) {
//      int horizontalScale = finalImageSize.Width / originalSvgSize.Width;
//      int verticalScale = finalImageSize.Height / originalSvgSize.Height;
//      Rect originalSection = GetSection(originalSvgSize, section);
      switch (section) {
        case ResizableSvgSection.TopLeft:
          return new Rect(Point.Zero, new Size(Left, Top));
        case ResizableSvgSection.TopCenter:
          return new Rect(new Point(Left, 0), new Size(finalImageSize.Width - Right - Left, Top));
        case ResizableSvgSection.TopRight:
          return new Rect(new Point(finalImageSize.Width - Right, 0), new Size(Right, Top));
        case ResizableSvgSection.CenterLeft:
          return new Rect(new Point(0, Top), new Size(Right, finalImageSize.Height - Bottom - Top));
        case ResizableSvgSection.CenterCenter:
          return new Rect(new Point(Left, Top), new Size(finalImageSize.Width - Right - Left, finalImageSize.Height - Bottom - Top));
        case ResizableSvgSection.CenterRight:
          return new Rect(new Point(finalImageSize.Width - Right, Top), new Size(Right, finalImageSize.Height - Bottom - Top));
        case ResizableSvgSection.BottomLeft:
          return new Rect(new Point(0, finalImageSize.Height - Bottom), new Size(Right, Bottom));
        case ResizableSvgSection.BottomCenter:
          return new Rect(new Point(Left, finalImageSize.Height - Bottom), new Size(finalImageSize.Width - Right - Left, Bottom));
        case ResizableSvgSection.BottomRight:
          return new Rect(new Point(finalImageSize.Width - Right, finalImageSize.Height - Bottom), new Size(Right, Bottom));
        default:
          throw new ArgumentOutOfRangeException("section", "Invalid resizable SVG section");
      }
    }

    public override bool Equals(object obj)
    {
      if (obj.GetType() != typeof(ResizableSvgInsets)) {
        return false;
      }
      return Equals((ResizableSvgInsets)obj);
    }

    public override int GetHashCode()
    {
      return (Top + Right + Bottom + Left).GetHashCode();
    }

    public static bool operator ==(ResizableSvgInsets inset1, ResizableSvgInsets inset2) 
    {
      return inset1.Equals(inset2);
    }

    public static bool operator !=(ResizableSvgInsets inset1, ResizableSvgInsets inset2) 
    {
      return !inset1.Equals(inset2);
    }

    #region IEquatable implementation
    public bool Equals(ResizableSvgInsets other)
    {
      return Top == other.Top
        && Right == other.Right
        && Bottom == other.Bottom
        && Left == other.Left;
    }
    #endregion
  }

  public class SvgImage : Image
  {
    /// <summary>
    /// The path to the svg file
    /// </summary>
    public static readonly BindableProperty SvgPathProperty =
      BindableProperty.Create("SvgPath", typeof(string), typeof(SvgImage), default(string));

    /// <summary>
    /// The path to the svg file
    /// </summary>
    public string SvgPath
    {
      get { return (string)GetValue(SvgPathProperty); }
      set { SetValue(SvgPathProperty, value); }
    }

    /// <summary>
    /// The assembly containing the svg file
    /// </summary>
    public static readonly BindableProperty SvgAssemblyProperty =
      BindableProperty.Create("SvgAssembly", typeof(Assembly), typeof(SvgImage), default(Assembly));

    /// <summary>
    /// The assembly containing the svg file
    /// </summary>
    public Assembly SvgAssembly
    {
      get { return (Assembly)GetValue(SvgAssemblyProperty); }
      set { SetValue(SvgAssemblyProperty, value); }
    }

    /// <summary>
    /// Optional SVG 9-slice insets
    /// </summary>
    public static readonly BindableProperty SvgStretchableInsetsProperty =
      BindableProperty.Create(nameof(SvgStretchableInsets), typeof(ResizableSvgInsets), typeof(SvgImage), default(ResizableSvgInsets));

    /// <summary>
    /// Optional SVG 9-slice insets
    /// </summary>
    public ResizableSvgInsets SvgStretchableInsets
    {
      get { return (ResizableSvgInsets)GetValue(SvgStretchableInsetsProperty); }
      set {
        SetValue(SvgStretchableInsetsProperty, value);
      }
    }

    public IImageCanvas RenderSvgToCanvas(Graphic graphics, Size originalSvgSize, Size outputSize, double finalScale, Func<Size, double, IImageCanvas> createPlatformImageCanvas)
    {
      var finalCanvas = createPlatformImageCanvas(outputSize, finalScale);

      if (SvgStretchableInsets != ResizableSvgInsets.Zero)
      {
        // Doing a stretchy 9-slice manipulation on the original SVG.
        // Partition into 9 segments, based on _formsControl.Svg9SliceInsets, storing both original and scaled sizes.
        var sliceInsets = SvgStretchableInsets;
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
          var sliceImage = RenderSectionToImage(graphics, sliceFramePair.Item1, sliceFramePair.Item2, finalScale, createPlatformImageCanvas);
          finalCanvas.DrawImage(sliceImage, sliceFramePair.Item2);
        }
      }
      else
      {
        // Typical approach to rendering an SVG; just draw it to the canvas.
        double proportionalOutputScale;
        if (outputSize.Height >= outputSize.Width)
        {
          proportionalOutputScale = outputSize.Width/originalSvgSize.Width;
        }
        else
        {
          proportionalOutputScale = outputSize.Height/originalSvgSize.Height;
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