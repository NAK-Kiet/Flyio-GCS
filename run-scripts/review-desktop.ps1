param([string]$Action = 'capture', [string]$Name = 'main', [int]$Width = 1280, [int]$Height = 800, [int]$X = 0, [int]$Y = 0)
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Windows.Forms,System.Drawing,UIAutomationClient,UIAutomationTypes
Add-Type @'
using System;
using System.Runtime.InteropServices;
public static class FlyioReviewNative {
 [DllImport("user32.dll")] public static extern bool SetProcessDPIAware();
 [DllImport("user32.dll", CharSet=CharSet.Unicode)] public static extern IntPtr FindWindow(string cls, string title);
 public delegate bool EnumProc(IntPtr h, IntPtr p);
 [DllImport("user32.dll")] public static extern bool EnumWindows(EnumProc callback, IntPtr p);
 [DllImport("user32.dll")] public static extern uint GetWindowThreadProcessId(IntPtr h, out uint process);
 [DllImport("user32.dll")] public static extern int GetWindowLong(IntPtr h, int index);
 public static IntPtr MainWindow(int process) {
  IntPtr result = IntPtr.Zero;
  EnumWindows((h,p) => { uint id; GetWindowThreadProcessId(h,out id);
   if (id == process && (GetWindowLong(h,-16) & 0x10000) != 0) result=h;
   return true;
  },IntPtr.Zero);
  return result;
 }
 [DllImport("user32.dll")] public static extern bool SetCursorPos(int x, int y);
 [DllImport("user32.dll")] public static extern void mouse_event(uint flags, uint x, uint y, uint data, UIntPtr extra);
 [DllImport("user32.dll")] public static extern bool SetForegroundWindow(IntPtr h);
 [DllImport("user32.dll")] public static extern bool ShowWindow(IntPtr h, int n);
 [DllImport("user32.dll")] public static extern bool MoveWindow(IntPtr h, int x, int y, int w, int height, bool repaint);
 [DllImport("user32.dll")] public static extern bool GetWindowRect(IntPtr h, out Rect r);
 [DllImport("user32.dll")] public static extern bool PrintWindow(IntPtr h, IntPtr dc, uint flags);
 public struct Rect { public int Left, Top, Right, Bottom; }
}
'@
[FlyioReviewNative]::SetProcessDPIAware() | Out-Null
$app = Get-Process -Name 'Fly.io-GCS' | Select-Object -First 1
$handle = [FlyioReviewNative]::MainWindow($app.Id)
if ($handle -eq [IntPtr]::Zero) { $handle = $app.MainWindowHandle }
$root = [System.Windows.Automation.AutomationElement]::FromHandle($handle)
if ($Action -eq 'list') {
 $root.FindAll([System.Windows.Automation.TreeScope]::Descendants,[System.Windows.Automation.Condition]::TrueCondition) | ForEach-Object {
  if ($_.Current.Name) { '{0} | {1} | {2}' -f $_.Current.ControlType.ProgrammaticName,$_.Current.Name,$_.Current.BoundingRectangle }
 }
 exit
}
[FlyioReviewNative]::ShowWindow($handle,9) | Out-Null
[FlyioReviewNative]::SetForegroundWindow($handle) | Out-Null
if ($Action -eq 'resize') { [FlyioReviewNative]::MoveWindow($handle,20,20,$Width,$Height,$true) | Out-Null }
if ($Action -eq 'point') {
 $position = New-Object FlyioReviewNative+Rect
 [FlyioReviewNative]::GetWindowRect($handle,[ref]$position) | Out-Null
 [FlyioReviewNative]::SetCursorPos(($position.Left+$X),($position.Top+$Y)) | Out-Null
 [FlyioReviewNative]::mouse_event(2,0,0,0,[UIntPtr]::Zero)
 [FlyioReviewNative]::mouse_event(4,0,0,0,[UIntPtr]::Zero)
}
if ($Action -eq 'click') {
 $condition = New-Object System.Windows.Automation.PropertyCondition([System.Windows.Automation.AutomationElement]::NameProperty,$Name)
 $element = $root.FindFirst([System.Windows.Automation.TreeScope]::Descendants,$condition)
 if (!$element) { throw "Native control not found: $Name" }
 $pattern = $element.GetCurrentPattern([System.Windows.Automation.InvokePattern]::Pattern)
 $pattern.Invoke()
}
Start-Sleep -Seconds 2
$rect = New-Object FlyioReviewNative+Rect
[FlyioReviewNative]::GetWindowRect($handle,[ref]$rect) | Out-Null
$bitmap = New-Object System.Drawing.Bitmap ($rect.Right-$rect.Left),($rect.Bottom-$rect.Top)
$graphics = [System.Drawing.Graphics]::FromImage($bitmap)
$graphics.CopyFromScreen($rect.Left,$rect.Top,0,0,$bitmap.Size)
$folder = Join-Path $PSScriptRoot '../artifacts/visual-review'
$safeName = $Name -replace '[^a-zA-Z0-9-]', '-'
$bitmap.Save((Join-Path $folder "$safeName.png"))
$graphics.Dispose(); $bitmap.Dispose()
Write-Output "Captured $safeName.png; window: $($app.MainWindowTitle)"
