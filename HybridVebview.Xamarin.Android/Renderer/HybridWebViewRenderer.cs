using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Provider;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Webkit;
using Android.Widget;
using Java.Interop;
using Newtonsoft.Json;
using Plugin.DownloadManager;
using Plugin.DownloadManager.Abstractions;
using Plugin.Permissions;
using Plugin.Permissions.Abstractions;
using HybridWebview.Xamarin;
using HybridWebview.Xamarin.Droid;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;
using WebView = Xamarin.Forms.WebView;
using Xamarin.Essentials;

[assembly: ExportRenderer(typeof(HybridWebView), typeof(HybridWebViewRenderer))]
namespace HybridWebview.Xamarin.Droid
{
    public class HybridWebViewRenderer : WebViewRenderer
    {
        Context _context;
        public HybridWebViewRenderer(Context context) : base(context)
        {
            _context = context;
        }

        protected override void OnElementChanged(ElementChangedEventArgs<WebView> e)
        {
            base.OnElementChanged(e);

            if (e.OldElement is HybridWebView oldWebView)
            {
                oldWebView.EvaluateJavascript = null;
                Control.RemoveJavascriptInterface("XamarinJsBridge");
            }

            if (e.NewElement is HybridWebView newWebView)
            {
                newWebView.EvaluateJavascript = async (js) =>
                {
                    ManualResetEvent reset = new ManualResetEvent(false);
                    var response = "";
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        System.Diagnostics.Debug.WriteLine("Javascript Send: " + js);
                        Control?.EvaluateJavascript(js, new XamarinJsCallback((r) =>
                        {
                            response = r;
                            reset.Set();
                        }));
                    });
                    await Task.Run(() => { reset.WaitOne(); });
                    if (response == "null")
                        response = string.Empty;

                    return response;
                };
            }

            if (Control != null && e.NewElement != null)
            {
                InitializeCommands((HybridWebView)e.NewElement);
                SetupControl();
            }
        }

        /// <summary>
        /// Sets up various settings for the Android WebView
        /// </summary>
        private void SetupControl()
        {
            // Ensure common functionality is enabled
            Control.Settings.DomStorageEnabled = true;
            Control.Settings.JavaScriptEnabled = true;

            Control.Settings.MinimumFontSize = 0;

            // Because Android 4.4 and below doesn't respect ViewPort in HTML
            if (Build.VERSION.SdkInt < BuildVersionCodes.Lollipop)
                Control.Settings.UseWideViewPort = true;

            Control.Settings.LoadWithOverviewMode = true;
            Control.Settings.SetPluginState(WebSettings.PluginState.On);
            Control.Settings.UserAgentString = $"{Control.Settings.UserAgentString} SistemiMobileApp/1.0";

            Control.Settings.SetAppCacheMaxSize(50 * 1024 * 1024);
            Control.Settings.SetAppCachePath(Context.CacheDir.AbsolutePath);
            Control.Settings.AllowFileAccess = true;
            Control.Settings.SetAppCacheEnabled(true);
            Control.Settings.CacheMode = CacheModes.CacheElseNetwork;

            // Necessario per far dimensionare correttamente la webview come il contenitore
            Control.LayoutParameters = new global::Android.Widget.RelativeLayout.LayoutParams(LayoutParams.MatchParent, LayoutParams.MatchParent);

            Control.AddJavascriptInterface(new XamarinJsBridge(this), "XamarinJsBridge");
        }

        /// <summary>
        /// Will wire up the commands in the WebViewer control to the native method calls
        /// </summary>
        /// <param name="element"></param>
        private void InitializeCommands(HybridWebView element)
        {
            element.GoBack = () =>
            {
                var ctrl = Control;
                if (ctrl == null)
                    return;

                if (ctrl.CanGoBack())
                {
                    ctrl.GoBack();
                }
                //else
                //{
                    //((MainActivity)_context).FinishAffinity();
                    //((MainActivity)_context).MoveTaskToBack(true);
                    //((MainActivity)this.Context).StartActivity(new Intent(Intent.ActionMain).AddCategory(Intent.CategoryHome));
                //}
            };

            element.CanGoBackFunction = () =>
            {
                var ctrl = Control;
                if (ctrl == null)
                    return false;

                return ctrl.CanGoBack();
            };

            var progressBar = new Android.Widget.ProgressBar(_context, null, Android.Resource.Attribute.ProgressBarStyleHorizontal);
            
            // This allows you to show a file chooser dialog from the WebView
            Control.SetWebChromeClient(new CustomWebChromeClient((uploadMsg, acceptType, capture) =>
            {
                MainActivity.UploadMessage = uploadMsg;
                if (Build.VERSION.SdkInt < BuildVersionCodes.Kitkat)
                {
                    var i = new Intent(Intent.ActionGetContent);

                    //To set all type of files
                    i.SetType("*/*");

                    //Here File Chooser dialog is started as Activity, and it gives result while coming back from that Activity.
                    ((MainActivity)this.Context).StartActivityForResult(Intent.CreateChooser(i, "File Chooser"), MainActivity.FILECHOOSER_RESULTCODE);
                }
                else
                {
                    var i = new Intent(Intent.ActionOpenDocument);
                    i.AddCategory(Intent.CategoryOpenable);

                    //To set all image file types. You can change to whatever you need
                    i.SetType("*/*");

                    //Here File Chooser dialog is started as Activity, and it gives result while coming back from that Activity.
                    ((MainActivity)this.Context).StartActivityForResult(Intent.CreateChooser(i, "File Chooser"), MainActivity.FILECHOOSER_RESULTCODE);
                }
            }, progressBar));


            Control.AddView(progressBar);
        }

        internal class XamarinJsCallback : Java.Lang.Object, IValueCallback
        {
            public XamarinJsCallback(Action<string> callback)
            {
                _callback = callback;
            }

            private Action<string> _callback;
            public void OnReceiveValue(Java.Lang.Object value)
            {
                System.Diagnostics.Debug.WriteLine("Javascript Return: " + Convert.ToString(value));
                _callback?.Invoke(Convert.ToString(value));
            }
        }

        internal class XamarinJsBridge : Java.Lang.Object
        {
            readonly WeakReference<HybridWebViewRenderer> _hybridWebViewRenderer;

            public XamarinJsBridge(HybridWebViewRenderer hybridRenderer)
            {
                _hybridWebViewRenderer = new WeakReference<HybridWebViewRenderer>(hybridRenderer);
            }

            [JavascriptInterface]
            [Export("invokeDownloadAction")]
            public async void InvokeDownloadAction(string data)
            {
                // Riferimento istanza di download manager e cross permission; il thread interno potrebbe sporcarle
                var downloadManager = CrossDownloadManager.Current;
                var crossPermission = CrossPermissions.Current;

                try
                {
                    var status = await crossPermission.CheckPermissionStatusAsync(Permission.Storage);
                    if (status != PermissionStatus.Granted)
                    {
                        var results = await crossPermission.RequestPermissionsAsync(new[] { Permission.Storage });
                        status = results[Permission.Storage];
                    }

                    if (status == PermissionStatus.Granted)
                    {
                        downloadManager.PathNameForDownloadedFile = new Func<IDownloadFile, string>(file =>
                        {
                            string fileName = Android.Net.Uri.Parse(file.Url).Query.Split('&').Last()
                                .Replace("_n=", "");

                            //string pathName = Android.App.Application.Context.GetExternalFilesDir(Android.OS.Environment.DirectoryDownloads).AbsolutePath;
                            string pathName = Path.Combine(Android.OS.Environment.ExternalStorageDirectory.AbsolutePath,
                                Android.OS.Environment.DirectoryDownloads);
                            return Path.Combine(pathName, fileName);
                        });

                        // Necessario per avere l'icona del file scaricato in download manager
                        ((DownloadManagerImplementation) downloadManager).NotificationVisibility =
                            DownloadVisibility.VisibleNotifyCompleted;

                        var fileDownloaded = downloadManager.CreateDownloadFile(data);

                        fileDownloaded.PropertyChanged += (sender, e) =>
                        {
                            if (e.PropertyName == "Status")
                            {
                                switch (((IDownloadFile)sender).Status)
                                {
                                    case DownloadFileStatus.COMPLETED:
                                    case DownloadFileStatus.FAILED:
                                    case DownloadFileStatus.CANCELED:

                                        Vibration.Vibrate();

                                        break;
                                }
                            }
                        };

                        downloadManager.Start(fileDownloaded);
                    }
                    else if (status != PermissionStatus.Unknown)
                    {
                        //location denied
                    }
                }
                catch (Exception ex)
                {
                    //Something went wrong
                }
            }
        }
    }
}