using ALsSoundSwitcher.Properties;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;
using static ALsSoundSwitcher.Globals;

namespace ALsSoundSwitcher
{
  public partial class Form1
  {
    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool SetForegroundWindow(IntPtr hwnd);

    private static void InvokeRightClick()
    {
      UpdateSliderAndTooltip_Async();

      var mi = typeof(NotifyIcon).GetMethod("ShowContextMenu", BindingFlags.Instance | BindingFlags.NonPublic);
      mi?.Invoke(notifyIcon1, null);
    }

    private static async void UpdateSliderAndTooltip_Async()
    {
      await Task.Run(() => MenuItemSlider.RefreshValue());
      var deviceText = ActiveMenuItemOutputDevice?.Text ?? "";
      await Task.Run(() => SetToolTip(deviceText));
    }

    private static void HandleCloseOnClick(object sender, ToolStripDropDownClosingEventArgs e)
    {
      if (e.CloseReason != ToolStripDropDownCloseReason.ItemClicked)
      {
        return;
      }

      var menuLocation = ((ContextMenuStrip)sender).PointToClient(MousePosition);
      var clickedItem = ((ContextMenuStrip)sender).GetItemAt(menuLocation);
      if (clickedItem != null && clickedItem.GetType() == typeof(ToolStripSeparator))
      {
        e.Cancel = true;
      }
    }

    private void notifyIcon1_MouseClick(object sender, MouseEventArgs e)
    {
      if (e.Button == MouseButtons.Right)
      {
        UpdateSliderAndTooltip_Async();
      }
      else if (e.Button == MouseButtons.Left)
      {
        HandleClickAsPerCurrentSettings(UserSettings.LeftClickFunction);
      }
      else if (e.Button == MouseButtons.Middle)
      {
        HandleClickAsPerCurrentSettings(UserSettings.MiddleClickFunction);
      }
    }

    private static void HandleClickAsPerCurrentSettings(MouseControlFunction mouseControlFunction)
    {
      switch (mouseControlFunction)
      {
        case MouseControlFunction.None:
          break;
        case MouseControlFunction.Exit:
          Instance.Close();
          break;
        case MouseControlFunction.Expand:
          InvokeRightClick();
          break;
        case MouseControlFunction.Browse:
          OpenFileExplorer();
          break;
        case MouseControlFunction.Refresh:
          ProcessUtils.Restart_ThreadSafe();
          break;
        case MouseControlFunction.Volume_Mixer:
          OpenVolumeMixer();
          break;
        case MouseControlFunction.Manage_Devices:
          OpenDeviceManager();
          break;
        case MouseControlFunction.Switch_Next_Device:
          ToggleOutput();
          break;
        default:
          throw new ArgumentOutOfRangeException();
      }
    }

    private static void menuItemMixer_Click(object sender, EventArgs e)
      => OpenVolumeMixer();

    private static void OpenVolumeMixer()
    {
      var processName = Path.GetFileNameWithoutExtension(VolumeMixerExe);
      var processes = Process.GetProcessesByName(processName);
      if (processes.Length > 0)
      {
        foreach (var process in processes)
        {
          SetForegroundWindow(process.MainWindowHandle);
        }
      }
      else
      {
        ProcessUtils.RunExe(VolumeMixerExe, VolumeMixerArgs, true);
      }
    }

    private static void menuItemDeviceManager_Click(object sender, EventArgs e)
      => OpenDeviceManager();

    private static void OpenDeviceManager()
      => ProcessUtils.RunExe(DeviceManagerExe, DeviceManagerArgs, true);

    private static void MenuItemLaunchOnStartup_Click(object sender, EventArgs e)
    {
      var regResult = UserSettings.LaunchOnStartup ?
        RegistryUtils.TryDeleteStartupRegistrySetting() : RegistryUtils.TrySaveStartupRegistrySetting();

      if (regResult == false)
      {
        return;
      }

      UserSettings.LaunchOnStartup = !UserSettings.LaunchOnStartup;

      Config.Save();

      SetBackgroundForMenuItemLaunchOnStartup();

      RestoreMenus((ToolStripItem)sender);
    }

    private static void menuItemOutput_Click(object sender, EventArgs e)
      => PerformOutputSwitch((ToolStripMenuItem)sender);

    private static void menuItemInput_Click(object sender, EventArgs e)
      => PerformInputSwitch((ToolStripMenuItem)sender);

    private static void menuItemExit_Click(object sender, EventArgs e)
      => Instance.Close();

    private static void menuItemUpdate_Click(object sender, EventArgs e)
    => UpgradeUtils.Run();

    private static void menuItemRefresh_Click(object sender, EventArgs e)
      => ProcessUtils.Restart_ThreadSafe();

    private static void menuItemHelp_Click(object sender, EventArgs e)
      => Process.Start(GithubUrl);

    private static void menuItemBrowse_Click(object sender, EventArgs e)
      => OpenFileExplorer();

    private static void OpenFileExplorer()
      => Process.Start(Directory.GetCurrentDirectory());

    //Not sure why non-BaseMenu hovers don't expand properly on first try without this call. 
    private static void menuItemExpandable_Hover(object sender, EventArgs e)
      => ((ToolStripMenuItem) sender)?.ShowDropDown();

    private static void menuItemTheme_Click(object sender, EventArgs e)
    {
      UserSettings.Theme = ((ToolStripMenuItem) sender).Text;

      RefreshUITheme();

      RestoreMenus((ToolStripItem)sender);

      Config.Save();
    }

    private static void menuItemPreventAutoSwitch_Click(object sender, EventArgs e)
    {
      UserSettings.PreventAutoSwitch = !UserSettings.PreventAutoSwitch;

      Config.Save();

      SetBackgroundForMenuItemPreventAutoSwitch();

      RestoreMenus((ToolStripItem)sender);
    }

    private static void menuItemLockDevice_Click(object sender, EventArgs e)
    {
      var selectedDeviceId = (string)((ToolStripMenuItem)sender).Tag;
      var isOutput = ((ToolStripMenuItem)sender).Text.StartsWith(OutputPrefix);
      
      if (isOutput)
      {
        if (UserSettings.DualDefault)
        {
          // toggle output device lock (both default + comms)
          if (UserSettings.LockedOutputDefaultDeviceId == selectedDeviceId &&
              UserSettings.LockedOutputCommsDeviceId == selectedDeviceId)
          {
            UserSettings.LockedOutputDefaultDeviceId = "";
            UserSettings.LockedOutputCommsDeviceId = "";
          }
          else
          {
            UserSettings.LockedOutputDefaultDeviceId = selectedDeviceId;
            UserSettings.LockedOutputCommsDeviceId = selectedDeviceId;

            // also switch to the locked device now (both roles)
            ProcessUtils.RunExe(SetDeviceExe, selectedDeviceId);
          }
        }
        else
        {
          // DualDefault OFF: alternate between setting Default and Comms
          var setComms = NextOutputLockIsComms;
          if (setComms)
          {
            if (UserSettings.LockedOutputCommsDeviceId == selectedDeviceId)
            {
              UserSettings.LockedOutputCommsDeviceId = "";
            }
            else
            {
              UserSettings.LockedOutputCommsDeviceId = selectedDeviceId;
              ProcessUtils.RunExe(SetDeviceExe, selectedDeviceId + " comms");
              NextOutputLockIsComms = false;
            }
          }
          else
          {
            if (UserSettings.LockedOutputDefaultDeviceId == selectedDeviceId)
            {
              UserSettings.LockedOutputDefaultDeviceId = "";
            }
            else
            {
              UserSettings.LockedOutputDefaultDeviceId = selectedDeviceId;
              ProcessUtils.RunExe(SetDeviceExe, selectedDeviceId + " default");
              NextOutputLockIsComms = true;
            }
          }
        }
      }
      else
      {
        if (UserSettings.DualDefault)
        {
          // toggle input device lock (both default + comms)
          if (UserSettings.LockedInputDefaultDeviceId == selectedDeviceId &&
              UserSettings.LockedInputCommsDeviceId == selectedDeviceId)
          {
            UserSettings.LockedInputDefaultDeviceId = "";
            UserSettings.LockedInputCommsDeviceId = "";
          }
          else
          {
            UserSettings.LockedInputDefaultDeviceId = selectedDeviceId;
            UserSettings.LockedInputCommsDeviceId = selectedDeviceId;

            // also switch to the locked device now (both roles)
            PowerShellUtils.SetInputDeviceCmdlet(selectedDeviceId);
          }
        }
        else
        {
          // DualDefault OFF: alternate between setting Default and Comms
          var setComms = NextInputLockIsComms;
          if (setComms)
          {
            if (UserSettings.LockedInputCommsDeviceId == selectedDeviceId)
            {
              UserSettings.LockedInputCommsDeviceId = "";
            }
            else
            {
              UserSettings.LockedInputCommsDeviceId = selectedDeviceId;
              PowerShellUtils.SetInputDeviceCmdlet(selectedDeviceId, PowerShellUtils.InputDeviceRoleSwitch.CommsOnly);
              NextInputLockIsComms = false;
            }
          }
          else
          {
            if (UserSettings.LockedInputDefaultDeviceId == selectedDeviceId)
            {
              UserSettings.LockedInputDefaultDeviceId = "";
            }
            else
            {
              UserSettings.LockedInputDefaultDeviceId = selectedDeviceId;
              PowerShellUtils.SetInputDeviceCmdlet(selectedDeviceId, PowerShellUtils.InputDeviceRoleSwitch.DefaultOnly);
              NextInputLockIsComms = true;
            }
          }
        }
      }

      Config.Save();

      SetBackgroundForMenuItemLockDevice();

      RestoreMenus((ToolStripItem)sender);
    }

    private static void menuItemLockDeviceClear_Click(object sender, EventArgs e)
    {
      UserSettings.LockedOutputDefaultDeviceId = "";
      UserSettings.LockedOutputCommsDeviceId = "";
      UserSettings.LockedInputDefaultDeviceId = "";
      UserSettings.LockedInputCommsDeviceId = "";

      // clear legacy fields too (kept for migration)
      UserSettings.LockedOutputDeviceId = "";
      UserSettings.LockedInputDeviceId = "";

      NextOutputLockIsComms = false;
      NextInputLockIsComms = false;

      Config.Save();

      SetBackgroundForMenuItemLockDevice();

      RestoreMenus((ToolStripItem)sender);
    }

    private static void menuItemDualDefault_Click(object sender, EventArgs e)
    {
      UserSettings.DualDefault = !UserSettings.DualDefault;

      Config.Save();

      SetBackgroundForMenuItemDualDefault();

      RestoreMenus((ToolStripItem)sender);
    }

    private static void menuItemLockVolumeLevel_Click(object sender, EventArgs e)
    {
      var level = (int)((ToolStripMenuItem)sender).Tag;
      
      if (level == -1)
      {
        // Off selected
        UserSettings.LockVolume = false;
      }
      else
      {
        UserSettings.LockVolume = true;
        UserSettings.LockVolumeLevel = level;
        // apply this level to all locked devices and SET the volume immediately
        var deviceIds = UserSettings.LockedVolumes.Keys.ToList();
        foreach (var deviceId in deviceIds)
        {
          UserSettings.LockedVolumes[deviceId] = level;
          DeviceUtils.SetDeviceLevel(deviceId, level);
        }
      }

      Config.Save();

      SetBackgroundForMenuItemLockVolume();

      RestoreMenus((ToolStripItem)sender);
    }

    private static void menuItemLockVolumeDevice_Click(object sender, EventArgs e)
    {
      var selectedDeviceId = (string)((ToolStripMenuItem)sender).Tag;
      
      // toggle device in/out of locked list
      if (UserSettings.LockedVolumes.ContainsKey(selectedDeviceId))
      {
        UserSettings.LockedVolumes.Remove(selectedDeviceId);
      }
      else
      {
        // If lock is enabled, lock device to the selected level. Otherwise, just capture current level.
        if (UserSettings.LockVolume)
        {
          UserSettings.LockedVolumes[selectedDeviceId] = UserSettings.LockVolumeLevel;
          DeviceUtils.SetDeviceLevel(selectedDeviceId, UserSettings.LockVolumeLevel);
        }
        else
        {
          var currentVolume = DeviceUtils.GetDeviceLevel(selectedDeviceId);
          UserSettings.LockedVolumes[selectedDeviceId] = currentVolume;
        }
      }

      Config.Save();

      SetBackgroundForMenuItemLockVolumeDevice();

      RestoreMenus((ToolStripItem)sender);
    }

    private static void menuItemLockVolumeDeviceClear_Click(object sender, EventArgs e)
    {
      // only clear volume locks
      UserSettings.LockedVolumes.Clear();

      Config.Save();

      SetBackgroundForMenuItemLockVolumeDevice();

      RestoreMenus((ToolStripItem)sender);
    }

    private static void menuItemCreateTheme_Click(object sender, EventArgs e)
      => new ThemeCreator().Show();

    private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
    => Process.Start(GithubUrl);

    private static void menuItemMouseControlFunction_Click(object sender, EventArgs e)
    {
      var mouseControlFunction =
        MouseFunctionDictionary.First(kvp => kvp.Value == ((ToolStripDropDownItem)sender).Tag.ToString()).Key;

      var parent = ((ToolStripDropDownItem)sender).OwnerItem.Text;

      if (parent == Resources.Form1_SetupMouseControlsSubmenu_Left_Click)
      {
        UserSettings.LeftClickFunction = mouseControlFunction;
      }
      else if (parent == Resources.Form1_SetupMouseControlsSubmenu_Middle_Click)
      {
        UserSettings.MiddleClickFunction = mouseControlFunction;
      }
      
      Config.Save();

      SetBackgroundForMouseControlSubmenus();
    }

    public static void RestoreMenus(ToolStripItem sender)
    {
      BaseMenu.Show();
      MenuItemMore.Select();
      MenuItemMore.DropDown.Show();
      sender.GetCurrentParent().Show();
      sender.Select();
    }
  }
}