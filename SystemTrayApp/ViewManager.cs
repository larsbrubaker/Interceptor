using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Reflection;
using Interceptor;
using DvorakKeyboard;

namespace SystemTrayApp
{
    public class ViewManager
    {
        private Input input;
        private KeyRecorder keyRecorder;

        public ViewManager(IDeviceManager deviceManager)
        {
            System.Diagnostics.Debug.Assert(deviceManager != null);

            _deviceManager = deviceManager;

            _components = new System.ComponentModel.Container();
            _notifyIcon = new System.Windows.Forms.NotifyIcon(_components)
            {
                ContextMenuStrip = new ContextMenuStrip(),
                Icon = SystemTrayApp.Properties.Resources.NotReadyIcon,
                Text = "System Tray App: Device Not Present",
                Visible = true,
            };

            _notifyIcon.ContextMenuStrip.Opening += ContextMenuStrip_Opening;
            _notifyIcon.DoubleClick += notifyIcon_DoubleClick;
            _notifyIcon.MouseUp += notifyIcon_MouseUp;

            _hiddenWindow = new System.Windows.Window();
            _hiddenWindow.Hide();

            input = new Input();

            keyRecorder = new KeyRecorder(input);

            input.KeyboardFilterMode = KeyboardFilterMode.All;

            // Finally, load the driver
            input.Load();

            input.OnKeyPressed += Input_OnKeyPressed;
        }

        private void Input_OnKeyPressed(object sender, KeyPressedEventArgs e)
        {
            // if the key is going down and it is a key we want to map to Dvorák
            if (enableDvorakMapping
                && e.DeviceId == mapToDvorakKeyboardId)
            {
                e.Key = QwertyToDvorak.MapKey(e.Key);
            }

            if (enableKeyRecording)
            {
                // let the macro recorder at the event
                keyRecorder.ProcessKey(ref e);
            }
        }

        System.Windows.Media.ImageSource AppIcon
        {
            get
            {
                System.Drawing.Icon icon = (_deviceManager.Status == DeviceStatus.Running) ? Properties.Resources.ReadyIcon : Properties.Resources.NotReadyIcon;
                return System.Windows.Interop.Imaging.CreateBitmapSourceFromHIcon(
                    icon.Handle, 
                    System.Windows.Int32Rect.Empty, 
                    System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());
            }
        }

        // This allows code to be run on a GUI thread
        private System.Windows.Window _hiddenWindow;

        private System.ComponentModel.IContainer _components;
        // The Windows system tray class
        private NotifyIcon _notifyIcon;  
        IDeviceManager _deviceManager;

        private ToolStripMenuItem _startDeviceMenuItem;
        private ToolStripMenuItem _stopDeviceMenuItem;
        private ToolStripMenuItem _exitMenuItem;
        private bool enableDvorakMapping = true;
        private int mapToDvorakKeyboardId =3;
        private bool enableKeyRecording = true;

        private void DisplayStatusMessage(string text)
        {
            _hiddenWindow.Dispatcher.Invoke(delegate
            {
                _notifyIcon.BalloonTipText = _deviceManager.DeviceName + ": " + text;
                // The timeout is ignored on recent Windows
                _notifyIcon.ShowBalloonTip(3000);
            });
        }

        private void UpdateStatusView()
        {
            if (_deviceManager != null)
            {
                List<KeyValuePair<string, bool>> flags = _deviceManager.StatusFlags;
                List<KeyValuePair<string, string>> statusItems = flags.Select(n => new KeyValuePair<string, string>(n.Key, n.Value.ToString())).ToList();
                statusItems.Insert(0, new KeyValuePair<string, string>("Device", _deviceManager.DeviceName));
                statusItems.Insert(1, new KeyValuePair<string, string>("Status", _deviceManager.Status.ToString()));
            }
        }

        public void OnStatusChange()
        {
            UpdateStatusView();

            switch (_deviceManager.Status)
            {
                case DeviceStatus.Initialised:
                    _notifyIcon.Text = _deviceManager.DeviceName + ": Ready";
                    _notifyIcon.Icon = Properties.Resources.NotReadyIcon;
                    DisplayStatusMessage("Idle");
                    break;
                case DeviceStatus.Running:
                    _notifyIcon.Text = _deviceManager.DeviceName + ": Running";
                    _notifyIcon.Icon = Properties.Resources.ReadyIcon;
                    DisplayStatusMessage("Running");
                    break;
                case DeviceStatus.Starting:
                    _notifyIcon.Text = _deviceManager.DeviceName + ": Starting";
                    _notifyIcon.Icon = Properties.Resources.NotReadyIcon;
                    DisplayStatusMessage("Starting");
                    break;
                case DeviceStatus.Uninitialised:
                    _notifyIcon.Text = _deviceManager.DeviceName + ": Not Ready";
                    _notifyIcon.Icon = Properties.Resources.NotReadyIcon;
                    break;
                case DeviceStatus.Error:
                    _notifyIcon.Text = _deviceManager.DeviceName + ": Error Detected";
                    _notifyIcon.Icon = Properties.Resources.NotReadyIcon;
                    break;
                default:
                    _notifyIcon.Text = _deviceManager.DeviceName + ": -";
                    _notifyIcon.Icon = Properties.Resources.NotReadyIcon;
                    break;
            }
            System.Windows.Media.ImageSource icon = AppIcon;
        }

        private void startStopReaderItem_Click(object sender, EventArgs e)
        {
            if (_deviceManager.Status == DeviceStatus.Running)
            {
                _deviceManager.Stop();
            }
            else
            {
                _deviceManager.Start();
            }
        }
        
        private ToolStripMenuItem ToolStripMenuItemWithHandler(string displayText, string tooltipText, EventHandler eventHandler)
        {
            var item = new ToolStripMenuItem(displayText);
            if (eventHandler != null)
            {
                item.Click += eventHandler;
            }

            item.ToolTipText = tooltipText;
            return item;
        }
        
        private void ShowStatusView()
        {
                UpdateStatusView();
        }

        private void showStatusItem_Click(object sender, EventArgs e)
        {
            ShowStatusView();
        }

        private void ShowAboutView()
        {
        }

        private void showHelpItem_Click(object sender, EventArgs e)
        {
            ShowAboutView();
        }

        private void showWebSite_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("http://www.CodeProject.com/");
        }
        
        private void exitItem_Click(object sender, EventArgs e)
        {
            Application.Exit();

            input.Unload();
        }

        private void notifyIcon_DoubleClick(object sender, EventArgs e)
        {
            ShowAboutView();
        }

        private void notifyIcon_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                MethodInfo mi = typeof(NotifyIcon).GetMethod("ShowContextMenu", BindingFlags.Instance | BindingFlags.NonPublic);
                mi.Invoke(_notifyIcon, null);
            }
        }
        
        private void SetMenuItems()
        {
            switch (_deviceManager.Status)
            {
                case DeviceStatus.Initialised:
                    _startDeviceMenuItem.Enabled = true;
                    _stopDeviceMenuItem.Enabled = false;
                    _exitMenuItem.Enabled = true;
                    break;
                case DeviceStatus.Starting:
                    _startDeviceMenuItem.Enabled = false;
                    _stopDeviceMenuItem.Enabled = false;
                    _exitMenuItem.Enabled = false;
                    break;
                case DeviceStatus.Running:
                    _startDeviceMenuItem.Enabled = false;
                    _stopDeviceMenuItem.Enabled = true;
                    _exitMenuItem.Enabled = true;
                    break;
                case DeviceStatus.Uninitialised:
                    _startDeviceMenuItem.Enabled = false;
                    _stopDeviceMenuItem.Enabled = false;
                    _exitMenuItem.Enabled = true;
                    break;
                case DeviceStatus.Error:
                    _startDeviceMenuItem.Enabled = false;
                    _stopDeviceMenuItem.Enabled = false;
                    _exitMenuItem.Enabled = true;
                    break;
                default:
                    System.Diagnostics.Debug.Assert(false, "SetButtonStatus() => Unknown state");
                    break;
            }
        }

        private void ContextMenuStrip_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = false;

            if (_notifyIcon.ContextMenuStrip.Items.Count == 0)
            {
                _startDeviceMenuItem = ToolStripMenuItemWithHandler(
                    "Start Device",
                    "Starts the device",
                    startStopReaderItem_Click);
                _notifyIcon.ContextMenuStrip.Items.Add(_startDeviceMenuItem);
                _stopDeviceMenuItem = ToolStripMenuItemWithHandler(
                    "Stop Device",
                    "Stops the device",
                    startStopReaderItem_Click);
                _notifyIcon.ContextMenuStrip.Items.Add(_stopDeviceMenuItem);
                _notifyIcon.ContextMenuStrip.Items.Add(new ToolStripSeparator());
                _notifyIcon.ContextMenuStrip.Items.Add(ToolStripMenuItemWithHandler("Device S&tatus", "Shows the device status dialog", showStatusItem_Click));
                _notifyIcon.ContextMenuStrip.Items.Add(ToolStripMenuItemWithHandler("&About", "Shows the About dialog", showHelpItem_Click));
                _notifyIcon.ContextMenuStrip.Items.Add(ToolStripMenuItemWithHandler("Code Project &Web Site", "Navigates to the Code Project Web Site", showWebSite_Click));
                _notifyIcon.ContextMenuStrip.Items.Add(new ToolStripSeparator());
                _exitMenuItem = ToolStripMenuItemWithHandler("&Exit", "Exits System Tray App", exitItem_Click);
                _notifyIcon.ContextMenuStrip.Items.Add(_exitMenuItem);
            }

            SetMenuItems();
        }
    }
}
