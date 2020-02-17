using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using WPFHelpers.Commands;
using WPFHelpers;
using System.Windows.Threading;
using System.Threading;
using ChartCanvasNamespace.Entities;
using Newtonsoft.Json;
using System.Windows.Media;
using System.Windows.Controls;

namespace ChartToJson.ViewModel
{
    public partial class MainVM
    {
        #region fields
        private readonly string _TEMP_IMAGE_FILE_NAME = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "CTJTmpIF");
        private readonly List<string> _ImageTypes = new List<string>(3) { "bmp", "jpg", "png" };
        private View.Windows.ExportToImageConfigWindow _ExportWindow;
        private string _SelectedImageType = "png";
        private int _DPI = 96;
        private Visibility _JPGQualityVisibility = Visibility.Collapsed;
        private int _JPGQuality = 50;
        private string _PreviewImageSource = "";
        private string _BottomBarText = "";
        private bool _CanUpdatePreviewImage = true;
        private BitmapEncoder _Encoder = null;
        private Task _ProgressAnimation;
        private CancellationTokenSource _CTS = new CancellationTokenSource();
        #endregion

        #region properties
        public IReadOnlyList<string> ImageTypes { get { return _ImageTypes.AsReadOnly(); } }
        public string SelectedImageType
        {
            get { return _SelectedImageType; }
            set
            {
                if (value == null)
                {
                    if (_SelectedImageType != null)
                    {
                        _SelectedImageType = null;
                        OnPropertyChanged(nameof(SelectedImageType));
                    }
                }
                else if (!value.Equals(_SelectedImageType))
                {
                    _SelectedImageType = value;
                    OnPropertyChanged(nameof(SelectedImageType));
                    if (value.Equals("jpg"))
                        JPGQualityVisibility = Visibility.Visible;
                    else
                        JPGQualityVisibility = Visibility.Collapsed;
                }
            }
        }
        public int DPI
        {
            get { return _DPI; }
            set
            {
                if (value != _DPI)
                {
                    _DPI = value;
                    OnPropertyChanged(nameof(DPI));
                }
            }
        }
        public Visibility JPGQualityVisibility
        {
            get { return _JPGQualityVisibility; }
            set
            {
                if (value != _JPGQualityVisibility)
                {
                    _JPGQualityVisibility = value;
                    OnPropertyChanged(nameof(JPGQualityVisibility));
                }
            }
        }
        public int JPGQuality
        {
            get { return _JPGQuality; }
            set
            {
                if (value != _JPGQuality)
                {
                    _JPGQuality = value;
                    OnPropertyChanged(nameof(JPGQuality));
                }
            }
        }
        public string PreviewImageSource
        {
            get { return _PreviewImageSource; }
            set
            {
                if (value == null)
                {
                    if (_PreviewImageSource != null)
                    {
                        _PreviewImageSource = null;
                        OnPropertyChanged(nameof(PreviewImageSource));
                    }
                }
                else if (!value.Equals(_PreviewImageSource))
                {
                    _PreviewImageSource = value;
                    OnPropertyChanged(nameof(PreviewImageSource));
                }
            }
        }
        public string BottomBarText
        {
            get { return _BottomBarText; }
            set
            {
                if (value == null)
                {
                    if (_BottomBarText != null)
                    {
                        _BottomBarText = null;
                        OnPropertyChanged(nameof(BottomBarText));
                    }
                }
                else if (!value.Equals(_BottomBarText))
                {
                    _BottomBarText = value;
                    OnPropertyChanged(nameof(BottomBarText));
                }
            }
        }
        public bool CanUpdatePreviewImage
        {
            get { return _CanUpdatePreviewImage; }
            set
            {
                if (value != _CanUpdatePreviewImage)
                {
                    _CanUpdatePreviewImage = value;
                    OnPropertyChanged(nameof(CanUpdatePreviewImage));
                }
            }
        }
        #endregion

        public void OnExportWindowLoaded(View.Windows.ExportToImageConfigWindow window)
        {
            _ExportWindow = window;
            UpdatePreviewImage();
        }
        public void OnEnterKey()
        {
            UpdatePreviewImage();
        }

        #region commands
        public ICommand ExportToImageCommand => new AsyncDelegateCommand(x => ExportToImageAsync(), null);
        public ICommand ExportToJSONCommand => new AsyncDelegateCommand(x => ExportToJSONAsync(), null);
        public ICommand PreviewImageCommand => new DelegateCommand(x => UpdatePreviewImage(), null);

        private async Task ExportToImageAsync()
        {
            if(EntitiesVM.Count == 0)
            {
                MessageBox.Show("Can't export with no entites!", "Error");
                return;
            }

            var exportWindow = new View.Windows.ExportToImageConfigWindow();
            var exportResult = exportWindow.ShowDialog();

            if (exportResult.HasValue && exportResult.Value)
            {
                var dialog = new SaveFileDialog();
                dialog.Title = Properties.Settings.Default.ExportToImageDialogTitle;
                dialog.InitialDirectory = Properties.Settings.Default.LastFileLoadedPath;
                dialog.Filter = $"{SelectedImageType.ToUpper()} (*.{SelectedImageType})|*.{SelectedImageType}"; //"PNG (*.png)|*.png|BMP (*.bmp)|*.bmp|JPG (*.jpg)|*.jpg";

                var result = dialog.ShowDialog();
                if (!result.HasValue || !result.Value)
                {
                    return;
                }

                SetProgressBar("Exporting to image...", true, 0);
                if (_ProgressAnimation != null && !_ProgressAnimation.IsCompleted)
                    _CTS.Cancel();
                _ProgressAnimation = AnimateProgressBarTo(90, _CTS.Token);

                switch (SelectedImageType)
                {
                    case "bmp":
                        _Encoder = new BmpBitmapEncoder();
                        break;
                    case "jpg":
                        _Encoder = new JpegBitmapEncoder() { QualityLevel = JPGQuality };
                        break;
                    default:// "png":
                        _Encoder = new PngBitmapEncoder();
                        break;
                }
                var render = RenderControl(_ExportWindow.ImagePreview);
                _Encoder.Frames.Add(BitmapFrame.Create(render));

                //var temp = Path.ChangeExtension(_TEMP_IMAGE_FILE_NAME, SelectedImageType);
                //Messenger.SendGuidedMessage(Properties.Messages.Default.AnimateProgressTo, 90);
                try
                {
                    using (var stream = new MemoryStream())
                    {
                        _Encoder.Save(stream);
                        using (FileStream fileStream = File.Open(dialog.FileName, FileMode.OpenOrCreate))
                        {
                            var array = stream.ToArray();
                            await fileStream.WriteAsync(array, 0, array.Length);
                        }
                    }
                    ////using (var stream = new MemoryStream())
                    ////{
                    ////_Encoder.Save(stream);

                    //using (FileStream tempStream = File.Open(temp, FileMode.Open))
                    //using (FileStream fileStream = File.Open(dialog.FileName, FileMode.OpenOrCreate))
                    //{
                    //    await tempStream.CopyToAsync(fileStream);
                    //    //    var array = stream.ToArray();
                    //    //await fileStream.WriteAsync(array, 0, array.Length);
                    //}
                    ////}
                }
                catch (Exception e)
                {
                    e.ShowException($"Error writing image file:{Environment.NewLine}");
                    if (!_ProgressAnimation.IsCompleted)
                        _CTS.Cancel();
                    SetProgressBar("", false, 0);
                    return;
                }

                if (_ProgressAnimation != null && !_ProgressAnimation.IsCompleted)
                    _CTS.Cancel();
                _ProgressAnimation = AnimateProgressBarTo(100, _CTS.Token);
                //Messenger.SendGuidedMessage(Properties.Messages.Default.AnimateProgressTo, 100);

                MessageBox.Show("Flow chart exported succesfully.", "Complete");
            }

            SetProgressBar("", false, 0);
        }
        private async Task ExportToJSONAsync()
        {
            if (EntitiesVM.Count == 0)
            {
                MessageBox.Show("Can't export with no entites!");
                return;
            }

            SetProgressBar("Exporting to JSON...", true, 0);
            var entitiesTask = GetAllEntities();

            var dialog = new SaveFileDialog();
            dialog.Title = Properties.Settings.Default.ExportToJSONDialogTitle;
            dialog.InitialDirectory = Properties.Settings.Default.LastJsonExportedPath;
            dialog.Filter = "JSON file (*.json)|*.json|All (*.*)|*.*";

            var result = dialog.ShowDialog();
            if (!result.HasValue || !result.Value)
            {
                SetProgressBar("", false, 0);
                return;
            }

            _CTS = new CancellationTokenSource();
            _ProgressAnimation = AnimateProgressBarTo(50, _CTS.Token);

            var entities = await entitiesTask;
            if (entities.Length == 0)
                entities = new IChartEntity[1] { EntitiesVM[0].Entity };

            if (!_ProgressAnimation.IsCompleted)
                _CTS.Cancel();
            SetProgressBar(50);
            _ProgressAnimation = AnimateProgressBarTo(90, _CTS.Token);

            var jsonMan = new JsonLibrary.JsonManager(new JsonSerializerSettings()
            {
                TypeNameHandling = TypeNameHandling.Auto,
                PreserveReferencesHandling = PreserveReferencesHandling.All
            });
            try
            {
                await jsonMan.SerializeToJsonInFile_ReadableAsync(entities, dialog.FileName);
            }
            catch(Exception e)
            {
                e.ShowException($"Error trying to export to JSON:{Environment.NewLine}");
                if (!_ProgressAnimation.IsCompleted)
                    _CTS.Cancel();
                SetProgressBar("", false, 0);
                return;
            }

            if (!_ProgressAnimation.IsCompleted)
                _CTS.Cancel();
            SetProgressBar(100);

            MessageBox.Show("Flow chart exported succesfully.");
            SetProgressBar("", false, 0);
        }
        private void UpdatePreviewImage()
        {
            CanUpdatePreviewImage = false;
            BottomBarText = "Updating preview image...";
            PreviewImageSource = "";
            switch (SelectedImageType)
            {
                case "bmp":
                    _Encoder = new BmpBitmapEncoder();
                    break;
                case "jpg":
                    _Encoder = new JpegBitmapEncoder() { QualityLevel = JPGQuality };
                    break;
                default:// "png":
                    _Encoder = new PngBitmapEncoder();
                    break;
            }

            ChartCanvasNamespace.ChartCustomControl.Instance.ClearTransforms();
            var render = RenderControl(ChartCanvasNamespace.ChartCustomControl.Instance.ChartCanvas);
            ChartCanvasNamespace.ChartCustomControl.Instance.ApplyTransforms();
            //var render = (RenderTargetBitmap)Messenger.SendGuidedMessageWithResponse(Properties.Messages.Default.GetCanvasRender, DPI).SingleOrDefault(); //await renderCanvasTask;

            if (render == null)
                return;

            var temp = Path.ChangeExtension(_TEMP_IMAGE_FILE_NAME, SelectedImageType);
            _Encoder.Frames.Add(BitmapFrame.Create(render));
            try
            {
                using (var stream = new MemoryStream())
                {
                    _Encoder.Save(stream);
                    using (FileStream fileStream = File.Open(temp, FileMode.OpenOrCreate))
                    {
                        var array = stream.ToArray();
                        fileStream.Write(array, 0, array.Length);
                    }
                }
            }
            catch (Exception e)
            {
                e.ShowException($"Error with preview image file:{Environment.NewLine}");
                SetProgressBar("", false, 0);
                return;
            }

            if (_ProgressAnimation != null && !_ProgressAnimation.IsCompleted)
                _CTS.Cancel();
            SetProgressBar(50);
            //Messenger.SendGuidedMessage(Properties.Messages.Default.SetProgressTo, 50);
            Application.Current.Dispatcher.Invoke(() => PreviewImageSource = temp);
            BottomBarText = "Preview image updated.";
            CanUpdatePreviewImage = true;
        }
        #endregion

        #region helpers
        private void SetProgressBar(string text, bool visible, int value)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                ProgressText = text;
                ProgressRowVisibility = visible ? Visibility.Visible : Visibility.Collapsed;
                ProgressValue = value;
            });
        }
        private void SetProgressBar(int value)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                ProgressValue = value;
            });
        }
        private async Task AnimateProgressBarTo(int value, CancellationToken token)
        {
            for (int i = ProgressValue; i < value; i++)
            {
                if (Application.Current == null || token.IsCancellationRequested)
                    break;

                await Task.Delay(100);

                Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, (Action)(() => ProgressValue = i));
            }
        }
        private async Task<IChartEntity[]> GetAllEntities()
        {
            return EntitiesVM
                .Select(x => x.Entity)
                .ToArray();
        }
        private RenderTargetBitmap RenderControl(FrameworkElement control)
        {
            try
            {
                Rect rect = new Rect(control.RenderSize);
                RenderTargetBitmap rtb = new RenderTargetBitmap(
                    (int)Math.Round(control.RenderSize.Width * DPI / 96),
                    (int)Math.Round(control.RenderSize.Height * DPI / 96),
                    DPI,
                    DPI,
                    System.Windows.Media.PixelFormats.Default);
                rtb.Render(control);
                return rtb;
            }
            catch (Exception e)
            {
                e.ShowException($"Error rendering flow chart canvas:{Environment.NewLine}");
                return null;
            }
        }
        #endregion
    }
}

#region old
//public IReadOnlyList<string> ImageTypes { get { return _ImageTypes.AsReadOnly(); } }
//public string SelectedImageType
//{
//    get { return _SelectedImageType; }
//    set
//    {
//        if (value == null)
//        {
//            if (_SelectedImageType != null)
//            {
//                _SelectedImageType = null;
//                OnPropertyChanged(nameof(SelectedImageType));
//            }
//        }
//        else if (!value.Equals(_SelectedImageType))
//        {
//            _SelectedImageType = value;
//            OnPropertyChanged(nameof(SelectedImageType));
//            if (value.Equals("jpg"))
//                JPGQualityVisibility = Visibility.Visible;
//            else
//                JPGQualityVisibility = Visibility.Collapsed;
//        }
//    }
//}
//public int DPI
//{
//    get { return _DPI; }
//    set
//    {
//        if (value != _DPI)
//        {
//            _DPI = value;
//            OnPropertyChanged(nameof(DPI));
//        }
//    }
//}
//public Visibility JPGQualityVisibility
//{
//    get { return _JPGQualityVisibility; }
//    set
//    {
//        if (value != _JPGQualityVisibility)
//        {
//            _JPGQualityVisibility = value;
//            OnPropertyChanged(nameof(JPGQualityVisibility));
//        }
//    }
//}
//public int JPGQuality
//{
//    get { return _JPGQuality; }
//    set
//    {
//        if (value != _JPGQuality)
//        {
//            _JPGQuality = value;
//            OnPropertyChanged(nameof(JPGQuality));
//        }
//    }
//}
//private readonly List<string> _ImageTypes = new List<string>(3) { "bmp", "jpg", "png" };
//private string _SelectedImageType = "png";
//private int _DPI = 120;
//private Visibility _JPGQualityVisibility = Visibility.Collapsed;
//private int _JPGQuality = 50;
//private async Task<RenderTargetBitmap> RenderCanvas()
//{
//    try
//    {
//        var canvas = ChartCanvasNamespace.ChartCustomControl.Instance.ChartCanvas;
//        Rect rect = new Rect(canvas.RenderSize);
//        RenderTargetBitmap rtb = new RenderTargetBitmap(
//            (int)Math.Round(canvas.RenderSize.Width * DPI / 96),
//            (int)Math.Round(canvas.RenderSize.Height * DPI / 96),
//            DPI,
//            DPI,
//            System.Windows.Media.PixelFormats.Default);
//        rtb.Render(canvas);
//        return rtb;
//    }
//    catch (Exception e)
//    {
//        e.ShowException($"Error rendering flow chart canvas:{Environment.NewLine}");
//        return null;
//    }
//}
#endregion
