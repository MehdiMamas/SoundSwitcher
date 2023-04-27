﻿using CSCore.CoreAudioAPI;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace ALsSoundSwitcher
{
  public static class Globals
  {
    public static Form Instance;

    public const string GithubUrl = "https://github.com/creepyLANguy/SoundSwitcher";

    public const string SetDeviceExe = "SetPlaybackDevice.exe";
    public const string GetVolumeArg = "GetVolume";
    public const string SetVolumeArg = "SetVolume ";
    public const string GetMicLevelArg = "GetMicLevel";
    public const string SetMicLevelArg = "SetMicLevel ";

    public const string VolumeMixerExe = "sndvol.exe";
    public const string VolumeMixerArgs = "-r 88888888";

    public const string DeviceManagerExe = "explorer.exe";
    public const string DeviceManagerArgs = "ms-settings:bluetooth";
    
    public const string ConfigFile = "settings.json";
    
    public const string ThemeFileExtension = ".skin";

    public const string ShowMenusPostThemeRestartFlag = "showMenusPostThemeSaveRestartFlag";

    public static bool WeAreSwitching = false;

    public static Point LastBaseMenuInvokedPosition;

    public static MMDeviceEnumerator DeviceEnumerator = new MMDeviceEnumerator();

    public static Dictionary<string, string> ActiveDevices = new Dictionary<string, string>();

    public static CustomRenderer Theme = new CustomRenderer();

    public static ContextMenuStrip BaseMenu;

    public static ToolStripMenuItem ActiveMenuItemDevice;

    public static ToolStripMenuItem MenuItemMore;
    
    public static ToolStripMenuItem MenuItemCreateTheme;
    
    public static SliderMenuItem MenuItemSlider = new SliderMenuItem();

    public struct MoreMenuItems
    {
      public static ToolStripMenuItem MenuItemExit;
      public static ToolStripMenuItem MenuItemHelp;
      public static ToolStripMenuItem MenuItemRefresh;
      public static ToolStripMenuItem MenuItemMode;
      public static ToolStripMenuItem MenuItemToggleTheme;
      public static ToolStripMenuItem MenuItemMixer;
      public static ToolStripMenuItem MenuItemDeviceManager;
    }
  }
}