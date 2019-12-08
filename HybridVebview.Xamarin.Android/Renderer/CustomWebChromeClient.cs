using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Webkit;
using Android.Widget;
using String = Java.Lang.String;

namespace HybridWebview.Xamarin.Droid
{
    public class CustomWebChromeClient : WebChromeClient
    {
        Action<IValueCallback, Java.Lang.String, Java.Lang.String> callback;
        Android.Widget.ProgressBar progressBar;

        public CustomWebChromeClient(Action<IValueCallback, Java.Lang.String, Java.Lang.String> callback, Android.Widget.ProgressBar progressBar)
        {
            this.callback = callback;
            this.progressBar = progressBar;
        }

        public override void OnProgressChanged(Android.Webkit.WebView view, int newProgress)
        {
            if (newProgress < 100 && progressBar.Visibility == ViewStates.Gone)
            {
                progressBar.Visibility = ViewStates.Visible;
            }
            
            progressBar.SetProgress(newProgress, true);

            if (newProgress == 100)
            {
                //progressBar.Visibility = ViewStates.Gone;
            }
        }

        //For Android 4.1+
        [Java.Interop.Export]
        public void openFileChooser(IValueCallback uploadMsg, Java.Lang.String acceptType, Java.Lang.String capture)
        {
            callback(uploadMsg, acceptType, capture);
        }

        // For Android 5.0+
        public override bool OnShowFileChooser(WebView webView, IValueCallback filePathCallback, FileChooserParams fileChooserParams)
        {
            callback(filePathCallback, null, null);

            return true;
        }
    }
}