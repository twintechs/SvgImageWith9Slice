using System.Reflection;

using Xamarin.Forms;
using SVG.Forms.Plugin.Abstractions;
using System.ComponentModel;
using System.Threading.Tasks;

namespace SvgSliceSpike {
    public class TestModel : INotifyPropertyChanged {
        int _AllSidesInset;
        public int AllSidesInset {
            get { return _AllSidesInset; }
            set {
                if (value != _AllSidesInset) {
                    _AllSidesInset = value;
                    _SvgInsets = new ResizableSvgInsets(AllSidesInset);
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(AllSidesInset)));
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SvgInsets)));
                }
            }
        }
        ResizableSvgInsets _SvgInsets;
        public ResizableSvgInsets SvgInsets {
            get {
                return _SvgInsets;
            }
        }

        public TestModel() {
            AllSidesInset = 0;
        }

        #region INotifyPropertyChanged implementation
        public event PropertyChangedEventHandler PropertyChanged = delegate { };
        #endregion
    }
    public class App : Application {
        readonly TestModel _ViewModel;
        int _CurrentStretchableInset = 0;
        SvgImage _SlicingSvg;
        readonly Slider _InsetSlider;
        public App() {
            _ViewModel = new TestModel();
            _InsetSlider = new Slider() {
                Minimum = 0,
                Maximum = 39.5,
                Value = _ViewModel.AllSidesInset,
            };
            _InsetSlider.SetBinding(Slider.ValueProperty, nameof(TestModel.AllSidesInset), BindingMode.TwoWay);
            _SlicingSvg = new SvgImage() {
                SvgPath = "SvgSliceSpike.Assets.MocastIcon.svg",
                SvgAssembly = typeof(App).GetTypeInfo().Assembly,
                SvgStretchableInsets = new ResizableSvgInsets(_InsetSlider.Value),
                WidthRequest = 300,
                HeightRequest = 300,
//                WidthRequest = 139,
//                HeightRequest = 79,
            };
            _SlicingSvg.SetBinding(SvgImage.SvgStretchableInsetsProperty, nameof(TestModel.SvgInsets));

            // The root page of your application
            MainPage = new ContentPage {
                Content = new StackLayout {
                    VerticalOptions = LayoutOptions.Center,
                    HorizontalOptions = LayoutOptions.Center,
                    Children = {
                        _InsetSlider,
                        _SlicingSvg,
                    },
                    BindingContext = _ViewModel,
                },
            };
        }

        protected override void OnStart() {
            // Handle when your app starts
        }

        protected override void OnSleep() {
            // Handle when your app sleeps
        }

        protected override void OnResume() {
            // Handle when your app resumes
        }
    }
}

