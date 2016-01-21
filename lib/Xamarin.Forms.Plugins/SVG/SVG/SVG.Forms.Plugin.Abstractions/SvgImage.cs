using System.Reflection;
using Xamarin.Forms;
using System;
using NGraphics.Custom.Models;
using Point = NGraphics.Custom.Models.Point;
using Size = NGraphics.Custom.Models.Size;

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
    public double Top { get; set; }
    public double Right { get; set; }
    public double Bottom { get; set; }
    public double Left { get; set; }
    
    public static ResizableSvgInsets Zero = new ResizableSvgInsets(0, 0, 0, 0);
    public ResizableSvgInsets(double top, double right, double bottom, double left) {
      Top = top;
      Right = right;
      Bottom = bottom;
      Left = left;
    }
    public ResizableSvgInsets(double vertical, double horizontal) : this(vertical, horizontal, vertical, horizontal) { }
    public ResizableSvgInsets(double allSides) : this(allSides, allSides, allSides, allSides) { }

    public Rect GetSection(Size originalSvgSize, ResizableSvgSection section) {
      switch (section) {
        case ResizableSvgSection.TopLeft:
          return new Rect(Point.Zero, new Size(Left, Top));
        case ResizableSvgSection.TopCenter:
          return new Rect(new Point(Left, 0), originalSvgSize);
        case ResizableSvgSection.TopRight:
          return new Rect(new Point(originalSvgSize.Width - Right, 0), originalSvgSize);
        case ResizableSvgSection.CenterLeft:
          return new Rect(new Point(0, Top), originalSvgSize);
        case ResizableSvgSection.CenterCenter:
          return new Rect(new Point(Left, Top), originalSvgSize);
        case ResizableSvgSection.CenterRight:
          return new Rect(new Point(originalSvgSize.Width - Right, Top), originalSvgSize);
        case ResizableSvgSection.BottomLeft:
          return new Rect(new Point(0, originalSvgSize.Height - Bottom), originalSvgSize);
        case ResizableSvgSection.BottomCenter:
          return new Rect(new Point(Left, originalSvgSize.Height - Bottom), originalSvgSize);
        case ResizableSvgSection.BottomRight:
          return new Rect(new Point(originalSvgSize.Width - Right, originalSvgSize.Height - Bottom), originalSvgSize);
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
      return Math.Abs(Top - other.Top) < double.Epsilon
        && Math.Abs(Right - other.Right) < double.Epsilon
        && Math.Abs(Bottom - other.Bottom) < double.Epsilon
        && Math.Abs(Left - other.Left) < double.Epsilon;
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
        public static readonly BindableProperty Svg9SliceInsetsProperty =
            BindableProperty.Create("SvgPath", typeof(ResizableSvgInsets), typeof(SvgImage), default(ResizableSvgInsets));

        /// <summary>
        /// The path to the svg file
        /// </summary>
        public ResizableSvgInsets Svg9SliceInsets
        {
            get { return (ResizableSvgInsets)GetValue(Svg9SliceInsetsProperty); }
            set { SetValue(Svg9SliceInsetsProperty, value); }
        }
  }
}