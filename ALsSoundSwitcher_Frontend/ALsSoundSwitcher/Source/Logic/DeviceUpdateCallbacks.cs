using System;
using System.Linq;
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

      // determine if this is output or input
      var isOutput = flow == DataFlow.Render;
      var isCommsRole = role == Role.Communications;
      
      if (Globals.UserSettings.PreventAutoSwitch)
      {
        if (isOutput)
        {
          string lockedDeviceId;
          string args;

          if (Globals.UserSettings.DualDefault)
          {
            lockedDeviceId = !string.IsNullOrEmpty(Globals.UserSettings.LockedOutputDefaultDeviceId)
              ? Globals.UserSettings.LockedOutputDefaultDeviceId
              : Globals.UserSettings.LockedOutputCommsDeviceId;
            args = lockedDeviceId;
          }
          else
          {
            lockedDeviceId = isCommsRole
              ? Globals.UserSettings.LockedOutputCommsDeviceId
              : Globals.UserSettings.LockedOutputDefaultDeviceId;
            args = lockedDeviceId + (isCommsRole ? " comms" : " default");
          }

          if (!string.IsNullOrEmpty(lockedDeviceId))
          {
            if (lockedDeviceId == deviceId)
            {
              // device is already correct, reset flag and return
              Globals.WeAreSwitching = false;
              return;
            }

            ProcessUtils.RunExe(Globals.SetDeviceExe, args);
            return;
          }
        }
        else
        {
          string lockedDeviceId;

          if (Globals.UserSettings.DualDefault)
          {
            lockedDeviceId = !string.IsNullOrEmpty(Globals.UserSettings.LockedInputDefaultDeviceId)
              ? Globals.UserSettings.LockedInputDefaultDeviceId
              : Globals.UserSettings.LockedInputCommsDeviceId;
          }
          else
          {
            lockedDeviceId = isCommsRole
              ? Globals.UserSettings.LockedInputCommsDeviceId
              : Globals.UserSettings.LockedInputDefaultDeviceId;
          }

          if (!string.IsNullOrEmpty(lockedDeviceId))
          {
            if (lockedDeviceId == deviceId)
            {
              // device is already correct, reset flag and return
              Globals.WeAreSwitching = false;
              return;
            }

            if (Globals.UserSettings.DualDefault)
            {
              PowerShellUtils.SetInputDeviceCmdlet(lockedDeviceId);
            }
            else
            {
              PowerShellUtils.SetInputDeviceCmdlet(
                lockedDeviceId,
                isCommsRole ? PowerShellUtils.InputDeviceRoleSwitch.CommsOnly : PowerShellUtils.InputDeviceRoleSwitch.DefaultOnly
              );
            }
            return;
          }
        }
      }

      if (LastMonitoredDeviceUpdate == deviceId)
      {
        return;
      }

      LastMonitoredDeviceUpdate = deviceId;

      if (Globals.WeAreSwitching)
      {
        Globals.WeAreSwitching = false;
      }
      else
      {
        ProcessUtils.Restart_ThreadSafe();
      }
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