using System;
using System.Windows.Input;
using GlobalHotKey;

namespace ALsSoundSwitcher
{
  //TODO - make hotkeys and actions generic and configurable.

  public static class KeyWatcher
  {
    private const ModifierKeys HkModifier = ModifierKeys.Alt;
    private const Key HkToggle= Key.OemPeriod;

    public static void Run(Action toggleAction)
    {
      try
      {
        Globals.GlobalHotKeyManager = new HotKeyManager();

        Globals.GlobalHotKeyManager.Register(HkToggle, HkModifier);

        Globals.GlobalHotKeyManager.KeyPressed += HotKeyPressed;

        void HotKeyPressed(object sender, KeyPressedEventArgs e)
        {
          if (e.HotKey.Key == HkToggle)
          {
            toggleAction.Invoke();
          }
        }
      }
      catch (Exception ex)
      {
        // hotkey registration failed (another app may have it) - continue without hotkey
        Console.WriteLine($"Hotkey registration failed: {ex.Message}");
      }
    }
  }
}