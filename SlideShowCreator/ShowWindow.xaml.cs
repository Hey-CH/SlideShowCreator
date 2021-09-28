using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace SlideShowCreator {
    /// <summary>
    /// ShowWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class ShowWindow : Window {
        public enum ShowMode {
            OnlyView,
            Single,
            SlideShow
        }
        MainWindowViewModel vm;
        bool running = true;//ループ抜ける用
        int startIndex = 0;//Show開始時のIndex
        int currentIndex = 0;//現在表示中のIndex
        ShowMode showMode = ShowMode.Single;//ShowMode保持用
        string filePath;//FileSearchWindowから表示したときのPath保存用

        public ShowWindow(MainWindowViewModel viewModel, ShowMode mode) {
            InitializeComponent();
            vm = viewModel;

            this.Background = new SolidColorBrush(new Color() {
                                                A = Convert.ToByte(byte.MaxValue * double.Parse(vm.Opacity)),
                                                R = byte.Parse(vm.R), G = byte.Parse(vm.G), B = byte.Parse(vm.B) });
            showMode = mode;
            startIndex = vm.SelectedIndex;
            currentIndex = vm.SelectedIndex;

            if (mode == ShowMode.Single) {
                Shows();
            } else {
                saveNextItem.Visibility = Visibility.Collapsed;
                saveCloseItem.Visibility = Visibility.Collapsed;
                Task.Run(() => {
                    while (running) {
                        Shows();
                        System.Threading.Thread.Sleep(int.Parse(vm.SlideInfos[currentIndex].Wait));
                        if (!running) break;
                        Next();

                        Dispatcher.Invoke(() => {
                            if (!IsRoop && startIndex == currentIndex) Close();
                        });
                    }
                });
            }
        }
        public ShowWindow(string path, MainWindowViewModel mainVM) {
            InitializeComponent();

            filePath = path;
            vm = mainVM;
            showMode = ShowMode.OnlyView;

            this.Background = new SolidColorBrush(new Color() {
                A = Convert.ToByte(byte.MaxValue * double.Parse(vm.Opacity)),
                R = byte.Parse(vm.R), G = byte.Parse(vm.G), B = byte.Parse(vm.B)
            });

            addItem.Visibility = Visibility.Visible;
            roopItem.Visibility = Visibility.Collapsed;
            nextItem.Visibility = Visibility.Collapsed;
            saveNextItem.Visibility = Visibility.Collapsed;
            saveCloseItem.Visibility = Visibility.Collapsed;

            image1.Source = Common.GetBitmapImage(path);
        }

        private bool IsRoop {
            get {
                return Dispatcher.Invoke(new Func<bool>(() => { return roopItem.IsChecked; }));
            }
        }

        private void Save() {
            if (showMode == ShowMode.OnlyView) return;

            var matrix = image1.RenderTransform.Value;
            vm.SlideInfos[currentIndex].Scale = matrix.M11;
            vm.SlideInfos[currentIndex].OffsetX = matrix.OffsetX;
            vm.SlideInfos[currentIndex].OffsetY = matrix.OffsetY;
        }
        private void Next() {
            if (showMode == ShowMode.OnlyView) return;
            var tmp = currentIndex;
            currentIndex += 1;
            if (currentIndex >= vm.SlideInfos.Count) currentIndex = 0;
            if (startIndex == currentIndex) {
                if (!IsRoop) {
                    if (showMode == ShowMode.Single) MessageBox.Show("This is the last item.", "Not Roop");
                    currentIndex = tmp;
                    return;
                }
            }
        }
        private void Prev() {
            if (showMode == ShowMode.OnlyView) return;
            var tmp = currentIndex;
            currentIndex -= 1;
            if (currentIndex < 0) currentIndex = vm.SlideInfos.Count - 1;
            if (startIndex == currentIndex) {
                if (!IsRoop) {
                    if (showMode == ShowMode.Single) MessageBox.Show("This is the first item.", "Not Roop");
                    currentIndex = tmp;
                    return;
                }
            }
        }
        private void Shows() {
            if (showMode == ShowMode.OnlyView) return;

            Dispatcher.Invoke(() => {
                image1.Source = Common.GetBitmapImage(vm.SlideInfos[currentIndex].Path);

                var matrix = image1.RenderTransform.Value;
                var s = vm.SlideInfos[currentIndex].Scale;
                var x = vm.SlideInfos[currentIndex].OffsetX;
                var y = vm.SlideInfos[currentIndex].OffsetY;
                if (vm.SlideInfos[currentIndex].Scale == 1.0 && vm.SlideInfos[currentIndex].OffsetX == 0 && vm.SlideInfos[currentIndex].OffsetY == 0) {
                    var sw = (double)SystemParameters.PrimaryScreenWidth / (double)image1.Source.Width;
                    var sh = (double)SystemParameters.PrimaryScreenHeight / (double)image1.Source.Height;
                    if (sw < 0) sw = double.MaxValue;
                    if (sh < 0) sh = double.MaxValue;
                    s = sw > sh ? sh : sw;
                    x = sw > sh ? (SystemParameters.PrimaryScreenWidth - image1.Source.Width * s) / 2 : 0;
                    y = sw > sh ? 0 : (SystemParameters.PrimaryScreenHeight - image1.Source.Height * s) / 2;
                }
                matrix.M11 = s;
                matrix.M12 = 0.0;
                matrix.M21 = 0.0;
                matrix.M22 = s;
                matrix.OffsetX = x;
                matrix.OffsetY = y;
                image1.RenderTransform = new MatrixTransform(matrix);
            });
        }

        #region MouseEvent
        private void image1_MouseWheel(object sender, MouseWheelEventArgs e) {
            const double scale = 1.1;

            var matrix = image1.RenderTransform.Value;
            if (e.Delta > 0) matrix.ScaleAt(scale, scale, e.GetPosition(this).X, e.GetPosition(this).Y);
            else matrix.ScaleAt(1.0 / scale, 1.0 / scale, e.GetPosition(this).X, e.GetPosition(this).Y);
            image1.RenderTransform = new MatrixTransform(matrix);
        }
        bool down = false;
        Point downPos;
        private void image1_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
            down = true;
            downPos = e.GetPosition(canvas1);
        }

        private void image1_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
            down = false;
        }

        private void image1_MouseMove(object sender, MouseEventArgs e) {
            if (down) {
                var p = e.GetPosition(canvas1);
                var matrix = image1.RenderTransform.Value;
                matrix.OffsetX += p.X - downPos.X;
                matrix.OffsetY += p.Y - downPos.Y;
                image1.RenderTransform = new MatrixTransform(matrix);
                downPos = p;
            }
        }

        private void image1_MouseLeave(object sender, MouseEventArgs e) {
            down = false;
        }
        #endregion

        #region ContextMenu
        private void addItem_Click(object sender, RoutedEventArgs e) {
            //FileSearchWindowから表示されたときのみ表示されるAddを押すとここに来る
            var matrix = image1.RenderTransform.Value;
            vm.SlideInfos.Add(new SlideInfo() {
                                Path = filePath,
                                Scale = matrix.M11,
                                OffsetX = matrix.OffsetX,
                                OffsetY = matrix.OffsetY
                            });
            Close();
        }
        private void Roop_Click(object sender, RoutedEventArgs e) {
            roopItem.IsChecked = !roopItem.IsChecked;
        }
        private void SaveAndNext_Click(object sender, RoutedEventArgs e) {
            Save();
            Next();
            Shows();
        }
        private void Next_Click(object sender, RoutedEventArgs e) {
            Next();
            Shows();
        }

        private void SaveAndClose_Click(object sender, RoutedEventArgs e) {
            Save();
            Close();
        }

        private void Close_Click(object sender, RoutedEventArgs e) {
            Close();
        }
        #endregion

        private void Window_KeyDown(object sender, KeyEventArgs e) {
            if (e.Key == Key.Escape)
                Close();
            else if (e.Key == Key.Space) {
                Save();
                Next();
                Shows();
            } else if (e.Key == Key.Right) {
                Next();
                Shows();
            } else if (e.Key == Key.Left || e.Key == Key.Back) {
                Prev();
                Shows();
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
            running = false;//閉じてもTaskが動き続ける
        }
    }
}
