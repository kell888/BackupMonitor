using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.Net;
using System.IO;

namespace 备份监控
{
    public partial class Form1 : Form
    {
        private bool sure;
        bool loadOver;
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            string dir1 = KellFileTransfer.Common.GetAppSettingConfig("dir1");
            string filter1 = KellFileTransfer.Common.GetAppSettingConfig("filter1");
            string includeSubDir1 = KellFileTransfer.Common.GetAppSettingConfig("includeSubDir1");
            string dir2 = KellFileTransfer.Common.GetAppSettingConfig("dir2");
            string filter2 = KellFileTransfer.Common.GetAppSettingConfig("filter2");
            string includeSubDir2 = KellFileTransfer.Common.GetAppSettingConfig("includeSubDir2");

            if (!string.IsNullOrEmpty(dir1))
                textBox1.Text = dir1;
            if (!string.IsNullOrEmpty(filter1))
                textBox4.Text = filter1;
            bool isd1 = false;
            if (!string.IsNullOrEmpty(includeSubDir1))
                isd1 = includeSubDir1 == "1";
            checkBox1.Checked = isd1;

            if (!string.IsNullOrEmpty(dir2))
                textBox2.Text = dir2;
            if (!string.IsNullOrEmpty(filter2))
                textBox3.Text = filter2;
            bool isd2 = false;
            if (!string.IsNullOrEmpty(includeSubDir2))
                isd2 = includeSubDir2 == "1";
            checkBox2.Checked = isd2;

            //string path = KellFileTransfer.Common.ReadRegistry("BackupFile");
            //if (path != "" && path.Equals(Application.ExecutablePath, StringComparison.InvariantCultureIgnoreCase))
            //    checkBox5.Checked = true;
            string filename = Path.GetFileName(Application.ExecutablePath);
            if (IsAutoStartupAllUsers(filename) || IsAutoStartupCurrentUser(filename))
                checkBox5.Checked = true;

            string monitor = KellFileTransfer.Common.GetAppSettingConfig("monitor");
            int M = 3;
            int R;
            if (!string.IsNullOrEmpty(monitor) && int.TryParse(monitor, out R))
                M = R;
            SetMonitor(M);
            textBox4.Text = fileSystemWatcher1.Filter;
            loadOver = true;
        }

        private void SetMonitorPath(string dir1, string dir2)
        {
            if (!string.IsNullOrEmpty(dir1))
            {
                if (Directory.Exists(dir1))
                {
                    fileSystemWatcher1.Path = dir1;
                }
                else
                {
                    try
                    {
                        Directory.CreateDirectory(dir1);
                        fileSystemWatcher1.Path = dir1;
                    }
                    catch (Exception e)
                    {
                        CreateLog("SetMonitorPath:fileSystemWatcher1.Path", "error", e.ToString());
                    }
                }
            }
            if (!string.IsNullOrEmpty(dir2))
            {
                if (Directory.Exists(dir2))
                {
                    fileSystemWatcher2.Path = dir2;
                }
                else
                {
                    try
                    {
                        Directory.CreateDirectory(dir2);
                        fileSystemWatcher2.Path = dir2;
                    }
                    catch (Exception e)
                    {
                        CreateLog("SetMonitorPath:fileSystemWatcher2.Path", "error", e.ToString());
                    }
                }
            }
        }

        private void SaveSettings()
        {
            KellFileTransfer.Common.SaveAppSettingConfig("dir1", textBox1.Text.Trim());
            KellFileTransfer.Common.SaveAppSettingConfig("filter1", textBox4.Text.Trim());
            KellFileTransfer.Common.SaveAppSettingConfig("includeSubDir1", checkBox1.Checked ? "1" : "0");
            KellFileTransfer.Common.SaveAppSettingConfig("dir2", textBox2.Text.Trim());
            KellFileTransfer.Common.SaveAppSettingConfig("filter2", textBox3.Text.Trim());
            KellFileTransfer.Common.SaveAppSettingConfig("includeSubDir2", checkBox2.Checked ? "1" : "0");
            KellFileTransfer.Common.SaveAppSettingConfig("monitor", GetMonitor().ToString());
        }

        private void button3_Click(object sender, EventArgs e)
        {
            StartMonitor();
        }

        private void StartMonitor()
        {
            button3.Enabled = false;
            button4.Enabled = true;
            int M = 0;
            try
            {
                string dir1 = textBox1.Text.Trim();
                string dir2 = textBox2.Text.Trim();
                fileSystemWatcher2.Filter = textBox3.Text.Trim();
                SetMonitorPath(dir1, dir2);
                M = GetMonitor();
                if (M == 0)
                {
                    if (fileSystemWatcher1.EnableRaisingEvents)
                        fileSystemWatcher1.EnableRaisingEvents = false;
                    if (fileSystemWatcher2.EnableRaisingEvents)
                        fileSystemWatcher2.EnableRaisingEvents = false;
                }
                else
                {
                    if (M == 1)
                    {
                        if (!fileSystemWatcher1.EnableRaisingEvents)
                            fileSystemWatcher1.EnableRaisingEvents = true;
                        if (fileSystemWatcher2.EnableRaisingEvents)
                            fileSystemWatcher2.EnableRaisingEvents = false;
                    }
                    else if (M == 2)
                    {
                        if (fileSystemWatcher1.EnableRaisingEvents)
                            fileSystemWatcher1.EnableRaisingEvents = false;
                        if (!fileSystemWatcher2.EnableRaisingEvents)
                            fileSystemWatcher2.EnableRaisingEvents = true;
                    }
                    else if (M == 3)
                    {
                        if (!fileSystemWatcher1.EnableRaisingEvents)
                            fileSystemWatcher1.EnableRaisingEvents = true;
                        if (!fileSystemWatcher2.EnableRaisingEvents)
                            fileSystemWatcher2.EnableRaisingEvents = true;
                    }
                    StringBuilder arg = new StringBuilder();
                    arg.Append(Environment.NewLine + "fileSystemWatcher1.Path=" + fileSystemWatcher1.Path);
                    arg.Append(Environment.NewLine + "fileSystemWatcher1.EnableRaisingEvents=" + fileSystemWatcher1.EnableRaisingEvents);
                    arg.Append(Environment.NewLine + "fileSystemWatcher1.IncludeSubdirectories=" + fileSystemWatcher1.IncludeSubdirectories);
                    arg.Append(Environment.NewLine + "fileSystemWatcher2.Path=" + fileSystemWatcher2.Path);
                    arg.Append(Environment.NewLine + "fileSystemWatcher2.EnableRaisingEvents=" + fileSystemWatcher2.EnableRaisingEvents);
                    arg.Append(Environment.NewLine + "fileSystemWatcher2.IncludeSubdirectories=" + fileSystemWatcher2.IncludeSubdirectories);
                    CreateLog("StartMonitor", "log", "开始监控成功！" + Environment.NewLine + arg.ToString());
                }
            }
            catch (Exception e)
            {
                string monitorPath = "";
                if (M == 1)
                {
                    monitorPath = Environment.NewLine + "fileSystemWatcher1.Path=[" + fileSystemWatcher1.Path+"]";
                }
                else if (M == 2)
                {
                    monitorPath = Environment.NewLine + "fileSystemWatcher2.Path=[" + fileSystemWatcher2.Path+"]";
                }
                else if (M == 3)
                {
                    monitorPath = Environment.NewLine + "fileSystemWatcher1.Path=[" + fileSystemWatcher1.Path+"]";
                    monitorPath += Environment.NewLine + "fileSystemWatcher2.Path=[" + fileSystemWatcher2.Path+"]";
                }
                CreateLog("StartMonitor", "error", "开始监控失败：" + e.ToString() + monitorPath);
                button3.Enabled = true;
                button4.Enabled = false;
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            StopMonitor();
        }

        private void StopMonitor()
        {
            button3.Enabled = true;
            button4.Enabled = false;
            try
            {
                if (fileSystemWatcher1.EnableRaisingEvents || fileSystemWatcher2.EnableRaisingEvents)
                {
                    if (fileSystemWatcher1.EnableRaisingEvents)
                        fileSystemWatcher1.EnableRaisingEvents = false;
                    if (fileSystemWatcher2.EnableRaisingEvents)
                        fileSystemWatcher2.EnableRaisingEvents = false;
                    CreateLog("StartMonitor", "log", "停止监控成功！");
                }
                else
                {
                    button3.Enabled = true;
                    button4.Enabled = false;
                }
            }
            catch (Exception e)
            {
                CreateLog("StopMonitor", "error", "停止监控失败：" + e.ToString());
                button3.Enabled = false;
                button4.Enabled = true;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (folderBrowserDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                textBox1.Text = folderBrowserDialog1.SelectedPath;
                fileSystemWatcher1.Path = folderBrowserDialog1.SelectedPath;
            }
            folderBrowserDialog1.Dispose();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (folderBrowserDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                textBox2.Text = folderBrowserDialog1.SelectedPath;
                fileSystemWatcher2.Path = folderBrowserDialog1.SelectedPath;
            }
            folderBrowserDialog1.Dispose();
        }

        private void fileSystemWatcher1_Created(object sender, System.IO.FileSystemEventArgs e)
        {
            CreateLog("fileSystemWatcher1_Created", "log", "新数据库备份[" + e.FullPath + "]");
            try
            {
                int waitMinutes = 1;//默认等待1分钟后再传送文件，避免数据库备份还没备份完毕
                string wm = KellFileTransfer.Common.GetAppSettingConfig("waitMinutes");
                int R;
                if (!string.IsNullOrEmpty(wm) && int.TryParse(wm, out R))
                    waitMinutes = R;
                int waitMillSeconds = 1000 * 60 * waitMinutes;
                Thread.Sleep(waitMillSeconds);
                int flag = SendFile(e.FullPath, 1);
                if (flag > -1)
                {
                    if (flag == 0)
                    {
                        CreateLog("SendFile", "log", "传送文件[" + e.FullPath + "]失败！");
                    }
                    else if (flag == 1)
                    {
                        CreateLog("SendFile", "log", "传送文件[" + e.FullPath + "]成功！");
                    }
                }
            }
            catch (Exception ex)
            {
                CreateLog("fileSystemWatcher1_Created", "error", "数据库备份时出错：" + ex.ToString());
            }
        }

        private static int SendFile(string filepath, int sendId)
        {
            if (Directory.Exists(filepath))//如果是个目录就不传送，直接返回
                return -1;
            try
            {
                string ip = KellFileTransfer.Common.GetAppSettingConfig("ip");
                string port = KellFileTransfer.Common.GetAppSettingConfig("port");
                string SendId = KellFileTransfer.Common.GetAppSettingConfig("SendId" + sendId);
                IPAddress IP = IPAddress.Loopback;
                IPAddress Ip;
                if (IPAddress.TryParse(ip, out Ip))
                    IP = Ip;
                int PORT = 8000;
                int R;
                if (int.TryParse(port, out R))
                    PORT = R;
                IPEndPoint hostEP = new IPEndPoint(IP, PORT);
                bool flag = KellFileTransfer.FileUploader.SendFile(filepath, hostEP, SendId);
                return flag ? 1 : 0;
            }
            catch (Exception ex)
            {
                CreateLog("SendFile", "error", "发送文件时出现异常：" + ex.Message);
                return 0;
            }
        }

        private void fileSystemWatcher2_Changed(object sender, System.IO.FileSystemEventArgs e)
        {
            CreateLog("fileSystemWatcher2_Changed", "log", "附件有修改[" + e.FullPath + "]");
            try
            {
                int flag = SendFile(e.FullPath, 2);
                if (flag > -1)
                {
                    if (flag == 0)
                    {
                        CreateLog("SendFile", "log", "传送文件[" + e.FullPath + "]失败！");
                    }
                    else if (flag == 1)
                    {
                        CreateLog("SendFile", "log", "传送文件[" + e.FullPath + "]成功！");
                    }
                }
            }
            catch (Exception ex)
            {
                CreateLog("fileSystemWatcher2_Changed", "error", "附件异地备份时出错：" + ex.ToString());
            }
        }

        private void fileSystemWatcher2_Created(object sender, System.IO.FileSystemEventArgs e)
        {
            CreateLog("fileSystemWatcher2_Created", "log", "新附件[" + e.FullPath + "]");
            try
            {
                int waitMinutes = 1;//默认等待1分钟后再传送文件，避免文件还没创建完毕
                string wm = KellFileTransfer.Common.GetAppSettingConfig("waitMinutes");
                int R;
                if (!string.IsNullOrEmpty(wm) && int.TryParse(wm, out R))
                    waitMinutes = R;
                int waitMillSeconds = 1000 * 60 * waitMinutes;
                Thread.Sleep(waitMillSeconds);
                int flag = SendFile(e.FullPath, 2);
                if (flag > -1)
                {
                    if (flag == 0)
                    {
                        CreateLog("SendFile", "log", "传送文件[" + e.FullPath + "]失败！");
                    }
                    else if (flag == 1)
                    {
                        CreateLog("SendFile", "log", "传送文件[" + e.FullPath + "]成功！");
                    }
                }
            }
            catch (Exception ex)
            {
                CreateLog("fileSystemWatcher2_Created", "error", "附件异地备份时出错：" + ex.ToString());
            }
        }

        private void fileSystemWatcher2_Renamed(object sender, System.IO.RenamedEventArgs e)
        {
            CreateLog("fileSystemWatcher2_Renamed", "log", "附件重命名[" + e.OldName + " -> " + e.Name + "]");
            try
            {
                int flag = SendFile(e.FullPath, 2);
                if (flag > -1)
                {
                    if (flag == 0)
                    {
                        CreateLog("SendFile", "log", "传送文件[" + e.FullPath + "]失败！");
                    }
                    else if (flag == 1)
                    {
                        CreateLog("SendFile", "log", "传送文件[" + e.FullPath + "]成功！");
                    }
                }
            }
            catch (Exception ex)
            {
                CreateLog("fileSystemWatcher2_Created", "error", "附件异地备份时出错：" + ex.ToString());
            }
        }

        private void 显示界面ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ShowUI();
        }

        private void 退出服务ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Exit();
        }

        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            ShowUI();
        }

        private void ShowUI()
        {
            this.WindowState = FormWindowState.Normal;
            this.Show();
            this.BringToFront();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (sure)
            {
                if (MessageBox.Show("确定要退出服务吗？", "退出提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == System.Windows.Forms.DialogResult.OK)
                {
                    StopMonitor();
                    SaveSettings();
                    notifyIcon1.Dispose();
                    Environment.Exit(0);
                }
                else
                {
                    e.Cancel = true;
                    sure = false;
                }
            }
            else
            {
                e.Cancel = true;
                this.Hide();
            }
        }

        private void Exit()
        {
            sure = true;
            this.Close();
        }

        private void Form1_Shown(object sender, EventArgs e)
        {
            this.Hide();
            StartMonitor();
        }

        private void checkBox5_CheckedChanged(object sender, EventArgs e)
        {
            if (loadOver)
            {
                AutoStartup(Application.ExecutablePath, checkBox5.Checked);
                //if (checkBox5.Checked)
                //    KellFileTransfer.Common.SetSelfStarting(Application.ExecutablePath, "BackupFile");
                //else
                //    KellFileTransfer.Common.CancelSelfStarting("BackupFile");
            }
        }

        private int GetMonitor()
        {
            int monitor = 3;

            if (checkBox8.Checked && checkBox11.Checked)
                monitor = 3;
            else if (checkBox8.Checked && !checkBox11.Checked)
                monitor = 1;
            else if (!checkBox8.Checked && checkBox11.Checked)
                monitor = 2;
            else
                monitor = 0;

            return monitor;
        }

        private void SetMonitor(int monitor)
        {
            if (monitor == 3)
            {
                checkBox8.Checked = checkBox11.Checked = true;
            }
            else if (monitor == 1)
            {
                checkBox8.Checked = true;
                checkBox11.Checked = false;
            }
            else if (monitor == 2)
            {
                checkBox8.Checked = false;
                checkBox11.Checked = true;
            }
            else
            {
                checkBox8.Checked = checkBox11.Checked = false;
            }
        }

        private void checkBox8_CheckedChanged(object sender, EventArgs e)
        {
            if (loadOver)
            {
                StartMonitor();
            }
        }

        private void checkBox11_CheckedChanged(object sender, EventArgs e)
        {
            if (loadOver)
            {
                StartMonitor();
            }
        }

        private void checkBox10_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox10.Checked)
                fileSystemWatcher2.Created += fileSystemWatcher2_Created;
            else
                fileSystemWatcher2.Created -= fileSystemWatcher2_Created;
        }

        private void checkBox9_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox9.Checked)
                fileSystemWatcher2.Changed += fileSystemWatcher2_Changed;
            else
                fileSystemWatcher2.Changed -= fileSystemWatcher2_Changed;
        }

        private void checkBox7_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox7.Checked)
                fileSystemWatcher2.Renamed += fileSystemWatcher2_Renamed;
            else
                fileSystemWatcher2.Renamed -= fileSystemWatcher2_Renamed;
        }

        private void textBox3_Leave(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(textBox3.Text.Trim()))
            {
                fileSystemWatcher2.Filter = textBox3.Text.Trim();
            }
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            fileSystemWatcher1.IncludeSubdirectories = checkBox1.Checked;
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            fileSystemWatcher2.IncludeSubdirectories = checkBox2.Checked;
        }

        /// <summary>
        /// 生成TXT文件,记录事件处理结果和错误信息
        /// </summary>
        /// <param name="filePath">错误出处的类和方法</param>
        /// <param name="addr">错误类型</param>
        /// <param name="content">错误内容</param>
        public static void CreateLog(string errorSource, string errorType, string errorContent)
        {
            DateTime dt = DateTime.Now;
            string time = dt.ToString("yyyy-MM-dd_HH_mm_ss");
            Random rand = new Random();
            string num = rand.Next(10000, 99999).ToString();
            string filename = time + "_" + num + ".txt"; //文件命名，随机数加当前时间

            string path = "LogReporter";
            if (errorType.Equals("error", StringComparison.InvariantCultureIgnoreCase))
                path = AppDomain.CurrentDomain.BaseDirectory + "LogReporter\\Error";
            else if (errorType.Equals("log", StringComparison.InvariantCultureIgnoreCase))
                path = AppDomain.CurrentDomain.BaseDirectory + "LogReporter\\Log";
            else
                path = AppDomain.CurrentDomain.BaseDirectory + "LogReporter\\" + errorType;
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            string filepath = path + "\\" + filename;
            StreamWriter sWrite = new StreamWriter(filepath, false, Encoding.UTF8);
            sWrite.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "[" + errorSource + "]>>" + errorContent);
            sWrite.Close();
        }

        private void AutoStartup(string executablePath, bool flag)
        {
            string filename = Path.GetFileName(executablePath);
            //string path = Environment.GetFolderPath(Environment.SpecialFolder.Startup);
            string path = Environment.GetFolderPath(Environment.SpecialFolder.System).Substring(0, 1) + @":\ProgramData\Microsoft\Windows\Start Menu\Programs\Startup";
            if (!Directory.Exists(path))
                path = Environment.GetFolderPath(Environment.SpecialFolder.Startup);
            string file = path + "\\" + filename + ".lnk";
            if (flag)
            {
                if (!IsAutoStartupAllUsers(filename))
                {
                    try
                    {
                        CreateShortcut(executablePath, path);
                    }
                    catch (Exception e)
                    {
                        if (!IsAutoStartupCurrentUser(filename))
                            CreateShortcut(executablePath, Environment.GetFolderPath(Environment.SpecialFolder.Startup));
                    }
                }
                else if (!IsAutoStartupCurrentUser(filename))
                {
                    CreateShortcut(executablePath, Environment.GetFolderPath(Environment.SpecialFolder.Startup));
                }
            }
            else
            {
                if (IsAutoStartupAllUsers(filename))
                {
                    File.Delete(file);
                }
                if (IsAutoStartupCurrentUser(filename))
                {
                    string file2 = Environment.GetFolderPath(Environment.SpecialFolder.Startup) + "\\" + filename + ".lnk";
                    File.Delete(file2);
                }
            }
        }

        private static void CreateShortcut(string executablePath, string path)
        {
            // 声明操作对象
            IWshRuntimeLibrary.WshShell shell = new IWshRuntimeLibrary.WshShellClass();
            string filename = Path.GetFileName(executablePath);
            string file = path + "\\" + filename + ".lnk";
            // 创建一个快捷方式
            IWshRuntimeLibrary.IWshShortcut shortcut = (IWshRuntimeLibrary.IWshShortcut)shell.CreateShortcut(file);
            // 关联的程序
            shortcut.TargetPath = executablePath;
            // 参数
            //shortcut.Arguments = "";
            // 快捷方式描述，鼠标放到快捷方式上会显示出来哦
            shortcut.Description = filename + "应用程序";
            // 全局热键
            //shortcut.Hotkey = "CTRL+SHIFT+N";
            // 设置快捷方式的图标，这里是取程序图标，如果希望指定一个ico文件，那么请写路径。
            //shortcut.IconLocation = "notepad.exe, 0";
            // 保存，创建就成功了。
            shortcut.Save();
        }

        private bool IsAutoStartupAllUsers(string filename)
        {
            string path = Environment.GetFolderPath(Environment.SpecialFolder.System).Substring(0, 1) + @":\ProgramData\Microsoft\Windows\Start Menu\Programs\Startup";
            if (Directory.Exists(path))
            {
                string lnk = path + "\\" + filename + ".lnk";
                string[] files = Directory.GetFiles(path, "*.lnk", SearchOption.TopDirectoryOnly);
                foreach (string file in files)
                {
                    if (file.Equals(lnk, StringComparison.InvariantCultureIgnoreCase))
                        return true;
                }
            }
            return false;
        }

        private bool IsAutoStartupCurrentUser(string filename)
        {
            string path = Environment.GetFolderPath(Environment.SpecialFolder.Startup);
            string lnk = path + "\\" + filename + ".lnk";
            string[] files = Directory.GetFiles(path, "*.lnk", SearchOption.TopDirectoryOnly);
            foreach (string file in files)
            {
                if (file.Equals(lnk, StringComparison.InvariantCultureIgnoreCase))
                    return true;
            }
            return false;
        }
    }
}
