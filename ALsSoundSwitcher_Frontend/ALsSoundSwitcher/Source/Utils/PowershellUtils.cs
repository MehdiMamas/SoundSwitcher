using System;
using System.Diagnostics;
using System.Management.Automation;
using System.Windows.Forms;
using ALsSoundSwitcher.Properties;

namespace ALsSoundSwitcher
{
  public class PowerShellUtils
  {
    public enum InputDeviceRoleSwitch
    {
      Both,
      DefaultOnly,
      CommsOnly
    }

    public static void SetInputDeviceCmdlet(string deviceId)
      => SetInputDeviceCmdlet(deviceId, InputDeviceRoleSwitch.Both);

    public static void SetInputDeviceCmdlet(string deviceId, InputDeviceRoleSwitch roleSwitch)
    {
      using var ps = PowerShell.Create();
      ps.AddCommand("Set-AudioDevice");
      ps.AddParameter("-ID", deviceId);

      switch (roleSwitch)
      {
        case InputDeviceRoleSwitch.Both:
          break;
        case InputDeviceRoleSwitch.DefaultOnly:
          ps.AddParameter("-DefaultOnly");
          break;
        case InputDeviceRoleSwitch.CommsOnly:
          ps.AddParameter("-CommunicationOnly");
          break;
        default:
          throw new ArgumentOutOfRangeException(nameof(roleSwitch), roleSwitch, null);
      }

      ps.Invoke();
    }

    public static bool VerifyAudioCmdletsAvailability()
    {
      if (AudioCmdletsNeedsInstallation())
      {
        return InstallAudioCmdlets();
      }

      return true;
    }

    private static bool AudioCmdletsNeedsInstallation()
    {
      using var powerShell = PowerShell.Create();
      powerShell.AddCommand("Get-Command");
      powerShell.AddParameter("-Name", "Set-AudioDevice");
      var results = powerShell.Invoke();
      return results.Count == 0;
    }

    private static bool InstallAudioCmdlets()
    {
      var selection = MessageBox.Show(
        Resources.PowerShellUtils_InstallAudioCmdlets_Message,
        Resources.ALs_Sound_Switcher,
        MessageBoxButtons.OKCancel, 
        MessageBoxIcon.Asterisk
      );

      if (selection == DialogResult.Cancel)
      {
        return false;
      }

      var process = new Process();
      process.StartInfo = new ProcessStartInfo
      {
        FileName = "powershell.exe",
        Verb = "runas",
        UseShellExecute = true,
        Arguments = "-Command \"Install-Module -Name AudioDeviceCmdlets -Force\""
      };

      try
      {
        process.Start();
        process.WaitForExit();

        if (AudioCmdletsNeedsInstallation())
        {
          throw new Exception();
        }
      }
      catch (Exception ex)
      {
        Console.WriteLine(ex);
        
        MessageBox.Show(
          Resources.PowerShellUtils_InstallAudioCmdlets_Error_Message,
          Resources.ALs_Sound_Switcher,
          MessageBoxButtons.OK,
          MessageBoxIcon.Error
        );

        return false;
      }

      return true;
    }
  }
}
