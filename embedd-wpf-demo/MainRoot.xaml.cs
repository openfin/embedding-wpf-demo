using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace embedd_wpf_demo
{
    /// <summary>
    /// Interaction logic for MainRoot.xaml
    /// </summary>
    public partial class MainRoot : Window
    {
        System.Windows.Threading.DispatcherTimer ownerTimer = new System.Windows.Threading.DispatcherTimer();

        Window win1, win2;
        MainWindow ofWindow;
        HwndSource mainRootHwndSource, win1HwndSource, win2HwndSource, ofHwndSource;
        Dictionary<IntPtr, Window> wins;

        public MainRoot()
        {
            InitializeComponent();
            this.Width = 500;
            this.Height = 650;

            this.Loaded += (sender, args) =>
            {
                var mainRootHwnd = new WindowInteropHelper(this).Handle; 
                /*
                var normalWindowOwner = new Window() { Owner = this };
                new System.Windows.Interop.WindowInteropHelper(normalWindowOwner).EnsureHandle();

                var openfinWindowOwner = new Window() { Owner = this };
                new System.Windows.Interop.WindowInteropHelper(openfinWindowOwner).EnsureHandle();
                */
                win1 = new Window()
                {
                    Title = "Empty 1",
                    Width = 300,
                    Height = 200,
                    Top = this.Top,
                    Left = this.Left + 400,
                    Owner = this
                };

                win1.Show();

                var win1Hwnd = new WindowInteropHelper(win1).Handle;

                win2 = new Window()
                {
                    Title = "Empty 2",
                    Width = 300,
                    Height = 200,
                    Top = this.Top + 150,
                    Left = this.Left + 400,
                    Owner = this
                };

                win2.Show();

                var win2Hwnd = new WindowInteropHelper(win2).Handle;

                ofWindow = new MainWindow()
                {
                    Title = "Embed Host",
                    Width = 300,
                    Height = 200,
                    Top = this.Top + 300,
                    Left = this.Left + 400,
                    Owner = this
                };

                ofWindow.Show();

                var ofHwnd = new WindowInteropHelper(ofWindow).Handle;



                ofWindow.Runtime.Connect(() =>
                {
                    Dispatcher.Invoke(new Action(() =>
                    {
                        var app = ofWindow.Runtime.CreateApplication(new Openfin.Desktop.ApplicationOptions("Plain OpenFin", "openfin-standalone", ofWindow.Url)
                        {
                            MainWindowOptions = new Openfin.Desktop.WindowOptions("openfin-standalone")
                            {
                                URL = ofWindow.Url,
                                AutoShow = true,
                                DefaultWidth = 300,
                                DefaultHeight = 200,
                                DefaultTop = (int)this.Top + 450,
                                DefaultLeft = (int)this.Left + 400
                            }
                        });

                        app.Run(() => 
                        {
                            app.getWindow().getNativeId(ack =>
                            {
                                var hexStr = ack.getJsonObject().Value<string>("data");
                                var hwnd = (IntPtr)uint.Parse(hexStr.Substring(2), System.Globalization.NumberStyles.HexNumber);
                                var handle = new HandleRef(null, hwnd);


                                SetWindowLongPtr(handle, (int)WindowLongFlags.GWLP_HWNDPARENT, mainRootHwnd);
                            });
                        });
                    }), null);
                });

                wins = new Dictionary<IntPtr, Window>()
                {
                    { IntPtr.Zero, null },
                    { mainRootHwnd, this },
                    { win1Hwnd, win1 },
                    { win2Hwnd, win2 },
                    { ofHwnd, ofWindow }
                };

                var hwndSources = Application.Current.Windows
                    .Cast<Window>()
                    .Select(w => new WindowInteropHelper(w).Handle) // If handle has not been created, EnsureHandle should be called first
                    .Select(h => HwndSource.FromHwnd(h))        
                    .ToList(); // Maybe have to keep these around to dispose later?
                /*
                hwndSources
                    .ForEach(s => s.AddHook(new HwndSourceHook(WndProc)));
                    */
                
                mainRootHwndSource = HwndSource.FromHwnd(mainRootHwnd);
                mainRootHwndSource.AddHook(new HwndSourceHook(logWinProc));
                win1HwndSource = HwndSource.FromHwnd(win1Hwnd);
                win1HwndSource.AddHook(new HwndSourceHook(logWinProc));
                win2HwndSource = HwndSource.FromHwnd(win2Hwnd);
                win2HwndSource.AddHook(new HwndSourceHook(logWinProc));
                
                ofHwndSource = HwndSource.FromHwnd(ofHwnd);
                ofHwndSource.AddHook(new HwndSourceHook(logWinProc));
                
                ownerTimer.Interval = TimeSpan.FromMilliseconds(500);
                ownerTimer.Tick += (s, e) =>
                {
                    //var sortedWindows = SortWindowsTopToBottom(Application.Current.Windows.Cast<Window>());
                    //MainRootText.Text = string.Join(Environment.NewLine, sortedWindows.Select(w => w.Title));


                    var sortedWindows = FindWindowsInGlobalZOrder(Application.Current.Windows.Cast<Window>());

                    MainRootText.Items.Clear();

                    foreach (var window in sortedWindows)
                    {
                        var item = new ListViewItem()
                        {
                            Content = window,
                            Background = window.Contains("openfin") ? new SolidColorBrush(Colors.Salmon) : new SolidColorBrush(Colors.LightBlue),
                            FontWeight = window.Contains("IME") ? FontWeights.Bold : FontWeights.Normal
                        };
                        MainRootText.Items.Add(item);
                    }

                    //MainRootText.Text = string.Join(Environment.NewLine, sortedWindows);
                };
                ownerTimer.Start();
            };
        }

        private IntPtr _hwndToInsertAfter = IntPtr.Zero;

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            var message = (WM)msg;

            switch (message)
            {
                case WM.WINDOWPOSCHANGING:
                    var windowPos = (WindowPos)Marshal.PtrToStructure(lParam, typeof(WindowPos));
                    var moveFlags = (SetWindowPosFlags)windowPos.flags;

                    switch (moveFlags)
                    {
                        // Flags for window which is about to activate
                        case (SetWindowPosFlags.IgnoreResize | SetWindowPosFlags.IgnoreMove):
                            _hwndToInsertAfter = windowPos.hwnd;
                            break;
                        // Flags for remaining windows - these messages are received in the correct order
                        // but sometimes their payloads are incorrect, resulting in an invalid Z-ordering
                        case (SetWindowPosFlags.IgnoreResize | SetWindowPosFlags.IgnoreMove | SetWindowPosFlags.DoNotActivate):
                            if (windowPos.hwndInsertAfter != _hwndToInsertAfter)
                            {
                                windowPos.hwndInsertAfter = _hwndToInsertAfter;
                                Marshal.StructureToPtr(windowPos, lParam, true);
                            }
                            _hwndToInsertAfter = windowPos.hwnd;
                            break;
                    }

                    break;
            }
               
            return IntPtr.Zero;
        }

        private IntPtr logWinProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            try
            {
                var message = (WM)msg;
                var windowTitle = wins[hwnd].Title;            

                switch (message)
                {
                    case WM.WINDOWPOSCHANGING:
                    case WM.WINDOWPOSCHANGED:
                        var windowPos = (WindowPos)Marshal.PtrToStructure(lParam, typeof(WindowPos));
                        var moveFlags = (SetWindowPosFlags)windowPos.flags;

                        var windowPosTitle = new StringBuilder();
                        var afterTitle = new StringBuilder();
                        GetWindowText(windowPos.hwnd, windowPosTitle, 1024);
                        GetWindowText(windowPos.hwndInsertAfter, afterTitle, 1024);

                        Debug.WriteLine($"{windowTitle}, {message} {wParam.ToString("X8")} [{afterTitle} ({windowPos.hwndInsertAfter}) / {windowPosTitle} ({windowPos.hwnd}) [{((SetWindowPosFlags)windowPos.flags).ToString()}] {(handled ? "!" : "")}]");
                        break;
                    default:
                        Debug.WriteLine($"{windowTitle}, {message} {wParam.ToString("X8")} {lParam.ToString("X8")}");
                        break;
                }
            }
            catch
            {
                Debug.WriteLine("Error in WinProc");
            }
            return IntPtr.Zero;
        }

        public static IEnumerable<Window> SortWindowsTopToBottom(IEnumerable<Window> unsorted)
        {
            var byHandle = unsorted.ToDictionary(win =>
              ((HwndSource)PresentationSource.FromVisual(win)).Handle);

            for (IntPtr hWnd = GetTopWindow(IntPtr.Zero); hWnd != IntPtr.Zero; hWnd = GetWindow(hWnd, GW_HWNDNEXT))
              if (byHandle.ContainsKey(hWnd))
                    yield return byHandle[hWnd];
        }

        public static IEnumerable<string> FindWindowsInGlobalZOrder(IEnumerable<Window> windows)
        {
            var byHandle = windows.ToDictionary(win =>
                ((HwndSource)PresentationSource.FromVisual(win)).Handle);
            var processes = Process.GetProcesses().ToDictionary(proc => (uint)proc.Id, proc => proc.ProcessName);
            var currentPid = Process.GetCurrentProcess().Id;

            var title = new StringBuilder(1024);
            uint pid;

            for (var hWnd = GetTopWindow(IntPtr.Zero); hWnd != IntPtr.Zero; hWnd = GetWindow(hWnd, GW_HWNDNEXT))
            {      
                GetWindowThreadProcessId(hWnd, out pid);
                var processName = processes[pid];

                if (!(pid == currentPid || processName == "openfin"))
                {
                    continue;
                }

                title.Clear();

                try
                {
                    GetWindowText(hWnd, title, title.Capacity);

                    if (title.Length == 0)
                        continue;
                }
                catch
                {
                    continue;
                }

                yield return $"[{hWnd.ToString("X8")}] {title} ({processName})";
            }
        }

        const uint GW_HWNDNEXT = 2;
        [DllImport("User32")]
        static extern IntPtr GetTopWindow(IntPtr hWnd);
        [DllImport("User32")]
        static extern IntPtr GetWindow(IntPtr hWnd, uint wCmd);
        [DllImport("User32", CharSet = CharSet.Auto, SetLastError = true)]
        static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);
        [DllImport("user32.dll", SetLastError = true)]
        static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint processId);

        // This static method is required because legacy OSes do not support
        // SetWindowLongPtr 
        public static IntPtr SetWindowLongPtr(HandleRef hWnd, int nIndex, IntPtr dwNewLong)
        {
            if (IntPtr.Size == 8)
                return SetWindowLongPtr64(hWnd, nIndex, dwNewLong);
            else
                return new IntPtr(SetWindowLong32(hWnd, nIndex, dwNewLong.ToInt32()));
        }

        [DllImport("user32.dll", EntryPoint = "SetWindowLong")]
        private static extern int SetWindowLong32(HandleRef hWnd, int nIndex, int dwNewLong);

        [DllImport("user32.dll", EntryPoint = "SetWindowLongPtr")]
        private static extern IntPtr SetWindowLongPtr64(HandleRef hWnd, int nIndex, IntPtr dwNewLong);

        public struct WindowPos
        {

            public IntPtr hwnd;

            public IntPtr hwndInsertAfter;

            public int x;

            public int y;

            public int cx;

            public int cy;

            public uint flags;

        };

        [Flags]
        public enum SetWindowPosFlags : uint
        {
            /// <summary>If the calling thread and the thread that owns the window are attached to different input queues, 
            /// the system posts the request to the thread that owns the window. This prevents the calling thread from 
            /// blocking its execution while other threads process the request.</summary>
            /// <remarks>SWP_ASYNCWINDOWPOS</remarks>
            SynchronousWindowPosition = 0x4000,
            /// <summary>Prevents generation of the WM_SYNCPAINT message.</summary>
            /// <remarks>SWP_DEFERERASE</remarks>
            DeferErase = 0x2000,
            /// <summary>Draws a frame (defined in the window's class description) around the window.</summary>
            /// <remarks>SWP_DRAWFRAME</remarks>
            DrawFrame = 0x0020,
            /// <summary>Applies new frame styles set using the SetWindowLong function. Sends a WM_NCCALCSIZE message to 
            /// the window, even if the window's size is not being changed. If this flag is not specified, WM_NCCALCSIZE 
            /// is sent only when the window's size is being changed.</summary>
            /// <remarks>SWP_FRAMECHANGED</remarks>
            FrameChanged = 0x0020,
            /// <summary>Hides the window.</summary>
            /// <remarks>SWP_HIDEWINDOW</remarks>
            HideWindow = 0x0080,
            /// <summary>Does not activate the window. If this flag is not set, the window is activated and moved to the 
            /// top of either the topmost or non-topmost group (depending on the setting of the hWndInsertAfter 
            /// parameter).</summary>
            /// <remarks>SWP_NOACTIVATE</remarks>
            DoNotActivate = 0x0010,
            /// <summary>Discards the entire contents of the client area. If this flag is not specified, the valid 
            /// contents of the client area are saved and copied back into the client area after the window is sized or 
            /// repositioned.</summary>
            /// <remarks>SWP_NOCOPYBITS</remarks>
            DoNotCopyBits = 0x0100,
            /// <summary>Retains the current position (ignores X and Y parameters).</summary>
            /// <remarks>SWP_NOMOVE</remarks>
            IgnoreMove = 0x0002,
            /// <summary>Does not change the owner window's position in the Z order.</summary>
            /// <remarks>SWP_NOOWNERZORDER</remarks>
            DoNotChangeOwnerZOrder = 0x0200,
            /// <summary>Does not redraw changes. If this flag is set, no repainting of any kind occurs. This applies to 
            /// the client area, the nonclient area (including the title bar and scroll bars), and any part of the parent 
            /// window uncovered as a result of the window being moved. When this flag is set, the application must 
            /// explicitly invalidate or redraw any parts of the window and parent window that need redrawing.</summary>
            /// <remarks>SWP_NOREDRAW</remarks>
            DoNotRedraw = 0x0008,
            /// <summary>Same as the SWP_NOOWNERZORDER flag.</summary>
            /// <remarks>SWP_NOREPOSITION</remarks>
            DoNotReposition = 0x0200,
            /// <summary>Prevents the window from receiving the WM_WINDOWPOSCHANGING message.</summary>
            /// <remarks>SWP_NOSENDCHANGING</remarks>
            DoNotSendChangingEvent = 0x0400,
            /// <summary>Retains the current size (ignores the cx and cy parameters).</summary>
            /// <remarks>SWP_NOSIZE</remarks>
            IgnoreResize = 0x0001,
            /// <summary>Retains the current Z order (ignores the hWndInsertAfter parameter).</summary>
            /// <remarks>SWP_NOZORDER</remarks>
            IgnoreZOrder = 0x0004,
            /// <summary>Displays the window.</summary>
            /// <remarks>SWP_SHOWWINDOW</remarks>
            ShowWindow = 0x0040,
        }

        enum WindowLongFlags : int
        {
            GWL_EXSTYLE = -20,
            GWLP_HINSTANCE = -6,
            GWLP_HWNDPARENT = -8,
            GWL_ID = -12,
            GWL_STYLE = -16,
            GWL_USERDATA = -21,
            GWL_WNDPROC = -4,
            DWLP_USER = 0x8,
            DWLP_MSGRESULT = 0x0,
            DWLP_DLGPROC = 0x4
        }
    }
}
