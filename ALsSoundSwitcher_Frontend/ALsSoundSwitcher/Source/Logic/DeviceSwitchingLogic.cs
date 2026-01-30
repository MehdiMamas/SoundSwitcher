using System;
using System.Linq;
using System.Windows.Forms;
using ALsSoundSwitcher.Properties;
using static ALsSoundSwitcher.Globals;

namespace ALsSoundSwitcher
{
  public partial class Form1
  {
    private static void PerformOutputSwitch(ToolStripMenuItem menuItem)
    {
      try
      {
        WeAreSwitching = true;

        var deviceId = (string) menuItem.Tag;
        var args = UserSettings.DualDefault ? deviceId : deviceId + " default";
        ProcessUtils.RunExe(SetDeviceExe, args);

        ActiveMenuItemOutputDevice = menuItem;

        var deviceName = menuItem.Text;

        IconUtils.SetTrayIcon(deviceName);

        SetToolTip(deviceName);

        SetActiveMenuItemMarkers();

        MenuItemSlider.RefreshValue();

        NotifyUserOfSwitchResult(deviceName, true);
      }
      catch (Exception ex)
      {
        WeAreSwitching = false;
        
        Console.WriteLine(ex.ToString());

        NotifyUserOfSwitchResult(null, true);
      }
    }

    private static void PerformInputSwitch(ToolStripMenuItem menuItem)
    {
      try
      {
        WeAreSwitching = true;

        var deviceId = (string) menuItem.Tag;
        PowerShellUtils.SetInputDeviceCmdlet(deviceId);

        ActiveMenuItemInputDevice = menuItem;

        var deviceName = menuItem.Text;

        SetActiveMenuItemMarkers();

        NotifyUserOfSwitchResult(deviceName, false);
      }
      catch (Exception ex)
      {
        WeAreSwitching = false;
        
        Console.WriteLine(ex.ToString());

        NotifyUserOfSwitchResult(null, false);
      }
    }

    private static void ToggleOutput()
    {
      if (ActiveOutputDevices.Count == 0)
      {
        return;
      }

      var items = BaseMenu.Items.OfType<ToolStripMenuItem>()
        .Where(it => it.Text.StartsWith(OutputPrefix))
        .ToList();

      if (items.Count == 0) return;

      var currentIndex = items.IndexOf(ActiveMenuItemOutputDevice);
      var nextIndex = (currentIndex + 1) % items.Count;

      PerformOutputSwitch(items[nextIndex]);
    }

    private static void NotifyUserOfSwitchResult(string deviceName, bool isOutput)
    {
      if (deviceName != null)
      {
        var title = isOutput
          ? Resources.Form1_PerformSwitch_Switched_Audio_Output_Device
          : Resources.Form1_PerformSwitch_Switched_Audio_Input_Device;

        notifyIcon1.ShowBalloonTip(
          UserSettings.BalloonTime,
          title,
          deviceName,
          ToolTipIcon.None
        );
      }
      else
      {
        notifyIcon1.ShowBalloonTip(
          UserSettings.BalloonTime,
          Resources.Form1_PerformSwitch_Error_Switching_Audio_Device,
          Resources.Form1_PerformSwitch_could_not_set_default_device,
          ToolTipIcon.Error
        );
      }
    }
  }
}