using System.Reflection;

using Xamarin.Forms;
using SVG.Forms.Plugin.Abstractions;

namespace SvgSliceSpike {
    public class App : Application {
        public App() {
            // The root page of your application
            MainPage = new ContentPage {
                Content = new StackLayout {
                    VerticalOptions = LayoutOptions.Center,
                    Children = {
                        new SvgImage() {
                            SvgPath = "SvgSliceSpike.Assets.twintechs-logo.svg",
                            SvgAssembly = typeof(App).GetTypeInfo().Assembly, 
                            HeightRequest = 100,
                            WidthRequest = 100
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

