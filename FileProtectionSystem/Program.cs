using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.IO;
using System.Security.Principal;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;

namespace FileProtectionSystem
{
    public class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            if (!IsAdministrator())
            {
                MessageBox.Show("This application requires administrative privileges.", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string rootPath = Path.GetFullPath(@"C:\Users\alhad\Documents\copyblocker\test"); // Change this to your root path
            var protector = new FileProtector(rootPath);
            
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            
            // Create system tray icon
            using (var trayIcon = new NotifyIcon())
            {
                trayIcon.Icon = SystemIcons.Shield;
                trayIcon.Text = "File Protection System";
                trayIcon.Visible = true;
                
                protector.Start();
                Application.Run();
            }
        }

        private static bool IsAdministrator()
        {
            var identity = WindowsIdentity.GetCurrent();
            var principal = new WindowsPrincipal(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }
    }

    public class FileProtector : IDisposable
    {
        private readonly string rootPath;
        private readonly ClipboardMonitor clipboardMonitor;
        private readonly FileSystemWatcher fileWatcher;
        
        public FileProtector(string rootPath)
        {
            this.rootPath = rootPath;
            Directory.CreateDirectory(rootPath);
            
            clipboardMonitor = new ClipboardMonitor();
            clipboardMonitor.ClipboardChanged += OnClipboardChanged;
            
            fileWatcher = new FileSystemWatcher(rootPath)
            {
                IncludeSubdirectories = true,
                EnableRaisingEvents = false
            };
            
            fileWatcher.Created += OnFileSystemEvent;
            fileWatcher.Deleted += OnFileSystemEvent;
            fileWatcher.Changed += OnFileSystemEvent;
            fileWatcher.Renamed += OnFileSystemEvent;
        }

        public void Start()
        {
            clipboardMonitor.Start();
            fileWatcher.EnableRaisingEvents = true;
        }

        private void OnClipboardChanged(object sender, EventArgs e)
        {
            if (Clipboard.ContainsFileDropList())
            {
                var files = Clipboard.GetFileDropList().Cast<string>();
                
                // Check if any files are from protected folder
                if (files.Any(file => IsFileInRootPath(file)))
                {
                    // Clear clipboard if files are from protected folder
                    Clipboard.Clear();
                    ShowNotification("File protection active", 
                        "Copying files from the protected folder is not allowed.");
                }
            }
        }

        private void OnFileSystemEvent(object sender, FileSystemEventArgs e)
        {
            if (e.ChangeType == WatcherChangeTypes.Created)
            {
                // Check if the file was copied from outside
                if (!IsFileInRootPath(e.FullPath))
                {
                    try
                    {
                        File.Delete(e.FullPath);
                        ShowNotification("File protection active", 
                            "Files can only be created within the protected folder.");
                    }
                    catch (Exception ex)
                    {
                        ShowNotification("Error", 
                            $"Failed to enforce protection: {ex.Message}");
                    }
                }
            }
        }

        private bool IsFileInRootPath(string filePath)
        {
            return Path.GetFullPath(filePath)
                .StartsWith(Path.GetFullPath(rootPath), StringComparison.OrdinalIgnoreCase);
        }

        private void ShowNotification(string title, string message)
        {
            MessageBox.Show(message, title, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        public void Dispose()
        {
            clipboardMonitor?.Dispose();
            fileWatcher?.Dispose();
        }
    }

    public class ClipboardMonitor : IDisposable
    {
        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr SetClipboardViewer(IntPtr hWndNewViewer);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool ChangeClipboardChain(IntPtr hWndRemove, IntPtr hWndNewNext);

        private class ClipboardMonitorForm : Form
        {
            public event EventHandler ClipboardChanged;
            private IntPtr nextClipboardViewer;

            public ClipboardMonitorForm()
            {
                this.Visible = false;
                this.ShowInTaskbar = false;
                this.FormBorderStyle = FormBorderStyle.None;
                this.Size = new System.Drawing.Size(0, 0);
            }

            protected override void WndProc(ref Message m)
            {
                const int WM_DRAWCLIPBOARD = 0x0308;
                const int WM_CHANGECBCHAIN = 0x030D;

                switch (m.Msg)
                {
                    case WM_DRAWCLIPBOARD:
                        ClipboardChanged?.Invoke(this, EventArgs.Empty);
                        base.WndProc(ref m);
                        break;

                    case WM_CHANGECBCHAIN:
                        if (m.WParam == nextClipboardViewer)
                            nextClipboardViewer = m.LParam;
                        else
                            base.WndProc(ref m);
                        break;

                    default:
                        base.WndProc(ref m);
                        break;
                }
            }

            public void InitializeClipboardMonitor()
            {
                nextClipboardViewer = SetClipboardViewer(this.Handle);
            }

            protected override void OnHandleDestroyed(EventArgs e)
            {
                ChangeClipboardChain(this.Handle, nextClipboardViewer);
                base.OnHandleDestroyed(e);
            }
        }

        private ClipboardMonitorForm monitorForm;
        public event EventHandler ClipboardChanged;

        public void Start()
        {
            if (monitorForm == null)
            {
                monitorForm = new ClipboardMonitorForm();
                monitorForm.ClipboardChanged += (s, e) => ClipboardChanged?.Invoke(this, EventArgs.Empty);
                monitorForm.InitializeClipboardMonitor();
            }
        }

        public void Dispose()
        {
            monitorForm?.Dispose();
        }
    }
}