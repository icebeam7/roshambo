using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Roshambo.Mobile
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            MainPage = new Views.PlayView();
        }

        protected override void OnStart()
        {
        }

        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
        }
    }
}
