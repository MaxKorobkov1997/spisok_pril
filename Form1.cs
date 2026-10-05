using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace spisok_pril
{
    public partial class Form1 : Form
    {
        private ImageList _imageList;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            InitImageList();
            LoadAppsFromFolder(Environment.GetFolderPath(Environment.SpecialFolder.Desktop));
            listView1.View = View.Details;
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
                            })
                            {
                                
                                ImageIndex = index,
                                Tag = "EXE"
                            };
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
                                    "EXE"                                   // Колонка 3
                                })
                                {
                                    ImageIndex = index,                                   // Путь к цели
                                    Tag = "LNK"                                         // Тип
                                };

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
    }
}
