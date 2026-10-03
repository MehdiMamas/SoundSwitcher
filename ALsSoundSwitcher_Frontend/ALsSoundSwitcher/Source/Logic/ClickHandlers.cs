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
      if (UserSettings.PreventAutoSwitch)
      {
        DeviceUtils.CaptureCurrentDevicesAsLocks();
      }

      Config.Save();

      SetBackgroundForMenuItemPreventAutoSwitch();
      SetBackgroundForMenuItemLockDevice();

      RestoreMenus((ToolStripItem)sender);
    }

    private static void menuItemLockDevice_Click(object sender, EventArgs e)
    {
      var menuItem = (ToolStripMenuItem)sender;
      var selectedDeviceId = (string)menuItem.Tag;
      var isOutput = menuItem.Text.StartsWith(OutputPrefix);

      if (isOutput)
      {
        var alreadyLocked = UserSettings.LockedOutputDefaultDeviceId == selectedDeviceId &&
                            UserSettings.LockedOutputCommsDeviceId == selectedDeviceId;
        if (alreadyLocked)
        {
          UserSettings.LockedOutputDefaultDeviceId = "";
          UserSettings.LockedOutputCommsDeviceId = "";
        }
        else
        {
          UserSettings.PreventAutoSwitch = true;
          UserSettings.LockedOutputDefaultDeviceId = selectedDeviceId;
          UserSettings.LockedOutputCommsDeviceId = selectedDeviceId;
          BeginOwnedDeviceChange();
          ProcessUtils.RunExe(SetDeviceExe, selectedDeviceId);
        }
      }
      else
      {
        var alreadyLocked = UserSettings.LockedInputDefaultDeviceId == selectedDeviceId &&
                            UserSettings.LockedInputCommsDeviceId == selectedDeviceId;
        if (alreadyLocked)
        {
          UserSettings.LockedInputDefaultDeviceId = "";
          UserSettings.LockedInputCommsDeviceId = "";
        }
        else if (PowerShellUtils.VerifyAudioCmdletsAvailability() &&
                 TryApplyInputDevice(selectedDeviceId, PowerShellUtils.InputDeviceRoleSwitch.Both))
        {
          UserSettings.PreventAutoSwitch = true;
          UserSettings.LockedInputDefaultDeviceId = selectedDeviceId;
          UserSettings.LockedInputCommsDeviceId = selectedDeviceId;
        }
      }

      Config.Save();

      SetBackgroundForMenuItemPreventAutoSwitch();
      SetBackgroundForMenuItemLockDevice();

      RestoreMenus((ToolStripItem)sender);
    }

    private static void menuItemLockDeviceRole_Click(object sender, EventArgs e)
    {
      var selection = (DeviceLockSelection)((ToolStripMenuItem)sender).Tag;
      if (IsRoleLocked(selection))
      {
        ClearRoleLock(selection);
      }
      else if (selection.IsOutput)
      {
        UserSettings.PreventAutoSwitch = true;
        SetRoleLock(selection);
        BeginOwnedDeviceChange();
        var args = selection.DeviceId + (selection.IsComms ? " comms" : " default");
        ProcessUtils.RunExe(SetDeviceExe, args);
      }
      else if (PowerShellUtils.VerifyAudioCmdletsAvailability() &&
               TryApplyInputDevice(
                 selection.DeviceId,
                 selection.IsComms
                   ? PowerShellUtils.InputDeviceRoleSwitch.CommsOnly
                   : PowerShellUtils.InputDeviceRoleSwitch.DefaultOnly))
      {
        UserSettings.PreventAutoSwitch = true;
        SetRoleLock(selection);
      }

      Config.Save();

      SetBackgroundForMenuItemPreventAutoSwitch();
      SetBackgroundForMenuItemLockDevice();

      RestoreMenus((ToolStripItem)sender);
    }

    private static bool TryApplyInputDevice(string deviceId, PowerShellUtils.InputDeviceRoleSwitch role)
    {
      try
      {
        BeginOwnedDeviceChange();
        PowerShellUtils.SetInputDeviceCmdlet(deviceId, role);
        return true;
      }
      catch (Exception ex)
      {
        Console.WriteLine(ex);
        NotifyUserOfSwitchResult(null, false);
        return false;
      }
    }

    private static bool IsRoleLocked(DeviceLockSelection selection)
    {
      if (selection.IsOutput)
      {
        return selection.IsComms
          ? UserSettings.LockedOutputCommsDeviceId == selection.DeviceId
          : UserSettings.LockedOutputDefaultDeviceId == selection.DeviceId;
      }

      return selection.IsComms
        ? UserSettings.LockedInputCommsDeviceId == selection.DeviceId
        : UserSettings.LockedInputDefaultDeviceId == selection.DeviceId;
    }

    private static void SetRoleLock(DeviceLockSelection selection)
    {
      if (selection.IsOutput)
      {
        if (selection.IsComms)
        {
          UserSettings.LockedOutputCommsDeviceId = selection.DeviceId;
        }
        else
        {
          UserSettings.LockedOutputDefaultDeviceId = selection.DeviceId;
        }
        return;
      }

      if (selection.IsComms)
      {
        UserSettings.LockedInputCommsDeviceId = selection.DeviceId;
      }
      else
      {
        UserSettings.LockedInputDefaultDeviceId = selection.DeviceId;
      }
    }

    private static void ClearRoleLock(DeviceLockSelection selection)
    {
      if (selection.IsOutput)
      {
        if (selection.IsComms && UserSettings.LockedOutputCommsDeviceId == selection.DeviceId)
        {
          UserSettings.LockedOutputCommsDeviceId = "";
        }
        else if (!selection.IsComms && UserSettings.LockedOutputDefaultDeviceId == selection.DeviceId)
        {
          UserSettings.LockedOutputDefaultDeviceId = "";
        }
        return;
      }

      if (selection.IsComms && UserSettings.LockedInputCommsDeviceId == selection.DeviceId)
      {
        UserSettings.LockedInputCommsDeviceId = "";
      }
      else if (!selection.IsComms && UserSettings.LockedInputDefaultDeviceId == selection.DeviceId)
      {
        UserSettings.LockedInputDefaultDeviceId = "";
      }
    }

    private sealed class DeviceLockSelection
    {
      public DeviceLockSelection(string deviceId, bool isOutput, bool isComms)
      {
        DeviceId = deviceId;
        IsOutput = isOutput;
        IsComms = isComms;
      }

      public string DeviceId { get; }
      public bool IsOutput { get; }
      public bool IsComms { get; }
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

      Config.Save();

      SetBackgroundForMenuItemLockDevice();

      RestoreMenus((ToolStripItem)sender);
    }

    private static void menuItemDualDefault_Click(object sender, EventArgs e)
    {
      UserSettings.DualDefault = !UserSettings.DualDefault;

      SetupLockDeviceSubmenu();

      Config.Save();

      SetBackgroundForMenuItemDualDefault();
      SetBackgroundForMenuItemLockDevice();

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
        else if (DeviceUtils.TryGetDeviceLevel(selectedDeviceId, out var currentVolume))
        {
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