using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        BitmapImage _Thumbnail;
        public BitmapImage Thumbnail {
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
        public static BitmapImage GetBitmapImage(string path, int? decodePixelHeight = null) {
            BitmapImage bmp;
            using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read)) {
                bmp = new BitmapImage();
                bmp.BeginInit();
                bmp.StreamSource = stream;
                if (decodePixelHeight != null) bmp.DecodePixelHeight = decodePixelHeight.Value;
                bmp.CacheOption = BitmapCacheOption.OnLoad;
                bmp.EndInit();
                bmp.Freeze();
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
