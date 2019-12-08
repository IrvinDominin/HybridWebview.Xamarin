using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace HybridWebview.Xamarin
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();

            BindingContext = new MainPageViewModel();
        }

        protected override bool OnBackButtonPressed()
        {
            if (hybridWebView.CanGoBackFunction())
            {
                hybridWebView.GoBack();
                return true;
            }
            else
                return base.OnBackButtonPressed();
        }
    }
}
