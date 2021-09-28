using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml.Serialization;

namespace SlideShowCreator {
    //setの時やTextChangedで入力値を制限しても良いけど勉強のためにIDataErrorInfoを使ってエラーを表現してみる
    //そこで、例えばWaitをintにするとTextBoxに「aaa」なんて入力すると中でどうなってるのかよくわからなくなるのでstringとして実装
    public class MainWindowViewModel : ViewModelBase {
        public string SettingPath { get; set; }
        string _Wait = "3000";
        public string Wait {
            get { return _Wait; }
            set {
                ValidateInt32(nameof(Wait), value, 0, int.MaxValue);
                _Wait = value;
                OnPropertyChanged(nameof(Wait));
            }
        }

        string _Opacity = "1.0";
        public string Opacity {
            get { return _Opacity; }
            set {
                ValidateDouble(nameof(Opacity), value, 0.0, 1.0);
                _Opacity = value;
                OnPropertyChanged(nameof(Opacity));
            }
        }

        string _R = "0";
        public string R {
            get { return _R; }
            set {
                ValidateInt32(nameof(R), value, 0, 255);
                _R = value;
                OnPropertyChanged(nameof(R));
            }
        }

        string _G = "0";
        public string G {
            get { return _G; }
            set {
                ValidateInt32(nameof(G), value, 0, 255);
                _G = value;
                OnPropertyChanged(nameof(G));
            }
        }

        string _B = "0";
        public string B {
            get { return _B; }
            set {
                ValidateInt32(nameof(B), value, 0, 255);
                _B = value;
                OnPropertyChanged(nameof(B));
            }
        }

        public ObservableCollection<SlideInfo> SlideInfos { get; set; } = new ObservableCollection<SlideInfo>();

        int _SelectedIndex = -1;
        [XmlIgnore]
        public int SelectedIndex {
            get { return _SelectedIndex; }
            set {
                _SelectedIndex = value;
                OnPropertyChanged(nameof(SelectedIndex));
            }
        }

    }
    public class SlideInfo : ViewModelBase {
        public string Path { get; set; }
        public string Name { get { return new FileInfo(Path).Name; } }
        double _Scale = 1.0;
        public double Scale {
            get { return _Scale; }
            set {
                _Scale = value;
                OnPropertyChanged(nameof(Scale));
            }
        }
        double _OffsetX = 0.0;
        public double OffsetX {
            get { return _OffsetX; }
            set {
                _OffsetX = value;
                OnPropertyChanged(nameof(OffsetX));
            }
        }
        double _OffsetY = 0.0;
        public double OffsetY {
            get { return _OffsetY; }
            set {
                _OffsetY = value;
                OnPropertyChanged(nameof(OffsetY));
            }
        }
        string _Wait = "3000";
        public string Wait {
            get { return _Wait; }
            set {
                ValidateInt32(nameof(Wait), value, 0, int.MaxValue);
                _Wait = value;
                OnPropertyChanged(nameof(Wait));
            }
        }
    }
    public class FileSearchWindowViewModel : ViewModelBase {
        public string DirPath { get; set; } = Properties.Settings.Default.DirPath;
        public string Condition { get; set; } = Properties.Settings.Default.Condition;
        public ObservableCollection<SearchResult> Results { get; set; } = new ObservableCollection<SearchResult>();
        public void Save() {
            Properties.Settings.Default.DirPath = DirPath;
            Properties.Settings.Default.Condition = Condition;
            Properties.Settings.Default.Save();
        }
    }
    public class SearchResult :ViewModelBase{
        bool _IsChecked = false;
        public bool IsChecked {
            get { return _IsChecked; }
            set {
                _IsChecked = value;
                OnPropertyChanged(nameof(IsChecked));
            }
        }
        public string Path { get; set; }
        BitmapSource _Thumbnail;
        public BitmapSource Thumbnail {
            get {
                if (_Thumbnail == null) _Thumbnail = Common.GetBitmapImage(Path, 100);
                return _Thumbnail;
            }
        }
    }
    public class ViewModelBase : INotifyPropertyChanged, IDataErrorInfo {
        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        internal void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        #endregion

        #region IDataErrorInfo
        private Dictionary<string, string> errors = new Dictionary<string, string>();
        public string Error {
            get {
                return string.Join('\n', errors.Select(kv => kv.Key + ":" + kv.Value));
            }
        }

        public string this[string columnName] {
            get {
                if (errors.ContainsKey(columnName))
                    return errors[columnName];
                else
                    return null;
            }
        }
        internal void ValidateInt32(string propertyName, string value, int min, int max) {//ジェネリックで書きたいけど無理ぽ
            if (int.TryParse(value, out int tmp)) {
                if ((tmp < min || tmp > max) && !errors.ContainsKey(propertyName)) {
                    errors.Add(propertyName, "Must be a number that from " + min + " to " + max);
                } else if (errors.ContainsKey(propertyName))
                    errors.Remove(propertyName);
            } else if (!errors.ContainsKey(propertyName))
                errors.Add(propertyName, "Must be a number that from " + min + " to " + max);
        }
        internal void ValidateDouble(string propertyName, string value, double min, double max) {
            if (double.TryParse(value, out double tmp)) {
                if ((tmp < min || tmp > max) && !errors.ContainsKey(propertyName)) {
                    errors.Add(propertyName, "Must be a number that from " + min + " to " + max);
                } else if (errors.ContainsKey(propertyName))
                    errors.Remove(propertyName);
            } else if (!errors.ContainsKey(propertyName))
                errors.Add(propertyName, "Must be a number that from " + min + " to " + max);
        }
        #endregion
    }
    public static class Common {
        public static BitmapSource GetBitmapImage(string path, int? decodePixelHeight = null) {
            BitmapSource bmp;
            try {
                var tmp = new BitmapImage();
                using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read)) {
                    tmp.BeginInit();
                    tmp.StreamSource = stream;
                    if (decodePixelHeight != null) tmp.DecodePixelHeight = decodePixelHeight.Value;
                    tmp.CacheOption = BitmapCacheOption.OnLoad;
                    tmp.EndInit();
                    tmp.Freeze();
                }
                bmp = tmp;
            } catch(Exception ex) {
                var size = 500;
                var msg = "An error occurred when reading the image.\r\n" + ex.Message + "\r\n" + path;
                var visual = new DrawingVisual();
                using (DrawingContext drawingContext = visual.RenderOpen()) {
                    drawingContext.DrawRectangle(
                                        System.Windows.Media.Brushes.Black,
                                        new System.Windows.Media.Pen(System.Windows.Media.Brushes.Black, 100.0),
                                        new System.Windows.Rect(0, 0, size, size));
                    var ft = new FormattedText(msg,
                                            CultureInfo.CurrentCulture,
                                            FlowDirection.LeftToRight,
                                            new Typeface(System.Windows.SystemFonts.MessageFontFamily.Source),
                                            24.0,
                                            System.Windows.Media.Brushes.White,
                                            1.0);
                    ft.MaxTextWidth = size-20;
                    drawingContext.DrawText(ft, new System.Windows.Point(10.0, 10.0));
                }
                var rtb = new RenderTargetBitmap(size, size, 96, 96, PixelFormats.Pbgra32);
                rtb.Render(visual);
                bmp = rtb;
            }
            return bmp;
        }
        public static void AddRange<T>(this Collection<T> s, IEnumerable<T> items) {
            foreach (var i in items)
                s.Add(i);
        }
        public static void InsertRange<T>(this Collection<T> s, int index, IEnumerable<T> items) {
            foreach (var i in items.Reverse())
                s.Insert(index, i);
        }
    }
}
