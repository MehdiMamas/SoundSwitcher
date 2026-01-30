using System;
using System.Collections.Generic;
using System.Linq;
using CSCore.CoreAudioAPI;
using ALsSoundSwitcher.Properties;
using static ALsSoundSwitcher.Globals;

namespace ALsSoundSwitcher
{
  public static class DeviceUtils
  {
    public static void Monitor()
    {
      DeviceEnumerator.RegisterEndpointNotificationCallbackNative((Form1)Instance);

      Console.WriteLine(Resources.DeviceUtils_Monitor);
    }
	
    public static MMDevice GetCurrentDefaultOutputDevice()
    {
      return DeviceEnumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
    }

    public static MMDevice GetCurrentDefaultInputDevice()
    {
      return DeviceEnumerator.GetDefaultAudioEndpoint(DataFlow.Capture, Role.Multimedia);
    }

    public static void GetDeviceList()
    {
      ActiveOutputDevices.Clear();
      ActiveInputDevices.Clear();

      // get output devices
      var outputCollection = DeviceEnumerator.EnumAudioEndpoints(DataFlow.Render, DeviceState.Active);
      var outputInfoList = outputCollection.Select(device => Tuple.Create(device.FriendlyName, device.DeviceID)).ToList();
      UpdateDuplicates(outputInfoList);
      foreach (var device in outputInfoList)
      {
        ActiveOutputDevices.Add(device.Item1, device.Item2);
      }

      // get input devices
      var inputCollection = DeviceEnumerator.EnumAudioEndpoints(DataFlow.Capture, DeviceState.Active);
      var inputInfoList = inputCollection.Select(device => Tuple.Create(device.FriendlyName, device.DeviceID)).ToList();
      UpdateDuplicates(inputInfoList);
      foreach (var device in inputInfoList)
      {
        ActiveInputDevices.Add(device.Item1, device.Item2);
      }
    }

    private static void UpdateDuplicates(List<Tuple<string, string>> deviceInfoList)
    {
      var duplicates = new HashSet<string>();

      for (var i = 0; i < deviceInfoList.Count; i++)
      {
        for (var j = i + 1; j < deviceInfoList.Count; j++)
        {
          if (deviceInfoList[i].Item1 == deviceInfoList[j].Item1)
          {
            duplicates.Add(deviceInfoList[i].Item1);
          }
        }
      }

      foreach (var duplicate in duplicates)
      {
        var count = 1;
        for (var k = 0; k < deviceInfoList.Count; k++)
        {
          if (duplicate != deviceInfoList[k].Item1)
          {
            continue;
          }

          var s = deviceInfoList[k].Item1;
          var label = s.Substring(0, s.IndexOf("(", StringComparison.Ordinal));
          var newName = "(" + label + " " + count + ")";
          deviceInfoList[k] = Tuple.Create(newName, deviceInfoList[k].Item2);
          count++;
        }
      }
    }

    public static int GetVolume()
    {
      var volume = ProcessUtils.RunExe(SetDeviceExe, GetVolumeArg);
      return volume;
    }

    public static int GetMicLevel()
    {
      var level = ProcessUtils.RunExe(SetDeviceExe, GetMicLevelArg);
      return level;
    }

    public static int GetDeviceLevel(string deviceId)
    {
      try
      {
        var device = DeviceEnumerator.GetDevice(deviceId);
        var volume = AudioEndpointVolume.FromDevice(device);

        var level = (int)Math.Round(volume.MasterVolumeLevelScalar * 100);

        volume.Dispose();
        device.Dispose();

        return level;
      }
      catch
      {
        return 0;
      }
    }

    public static void SetDeviceLevel(string deviceId, int level)
    {
      try
      {
        var device = DeviceEnumerator.GetDevice(deviceId);
        var volume = AudioEndpointVolume.FromDevice(device);

        var scalar = Math.Min(100, Math.Max(0, level)) / 100f;
        volume.SetMasterVolumeLevelScalar(scalar, Guid.Empty);

        volume.Dispose();
        device.Dispose();
      }
      catch
      {
        // ignore
      }
    }

    public static void EnforceLockedVolumes()
    {
      if (!UserSettings.LockVolume || UserSettings.LockedVolumes == null || UserSettings.LockedVolumes.Count == 0)
      {
        return;
      }

      foreach (var kvp in UserSettings.LockedVolumes.ToList())
      {
        var deviceId = kvp.Key;
        var targetLevel = kvp.Value;

        var currentLevel = GetDeviceLevel(deviceId);
        if (Math.Abs(currentLevel - targetLevel) >= 2)
        {
          SetDeviceLevel(deviceId, targetLevel);
        }
      }
    }

  }
}