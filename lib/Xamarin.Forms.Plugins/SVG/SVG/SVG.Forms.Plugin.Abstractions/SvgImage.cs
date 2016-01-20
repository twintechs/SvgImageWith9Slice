using System.Reflection;
using Xamarin.Forms;

namespace SVG.Forms.Plugin.Abstractions
{
    public struct ResizableSvgInsets {
        public double Top { get; set; }
        public double Right { get; set; }
        public double Bottom { get; set; }
        public double Left { get; set; }

        public ResizableSvgInsets(double top, double right, double bottom, double left) {
            Top = top;
            Right = right;
            Bottom = bottom;
            Left = left;
        }
        public ResizableSvgInsets(double vertical, double horizontal) : this(vertical, horizontal, vertical, horizontal) { }
        public ResizableSvgInsets(double allSides) : this(allSides, allSides, allSides, allSides) { }
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