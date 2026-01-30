using System;
using System.Drawing;
using System.Windows.Forms;
using ALsSoundSwitcher.Properties;

namespace ALsSoundSwitcher
{
  public partial class Form1 : Form
  {
    public Form1()
      => InitializeComponent();

    private void Form1_Load(object sender, EventArgs e)
    {
      ProcessUtils.SetWorkingDirectory();

      Globals.Instance = this;

      if (System.Diagnostics.Debugger.IsAttached)
      {
        TestUtils.RunDebugCode();
      }

      if (Config.Read() == false)
      {
        NotifyUserOfConfigReadFail();
      }

      if (Globals.UserSettings.LaunchOnStartup)
      {
        if (RegistryUtils.DoesStartupRegistrySettingAlreadyExistForThisPath() == false)
        {
          Globals.UserSettings.LaunchOnStartup = false;
          Config.Save();
        }
      }

      SetupUI();

      if (Globals.VolumeLockTimer == null)
      {
        Globals.VolumeLockTimer = new Timer();
        Globals.VolumeLockTimer.Interval = 500;
        Globals.VolumeLockTimer.Tick += (_, _) => DeviceUtils.EnforceLockedVolumes();
        Globals.VolumeLockTimer.Start();
      }

      Minimize();

      DeviceUtils.Monitor();
      
      UpgradeUtils.PollForUpdates_Async();
      
      UpgradeUtils.MonitorForOutdatedFilesAndAttemptRemoval_Async();

      FileWatcher.Run();

      KeyWatcher.Run(ToggleOutput);
    }

    private void NotifyUserOfConfigReadFail()
    {
      notifyIcon1.ShowBalloonTip(
        Globals.UserSettings.BalloonTime,
        Resources.Form1_ReadConfig_Error_reading_config_file_ + Globals.ConfigFile,
        Resources.Form1_ReadConfig_Will_use_default_values,
        ToolTipIcon.Error
        );
    }

    private void Minimize()
    {
      WindowState = FormWindowState.Minimized;
      ShowInTaskbar = false;
      Visible = false;
    }

    public void ShowTrayIcon()
    {
      if (InvokeRequired)
      {
        Invoke(new MethodInvoker(ShowTrayIcon));
      }
      else
      {
        notifyIcon1.Visible = true;
      }
    }

    public void HideTrayIcon()
    {
      if (InvokeRequired)
      {
        Invoke(new MethodInvoker(HideTrayIcon));
      }
      else
      {
        notifyIcon1.Visible = false;
      }
    }

    public void SetTrayIcon(Icon icon)
    {
      if (InvokeRequired)
      {
        Invoke((MethodInvoker) delegate
        {
          SetTrayIcon(icon);
        });
      }
      else
      {
        notifyIcon1.Icon = icon;
      }
    }

    private void btn_restart_Click(object sender, EventArgs e) 
      => Application.Restart();

    private void Form1_FormClosing(object sender, FormClosingEventArgs e) 
      => Globals.GlobalHotKeyManager.Dispose();
  }
}