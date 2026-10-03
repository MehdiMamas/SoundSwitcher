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

    public static bool TryGetDefaultEndpoint(DataFlow flow, Role role, out MMDevice device)
    {
      device = null;
      try
      {
        device = DeviceEnumerator.GetDefaultAudioEndpoint(flow, role);
        return device != null;
      }
      catch
      {
        return false;
      }
    }

    public static void GetDeviceList()
    {
      ActiveOutputDevices.Clear();
      ActiveInputDevices.Clear();

      var outputInfoList = ReadActiveEndpoints(DataFlow.Render);
      UpdateDuplicates(outputInfoList);
      foreach (var device in outputInfoList)
      {
        ActiveOutputDevices.Add(device.Item1, device.Item2);
      }

      var inputInfoList = ReadActiveEndpoints(DataFlow.Capture);
      UpdateDuplicates(inputInfoList);
      foreach (var device in inputInfoList)
      {
        ActiveInputDevices.Add(device.Item1, device.Item2);
      }
    }

    private static List<Tuple<string, string>> ReadActiveEndpoints(DataFlow flow)
    {
      var deviceInfoList = new List<Tuple<string, string>>();
      var deviceCollection = DeviceEnumerator.EnumAudioEndpoints(flow, DeviceState.Active);
      try
      {
        foreach (var device in deviceCollection)
        {
          deviceInfoList.Add(Tuple.Create(device.FriendlyName, device.DeviceID));
          device.Dispose();
        }
      }
      finally
      {
        deviceCollection.Dispose();
      }

      return deviceInfoList;
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
      return TryGetDeviceLevel(deviceId, out var level) ? level : 0;
    }

    public static bool TryGetDeviceLevel(string deviceId, out int level)
    {
      level = 0;
      try
      {
        var device = DeviceEnumerator.GetDevice(deviceId);
        var volume = AudioEndpointVolume.FromDevice(device);

        level = (int)Math.Round(volume.MasterVolumeLevelScalar * 100);

        volume.Dispose();
        device.Dispose();

        return true;
      }
      catch
      {
        return false;
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

    public static void CaptureCurrentDevicesAsLocks()
    {
      if (string.IsNullOrEmpty(UserSettings.LockedOutputDefaultDeviceId) &&
          TryGetDefaultEndpoint(DataFlow.Render, Role.Multimedia, out var output))
      {
        UserSettings.LockedOutputDefaultDeviceId = output.DeviceID;
        if (UserSettings.DualDefault)
        {
          UserSettings.LockedOutputCommsDeviceId = output.DeviceID;
        }
        output.Dispose();
      }

      if (!UserSettings.DualDefault &&
          string.IsNullOrEmpty(UserSettings.LockedOutputCommsDeviceId) &&
          TryGetDefaultEndpoint(DataFlow.Render, Role.Communications, out var outputComms))
      {
        UserSettings.LockedOutputCommsDeviceId = outputComms.DeviceID;
        outputComms.Dispose();
      }

      if (string.IsNullOrEmpty(UserSettings.LockedInputDefaultDeviceId) &&
          TryGetDefaultEndpoint(DataFlow.Capture, Role.Multimedia, out var input))
      {
        UserSettings.LockedInputDefaultDeviceId = input.DeviceID;
        if (UserSettings.DualDefault)
        {
          UserSettings.LockedInputCommsDeviceId = input.DeviceID;
        }
        input.Dispose();
      }

      if (!UserSettings.DualDefault &&
          string.IsNullOrEmpty(UserSettings.LockedInputCommsDeviceId) &&
          TryGetDefaultEndpoint(DataFlow.Capture, Role.Communications, out var inputComms))
      {
        UserSettings.LockedInputCommsDeviceId = inputComms.DeviceID;
        inputComms.Dispose();
      }
    }

    public static void EnforceLockedDevice()
    {
      if (!UserSettings.PreventAutoSwitch)
      {
        return;
      }

      try
      {
        if (UserSettings.DualDefault)
        {
          RestoreOutputLock(FirstLock(UserSettings.LockedOutputDefaultDeviceId, UserSettings.LockedOutputCommsDeviceId), Role.Multimedia, true);
          RestoreInputLock(
            FirstLock(UserSettings.LockedInputDefaultDeviceId, UserSettings.LockedInputCommsDeviceId),
            Role.Multimedia,
            PowerShellUtils.InputDeviceRoleSwitch.Both);
        }
        else
        {
          RestoreOutputLock(UserSettings.LockedOutputDefaultDeviceId, Role.Multimedia, false);
          RestoreOutputLock(UserSettings.LockedOutputCommsDeviceId, Role.Communications, false);
          RestoreInputLock(
            UserSettings.LockedInputDefaultDeviceId,
            Role.Multimedia,
            PowerShellUtils.InputDeviceRoleSwitch.DefaultOnly);
          RestoreInputLock(
            UserSettings.LockedInputCommsDeviceId,
            Role.Communications,
            PowerShellUtils.InputDeviceRoleSwitch.CommsOnly);
        }
      }
      catch (Exception ex)
      {
        Console.WriteLine(@"device lock timer: " + ex.Message);
      }
    }

    private static string FirstLock(string primary, string fallback)
    {
      return !string.IsNullOrEmpty(primary) ? primary : fallback;
    }

    private static void RestoreOutputLock(string lockedDeviceId, Role role, bool setBothRoles)
    {
      if (string.IsNullOrEmpty(lockedDeviceId))
      {
        return;
      }

      var matches = EndpointMatches(DataFlow.Render, role, lockedDeviceId);
      if (setBothRoles)
      {
        matches = matches && EndpointMatches(DataFlow.Render, Role.Communications, lockedDeviceId);
      }

      if (matches)
      {
        return;
      }

      BeginOwnedDeviceChange();
      var args = setBothRoles ? lockedDeviceId : lockedDeviceId + (role == Role.Communications ? " comms" : " default");
      ProcessUtils.RunExe(SetDeviceExe, args);
    }

    private static void RestoreInputLock(string lockedDeviceId, Role role, PowerShellUtils.InputDeviceRoleSwitch roleSwitch)
    {
      if (string.IsNullOrEmpty(lockedDeviceId))
      {
        return;
      }

      var matches = EndpointMatches(DataFlow.Capture, role, lockedDeviceId);
      if (roleSwitch == PowerShellUtils.InputDeviceRoleSwitch.Both)
      {
        matches = matches && EndpointMatches(DataFlow.Capture, Role.Communications, lockedDeviceId);
      }

      if (matches || !PowerShellUtils.AudioCmdletsAreInstalled())
      {
        return;
      }

      BeginOwnedDeviceChange();
      PowerShellUtils.SetInputDeviceCmdlet(lockedDeviceId, roleSwitch);
    }

    private static bool EndpointMatches(DataFlow flow, Role role, string deviceId)
    {
      if (!TryGetDefaultEndpoint(flow, role, out var current))
      {
        return false;
      }

      var matches = current.DeviceID == deviceId;
      current.Dispose();
      return matches;
    }

    private static bool _enforcingVolumes;

    public static void EnforceLockedVolumes()
    {
      if (_enforcingVolumes || !UserSettings.LockVolume || UserSettings.LockedVolumes == null || UserSettings.LockedVolumes.Count == 0)
      {
        return;
      }

      _enforcingVolumes = true;
      try
      {
        foreach (var kvp in UserSettings.LockedVolumes.ToList())
        {
          var deviceId = kvp.Key;
          var targetLevel = kvp.Value;

          if (!TryGetDeviceLevel(deviceId, out var currentLevel))
          {
            continue;
          }

          if (Math.Abs(currentLevel - targetLevel) >= 2)
          {
            SetDeviceLevel(deviceId, targetLevel);
          }
        }
      }
      finally
      {
        _enforcingVolumes = false;
      }
    }

  }
}