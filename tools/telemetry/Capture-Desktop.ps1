param([ValidateSet('Fly.io-GCS','QGroundControl')][string]$ProcessName = 'Fly.io-GCS', [string]$OutputPath = 'C:\Flyio-GCS\desktop-validation.png')
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Windows.Forms
Add-Type -AssemblyName System.Drawing
Add-Type @'
using System;
using System.Runtime.InteropServices;
public static class FlyioWindowCapture {
 [DllImport("user32.dll")] public static extern bool SetProcessDPIAware();
 [DllImport("user32.dll")] public static extern IntPtr GetForegroundWindow();
 [DllImport("user32.dll")] public static extern bool SetForegroundWindow(IntPtr hWnd);
 [DllImport("user32.dll")] public static extern bool ShowWindow(IntPtr hWnd, int command);
}
'@
[FlyioWindowCapture]::SetProcessDPIAware() | Out-Null
$appProcess = Get-Process -Name $ProcessName | Select-Object -First 1
if ($appProcess.MainWindowHandle -eq 0) { throw 'Application has no visible window.' }
[FlyioWindowCapture]::ShowWindow($appProcess.MainWindowHandle, 3) | Out-Null
[FlyioWindowCapture]::SetForegroundWindow($appProcess.MainWindowHandle) | Out-Null
Start-Sleep -Milliseconds 700
if ([FlyioWindowCapture]::GetForegroundWindow() -ne $appProcess.MainWindowHandle) { throw 'Foreground verification failed; no screenshot saved.' }
$bounds = [Windows.Forms.SystemInformation]::VirtualScreen
$capture = New-Object Drawing.Bitmap $bounds.Width,$bounds.Height
$graphics = [Drawing.Graphics]::FromImage($capture)
try {
 $graphics.CopyFromScreen($bounds.Location,[Drawing.Point]::Empty,$bounds.Size)
 $capture.Save($OutputPath)
} finally { $graphics.Dispose(); $capture.Dispose() }
