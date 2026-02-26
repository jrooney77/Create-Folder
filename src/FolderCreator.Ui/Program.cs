using Avalonia;
using Avalonia.Controls;
using System;
using System.Runtime.InteropServices;

namespace FolderCreator.Ui;

sealed class Program
{
    private const uint ProcessTransformToForegroundApplication = 1;

    [STAThread]
    public static int Main(string[] args)
    {
        try
        {
            PromoteToForegroundProcessOnMacOs();
            BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
            return 0;
        }
        catch (InvalidOperationException ex)
            when (OperatingSystem.IsMacOS() && ex.Message.Contains("RenderTimer", StringComparison.OrdinalIgnoreCase))
        {
            Console.Error.WriteLine("Unable to start Avalonia native RenderTimer on this macOS session.");
            Console.Error.WriteLine("Try running from a logged-in desktop session with a connected display,");
            Console.Error.WriteLine("or run the UI with a .NET/Avalonia version combo known to work on this host.");
            Console.Error.WriteLine($"Details: {ex.Message}");
            return 2;
        }
    }

    public static AppBuilder BuildAvaloniaApp()
    {
        return AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .With(new MacOSPlatformOptions
            {
                ShowInDock = true
            })
            .With(new AvaloniaNativePlatformOptions
            {
                RenderingMode =
                [
                    AvaloniaNativeRenderingMode.Metal,
                    AvaloniaNativeRenderingMode.OpenGl,
                    AvaloniaNativeRenderingMode.Software
                ]
            })
            .WithInterFont()
            .LogToTrace();
    }

    private static void PromoteToForegroundProcessOnMacOs()
    {
        if (!OperatingSystem.IsMacOS())
            return;

        try
        {
            if (GetCurrentProcess(out var psn) == 0)
            {
                TransformProcessType(ref psn, ProcessTransformToForegroundApplication);
                SetFrontProcess(ref psn);
            }
        }
        catch
        {
            // Best-effort: if this fails, continue normal startup.
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct ProcessSerialNumber
    {
        public uint HighLongOfPSN;
        public uint LowLongOfPSN;
    }

    [DllImport("/System/Library/Frameworks/ApplicationServices.framework/ApplicationServices")]
    private static extern int GetCurrentProcess(out ProcessSerialNumber psn);

    [DllImport("/System/Library/Frameworks/ApplicationServices.framework/ApplicationServices")]
    private static extern int TransformProcessType(ref ProcessSerialNumber psn, uint transformState);

    [DllImport("/System/Library/Frameworks/ApplicationServices.framework/ApplicationServices")]
    private static extern int SetFrontProcess(ref ProcessSerialNumber psn);
}
