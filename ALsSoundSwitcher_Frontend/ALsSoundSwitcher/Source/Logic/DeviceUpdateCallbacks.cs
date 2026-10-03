using System;
using ALsSoundSwitcher.Properties;
using CSCore.CoreAudioAPI;
using CSCore.Win32;

namespace ALsSoundSwitcher
{
  public partial class Form1 : IMMNotificationClient
  {
    public static string LastMonitoredDeviceUpdate;

    public void OnDeviceStateChanged(string deviceId, DeviceState newState)
    {
      Console.WriteLine(Resources.EndpointNotificationCallback_OnDeviceStateChanged, deviceId, newState);

      if (LastMonitoredDeviceUpdate == deviceId)
      {
        return;
      }
      
      LastMonitoredDeviceUpdate = deviceId;

      ProcessUtils.Restart_ThreadSafe();
    }

    public void OnDeviceAdded(string deviceId)
    {
      Console.WriteLine(Resources.EndpointNotificationCallback_OnDeviceAdded, deviceId);

      ProcessUtils.Restart_ThreadSafe();
    }

    public void OnDeviceRemoved(string deviceId)
    {
      Console.WriteLine(Resources.EndpointNotificationCallback_OnDeviceRemoved, deviceId);
    }

    public void OnDefaultDeviceChanged(DataFlow flow, Role role, string deviceId)
    {
      Console.WriteLine(Resources.EndpointNotificationCallback_OnDefaultDeviceChanged, deviceId);

      if (Globals.ShouldIgnoreDeviceChange())
      {
        return;
      }

      if (Globals.UserSettings.PreventAutoSwitch && TryRestoreLockedDevice(flow, role, deviceId))
      {
        return;
      }

      if (LastMonitoredDeviceUpdate == deviceId)
      {
        return;
      }

      LastMonitoredDeviceUpdate = deviceId;

      if (Globals.WeAreSwitching)
      {
        Globals.WeAreSwitching = false;
        SyncActiveDeviceMenu(flow == DataFlow.Render, deviceId);
        return;
      }

      ProcessUtils.Restart_ThreadSafe();
    }

    private static bool TryRestoreLockedDevice(DataFlow flow, Role role, string deviceId)
    {
      var isOutput = flow == DataFlow.Render;
      var isCommsRole = role == Role.Communications;
      var settings = Globals.UserSettings;

      string lockedDeviceId;
      if (isOutput)
      {
        lockedDeviceId = settings.DualDefault
          ? FirstNonEmpty(settings.LockedOutputDefaultDeviceId, settings.LockedOutputCommsDeviceId)
          : (isCommsRole ? settings.LockedOutputCommsDeviceId : settings.LockedOutputDefaultDeviceId);
      }
      else
      {
        lockedDeviceId = settings.DualDefault
          ? FirstNonEmpty(settings.LockedInputDefaultDeviceId, settings.LockedInputCommsDeviceId)
          : (isCommsRole ? settings.LockedInputCommsDeviceId : settings.LockedInputDefaultDeviceId);
      }

      if (string.IsNullOrEmpty(lockedDeviceId))
      {
        return false;
      }

      SyncActiveDeviceMenu(isOutput, lockedDeviceId);

      if (lockedDeviceId == deviceId)
      {
        return true;
      }

      Globals.BeginOwnedDeviceChange();

      if (isOutput)
      {
        var args = settings.DualDefault
          ? lockedDeviceId
          : lockedDeviceId + (isCommsRole ? " comms" : " default");
        ProcessUtils.RunExe(Globals.SetDeviceExe, args);
        return true;
      }

      if (!PowerShellUtils.AudioCmdletsAreInstalled())
      {
        return true;
      }

      var roleSwitch = settings.DualDefault
        ? PowerShellUtils.InputDeviceRoleSwitch.Both
        : (isCommsRole ? PowerShellUtils.InputDeviceRoleSwitch.CommsOnly : PowerShellUtils.InputDeviceRoleSwitch.DefaultOnly);
      PowerShellUtils.SetInputDeviceCmdlet(lockedDeviceId, roleSwitch);
      return true;
    }

    private static string FirstNonEmpty(string primary, string fallback)
    {
      return !string.IsNullOrEmpty(primary) ? primary : fallback;
    }

    public void OnPropertyValueChanged(string deviceId, PropertyKey propertyKey)
    {
      //Console.WriteLine($"Audio device {deviceId} property {propertyKey} value changed");

      var propString = propertyKey.ToString();
      var cachedActiveOutputDeviceId = Globals.ActiveMenuItemOutputDevice != null 
        ? (string)Globals.ActiveMenuItemOutputDevice.Tag 
        : null;
      
      if (propString == Globals.VolumeChangedPropertyKey)
      {
        // enforce locked volumes
        DeviceUtils.EnforceLockedVolumes();
        
        // update slider and tooltip for active output device
        if (cachedActiveOutputDeviceId != null && deviceId == cachedActiveOutputDeviceId)
        {
          UpdateSliderAndTooltip_Async();
        }
      }
    }
  }
}