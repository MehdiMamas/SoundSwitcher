using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using static ALsSoundSwitcher.Globals;

namespace ALsSoundSwitcher
{
  public class Config
  {
    public static bool Read()
    {
      try
      {
        EnsureConfigFileExists();

        ProcessJsonSettings();

        return true;
      }
      catch (Exception ex)
      {
        Console.WriteLine(ex.ToString());

        Save();

        return false;
      }
    }

    private static void EnsureConfigFileExists()
    {
      if (File.Exists(ConfigFile))
      {
        return;
      }

      // no config file yet — write out defaults so the rest of startup has something to read
      UserSettings = new Settings();
      UserSettings.LockedVolumes = new Dictionary<string, int>();
      Save();
    }

    private static void ProcessJsonSettings()
    {
      var jsonString = File.ReadAllText(ConfigFile);

      UserSettings = JsonConvert.DeserializeObject<Settings>(jsonString);

      // ensure dictionary is initialized
      if (UserSettings.LockedVolumes == null)
      {
        UserSettings.LockedVolumes = new Dictionary<string, int>();
      }

      // migrate legacy device lock fields into role-specific locks (one-time compatibility)
      if (string.IsNullOrEmpty(UserSettings.LockedOutputDefaultDeviceId) &&
          string.IsNullOrEmpty(UserSettings.LockedOutputCommsDeviceId) &&
          !string.IsNullOrEmpty(UserSettings.LockedOutputDeviceId))
      {
        UserSettings.LockedOutputDefaultDeviceId = UserSettings.LockedOutputDeviceId;
        UserSettings.LockedOutputCommsDeviceId = UserSettings.LockedOutputDeviceId;
      }

      if (string.IsNullOrEmpty(UserSettings.LockedInputDefaultDeviceId) &&
          string.IsNullOrEmpty(UserSettings.LockedInputCommsDeviceId) &&
          !string.IsNullOrEmpty(UserSettings.LockedInputDeviceId))
      {
        UserSettings.LockedInputDefaultDeviceId = UserSettings.LockedInputDeviceId;
        UserSettings.LockedInputCommsDeviceId = UserSettings.LockedInputDeviceId;
      }

      TryUpdateFileStructure();

      SettingsHash = jsonString.GetHashCode();
    }

    public static void TryUpdateFileStructure()
    {
      try
      {
        var jsonString = File.ReadAllText(ConfigFile);
        var keysInFile = JObject.Parse(jsonString).Properties().Select(p => p.Name).Count();

        var keysInStruct = typeof(Settings).GetProperties().Length;

        if (keysInFile != keysInStruct)
        {
          Save();
        }
      }
      catch (Exception ex)
      {
        Console.WriteLine(@"failed to update config file structure: " + ex.Message);
      }
    }

    public static void Save()
    {
      try
      {
        var jsonString = JsonConvert.SerializeObject(UserSettings, Formatting.Indented);

        using var sw = File.CreateText(ConfigFile);
        sw.Write(jsonString);

        // prevent triggering of file watcher as this is an internally driven update
        SettingsHash = jsonString.GetHashCode();
      }
      catch (Exception ex)
      {
        Console.WriteLine(@"failed to save config: " + ex.Message);
      }
    }
  }
}
