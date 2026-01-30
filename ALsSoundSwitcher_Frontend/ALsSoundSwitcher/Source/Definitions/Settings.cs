using Newtonsoft.Json;
using System.Collections.Generic;
using System.ComponentModel;

namespace ALsSoundSwitcher
{
  public struct Settings
  {
    private const int DefaultBalloonTime = 1500;
    private const int DefaultBestNameMatchPercentageMinimum = 15;
    private const string DefaultTheme = "Dark";
    private const string DefaultDefaultIcon = "";
    private const bool DefaultPreventAutoSwitch = false;
    private const bool DefaultLaunchOnStartup = false;
    private const MouseControlFunction DefaultLeftClickFunction = MouseControlFunction.Switch_Next_Device;
    private const MouseControlFunction DefaultMiddleClickFunction = MouseControlFunction.Volume_Mixer;
    private const string DefaultUpgradePollingTime = "0d1h0m0s";
    private const bool DefaultShowKnownIssueCrashMessages = false;
    private const bool DefaultDualDefault = true;
    private const bool DefaultLockVolume = false;
    private const int DefaultLockVolumeLevel = 50;
    private const string DefaultLockedOutputDeviceId = "";
    private const string DefaultLockedInputDeviceId = "";
    private const string DefaultLockedOutputDefaultDeviceId = "";
    private const string DefaultLockedOutputCommsDeviceId = "";
    private const string DefaultLockedInputDefaultDeviceId = "";
    private const string DefaultLockedInputCommsDeviceId = "";

    [DefaultValue(DefaultBalloonTime)]
    [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
    public int BalloonTime { get; set; }

    [DefaultValue(DefaultBestNameMatchPercentageMinimum)]
    [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
    public int BestNameMatchPercentageMinimum { get; set; }

    [DefaultValue(DefaultTheme)]
    [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
    public string Theme { get; set; }

    [DefaultValue(DefaultDefaultIcon)]
    [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
    public string DefaultIcon { get; set; }

    [DefaultValue(DefaultPreventAutoSwitch)]
    [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)] 
    public bool PreventAutoSwitch { get; set; }

    [DefaultValue(DefaultLaunchOnStartup)]
    [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
    public bool LaunchOnStartup { get; set; }

    [DefaultValue(DefaultLeftClickFunction)]
    [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
    public MouseControlFunction LeftClickFunction { get; set; }

    [DefaultValue(DefaultMiddleClickFunction)]
    [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)] 
    public MouseControlFunction MiddleClickFunction { get; set; }

    [DefaultValue(DefaultUpgradePollingTime)]
    [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
    public string UpgradePollingTime { get; set; }

    [DefaultValue(DefaultShowKnownIssueCrashMessages)]
    [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
    public bool ShowKnownIssueCrashMessages { get; set; }

    [DefaultValue(DefaultDualDefault)]
    [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
    public bool DualDefault { get; set; }

    [DefaultValue(DefaultLockVolume)]
    [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
    public bool LockVolume { get; set; }

    [DefaultValue(DefaultLockVolumeLevel)]
    [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
    public int LockVolumeLevel { get; set; }

    // dictionary of deviceId -> locked volume level
    [JsonProperty]
    public Dictionary<string, int> LockedVolumes { get; set; }

    // locked device IDs for PreventAutoSwitch (role-specific)
    [DefaultValue(DefaultLockedOutputDefaultDeviceId)]
    [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
    public string LockedOutputDefaultDeviceId { get; set; }

    [DefaultValue(DefaultLockedOutputCommsDeviceId)]
    [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
    public string LockedOutputCommsDeviceId { get; set; }

    [DefaultValue(DefaultLockedInputDefaultDeviceId)]
    [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
    public string LockedInputDefaultDeviceId { get; set; }

    [DefaultValue(DefaultLockedInputCommsDeviceId)]
    [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
    public string LockedInputCommsDeviceId { get; set; }

    // legacy locked device IDs for PreventAutoSwitch (kept for migration)
    [DefaultValue(DefaultLockedOutputDeviceId)]
    [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
    public string LockedOutputDeviceId { get; set; }

    [DefaultValue(DefaultLockedInputDeviceId)]
    [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
    public string LockedInputDeviceId { get; set; }

    public Settings()
    {
      BalloonTime = DefaultBalloonTime;
      BestNameMatchPercentageMinimum = DefaultBestNameMatchPercentageMinimum;
      Theme = DefaultTheme;
      DefaultIcon = DefaultDefaultIcon;
      PreventAutoSwitch = DefaultPreventAutoSwitch;
      LaunchOnStartup = DefaultLaunchOnStartup;
      LeftClickFunction = DefaultLeftClickFunction;
      MiddleClickFunction = DefaultMiddleClickFunction;
      UpgradePollingTime = DefaultUpgradePollingTime;
      ShowKnownIssueCrashMessages = DefaultShowKnownIssueCrashMessages;
      DualDefault = DefaultDualDefault;
      LockVolume = DefaultLockVolume;
      LockVolumeLevel = DefaultLockVolumeLevel;
      LockedVolumes = new Dictionary<string, int>();
      LockedOutputDefaultDeviceId = DefaultLockedOutputDefaultDeviceId;
      LockedOutputCommsDeviceId = DefaultLockedOutputCommsDeviceId;
      LockedInputDefaultDeviceId = DefaultLockedInputDefaultDeviceId;
      LockedInputCommsDeviceId = DefaultLockedInputCommsDeviceId;
      LockedOutputDeviceId = DefaultLockedOutputDeviceId;
      LockedInputDeviceId = DefaultLockedInputDeviceId;
    }
  }
}
