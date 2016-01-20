using System.Reflection;

using Xamarin.Forms;
using SVG.Forms.Plugin.Abstractions;

namespace SvgSliceSpike {
    public class App : Application {
        public App() {
            var SvgSliceInsets = new ResizableSvgInsets(5);
            // The root page of your application
            MainPage = new ContentPage {
                Content = new StackLayout {
                    VerticalOptions = LayoutOptions.Center,
                    HorizontalOptions = LayoutOptions.Center,
                    Children = {
                        new SvgImage() {
                            SvgPath = "SvgSliceSpike.Assets.twintechs-logo.svg",
                            SvgAssembly = typeof(App).GetTypeInfo().Assembly, 
                            Svg9SliceInsets = SvgSliceInsets,
                            WidthRequest = 139,
                            HeightRequest = 79,
                        },
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

