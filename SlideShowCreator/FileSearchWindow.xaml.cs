using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
    /// FileSearchWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class FileSearchWindow : Window {
        FileSearchWindowViewModel vm;
        MainWindowViewModel mvm;
        public FileSearchWindow(MainWindowViewModel mainvm) {
            InitializeComponent();

            mvm = mainvm;
            vm = new FileSearchWindowViewModel();
            this.DataContext = vm;
        }
        #region ContextMenu
        private void addSelected_Click(object sender, RoutedEventArgs e) {
            if (dataGrid1.SelectedItems.Count <= 0) return;
            foreach (SearchResult r in dataGrid1.SelectedItems) mvm.SlideInfos.Add(new SlideInfo() { Path = r.Path });
        }

        private void addChecked_Click(object sender, RoutedEventArgs e) {
            foreach(var r in vm.Results.Where(res=>res.IsChecked)) mvm.SlideInfos.Add(new SlideInfo() { Path = r.Path });
        }
        #endregion

        #region DragAndDrop
        private void TextBox_DragEnter(object sender, DragEventArgs e) {
            if (e.Data.GetDataPresent(DataFormats.FileDrop)) e.Effects = DragDropEffects.Copy;
        }

        private void TextBox_Drop(object sender, DragEventArgs e) {
            var ds = (string[])e.Data.GetData(DataFormats.FileDrop);
            vm.DirPath = ds.Where(d => Directory.Exists(d)).FirstOrDefault();
        }

        private void DataGrid_DragEnter(object sender, DragEventArgs e) {
            if (e.Data.GetDataPresent(DataFormats.FileDrop)) e.Effects = DragDropEffects.Copy;
        }

        private void DataGrid_Drop(object sender, DragEventArgs e) {
            var ds = (string[])e.Data.GetData(DataFormats.FileDrop);
            foreach (var d in ds) {
                SearchDirectory(d, vm.Condition);
            }
        }
        #endregion

        #region Button
        private void allCheck_Click(object sender, RoutedEventArgs e) {
            if (vm.Results.Count > 0 && allCheck.IsChecked != null)
                foreach (var i in vm.Results) i.IsChecked = allCheck.IsChecked.Value;
        }

        private void searchBtn_Click(object sender, RoutedEventArgs e) {
            if (start) {
                ChangeStart();
            } else {
                if (!Directory.Exists(vm.DirPath)) return;
                SearchDirectory(vm.DirPath, vm.Condition);
            }
        }

        private void clearBtn_Click(object sender, RoutedEventArgs e) {
            vm.Results.Clear();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
            vm.Save();
        }
        #endregion

        bool start = false;
        private void SearchDirectory(string dirPath,string cond, int? cnt = null) {
            if (!Directory.Exists(dirPath)) return;
            if (cnt == null) ChangeStart();
            if (!start) return;
            //ファイル
            foreach (var f in Directory.GetFiles(dirPath)) {
                if (!start) return;
                if (string.IsNullOrEmpty(cond.Trim()) || Regex.IsMatch(f, cond.Trim()))
                    if (!vm.Results.Select(r => r.Path).Contains(f)) vm.Results.Add(new SearchResult() { Path = f });
            }

            //フォルダ
            foreach (var d in Directory.GetDirectories(dirPath)) {
                if (!start) return;
                SearchDirectory(d, cond, cnt == null ? 1 : cnt.Value + 1);
            }

            if (!start) return;
            if (cnt == null) ChangeStart();
        }

        private void ChangeStart() {
            start = !start;
            searchBtn.Content = start ? "Stop" : "Search";
        }

        private void dataGrid1_MouseDoubleClick(object sender, MouseButtonEventArgs e) {
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
            if (rowIndex < 0) return;
            new ShowWindow(vm.Results[rowIndex].Path, mvm).ShowDialog();
        }
    }
}
