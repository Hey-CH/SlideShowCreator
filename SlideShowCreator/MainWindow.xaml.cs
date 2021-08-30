using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml.Serialization;

namespace SlideShowCreator {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        MainWindowViewModel vm;
        FileSearchWindow fsw;
        public MainWindow() {
            InitializeComponent();

            vm = new MainWindowViewModel();
            this.DataContext = vm;
        }

        #region DragAndDrop
        private void DataGrid_DragEnter(object sender, DragEventArgs e) {
            if (e.Data.GetDataPresent(DataFormats.FileDrop)) {
                e.Effects = DragDropEffects.Move;
            } else {
                e.Handled = true;
            }
        }

        private void DataGrid_Drop(object sender, DragEventArgs e) {
            var rowIndex = -1;
            var dropP = e.GetPosition(dataGrid1);
            var hitRes = VisualTreeHelper.HitTest(dataGrid1, dropP);
            if (hitRes != null) {
                var obj = hitRes.VisualHit;
                while (obj != null) {
                    if (obj is DataGridRow) {
                        rowIndex = ((DataGridRow)obj).GetIndex();
                        break;
                    }
                    obj = VisualTreeHelper.GetParent(obj);
                }
            }

            if (e.Data.GetDataPresent(DataFormats.FileDrop)) {
                var fs = (IEnumerable<string>)e.Data.GetData(DataFormats.FileDrop);
                if (rowIndex < 0)
                    vm.SlideInfos.AddRange(fs.Where(f => File.Exists(f)).Select(f => new SlideInfo() { Path = f }));
                else
                    vm.SlideInfos.InsertRange(rowIndex, fs.Where(f => File.Exists(f)).Select(f => new SlideInfo() { Path = f }));
            }
        }
        #endregion

        #region ContextMenu
        private void SlideSetting_Click(object sender, RoutedEventArgs e) {
            if (vm.SelectedIndex >= 0) {
                var sw = new ShowWindow(vm, ShowWindow.ShowMode.Single);
                sw.ShowDialog();
            }
        }
        private void StartShow_Click(object sender, RoutedEventArgs e) {
            if (vm.SlideInfos.Count > 0) {
                var sw = new ShowWindow(vm, ShowWindow.ShowMode.SlideShow);
                sw.ShowDialog();
            }
        }
        private void FileSearch_Click(object sender, RoutedEventArgs e) {
            fsw = new FileSearchWindow(vm);
            fsw.Show();
        }
        #endregion

        #region Button
        private void openBtn_Click(object sender, RoutedEventArgs e) {
            var ofd = new OpenFileDialog();
            ofd.InitialDirectory = AppDomain.CurrentDomain.BaseDirectory;
            if (ofd.ShowDialog() == true) {
                var xs = new XmlSerializer(typeof(MainWindowViewModel));
                using (var fs = new FileStream(ofd.FileName, FileMode.Open, FileAccess.Read)) {
                    vm = (MainWindowViewModel)xs.Deserialize(fs);
                    this.DataContext = vm;
                }
            }
        }

        private void saveBtn_Click(object sender, RoutedEventArgs e) {
            var sfd = new SaveFileDialog();
            sfd.InitialDirectory = AppDomain.CurrentDomain.BaseDirectory;
            sfd.Filter = "setting.xml|*.xml";
            if (sfd.ShowDialog() == true) {
                var xs = new XmlSerializer(typeof(MainWindowViewModel));
                using (var fs = new FileStream(sfd.FileName,FileMode.OpenOrCreate,FileAccess.Write)) {
                    xs.Serialize(fs, vm);
                }
            }
        }

        private void setWaitBtn_Click(object sender, RoutedEventArgs e) {
            foreach (var si in vm.SlideInfos) si.Wait = vm.Wait;
        }
        #endregion

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
            try {
                if (fsw != null) fsw.Close();
            } catch { }
        }
    }
}
