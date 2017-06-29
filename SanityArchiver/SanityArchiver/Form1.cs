using SeekFiles;
using System;
using System.IO;
using System.Windows.Forms;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
   
namespace SanityArchiver
{
    public partial class Form1 : Form
    {
        SeekFile seekfile = new SeekFile();

        public Form1()
        {
            InitializeComponent();
            PopulateTreeView();
            /*Timer timer = new Timer();
            timer.Interval = 5000; //refresh in 5sec
            timer.Tick += new EventHandler(timer_Tick);
            timer.Start();*/
        }

        private void PopulateTreeView()
        {
            TreeNode rootNode;
            DirectoryInfo info = new DirectoryInfo(@"C:/");

            if (info.Exists)
            {
                rootNode = new TreeNode(info.Name);
                rootNode.Tag = info;
                //GetDirectories(info.GetDirectories(), rootNode);
                treeView1.Nodes.Add(rootNode);
            }
        }

        private void GetDirectories(DirectoryInfo[] subDirs, TreeNode nodeToAddTo)
        {
            TreeNode aNode;
            DirectoryInfo[] subSubDirs;
            foreach (DirectoryInfo subDir in subDirs)
            {
                aNode = new TreeNode(subDir.Name, 0, 0);
                aNode.Tag = subDir;
                aNode.ImageKey = "Folder";
                subSubDirs = subDir.GetDirectories();
                if (subSubDirs.Length != 0)
                {
                    GetDirectories(subSubDirs, aNode);
                }
                nodeToAddTo.Nodes.Add(aNode);
            }
        }

        private void treeView1_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            TreeNode newSelected = e.Node;
            listView1.Items.Clear();
            DirectoryInfo nodeDirInfo = (DirectoryInfo)newSelected.Tag;
            ListViewItem.ListViewSubItem[] subItems;
            ListViewItem item = null;

            foreach (DirectoryInfo dir in nodeDirInfo.GetDirectories())
            {
                item = new ListViewItem(dir.Name, 0);
                subItems = new ListViewItem.ListViewSubItem[]
                          {new ListViewItem.ListViewSubItem(item, "File folder"),
                           new ListViewItem.ListViewSubItem(item, ""),
                           new ListViewItem.ListViewSubItem(item, dir.LastAccessTime.ToString("yyyy.MM.dd. hh:mm"))};
                item.SubItems.AddRange(subItems);
                listView1.Items.Add(item);
            }

            foreach (FileInfo file in nodeDirInfo.GetFiles())
            {
                item = new ListViewItem(file.Name, 1);
                string fileSize = file.Length.ToString();
                if (file.Length >= (1 << 10))
                    fileSize = string.Format("{0} KB", file.Length >> 10);
                else if (file.Length >= (1 << 20))
                    fileSize = string.Format("{0} MB", file.Length >> 20);
                else if (file.Length >= (1 << 30))
                    fileSize = string.Format("{0} GB", file.Length >> 30);
                subItems = new ListViewItem.ListViewSubItem[]
                          {new ListViewItem.ListViewSubItem(item, "File"),
                           new ListViewItem.ListViewSubItem(item, fileSize),
                           new ListViewItem.ListViewSubItem(item, file.LastAccessTime.ToString("yyyy.MM.dd. hh:mm"))};
                item.SubItems.AddRange(subItems);
                listView1.Items.Add(item);
            }
            listView1.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
        }

        private void listView1_DoubleClick(object sender, EventArgs e)
        {
            string fileName = listView1.SelectedItems[0].Text;
            DirectoryInfo directoryProperties = seekfile.RecursiveDirectorySearch(fileName, @"C:/");
            listView1.Items.Clear();
            ListViewItem.ListViewSubItem[] subItems;
            ListViewItem item = null;
            try
            {
                foreach (DirectoryInfo currentDirectoryInfo in directoryProperties.GetDirectories())
                {
                    try { 
                    item = new ListViewItem(currentDirectoryInfo.Name, 0);
                    item.Text = currentDirectoryInfo.Name;
                    item.Tag = currentDirectoryInfo.FullName;
                    subItems = new ListViewItem.ListViewSubItem[]
                              {new ListViewItem.ListViewSubItem(item, "File folder"),
                               new ListViewItem.ListViewSubItem(item, ""),
                               new ListViewItem.ListViewSubItem(item, currentDirectoryInfo.LastAccessTime.ToString("yyyy.MM.dd. hh:mm"))};
                        item.SubItems.AddRange(subItems);
                        listView1.Items.Add(item);
                    } catch (UnauthorizedAccessException exc) {
                        continue;
                    }
                }

                foreach (FileInfo currentFileInfo in directoryProperties.GetFiles()) {
                    if (currentFileInfo.Name.Contains(".zip")) {
                        item = new ListViewItem(currentFileInfo.Name, 2);
                    } else {
                        item = new ListViewItem(currentFileInfo.Name, 1);
                    }

                    item.Text = currentFileInfo.Name;
                    item.Tag = currentFileInfo.FullName;
                    string fileSize = currentFileInfo.Length.ToString();
                    if (currentFileInfo.Length >= (1 << 10))
                        fileSize = string.Format("{0} KB", currentFileInfo.Length >> 10);
                    else if (currentFileInfo.Length >= (1 << 20))
                        fileSize = string.Format("{0} MB", currentFileInfo.Length >> 20);
                    else if (currentFileInfo.Length >= (1 << 30))
                        fileSize = string.Format("{0} GB", currentFileInfo.Length >> 30);
                    subItems = new ListViewItem.ListViewSubItem[]
                              {new ListViewItem.ListViewSubItem(item, "File"),
                               new ListViewItem.ListViewSubItem(item, fileSize),
                               new ListViewItem.ListViewSubItem(item, currentFileInfo.LastAccessTime.ToString("yyyy.MM.dd. hh:mm"))};
                    item.SubItems.AddRange(subItems);
                    listView1.Items.Add(item);
                }
            }
            catch (UnauthorizedAccessException exc) { }
            catch (NullReferenceException exc) { }
        }

        private void compressButton_Click(object sender, EventArgs e)
        {
            string fileName = listView1.SelectedItems[0].Tag.ToString();
            try
            {
                string zipPath = fileName + ".zip";
                File.SetAttributes(fileName, FileAttributes.Normal);
                ZipFile.CreateFromDirectory(fileName, zipPath);
            }
            catch (NullReferenceException exc) { }
            catch (UnauthorizedAccessException exc) { }
            catch (IOException exc) { }
            listView1_DoubleClick_compress(listView1.SelectedItems[0].Tag.ToString());
        }

        private void listView1_DoubleClick_compress(string fileName) {
            DirectoryInfo directoryProperties = seekfile.RecursiveDirectorySearch(fileName, @"C:/");
            ListViewItem.ListViewSubItem[] subItems;
            ListViewItem item = null;
            FileInfo currentFileInfo = new FileInfo(fileName + ".zip");
            item = new ListViewItem(currentFileInfo.Name, 2);
            item.Text = currentFileInfo.Name;
            item.Tag = currentFileInfo.FullName;
            string fileSize = currentFileInfo.Length.ToString();
            if (currentFileInfo.Length >= (1 << 10))
                fileSize = string.Format("{0} KB", currentFileInfo.Length >> 10);
            else if (currentFileInfo.Length >= (1 << 20))
                fileSize = string.Format("{0} MB", currentFileInfo.Length >> 20);
            else if (currentFileInfo.Length >= (1 << 30))
                fileSize = string.Format("{0} GB", currentFileInfo.Length >> 30);
            subItems = new ListViewItem.ListViewSubItem[]
                      {new ListViewItem.ListViewSubItem(item, "File"),
                       new ListViewItem.ListViewSubItem(item, fileSize), new ListViewItem.ListViewSubItem(item,
            currentFileInfo.LastAccessTime.ToString("yyyy.MM.dd. hh:mm"))};
            item.SubItems.AddRange(subItems);
            listView1.Items.Add(item);
        }

        private void decompressButton_Click(object sender, EventArgs e)
        {
            string fileName = listView1.SelectedItems[0].Tag.ToString();
            try
            {
                string zipPath = fileName;
                string extractPath = fileName.Remove(fileName.Length - 4) + "(1)";
                Console.WriteLine(extractPath);
                File.SetAttributes(zipPath, FileAttributes.Normal);
                ZipFile.ExtractToDirectory(zipPath, extractPath);
            }
            catch (NullReferenceException exc) { }
            catch (UnauthorizedAccessException exc) { }
            listView1_DoubleClick_deCompress(listView1.SelectedItems[0].Tag.ToString());
        }

        private void listView1_DoubleClick_deCompress(string fileName)
        {
            DirectoryInfo directoryProperties = seekfile.RecursiveDirectorySearch(fileName, @"C:/");
            ListViewItem.ListViewSubItem[] subItems;
            ListViewItem item = null;
            FileInfo currentFileInfo = new FileInfo(fileName.Remove(fileName.Length - 4) + "(1)");
            string fileSize = "";
            if (fileName.Contains(".txt"))
            {
                item = new ListViewItem(currentFileInfo.Name, 1);
            }
            else
            {
                item = new ListViewItem(currentFileInfo.Name, 0);
            }
            item.Text = currentFileInfo.Name;
            item.Tag = currentFileInfo.FullName;
            subItems = new ListViewItem.ListViewSubItem[]
                      {new ListViewItem.ListViewSubItem(item, "File"),
                       new ListViewItem.ListViewSubItem(item, fileSize),
                       new ListViewItem.ListViewSubItem(item, currentFileInfo.LastAccessTime.ToString("yyyy.MM.dd. hh:mm"))};
            item.SubItems.AddRange(subItems);
            listView1.Items.Add(item);
        }

        private void cryptButton_Click(object sender, EventArgs e)
        {
            string fileName = listView1.SelectedItems[0].Tag.ToString();
            Console.WriteLine(fileName);
            try
            {
                File.Encrypt(fileName);
            }
            catch (Exception exc) { }
        }

        private void decryptButton_Click_1(object sender, EventArgs e)
        {
            string fileName = listView1.SelectedItems[0].Tag.ToString();
            Console.WriteLine(fileName);
            try
            {
                File.Decrypt(fileName);
            }
            catch (Exception exc) { }
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            Refresh();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string fileName = searchBox.Text;
            DirectoryInfo directoryProperties = seekfile.RecursiveDirectorySearch(fileName, @"C:/");
            pathBox.Text = directoryProperties.FullName;
        }
    }
}