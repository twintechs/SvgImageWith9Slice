using System.Reflection;

using Xamarin.Forms;
using SVG.Forms.Plugin.Abstractions;
using System.ComponentModel;
using System.Threading.Tasks;

namespace SvgSliceSpike {
    public class TestModel : INotifyPropertyChanged {
        double _AllSidesInset;
        public double AllSidesInset {
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
            AllSidesInset = 38;
        }

        #region INotifyPropertyChanged implementation
        public event PropertyChangedEventHandler PropertyChanged = delegate { };
        #endregion
    }
    public class App : Application {
        readonly TestModel _ViewModel;
        SvgImage _SlicingSvg;
        readonly Slider _InsetSlider;
        public App() {
            _ViewModel = new TestModel();
            _InsetSlider = new Slider() {
                Minimum = 0,
                Maximum = 39.5,
                Value = _ViewModel.AllSidesInset,
            };
            _SlicingSvg = new SvgImage() {
                SvgPath = "SvgSliceSpike.Assets.twintechs-logo.svg",
                SvgAssembly = typeof(App).GetTypeInfo().Assembly,
                SvgStretchableInsets = new ResizableSvgInsets(_InsetSlider.Value),
                WidthRequest = 300,
                HeightRequest = 300,
//                WidthRequest = 139,
//                HeightRequest = 79,
            };

            _InsetSlider.ValueChanged += (sender, e) => {
                // HACK: SvgImage can't be changed after creation (yet). Toss the old and create a new one.
                _SlicingSvg.SvgStretchableInsets = new ResizableSvgInsets(e.NewValue);
            };

            // The root page of your application
            MainPage = new ContentPage {
                Content = new ScrollView {
                    Content = new StackLayout {
                        VerticalOptions = LayoutOptions.Center,
                        HorizontalOptions = LayoutOptions.Center,
                        Children = {
                            _InsetSlider,
                            _SlicingSvg,
                            new BoxView {
                                HeightRequest = 700,
                            },
                        },
                        BindingContext = _ViewModel,
                    },
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

