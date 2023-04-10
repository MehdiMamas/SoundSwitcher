﻿using System;
using ALsSoundSwitcher.Properties;
using CSCore.CoreAudioAPI;
using CSCore.Win32;

namespace ALsSoundSwitcher
{
  public class DeviceUpdateCallbacks : IMMNotificationClient
  {
    public static string LastMonitoredDeviceUpdate;

    public void OnDeviceStateChanged(string deviceId, DeviceState newState)
    {
      if (IgnoreThisUpdate(deviceId))
      {
        return;
      }

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
      if (IgnoreThisUpdate(deviceId))
      {
        return;
      }

      Console.WriteLine(Resources.EndpointNotificationCallback_OnDeviceAdded, deviceId);

      ProcessUtils.Restart_ThreadSafe();
    }

    public void OnDeviceRemoved(string deviceId)
    {
      if (IgnoreThisUpdate(deviceId))
      {
        return;
      }

      Console.WriteLine(Resources.EndpointNotificationCallback_OnDeviceRemoved, deviceId);
    }

    public void OnDefaultDeviceChanged(DataFlow flow, Role role, string deviceId)
    {
      if (IgnoreThisUpdate(deviceId))
      {
        return;
      }

      Console.WriteLine(Resources.EndpointNotificationCallback_OnDefaultDeviceChanged, deviceId);

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
    }

    private static bool IgnoreThisUpdate(string deviceId)
    {
      var device = Globals.DeviceEnumerator.GetDevice(deviceId);
      var dataFlow = Settings.Current.Mode == DeviceMode.Output ? DataFlow.Render : DataFlow.Capture;
      return device.DataFlow != dataFlow;
    }
  }
}