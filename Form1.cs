using System.Diagnostics;

namespace spisok_pril
{
    public partial class Form1 : Form
    {
        private ImageList _imageList;

        private List<(string Name, string Path, string Type, int ImageIndex)> _allApps = new();

        public Form1()
        {
            InitializeComponent();
        }

        private void RunSelectedApp()
        {
            string path = listView1.SelectedItems[0].SubItems[1].Text;
            try
            {
                var startInfo = new ProcessStartInfo(path)
                {
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    ErrorDialog = false
                };
                Process.Start(startInfo);
            }
            catch (Exception ex)
            {
                // Лучше логировать в файл, а не в консоль
                Debug.WriteLine($"Не удалось запустить {path}: {ex.Message}");
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            InitImageList();
            LoadAppsFromFolder(Environment.GetFolderPath(Environment.SpecialFolder.Desktop));
            listView1.View = View.Details;
            listView1.FullRowSelect = true;      // выделять всю строку
            listView1.HideSelection = false;     // строка остаётся выделенной, даже если фокус ушёл
            listView1.MultiSelect = false;       // только одна строка может быть выбрана (для Enter это логичнее)
            txtSearch.TabIndex = 0;
            listView1.TabIndex = 1;
            listView1.Columns.Add("Имя файла", 200);
            listView1.Columns.Add("Путь", 300);
            listView1.Columns.Add("Размер", 100);
        }

        private void InitImageList()
        {
            _imageList = new ImageList
            {
                ImageSize = new Size(32, 32),
                ColorDepth = ColorDepth.Depth32Bit
            };
            listView1.LargeImageList = _imageList;
            listView1.SmallImageList = _imageList;
        }

        private void LoadAppsFromFolder(string folderPath)
        {
            _allApps.Clear();
            if (!Directory.Exists(folderPath)) return;

            var exeFiles = Directory.GetFiles(folderPath, "*.exe", SearchOption.TopDirectoryOnly);
            var lnkFiles = Directory.GetFiles(folderPath, "*.lnk", SearchOption.TopDirectoryOnly);

            foreach (var file in exeFiles)
            {
                try
                {
                    var icon = Icon.ExtractAssociatedIcon(file);
                    if (icon != null)
                    {
                        using (var bmp = icon.ToBitmap())
                        {
                            int index = ((System.Collections.IList)_imageList.Images).Add(bmp); ;
                            var item = new ListViewItem(new[]
                            {
                                Path.GetFileNameWithoutExtension(file), // Колонка 1
                                file,                                   // Колонка 2
                                "EXE"                                   // Колонка 3
                            });
                            item.ImageIndex = index;
                            listView1.Items.Add(item);
                        }
                    }
                }
                catch { /* пропускаем проблемные файлы */ }
            }

            foreach (var lnkFile in lnkFiles)
            {
                string targetPath = GetLnkTargetPath(lnkFile);
                if (!string.IsNullOrEmpty(targetPath) && File.Exists(targetPath))
                {
                    try
                    {
                        var icon = Icon.ExtractAssociatedIcon(targetPath);
                        if (icon != null)
                        {
                            using (var bmp = icon.ToBitmap())
                            {
                                int index = ((System.Collections.IList)_imageList.Images).Add(bmp);
                                var item = new ListViewItem
                                (new[]
                                {
                                    Path.GetFileNameWithoutExtension(lnkFile), // Колонка 1
                                    targetPath,                                   // Колонка 2
                                    "LNK"                                   // Колонка 3
                                });
                                item.ImageIndex = index;
                                listView1.Items.Add(item);
                            }
                        }
                    }
                    catch { /* пропускаем */ }
                }
            }
        }

        private string GetLnkTargetPath(string lnkPath)
        {
            try
            {
                // Полное имя вместо relying на using
                var shell = new Shell32.Shell();
                var folder = shell.NameSpace(Path.GetDirectoryName(lnkPath));
                var item = folder.ParseName(Path.GetFileName(lnkPath));
                var link = (Shell32.ShellLinkObject)item.GetLink;
                return link.Path;
            }
            catch
            {
                return null;
            }
        }

        private void listView1_ItemActivate(object sender, EventArgs e)
        {
            RunSelectedApp();
        }

        private void txtSearch_TextChanged(object sender, EventArgs e)
        {
            PerformSearch(txtSearch.Text);
        }

        private void PerformSearch(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                // Если очистили поле — снимаем выделение
                listView1.SelectedItems.Clear();
                return;
            }

            // FindItemWithText ищет по Text (первая колонка)
            var item = listView1.FindItemWithText(query, false, startIndex: 0, true);


            if (item != null)
            {
                item.Selected = true;
                item.Focused = true;          // фокус на строке
                listView1.EnsureVisible(item.Index); // прокрутить, если строка вне экрана
            }
            else
            {
                listView1.SelectedItems.Clear(); // ничего не найдено
            }
        }

        private void listView1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                RunSelectedApp();
            }
            
        }
    }
}
