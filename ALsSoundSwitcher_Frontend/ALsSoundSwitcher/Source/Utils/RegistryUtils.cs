using System.Windows.Forms;
using Microsoft.Win32;
using System;

namespace ALsSoundSwitcher
{
  public class RegistryUtils
  {
    private static string GetName() 
      => Application.ProductName;

    private static RegistryKey GetRegKey() 
      => Registry.CurrentUser.OpenSubKey(Globals.StartupRegistryKey, true);
    
    public static bool TryDeleteStartupRegistrySetting()
    {
      try
      {
        var rk = GetRegKey();
        var name = GetName();
        rk.DeleteValue(name, false);

        return true;
      }
      catch (Exception ex)
      {
        Console.WriteLine(ex.ToString());
        return false;
      }
    }

    public static bool TrySaveStartupRegistrySetting()
    {
      try
      {
        var rk = GetRegKey();
        var name = GetName();
        rk.SetValue(name, Application.ExecutablePath);

        return true;
      }
      catch (Exception ex)
      {
        Console.WriteLine(ex.ToString());
        return false;
      }
    }

    public static bool DoesStartupRegistrySettingAlreadyExistForThisPath()
    {
      try
      {
        var rk = GetRegKey();
        var name = GetName();
        var regValue = (string)rk.GetValue(name);
        return regValue == Application.ExecutablePath;
      }
      catch (Exception ex)
      {
        Console.WriteLine(ex.ToString());
        return false;
      }
    }
  }
}
