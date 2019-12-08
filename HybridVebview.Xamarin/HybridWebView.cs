using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace HybridWebview.Xamarin
{
    public class HybridWebView : WebView
    {
        public static BindableProperty EvaluateJavascriptProperty =
            BindableProperty.Create(nameof(EvaluateJavascript), typeof(Func<string, Task<string>>), typeof(HybridWebView), null, BindingMode.OneWayToSource);

        public Func<string, Task<string>> EvaluateJavascript
        {
            get
            {
                return (Func<string, Task<string>>)GetValue(EvaluateJavascriptProperty);
            }
            set
            {
                SetValue(EvaluateJavascriptProperty, value);
            }
        }

        public Action Refresh
        {
            get { return (Action)GetValue(RefreshProperty); }
            set { SetValue(RefreshProperty, value); }
        }

        public static BindableProperty RefreshProperty =
            BindableProperty.Create(nameof(Refresh), typeof(Action), typeof(HybridWebView), null, BindingMode.OneWayToSource);

        public static BindableProperty GoBackProperty =
            BindableProperty.Create(nameof(GoBack), typeof(Action), typeof(HybridWebView), null, BindingMode.OneWayToSource);

        public new Action GoBack
        {
            get { return (Action)GetValue(GoBackProperty); }
            set { SetValue(GoBackProperty, value); }
        }

        public static BindableProperty CanGoBackFunctionProperty =
            BindableProperty.Create(nameof(CanGoBackFunction), typeof(Func<bool>), typeof(HybridWebView), null, BindingMode.OneWayToSource);

        public Func<bool> CanGoBackFunction
        {
            get { return (Func<bool>)GetValue(CanGoBackFunctionProperty); }
            set { SetValue(CanGoBackFunctionProperty, value); }
        }

        public static BindableProperty GoBackNavigationProperty =
            BindableProperty.Create(nameof(GoBackNavigation), typeof(Action), typeof(HybridWebView), null, BindingMode.OneWay);

        public Action GoBackNavigation
        {
            get { return (Action)GetValue(GoBackNavigationProperty); }
            set { SetValue(GoBackNavigationProperty, value); }
        }
    }
}
