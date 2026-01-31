using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace TaskPlanner
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            Application.Current.Resources.Clear();

            MainPage = new NavigationPage(new MainPage()) // обозначаем MainPage как главную
            {
                BarBackgroundColor = Color.FromHex("#2196F3"),
                BarTextColor = Color.White
            };
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