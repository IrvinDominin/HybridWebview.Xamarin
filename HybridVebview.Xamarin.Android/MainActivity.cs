using System;
using System.Configuration;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Android.Webkit;
using Plugin.Permissions;

namespace HybridWebview.Xamarin.Droid
{
    [Activity(Label = "HybridWebview.Xamarin", 
        Icon = "@mipmap/icon", 
        Theme = "@style/Theme.Splash", 
        MainLauncher = true, 
        ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        public static IValueCallback UploadMessage; // Used for File Chooser in WebViewRender

        protected override void OnCreate(Bundle bundle)
        {
            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;

            base.OnCreate(bundle);

            Plugin.CurrentActivity.CrossCurrentActivity.Current.Init(this, bundle);

#if DEBUG
            global::Android.Webkit.WebView.SetWebContentsDebuggingEnabled(true);
#endif

            global::Xamarin.Forms.Forms.Init(this, bundle);

            LoadApplication(new App());
        }

        internal static int FILECHOOSER_RESULTCODE = 1;

        protected override void OnActivityResult(int requestCode, Result resultCode, Intent intent)
        {
            // Handles the response from the FileChooser
            if (requestCode == FILECHOOSER_RESULTCODE)
            {
                if (null == UploadMessage)
                    return;
                Java.Lang.Object result = intent == null || resultCode != Result.Ok
                    ? null
                    : intent.Data;
                UploadMessage.OnReceiveValue((result == null) ? null : new Android.Net.Uri[] { (Android.Net.Uri)result });
                UploadMessage = null;
            }
            else
            {
                base.OnActivityResult(requestCode, resultCode, intent);
            }
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            PermissionsImplementation.Current.OnRequestPermissionsResult(requestCode, permissions, grantResults);
            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }
    }
}