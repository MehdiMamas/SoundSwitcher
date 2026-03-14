using System;
using System.Threading;
using System.Windows.Forms;

namespace ALsSoundSwitcher
{
  static class Program
  {
    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main()
    {
      Application.EnableVisualStyles();
      Application.SetCompatibleTextRenderingDefault(false);

      Application.ThreadException += OnThreadException;
      AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
      Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);

      try
      {
        Application.Run(new Form1());
      }
      catch (Exception ex)
      {
        ShowFatalError(ex);
      }
    }

    private static void OnThreadException(object sender, ThreadExceptionEventArgs e)
    {
      ShowFatalError(e.Exception);
    }

    private static void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
      if (e.ExceptionObject is Exception ex)
      {
        ShowFatalError(ex);
      }
    }

    private static void ShowFatalError(Exception ex)
    {
      var message = "AL's Sound Switcher encountered an error:\n\n" + ex.Message +
                    "\n\nThe application will continue running. " +
                    "If problems persist, try deleting settings.json next to the exe and relaunching.";

      try
      {
        MessageBox.Show(message, @"AL's Sound Switcher", MessageBoxButtons.OK, MessageBoxIcon.Warning);
      }
      catch
      {
        // last resort — nothing we can do
      }
    }
  }
}
