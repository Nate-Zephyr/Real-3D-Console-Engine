using System;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;
using System.Linq.Expressions;
using System.Linq;

namespace VolumeRayCasting
{
    static class SetScreenColorsApp // Кастомные цвета в консоли
    {
        [StructLayout(LayoutKind.Sequential)]
        internal struct COORD
        {
            internal short X;
            internal short Y;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct SMALL_RECT
        {
            internal short Left;
            internal short Top;
            internal short Right;
            internal short Bottom;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct COLORREF
        {
            internal uint ColorDWORD;

            internal COLORREF(Color color)
            {
                ColorDWORD = (uint)color.R + (((uint)color.G) << 8) + (((uint)color.B) << 16);
            }

            internal COLORREF(uint r, uint g, uint b)
            {
                ColorDWORD = r + (g << 8) + (b << 16);
            }

            internal Color GetColor()
            {
                return Color.FromArgb((int)(0x000000FFU & ColorDWORD),
                                      (int)(0x0000FF00U & ColorDWORD) >> 8, (int)(0x00FF0000U & ColorDWORD) >> 16);
            }

            internal void SetColor(Color color)
            {
                ColorDWORD = (uint)color.R + (((uint)color.G) << 8) + (((uint)color.B) << 16);
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct CONSOLE_SCREEN_BUFFER_INFO_EX
        {
            internal int cbSize;
            internal COORD dwSize;
            internal COORD dwCursorPosition;
            internal ushort wAttributes;
            internal SMALL_RECT srWindow;
            internal COORD dwMaximumWindowSize;
            internal ushort wPopupAttributes;
            internal bool bFullscreenSupported;
            internal COLORREF black;
            internal COLORREF darkBlue;
            internal COLORREF darkGreen;
            internal COLORREF darkCyan;
            internal COLORREF darkRed;
            internal COLORREF darkMagenta;
            internal COLORREF darkYellow;
            internal COLORREF gray;
            internal COLORREF darkGray;
            internal COLORREF blue;
            internal COLORREF green;
            internal COLORREF cyan;
            internal COLORREF red;
            internal COLORREF magenta;
            internal COLORREF yellow;
            internal COLORREF white;
        }

        const int STD_OUTPUT_HANDLE = -11;                                        // per WinBase.h
        internal static readonly IntPtr INVALID_HANDLE_VALUE = new IntPtr(-1);    // per WinBase.h

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr GetStdHandle(int nStdHandle);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool GetConsoleScreenBufferInfoEx(IntPtr hConsoleOutput, ref CONSOLE_SCREEN_BUFFER_INFO_EX csbe);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool SetConsoleScreenBufferInfoEx(IntPtr hConsoleOutput, ref CONSOLE_SCREEN_BUFFER_INFO_EX csbe);

        // Set a specific console color to an RGB color
        // The default console colors used are gray (foreground) and black (background)
        public static int SetColor(ConsoleColor consoleColor, Color targetColor)
        {
            return SetColor(consoleColor, targetColor.R, targetColor.G, targetColor.B);
        }

        public static int SetColor(ConsoleColor color, uint r, uint g, uint b)
        {
            CONSOLE_SCREEN_BUFFER_INFO_EX csbe = new CONSOLE_SCREEN_BUFFER_INFO_EX();
            csbe.cbSize = (int)Marshal.SizeOf(csbe);                    // 96 = 0x60
            IntPtr hConsoleOutput = GetStdHandle(STD_OUTPUT_HANDLE);    // 7
            if (hConsoleOutput == INVALID_HANDLE_VALUE)
            {
                return Marshal.GetLastWin32Error();
            }
            bool brc = GetConsoleScreenBufferInfoEx(hConsoleOutput, ref csbe);
            if (!brc)
            {
                return Marshal.GetLastWin32Error();
            }

            switch (color)
            {
                case ConsoleColor.Black:
                    csbe.black = new COLORREF(r, g, b);
                    break;
                case ConsoleColor.DarkBlue:
                    csbe.darkBlue = new COLORREF(r, g, b);
                    break;
                case ConsoleColor.DarkGreen:
                    csbe.darkGreen = new COLORREF(r, g, b);
                    break;
                case ConsoleColor.DarkCyan:
                    csbe.darkCyan = new COLORREF(r, g, b);
                    break;
                case ConsoleColor.DarkRed:
                    csbe.darkRed = new COLORREF(r, g, b);
                    break;
                case ConsoleColor.DarkMagenta:
                    csbe.darkMagenta = new COLORREF(r, g, b);
                    break;
                case ConsoleColor.DarkYellow:
                    csbe.darkYellow = new COLORREF(r, g, b);
                    break;
                case ConsoleColor.Gray:
                    csbe.gray = new COLORREF(r, g, b);
                    break;
                case ConsoleColor.DarkGray:
                    csbe.darkGray = new COLORREF(r, g, b);
                    break;
                case ConsoleColor.Blue:
                    csbe.blue = new COLORREF(r, g, b);
                    break;
                case ConsoleColor.Green:
                    csbe.green = new COLORREF(r, g, b);
                    break;
                case ConsoleColor.Cyan:
                    csbe.cyan = new COLORREF(r, g, b);
                    break;
                case ConsoleColor.Red:
                    csbe.red = new COLORREF(r, g, b);
                    break;
                case ConsoleColor.Magenta:
                    csbe.magenta = new COLORREF(r, g, b);
                    break;
                case ConsoleColor.Yellow:
                    csbe.yellow = new COLORREF(r, g, b);
                    break;
                case ConsoleColor.White:
                    csbe.white = new COLORREF(r, g, b);
                    break;
            }
            ++csbe.srWindow.Bottom;
            ++csbe.srWindow.Right;
            brc = SetConsoleScreenBufferInfoEx(hConsoleOutput, ref csbe);
            if (!brc)
            {
                return Marshal.GetLastWin32Error();
            }
            return 0;
        }

        public static int SetScreenColors(Color foregroundColor, Color backgroundColor)
        {
            int irc;
            irc = SetColor(ConsoleColor.Gray, foregroundColor);
            if (irc != 0) return irc;
            irc = SetColor(ConsoleColor.Black, backgroundColor);
            if (irc != 0) return irc;

            return 0;
        }
    }
    static class DisableConsoleQuickEdit // отключение редактирования (выделения) ЛКМ
    {
        const uint ENABLE_QUICK_EDIT = 0x0040;

        const int STD_INPUT_HANDLE = -10;

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern IntPtr GetStdHandle(int nStdHandle);
        [DllImport("kernel32.dll")]
        static extern bool GetConsoleMode(IntPtr hConsoleHandle, out uint lpMode);
        [DllImport("kernel32.dll")]
        static extern bool SetConsoleMode(IntPtr hConsoleHandle, uint dwMode);
        internal static bool Go()
        {
            IntPtr consoleHandle = GetStdHandle(STD_INPUT_HANDLE);

            uint consoleMode;
            if (!GetConsoleMode(consoleHandle, out consoleMode))
                return false;

            consoleMode &= ~ENABLE_QUICK_EDIT;

            if (!SetConsoleMode(consoleHandle, consoleMode))
                return false;

            return true;
        }
    }
    static class Game
    {
        #region Подключение библиотек

        //Доступ к буферу консоли
        [DllImport("Kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        static extern SafeFileHandle CreateFile(
            string fileName,
            [MarshalAs(UnmanagedType.U4)] uint fileAccess,
            [MarshalAs(UnmanagedType.U4)] uint fileShare,
            IntPtr securityAttributes,
            [MarshalAs(UnmanagedType.U4)] FileMode creationDisposition,
            [MarshalAs(UnmanagedType.U4)] int flags,
            IntPtr template);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool WriteConsoleOutput(
          SafeFileHandle hConsoleOutput,
          CharInfo[] lpBuffer,
          Coord dwBufferSize,
          Coord dwBufferCoord,
          ref SmallRect lpWriteRegion);

        [StructLayout(LayoutKind.Sequential)]
        public struct Coord
        {
            public short X;
            public short Y;

            public Coord(short X, short Y)
            {
                this.X = X;
                this.Y = Y;
            }
        };

        [StructLayout(LayoutKind.Explicit)]
        public struct CharUnion
        {
            [FieldOffset(0)] public char UnicodeChar;
            [FieldOffset(0)] public byte AsciiChar;
        }

        [StructLayout(LayoutKind.Explicit)]
        public struct CharInfo
        {
            [FieldOffset(0)] public CharUnion Char;
            [FieldOffset(2)] public short Attributes;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct SmallRect
        {
            public short Left;
            public short Top;
            public short Right;
            public short Bottom;
        }

        // Максимизация консоли
        [DllImport("kernel32.dll", ExactSpelling = true)]
        private static extern IntPtr GetConsoleWindow();
        private static IntPtr ThisConsole = GetConsoleWindow();
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        // Выбор шрифтра консоли
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        internal unsafe struct CONSOLE_FONT_INFO_EX
        {
            internal uint cbSize;
            internal uint nFont;
            internal COORD dwFontSize;
            internal int FontFamily;
            internal int FontWeight;
            internal fixed char FaceName[LF_FACESIZE];
        }
        [StructLayout(LayoutKind.Sequential)]
        internal struct COORD
        {
            internal short X;
            internal short Y;
            internal COORD(short x, short y)
            {
                X = x;
                Y = y;
            }
        }
        private const int STD_OUTPUT_HANDLE = -11;
        private const int TMPF_TRUETYPE = 4;
        private const int LF_FACESIZE = 32;
        private static IntPtr INVALID_HANDLE_VALUE = new IntPtr(-1);
        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool SetCurrentConsoleFontEx(IntPtr consoleOutput, bool maximumWindow, ref CONSOLE_FONT_INFO_EX consoleCurrentFontEx);
        [DllImport("kernel32.dll", SetLastError = true)]
        static extern IntPtr GetStdHandle(int dwType);
        [DllImport("kernel32.dll", SetLastError = true)]
        static extern int SetConsoleFont(IntPtr hOut, uint dwFontNum);

        // Эмуляция нажатия клавиш
        [DllImport("user32.dll")]
        public static extern void keybd_event(byte vk, byte scan, int flags, int extrainfo);

        // Проверка асинхронного состония клавиш
        [DllImport("user32.dll")]
        static extern short GetAsyncKeyState(int key);
        #endregion

        #region Объявление глобальных переменных
        static SafeFileHandle h = CreateFile("CONOUT$", 0x40000000, 2, IntPtr.Zero, FileMode.Open, 0, IntPtr.Zero);
        static Random Rand = new Random();

        static int ScreenWidth;
        static int ScreenHeight;
        static int ConsoleWindowWidth;
        static int ConsoleWindowHeight;
        static int FrameWidth;
        static int FrameHeight;

        static double AspectRatio = 16 / 9d;
        static double hFov = Math.PI / 2;
        static double vFov = hFov / AspectRatio;
        static double hFovCurrent = hFov;
        static double vFovCurrent = vFov;

        static double RenderQuality = 0.01;
        static double RenderDistance = 16;

        //static int FPSLimit = 1000 / 60;
        static int GraphicsLevel = 1;
        static int GraphicsMode = 1;

        static double PlayerX;
        static double PlayerY;
        static double PlayerZ;
        static double PlayerHeight = 1.5;
        static double PlayerRadius = PlayerHeight * 0.15;
        static double PlayerCrouchHeihgt = PlayerHeight / 3d;
        static double PlayerCurrentHeight = PlayerHeight;

        static bool isCrouch = false;
        static bool isScope = false;
        static bool isScopeActivated = false;
        static bool isReleaseMouse = false;
        static bool isFullScreenChanged = false;
        static bool isFovChanged = false;
        static bool isRecoil = false;

        static bool isFishEye = false;
        static bool isNewEngine = false;

        static double MouseSensitivityH = 0.005;
        static double MouseSensitivityV = MouseSensitivityH / AspectRatio;

        static double ScopeMult = 4;
        static double ScopeCameraSpeedMult = ScopeMult;
        static double PlayerScopeSpeedMult = 2;

        static double WeaponRecoilAngle = 0;
        static double WeaponRecoil = 1;
        static double WeaponRecoilSpeedCurrent = 0;
        static double WeaponRecoilBackSpeedCurrent = 0;

        static double PlayerAngleH;
        static double PlayerAngleV;

        static double PlayerSpeed = 0.01;
        static double PlayerSideSpeed = PlayerSpeed / 2d;

        static double PlayerCrouchSpeedMult = 1.5;
        static double PlayerBackSpeedMult = 2;

        static double PlayerJumpStrength = 2;
        static double PlayerBounce = 0.025; // 0.0725 это примерно 50%

        static double PlayerCurrentSpeed = 0;
        static double PlayerCurrentSideSpeed = 0;
        static double PlayerFallingSpeed = 0;

        static double Gravity = 9.80665;

        static int MapX = 512;
        static int MapY = 512;
        static int MapZ = 16;

        static int LightRotationRadius = 10000;
        static double LightRotationSpeed = 0.0001;
        static vector3 LightPosition = new vector3(LightRotationRadius, LightRotationRadius, LightRotationRadius * 1.5);
        //static double LightIntensity = 1; 
        //static int AmbientLightIntensity = 0; // {0...15}

        struct vector2
        {
            public double X;
            public double Y;
            public vector2(double x, double y)
            {
                X = x;
                Y = y;
            }
        }

        struct vector3
        {
            public double X;
            public double Y;
            public double Z;

            public vector3(double x, double y, double z)
            {
                X = x;
                Y = y;
                Z = z;
            }
            public static vector3 operator - (vector3 a, vector3 b)
            {
                vector3 temp = new vector3();
                temp.X = a.X - b.X;
                temp.Y = a.Y - b.Y;
                temp.Z = a.Z - b.Z;
                return temp;
            }
            public static vector3 Abs(vector3 a)
            {
                vector3 temp = new vector3();
                temp.X = Math.Abs(a.X);
                temp.Y = Math.Abs(a.Y);
                temp.Z = Math.Abs(a.Z);
                return temp;
            }
        }

        static byte[,,] Map;

        static byte[,] skyTextureFar;
        static byte[,] skyTextureNear;
        static byte[,] skyTextureFarTemp;
        static byte[,] skyTextureNearTemp;

        struct ScreenCell
        {
            public byte ASCIIChar;
            public short Color;
        }

        static ScreenCell[] Screen;
        static ScreenCell[] Frame;
        static ScreenCell[] PreviousFrame;
        static ScreenCell[] OutputFrame;
        static ScreenCell[] Buffer;

        static Thread Load = new Thread(Loading);
        static Thread SkyRotatationFar = new Thread(SkyRotateFar);
        static Thread SkyRotatationNear = new Thread(SkyRotateNear);
        static Thread Render = new Thread(RenderScreen);
        static Thread Refresh = new Thread(RefreshScreen);
        static Thread PlayerMouse = new Thread(Mouse);
        static Thread PlayerKeyboard0 = new Thread(Keyboard0);
        static Thread PlayerKeyboard1 = new Thread(Keyboard1);
        static Thread PlayerKeyboard2 = new Thread(Keyboard2);
        static Thread PlayerKeyboard3 = new Thread(Keyboard3);
        static Thread PlayerKeyboard4 = new Thread(Keyboard4);
        static Thread PlayerPhysics = new Thread(Physics);

        static Thread LightRotation = new Thread(LightRotate);
        #endregion
        static void Main()
        {
            SetConsoleColorGardient(0, 0, 0, 15, 15, 15);
            Console.BackgroundColor = (ConsoleColor)0;
            Console.ForegroundColor = (ConsoleColor)15;
            Load.Start();

            Console.Title = "Volume Ray-Casting";
            SetConsoleFont("Consolas", 16);
            Console.SetWindowSize(80, 33);
            ShowWindow(ThisConsole, 3);
            keybd_event(0x7A, 0, 0, 0);
            keybd_event(0x7A, 0, 0x2, 0);
            Thread.Sleep(300);

            DisableConsoleQuickEdit.Go();

            ConsoleWindowHeight = Console.WindowHeight;
            ConsoleWindowWidth = Console.WindowWidth;

            ScreenHeight = ConsoleWindowHeight - 1;
            ScreenWidth = ConsoleWindowWidth;
            FrameHeight = ScreenHeight;
            FrameWidth = ScreenWidth;

            try
            {
                Console.SetBufferSize(ConsoleWindowWidth, ConsoleWindowHeight);
            }
            catch
            {
                Console.SetWindowPosition(0, 0);
                Console.SetBufferSize(ConsoleWindowWidth, ConsoleWindowHeight);
            }

            Screen = new ScreenCell[ScreenWidth * ScreenHeight];
            Frame = new ScreenCell[FrameWidth * FrameHeight];
            PreviousFrame = new ScreenCell[FrameWidth * FrameHeight];
            OutputFrame = new ScreenCell[FrameWidth * FrameHeight];
            Buffer = new ScreenCell[ScreenWidth * ScreenHeight];

            GenerateMap();
            GenerateSkyFar();
            GenerateSkyNear();

            SkyRotatationFar.Start();
            SkyRotatationNear.Start();
            PlayerMouse.Start();
            PlayerKeyboard0.Start();
            PlayerKeyboard1.Start();
            PlayerKeyboard2.Start();
            PlayerKeyboard3.Start();
            PlayerKeyboard4.Start();

            AspectRatio = ScreenWidth / (ScreenHeight * 2d);
            vFov = hFov / AspectRatio;

            MouseSensitivityV = MouseSensitivityH / AspectRatio;

            LocatePlayer();

            Load.Suspend();

            Render.Start();
            Refresh.Start();
            PlayerPhysics.Start();

            LightRotation.Start();

            Load.Priority = ThreadPriority.Lowest;
            SkyRotatationFar.Priority = ThreadPriority.Lowest;
            SkyRotatationNear.Priority = ThreadPriority.Lowest;
            Render.Priority = ThreadPriority.Normal;
            PlayerMouse.Priority = ThreadPriority.Highest;
            PlayerKeyboard0.Priority = ThreadPriority.AboveNormal;
            PlayerKeyboard1.Priority = ThreadPriority.Highest;
            PlayerKeyboard2.Priority = ThreadPriority.Highest;
            PlayerKeyboard3.Priority = ThreadPriority.Highest;
            PlayerKeyboard4.Priority = ThreadPriority.Highest;
            Refresh.Priority = ThreadPriority.Highest;
            PlayerPhysics.Priority = ThreadPriority.Highest;

            LightRotation.Priority = ThreadPriority.Lowest;
        }
        static void RefreshScreen()
        {
            DateTime startingTime = DateTime.Now;
            int tempFramesCounter = 0;
            string tempOutputInfo = $"Res: {ConsoleWindowWidth}x{ConsoleWindowHeight} | Aspect: {AspectRatio:f3} | Output FPS: 000";

            int tempLength;
            tempLength = Buffer.Length;

            while (true)
            {

                if (Load.ThreadState != ThreadState.Suspended) continue;

                if (!h.IsInvalid)
                {
                    CharInfo[] buf = new CharInfo[ScreenWidth * ScreenHeight];
                    SmallRect rect = new SmallRect() { Left = 0, Top = 0, Right = (short)ScreenWidth, Bottom = (short)ScreenHeight };

                    for (int i = 0; i < tempLength; i++)
                    {
                        buf[i].Attributes = Buffer[i].Color;
                        buf[i].Char.AsciiChar = Buffer[i].ASCIIChar;

                        tempLength = Buffer.Length < buf.Length ? Buffer.Length : buf.Length;
                    }

                    WriteConsoleOutput(h, buf,
                        new Coord() { X = (short)ScreenWidth, Y = (short)ScreenHeight },
                        new Coord() { X = 0, Y = 0 },
                        ref rect);
                }



                if ((DateTime.Now - startingTime).TotalSeconds > 0.5)
                {
                    if (tempFramesCounter > 0)
                    {
                        tempOutputInfo = $"Res: {ConsoleWindowWidth}x{ConsoleWindowHeight} | Aspect: {AspectRatio:f3} | Output FPS: {tempFramesCounter * 2:d3}";
                    }
                    else
                    {
                        tempOutputInfo = $"Res: {ConsoleWindowWidth}x{ConsoleWindowHeight} | Aspect: {AspectRatio:f3} | Output FPS: <0 ";
                    }
                    tempFramesCounter = 0;
                    startingTime = DateTime.Now;
                }
                else tempFramesCounter++;

                int tempLength2 = ScreenWidth - tempOutputInfo.Length - 1;
                for (int i = 0; i < tempLength2; i++)
                    tempOutputInfo += " ";
                Console.CursorVisible = false;
                Console.SetCursorPosition(0, ScreenHeight);
                Console.Write(tempOutputInfo);

                //Thread.Sleep(FPSLimit);
            }
        }
        static async void RenderScreen()
        {
            double frameWidthDivBy2d = FrameWidth / 2d;
            double frameHeightDivBy2d = FrameHeight / 2d;
            int frameWidthDivBy2 = FrameWidth / 2;
            int frameHeightDivBy2 = FrameHeight / 2;

            double hFovCurrentDivBy2d = hFovCurrent / 2d;
            double vFovCurrentDivBy2d = vFovCurrent / 2d;
            double hFovCurrentDivByFrameWidth = hFovCurrent / FrameWidth;
            double vFovCurrentDivByFrameHeight = vFovCurrent / FrameHeight;

            int tempGraphicsLevel = GraphicsLevel;
            double tempRenderDistance = RenderDistance;

            DateTime startingTime = DateTime.Now;
            int tempFramesCounter = 0;
            string temP3DFPS = "000";
            string temP3DTime = "0.000";

            while (true)
            {
                if (Load.ThreadState != ThreadState.Suspended) continue;

                if (isFullScreenChanged || ConsoleWindowHeight != Console.WindowHeight || ConsoleWindowWidth != Console.WindowWidth)
                {
                    ConsoleWindowHeight = Console.WindowHeight;
                    ConsoleWindowWidth = Console.WindowWidth;

                    AspectRatio = ScreenWidth / (ScreenHeight * 2d);
                    vFov = hFov / AspectRatio;

                    MouseSensitivityV = MouseSensitivityH / AspectRatio;

                    ScreenHeight = ConsoleWindowHeight - 1;
                    ScreenWidth = ConsoleWindowWidth;
                    FrameHeight = ScreenHeight;
                    FrameWidth = ScreenWidth;

                    try
                    {
                        Console.SetBufferSize(ConsoleWindowWidth, ConsoleWindowHeight);
                    }
                    catch
                    {
                        Console.SetWindowPosition(0, 0);
                        Console.SetBufferSize(ConsoleWindowWidth, ConsoleWindowHeight);
                    }

                    Screen = new ScreenCell[ScreenWidth * ScreenHeight];
                    Frame = new ScreenCell[FrameWidth * FrameHeight];
                    PreviousFrame = new ScreenCell[FrameWidth * FrameHeight];
                    OutputFrame = new ScreenCell[FrameWidth * FrameHeight];
                    Buffer = new ScreenCell[ScreenWidth * ScreenHeight];

                    isFullScreenChanged = false;

                    ConsoleWindowHeight = Console.WindowHeight;
                    ConsoleWindowWidth = Console.WindowWidth;

                    frameWidthDivBy2d = FrameWidth / 2d;
                    frameHeightDivBy2d = FrameHeight / 2d;
                    frameWidthDivBy2 = FrameWidth / 2;
                    frameHeightDivBy2 = FrameHeight / 2;

                    hFovCurrentDivBy2d = hFovCurrent / 2d;
                    vFovCurrentDivBy2d = vFovCurrent / 2d;
                    hFovCurrentDivByFrameWidth = hFovCurrent / FrameWidth;
                    vFovCurrentDivByFrameHeight = vFovCurrent / FrameHeight;

                    GenerateSkyFar();
                    GenerateSkyNear();
                }

                if (isScope & !isScopeActivated)
                {
                    hFovCurrent /= ScopeMult;
                    vFovCurrent /= ScopeMult;
                    tempRenderDistance = RenderDistance * ScopeMult;
                    isScopeActivated = true;
                    isFovChanged = true;
                }
                else if (!isScope)
                {
                    hFovCurrent = hFov;
                    vFovCurrent = vFov;
                    tempRenderDistance = RenderDistance;
                    isScopeActivated = false;
                    isFovChanged = true;
                }

                if (isFovChanged)
                {
                    hFovCurrentDivBy2d = hFovCurrent / 2d;
                    vFovCurrentDivBy2d = vFovCurrent / 2d;
                    hFovCurrentDivByFrameWidth = hFovCurrent / FrameWidth;
                    vFovCurrentDivByFrameHeight = vFovCurrent / FrameHeight;
                    isFovChanged = false;
                }

                Frame = new ScreenCell[FrameWidth * FrameHeight];

                var rayCastingTasks = new List<Task<Dictionary<int, ScreenCell>>>();

                for (int x = 0; x < FrameWidth; x += GraphicsLevel)
                {
                    for (int y = 0; y < FrameHeight; y += GraphicsLevel)
                    {
                        if (isScopeActivated && (x < frameWidthDivBy2d - FrameHeight - tempGraphicsLevel || x > frameWidthDivBy2d + FrameHeight - tempGraphicsLevel))
                        {
                            Frame[y * FrameWidth + x].ASCIIChar = 32;
                            Frame[y * FrameWidth + x].Color = 0 | 1 << 4;
                        }
                        else
                        {
                            int xi = x;
                            int yi = y;

                            if (isNewEngine)
                                rayCastingTasks.Add(Task.Run(() => NewCastRay(xi, yi, tempRenderDistance, hFovCurrentDivBy2d, vFovCurrentDivBy2d, hFovCurrentDivByFrameWidth, vFovCurrentDivByFrameHeight, frameWidthDivBy2d, frameHeightDivBy2d)));
                            else
                                rayCastingTasks.Add(Task.Run(() => CastRay(xi, yi, tempRenderDistance, hFovCurrentDivBy2d, vFovCurrentDivBy2d, hFovCurrentDivByFrameWidth, vFovCurrentDivByFrameHeight, frameWidthDivBy2d, frameHeightDivBy2d)));
                        }
                    }
                }

                Dictionary<int, ScreenCell>[] rays = await Task.WhenAll(rayCastingTasks);

                foreach (Dictionary<int, ScreenCell> dictionary in rays)
                {
                    foreach (int key in dictionary.Keys)
                    {
                        Frame[key] = dictionary[key];
                    }
                }

                // Апскейл
                try
                {
                    if (GraphicsLevel > 1)
                    {
                        ScreenCell[] tempFrame = Frame;
                        Frame = new ScreenCell[FrameWidth * FrameHeight];

                        switch (GraphicsMode)
                        {
                            case 1:
                                {
                                    // AdvMAME2x
                                    for (int x = 0; x < FrameWidth; x += GraphicsLevel)
                                    {
                                        for (int y = 0; y < FrameHeight; y += GraphicsLevel)
                                        {
                                            ScreenCell P1, P2, P3, P4, a, b, c, d;
                                            P1 = P2 = P3 = P4 = tempFrame[y * FrameWidth + x];

                                            try { a = tempFrame[(y - GraphicsLevel) * FrameWidth + x]; } catch { a = P1; }
                                            try { b = tempFrame[y * FrameWidth + x + GraphicsLevel]; } catch { b = P1; }
                                            try { c = tempFrame[y * FrameWidth + x - GraphicsLevel]; } catch { c = P1; }
                                            try { d = tempFrame[(y + GraphicsLevel) * FrameWidth + x]; } catch { d = P1; }

                                            if (c.Color == a.Color && c.Color != d.Color && a.Color != b.Color) P1.Color = a.Color;
                                            if (a.Color == b.Color && a.Color != c.Color && b.Color != d.Color) P2.Color = b.Color;
                                            if (d.Color == c.Color && d.Color != b.Color && c.Color != a.Color) P3.Color = c.Color;
                                            if (b.Color == d.Color && b.Color != a.Color && d.Color != c.Color) P4.Color = d.Color;

                                            try { Frame[y * FrameWidth + x] = P1; } catch { continue; }
                                            try { Frame[y * FrameWidth + x + 1] = P2; } catch { continue; }
                                            try { Frame[(y + 1) * FrameWidth + x] = P3; } catch { continue; }
                                            try { Frame[(y + 1) * FrameWidth + x + 1] = P4; } catch { continue; }
                                        }
                                    }
                                    break;
                                }

                            case 2:
                                {
                                    for (int x = 0; x < FrameWidth; x += GraphicsLevel)
                                    {
                                        for (int y = 0; y < FrameHeight; y += GraphicsLevel)
                                        {
                                            // Eagle
                                            ScreenCell P1, P2, P3, P4, S, T, U, V, W, X, Y, Z;
                                            P1 = P2 = P3 = P4 = tempFrame[y * FrameWidth + x];

                                            try { S = tempFrame[(y - GraphicsLevel) * FrameWidth + x - GraphicsLevel]; } catch { S = P1; }
                                            try { T = tempFrame[(y - GraphicsLevel) * FrameWidth + x]; } catch { T = P1; }
                                            try { U = tempFrame[(y - GraphicsLevel) * FrameWidth + x + GraphicsLevel]; } catch { U = P1; }
                                            try { V = tempFrame[y * FrameWidth + x - GraphicsLevel]; } catch { V = P1; }
                                            try { W = tempFrame[y * FrameWidth + x + GraphicsLevel]; } catch { W = P1; }
                                            try { X = tempFrame[(y + GraphicsLevel) * FrameWidth + x - GraphicsLevel]; } catch { X = P1; }
                                            try { Y = tempFrame[(y + GraphicsLevel) * FrameWidth + x]; } catch { Y = P1; }
                                            try { Z = tempFrame[(y + GraphicsLevel) * FrameWidth + x + GraphicsLevel]; } catch { Z = P1; }

                                            if (V.Color == S.Color && S.Color == T.Color) P1.Color = S.Color;
                                            if (T.Color == U.Color && U.Color == W.Color) P2.Color = U.Color;
                                            if (V.Color == X.Color && X.Color == Y.Color) P3.Color = X.Color;
                                            if (W.Color == Z.Color && Z.Color == Y.Color) P4.Color = Z.Color;

                                            try { Frame[y * FrameWidth + x] = P1; } catch { continue; }
                                            try { Frame[y * FrameWidth + x + 1] = P2; } catch { continue; }
                                            try { Frame[(y + 1) * FrameWidth + x] = P3; } catch { continue; }
                                            try { Frame[(y + 1) * FrameWidth + x + 1] = P4; } catch { continue; }
                                        }
                                    }
                                    break;
                                }

                            case 3:
                                {
                                    // Average
                                    for (int x = 0; x < FrameWidth; x += GraphicsLevel)
                                    {
                                        for (int y = 0; y < FrameHeight; y += GraphicsLevel)
                                        {
                                            if (y * FrameWidth + x + GraphicsLevel >= Frame.Length)
                                                continue;
                                            for (int i = 0; i < GraphicsLevel; i++)
                                            {
                                                Frame[y * FrameWidth + x + i].Color = (byte)((byte)(((tempFrame[y * FrameWidth + x].Color & 15) + (tempFrame[y * FrameWidth + x + GraphicsLevel].Color & 15)) / 2d) |
                                                                                             (byte)(((tempFrame[y * FrameWidth + x].Color >> 4) + (tempFrame[y * FrameWidth + x + GraphicsLevel].Color >> 4)) / 2d) << 4);
                                                Frame[y * FrameWidth + x + i].ASCIIChar = tempFrame[y * FrameWidth + x].ASCIIChar;
                                            }
                                        }
                                    }
                                    for (int x = 0; x < FrameWidth; x++)
                                    {
                                        for (int y = 0; y < FrameHeight; y += GraphicsLevel)
                                        {
                                            for (int i = 1; i < GraphicsLevel; i++)
                                            {
                                                if ((y + GraphicsLevel) * FrameWidth + x >= Frame.Length)
                                                    continue;
                                                Frame[(y + i) * FrameWidth + x].Color = (byte)((byte)(((Frame[y * FrameWidth + x].Color & 15) + (Frame[(y + GraphicsLevel) * FrameWidth + x].Color & 15)) / 2d) |
                                                                                               (byte)(((Frame[y * FrameWidth + x].Color >> 4) + (Frame[(y + GraphicsLevel) * FrameWidth + x].Color >> 4)) / 2d) << 4);
                                                Frame[(y + i) * FrameWidth + x].ASCIIChar = Frame[y * FrameWidth + x].ASCIIChar;
                                            }
                                        }
                                    } 
                                    break;
                                }
                        }
                    }
                }
                catch { continue; }

                // Отрисовка неба
                /*for (int y = 0; y < FrameHeight; y++)
                {
                    for (int x = 0; x < FrameWidth; x++)
                    {
                        if (Frame[y * FrameWidth + x].Color == 0)
                        {
                            double tempH = -PlayerAngleH;
                            double tempV = -((PlayerAngleV * 2 / 0.75 - Math.PI / 2d) / 0.75);

                            if (tempH < 0)
                                tempH += Math.PI * 2;
                            else if (tempH > Math.PI * 2)
                                tempH -= Math.PI * 2;

                            if (tempV < 0)
                                tempV += Math.PI * 2;
                            else if (tempV > Math.PI * 2)
                                tempV -= Math.PI * 2;

                            int xx = (int)(skyTextureFar.GetLength(1) / (Math.PI * 2 / tempH) + x);
                            int yy = (int)(skyTextureFar.GetLength(0) / (Math.PI * 2 / tempV) + y);

                            if (xx < 0)
                                xx += skyTextureFar.GetLength(1) - 1;
                            else if (xx >= skyTextureFar.GetLength(1))
                                xx -= skyTextureFar.GetLength(1) - 1;

                            if (yy < 0)
                                yy += skyTextureFar.GetLength(0) - 1;
                            else if (yy >= skyTextureFar.GetLength(0))
                                yy -= skyTextureFar.GetLength(0) - 1;

                            if (skyTextureNear[yy, xx] != 0)
                            {
                                Frame[y * FrameWidth + x].ASCIIChar = 32;
                                Frame[y * FrameWidth + x].Color = 0 | 12 << 4;
                            }
                            else
                            {
                                Frame[y * FrameWidth + x].Color = 15 | 0 << 4;
                                switch (skyTextureFar[yy, xx])
                                {
                                    case 39:
                                        {
                                            if (Rand.Next(0, 100) < 1)
                                                Frame[y * FrameWidth + x].ASCIIChar = 32;
                                            else
                                                Frame[y * FrameWidth + x].ASCIIChar = skyTextureFar[yy, xx];
                                            break;
                                        }
                                    case 42:
                                        {
                                            if (Rand.Next(0, 100) < 1)
                                                Frame[y * FrameWidth + x].ASCIIChar = 39;
                                            else
                                                Frame[y * FrameWidth + x].ASCIIChar = skyTextureFar[yy, xx];
                                            break;
                                        }
                                }
                            }
                        }

                    }
                }*/

                // Отсечение "Лишнего" для создания прицела в форме круга
                for (int x = 0; x < FrameWidth; x++)
                {
                    for (int y = 0; y < FrameHeight; y++)
                    {
                        if (isScopeActivated && (((frameWidthDivBy2d - x) * (frameWidthDivBy2d - x) / 4d + (frameHeightDivBy2d - y) * (frameHeightDivBy2d - y) > (frameHeightDivBy2d) * (frameHeightDivBy2d)) ||
                            ((x == frameWidthDivBy2 && (y < frameHeightDivBy2 - 2 || y > frameHeightDivBy2 + 2)) || (x == frameWidthDivBy2 + 1 && (y < frameHeightDivBy2 - 2 || y > frameHeightDivBy2 + 2)) || (y == frameHeightDivBy2 && (x < frameWidthDivBy2 - 4 || x > frameWidthDivBy2 + 5)))))
                        {
                            Frame[y * FrameWidth + x].ASCIIChar = 32;
                            Frame[y * FrameWidth + x].Color = 0 | 1 << 4;
                        }
                    }
                }

                // Отрисовка эффекта плавного перехода кадра
                OutputFrame = Frame;
                /*for (int i = 0; i < FrameHeight * FrameWidth; i++)
                {
                    if (Frame[i].Color != PreviousFrame[i].Color)
                    {
                        OutputFrame[i].Color = (byte)((byte)(((Frame[i].Color & 15) + (PreviousFrame[i].Color & 15)) / 2d) |
                                                (byte)(((Frame[i].Color >> 4) + (PreviousFrame[i].Color >> 4)) / 2d) << 4);
                    }
                    else
                    {
                        OutputFrame[i].Color = Frame[i].Color;
                    }
                }
                for (int i = 0; i < FrameHeight * FrameWidth; i++)
                {
                    PreviousFrame[i] = Frame[i];
                }*/

                // Отрисовка 3Д графики
                OutputFrame.CopyTo(Screen, 0);

                // Отрисовка перекрестья
                if (!isScopeActivated)
                {
                    Screen[ScreenWidth * (ScreenHeight / 2) - ScreenWidth / 2 - ScreenWidth].ASCIIChar = (byte)'|';
                    Screen[ScreenWidth * (ScreenHeight / 2) - ScreenWidth / 2 - 2].ASCIIChar = (byte)'-';
                    Screen[ScreenWidth * (ScreenHeight / 2) - ScreenWidth / 2 + 2].ASCIIChar = (byte)'-';
                    Screen[ScreenWidth * (ScreenHeight / 2) - ScreenWidth / 2 + ScreenWidth].ASCIIChar = (byte)'|';
                }

                // Отрисовка статов
                string tempStringGraphicsLevel = "";
                switch (GraphicsLevel)
                {
                    case 1:
                        tempStringGraphicsLevel = "Ultra";
                        break;
                    case 2:
                        tempStringGraphicsLevel = "High";
                        break;
                    case 3:
                        tempStringGraphicsLevel = "Medium";
                        break;
                    case 4:
                        tempStringGraphicsLevel = "Low";
                        break;
                }

                string tempStringGraphicsMode = "";
                if (GraphicsLevel == 1)
                {
                    tempStringGraphicsMode = "None";
                }
                else
                {
                    switch (GraphicsMode)
                    {
                        case 1:
                            tempStringGraphicsMode = "AdvMAME2x";
                            break;
                        case 2:
                            tempStringGraphicsMode = "Eagle";
                            break;
                        case 3:
                            tempStringGraphicsMode = "Average";
                            break;
                    }
                }

                if ((DateTime.Now - startingTime).TotalSeconds > 0.5)
                {
                    if (tempFramesCounter > 0)
                    {
                        temP3DTime = $"{(DateTime.Now - startingTime).TotalSeconds / tempFramesCounter:f3}";
                        temP3DFPS = $"{tempFramesCounter * 2:d3}";
                    }
                    else
                    {
                        temP3DTime = ">1   ";
                        temP3DFPS = "<0 ";
                    }
                    tempFramesCounter = 0;
                    startingTime = DateTime.Now;
                }
                else tempFramesCounter++;

                int tempPlayerAngleH = (int)Math.Round(PlayerAngleH * 180 / Math.PI);
                string tempPlayerAngleV = (int)Math.Round((PlayerAngleV + WeaponRecoilAngle) * 180 / Math.PI) < 0 ? $"{(int)Math.Round((PlayerAngleV + WeaponRecoilAngle) * 180 / Math.PI):d3}" : $"+{(int)Math.Round((PlayerAngleV + WeaponRecoilAngle) * 180 / Math.PI):d3}";// + 90;
                string tempMapSizeMB = MapX * MapY * MapZ / 1024d / 1024d < 1 ? "<1" : $"{Math.Round(MapX * MapY * MapZ / 1024d / 1024d)}";
                string tempPlayerCurrentSpeed = PlayerCurrentSpeed < 0 ? $"{PlayerCurrentSpeed:f3}" : $"+{PlayerCurrentSpeed:f3}";
                string tempPlayerCurrentSideSpeed = -PlayerCurrentSideSpeed < 0 ? $"{-PlayerCurrentSideSpeed:f3}" : $"+{-PlayerCurrentSideSpeed:f3}";
                string tempPlayerFallingSpeed = PlayerFallingSpeed < 0 ? $"{PlayerFallingSpeed:f3}" : $"+{PlayerFallingSpeed:f3}";
                string tempIsScopeActivated = isScopeActivated ? $"{ScopeMult:f1}" + "x" : "Off ";
                string stats = $"3DRes: {FrameWidth / GraphicsLevel}x{FrameHeight / GraphicsLevel} | FPS: {temP3DFPS} | Time: {temP3DTime} | Graphics: {tempStringGraphicsLevel} | Mode: {tempStringGraphicsMode} | FishEye: {isFishEye}    Map Size: {MapX}x{MapY}x{MapZ} | {tempMapSizeMB}MB    X: {(int)PlayerX:d3} | Y: {(int)PlayerY:d3} | Z: {(int)PlayerZ:d3} | Heihgt: {PlayerCurrentHeight:f2}    H: {tempPlayerAngleH:d3} | V: {tempPlayerAngleV} | hFov: {(int)(hFov / Math.PI * 180):d3} | vFov: {(int)(vFov / Math.PI * 180):d3} | Scope: {tempIsScopeActivated}    Speed: {tempPlayerCurrentSpeed} | Side: {tempPlayerCurrentSideSpeed} | Falling: {tempPlayerFallingSpeed}    Light: {(int)LightPosition.X:d5} {(int)LightPosition.Y:d5} {(int)LightPosition.Z:d5}".Replace(',', '.');

                int tempLength = ScreenWidth - stats.Length;
                for (int i = 0; i < tempLength; i++)
                    stats += " ";
                for (int i = 0; i < stats.Length; i++)
                {
                    Screen[i].ASCIIChar = (byte)stats[i];
                    Screen[i].Color = 15 | 0 << 4;
                }

                // Сохранение результата в буфер
                int k = ScreenWidth * ScreenHeight;
                for (int i = 0; i < k; i++)
                {
                    Buffer[i] = Screen[i];
                }

                tempGraphicsLevel = GraphicsLevel;
            }
        }
        static Dictionary<int, ScreenCell> CastRay(int x, int y, double tempRenderDistance, double hFovCurrentDivBy2d, double vFovCurrentDivBy2d, double hFovCurrentDivByFrameWidth, double vFovCurrentDivByFrameHeight, double frameWidthDivBy2d, double frameHeightDivBy2d)
        {
            var result = new Dictionary<int, ScreenCell>();

            double dh;
            double dv;
            if (isFishEye)
            {
                dh = Math.Tan(hFovCurrentDivByFrameWidth * (frameWidthDivBy2d - x));
                dv = Math.Tan(vFovCurrentDivByFrameHeight * (frameHeightDivBy2d - y));
            }
            else
            { 
                dh = Math.Tan(hFovCurrentDivBy2d) * (1 - x / frameWidthDivBy2d);
                dv = Math.Tan(vFovCurrentDivBy2d) * (1 - y / frameHeightDivBy2d);
            }

            double sinH = Math.Sin(PlayerAngleH);
            double cosH = Math.Cos(PlayerAngleH);

            double sinV = Math.Sin(PlayerAngleV + WeaponRecoilAngle);
            double cosV = Math.Cos(PlayerAngleV + WeaponRecoilAngle);

            double rayX = sinH * cosV + dh * cosH - dv * sinH * sinV;
            double rayY = cosH * cosV - dh * sinH - dv * cosH * sinV;
            double rayZ = sinV + dv * cosV;

            double distanceToWall = 0;
            bool hitWall = false;

            ScreenCell shade;
            shade.ASCIIChar = 32;
            shade.Color = 0 | 0 << 4;

            while (!hitWall && distanceToWall < tempRenderDistance)
            {
                distanceToWall += RenderQuality;

                int testX = (int)(PlayerX + rayX * distanceToWall);
                int testY = (int)(PlayerY + rayY * distanceToWall);
                int testZ = (int)(PlayerZ + PlayerCurrentHeight + rayZ * distanceToWall);

                if (Math.Round(PlayerX + rayX * distanceToWall - 0.5) < 0 || testX >= tempRenderDistance + PlayerX || testX >= MapX ||
                    Math.Round(PlayerY + rayY * distanceToWall - 0.5) < 0 || testY >= tempRenderDistance + PlayerY || testY >= MapY ||
                    Math.Round(PlayerZ + PlayerCurrentHeight + rayZ * distanceToWall - 0.5) < 0 || testZ >= tempRenderDistance + PlayerZ + PlayerCurrentHeight || testZ >= MapZ)
                {
                    hitWall = true;
                    distanceToWall = tempRenderDistance;
                }
                else
                {
                    byte testCell = Map[testX, testY, testZ];

                    if ((testCell == 1) ||
                        (testCell == 2) ||
                        (testCell == 3 && GetDistance(PlayerX + rayX * distanceToWall - testX, PlayerY + rayY * distanceToWall - testY, PlayerZ + PlayerCurrentHeight + rayZ * distanceToWall - testZ, 0.5, 0.5, 0.5) <= (0.25 + (PlayerZ + PlayerCurrentHeight + rayZ * distanceToWall) / (double)(MapZ * 2) < 0.5 ? 0.25 + (PlayerZ + PlayerCurrentHeight + rayZ * distanceToWall) / (double)(MapZ * 2) : 0.5)) ||
                        (testCell == 4 && GetDistance(PlayerX + rayX * distanceToWall - testX, PlayerY + rayY * distanceToWall - testY, 0, 0.5, 0.5, 0) <= (0.5 - (PlayerZ + PlayerCurrentHeight + rayZ * distanceToWall) / (double)(MapZ * 2) > 0.1 ? 0.5 - (PlayerZ + PlayerCurrentHeight + rayZ * distanceToWall) / (double)(MapZ * 2) : 0.1)))
                    {
                        hitWall = true;

                        vector3 LightVector = new vector3(LightPosition.X - (PlayerX + rayX * distanceToWall), LightPosition.Y - (PlayerZ + rayZ * distanceToWall), LightPosition.Z - (PlayerZ + PlayerCurrentHeight + rayZ * distanceToWall));

                        bool NotLighten = false;


                        // освещение в зависимости от положения источника света
                        vector3 NormalVector = new vector3(0, 0, 0);

                        // ищу, на какой грани объекта находится текущая точка (пиксель) (к какой грани точка ближе)
                        // чтобы выяснить нормаль пикселя на кубе
                        if (testCell == 1 || testCell == 2)
                        {
                            if (PlayerZ + PlayerCurrentHeight + rayZ * distanceToWall <= 1)
                            {
                                NormalVector = new vector3(0, 0, 1);
                            }
                            else
                            {
                                Dictionary<int, double> Matches = new Dictionary<int, double>(6);

                                Matches.Add(0, Math.Abs((PlayerX + rayX * distanceToWall) - testX));
                                Matches.Add(1, Math.Abs((PlayerX + rayX * distanceToWall) - (testX + 1)));
                                Matches.Add(2, Math.Abs((PlayerY + rayY * distanceToWall) - testY));
                                Matches.Add(3, Math.Abs((PlayerY + rayY * distanceToWall) - (testY + 1)));
                                Matches.Add(4, Math.Abs((PlayerZ + PlayerCurrentHeight + rayZ * distanceToWall) - testZ));
                                Matches.Add(5, Math.Abs((PlayerZ + PlayerCurrentHeight + rayZ * distanceToWall) - (testZ + 1)));

                                var sortedMatches = from entry in Matches orderby entry.Value ascending select entry;

                                switch (sortedMatches.ElementAt(0).Key)
                                {
                                    case 0:
                                        {
                                            NormalVector = new vector3(-1, 0, 0);
                                            break;
                                        }
                                    case 1:
                                        {
                                            NormalVector = new vector3(1, 0, 0);
                                            break;
                                        }
                                    case 2:
                                        {
                                            NormalVector = new vector3(0, -1, 0);
                                            break;
                                        }
                                    case 3:
                                        {
                                            NormalVector = new vector3(0, 1, 0);
                                            break;
                                        }
                                    case 4:
                                        {
                                            NormalVector = new vector3(0, 0, -1);
                                            break;
                                        }
                                    case 5:
                                        {
                                            NormalVector = new vector3(0, 0, 1);
                                            break;
                                        }
                                }
                            }
                        }
                        // или беру вектор от центра сферы до точки пересечния
                        // чтобы выяснить нормаль пикселя на сферы
                        else if (testCell == 3)
                        {
                            vector3 tempNormal = new vector3((PlayerX + rayX * distanceToWall) - (testX + 0.5), (PlayerY + rayY * distanceToWall) - (testY + 0.5), (PlayerZ + PlayerCurrentHeight + rayZ * distanceToWall) - (testZ + 0.5));
                            double tempNormalLenght = Math.Sqrt((tempNormal.X * tempNormal.X) + (tempNormal.Y * tempNormal.Y) + (tempNormal.Z * tempNormal.Z));
                            NormalVector = new vector3(tempNormal.X / tempNormalLenght, tempNormal.Y / tempNormalLenght, tempNormal.Z / tempNormalLenght);
                        }

                        // или беру вектор от центра цилиндра (без учёта высоты) до точки пересечния
                        // чтобы выяснить нормаль пикселя на цилиндра
                        else if (testCell == 4)
                        {
                            vector3 tempNormal = new vector3((PlayerX + rayX * distanceToWall) - (testX + 0.5), (PlayerY + rayY * distanceToWall) - (testY + 0.5), 0);
                            double tempNormalLenght = Math.Sqrt((tempNormal.X * tempNormal.X) + (tempNormal.Y * tempNormal.Y) + (tempNormal.Z * tempNormal.Z));
                            NormalVector = new vector3(tempNormal.X / tempNormalLenght, tempNormal.Y / tempNormalLenght, tempNormal.Z / tempNormalLenght);
                        }

                        // вычесление интенсивности света для пересечения
                        // и нормализация к {0...1}
                        double cosAB = ((LightVector.X * NormalVector.X) + (LightVector.Y * NormalVector.Y) + (LightVector.Z * NormalVector.Z)) /
                            ((Math.Sqrt((LightVector.X * LightVector.X) + (LightVector.Y * LightVector.Y) + (LightVector.Z * LightVector.Z))) *
                            (Math.Sqrt((NormalVector.X * NormalVector.X) + (NormalVector.Y * NormalVector.Y) + (NormalVector.Z * NormalVector.Z))));

                        if (cosAB <= 0) NotLighten = true;

                        double NormalizedLight = Math.Acos(-cosAB) / Math.PI;

                        int Light = (int)(NormalizedLight * 15/* * LightIntensity + AmbientLightIntensity*/);

                        if (Light > 15)
                        {
                            Light = 15;
                        }

                        shade.Color = (short)((shade.Color & 15) | (((shade.Color >> 4) + Light) << 4));

                        // для теста гардиента
                        //shade.ASCIIChar = 48;
                        //shade.ASCIIChar += (byte)Light;

                        // Тени
                        double LightVectorLength = Math.Sqrt((LightVector.X * LightVector.X) + (LightVector.Y * LightVector.Y) + (LightVector.Z * LightVector.Z));
                        vector3 dLight = new vector3(LightVector.X / LightVectorLength, LightVector.Y / LightVectorLength, LightVector.Z / LightVectorLength);

                        vector3 DistanceToLight = new vector3(dLight.X, dLight.Y, dLight.Z);

                        bool Shadow = false;
                        while (!NotLighten && !Shadow && DistanceToLight.X + DistanceToLight.Y + DistanceToLight.Z < RenderDistance && DistanceToLight.X < MapX && DistanceToLight.Y < MapY && DistanceToLight.Z < MapZ)
                        {
                            try
                            {
                                if (Map[(int)(PlayerX + rayX * distanceToWall + DistanceToLight.X), (int)(PlayerY + rayY * distanceToWall + DistanceToLight.Y), (int)(PlayerZ + PlayerCurrentHeight + rayZ * distanceToWall + DistanceToLight.Z)] != 0)
                                {
                                    vector3 distanceToLight = new vector3(0, 0, 0);
                                    while (!Shadow && distanceToLight.X + distanceToLight.Y + distanceToLight.Z < 1.7320508075688772935274463415059)
                                    {
                                        if ((Map[(int)(PlayerX + rayX * distanceToWall + DistanceToLight.X), (int)(PlayerY + rayY * distanceToWall + DistanceToLight.Y), (int)(PlayerZ + PlayerCurrentHeight + rayZ * distanceToWall + DistanceToLight.Z)] == 1) ||
                                            (Map[(int)(PlayerX + rayX * distanceToWall + DistanceToLight.X), (int)(PlayerY + rayY * distanceToWall + DistanceToLight.Y), (int)(PlayerZ + PlayerCurrentHeight + rayZ * distanceToWall + DistanceToLight.Z)] == 2) ||
                                            (Map[(int)(PlayerX + rayX * distanceToWall + DistanceToLight.X), (int)(PlayerY + rayY * distanceToWall + DistanceToLight.Y), (int)(PlayerZ + PlayerCurrentHeight + rayZ * distanceToWall + DistanceToLight.Z)] == 3 && GetDistance(PlayerX + rayX * distanceToWall - testX + distanceToLight.X, PlayerY + rayY * distanceToWall - testY + distanceToLight.Y, PlayerZ + PlayerCurrentHeight + rayZ * distanceToWall - testZ + distanceToLight.Z, 0.5, 0.5, 0.5) < (0.25 + (PlayerZ + PlayerCurrentHeight + rayZ * distanceToWall - testZ + DistanceToLight.Z + distanceToLight.Z) / (double)(MapZ * 2) < 0.5 ? 0.25 + (PlayerZ + PlayerCurrentHeight + rayZ * distanceToWall - testZ + distanceToLight.Z) / (double)(MapZ * 2) : 0.5)) ||
                                            (Map[(int)(PlayerX + rayX * distanceToWall + DistanceToLight.X), (int)(PlayerY + rayY * distanceToWall + DistanceToLight.Y), (int)(PlayerZ + PlayerCurrentHeight + rayZ * distanceToWall + DistanceToLight.Z)] == 4 && GetDistance(PlayerX + rayX * distanceToWall - testX + distanceToLight.X, PlayerY + rayY * distanceToWall - testY + distanceToLight.Y, 0, 0.5, 0.5, 0) <= (0.5 - (PlayerZ + PlayerCurrentHeight + rayZ * distanceToWall - testZ + distanceToLight.Z) / (double)(MapZ * 2) > 0.1 ? 0.5 - (PlayerZ + PlayerCurrentHeight + rayZ * distanceToWall - testZ + distanceToLight.Z) / (double)(MapZ * 2) : 0.1)))
                                        {
                                            Shadow = true;
                                        }
                                        distanceToLight.X += dLight.X / 100d;
                                        distanceToLight.Y += dLight.Y / 100d;
                                        distanceToLight.Z += dLight.Z / 100d;
                                    }
                                    if (Shadow)
                                    {
                                        shade.Color = (short)((shade.Color & 15) | (((shade.Color >> 4) - 3) >= 1 ? ((shade.Color >> 4) - 3) : 1) << 4);
                                    }
                                }
                            }
                            catch { }
                            DistanceToLight.X += dLight.X;
                            DistanceToLight.Y += dLight.Y;
                            DistanceToLight.Z += dLight.Z;
                        }
                        
                        // ТУМАН (затемнение) (освещение в зависимости от растояния от камеры)
                        if (distanceToWall >= tempRenderDistance - 0.1)
                            shade.Color = (short)((shade.Color & 15) | (((shade.Color >> 4) - 15) >= 0 ? ((shade.Color >> 4) - 15) : 0) << 4);
                        else if (distanceToWall >= tempRenderDistance - 0.2)
                            shade.Color = (short)((shade.Color & 15) | (((shade.Color >> 4) - 14) >= 0 ? ((shade.Color >> 4) - 14) : 0) << 4);
                        else if (distanceToWall >= tempRenderDistance - 0.3)
                            shade.Color = (short)((shade.Color & 15) | (((shade.Color >> 4) - 13) >= 0 ? ((shade.Color >> 4) - 13) : 0) << 4);
                        else if (distanceToWall >= tempRenderDistance - 0.4)
                            shade.Color = (short)((shade.Color & 15) | (((shade.Color >> 4) - 12) >= 0 ? ((shade.Color >> 4) - 12) : 0) << 4);
                        else if (distanceToWall >= tempRenderDistance - 0.5)
                            shade.Color = (short)((shade.Color & 15) | (((shade.Color >> 4) - 11) >= 0 ? ((shade.Color >> 4) - 11) : 0) << 4);
                        else if (distanceToWall >= tempRenderDistance - 0.6)
                            shade.Color = (short)((shade.Color & 15) | (((shade.Color >> 4) - 10) >= 0 ? ((shade.Color >> 4) - 10) : 0) << 4);
                        else if (distanceToWall >= tempRenderDistance - 0.7)
                            shade.Color = (short)((shade.Color & 15) | (((shade.Color >> 4) - 9) >= 0 ? ((shade.Color >> 4) - 9) : 0) << 4);
                        else if (distanceToWall >= tempRenderDistance - 0.8)
                            shade.Color = (short)((shade.Color & 15) | (((shade.Color >> 4) - 8) >= 0 ? ((shade.Color >> 4) - 8) : 0) << 4);
                        else if (distanceToWall >= tempRenderDistance - 0.9)
                            shade.Color = (short)((shade.Color & 15) | (((shade.Color >> 4) - 7) >= 0 ? ((shade.Color >> 4) - 7) : 0) << 4);
                        else if (distanceToWall >= tempRenderDistance - 1.0)
                            shade.Color = (short)((shade.Color & 15) | (((shade.Color >> 4) - 6) >= 0 ? ((shade.Color >> 4) - 6) : 0) << 4);
                        else if (distanceToWall >= tempRenderDistance - 1.1)
                            shade.Color = (short)((shade.Color & 15) | (((shade.Color >> 4) - 5) >= 0 ? ((shade.Color >> 4) - 5) : 0) << 4);
                        else if (distanceToWall >= tempRenderDistance - 1.2)
                            shade.Color = (short)((shade.Color & 15) | (((shade.Color >> 4) - 4) >= 0 ? ((shade.Color >> 4) - 4) : 0) << 4);
                        else if (distanceToWall >= tempRenderDistance - 1.3)
                            shade.Color = (short)((shade.Color & 15) | (((shade.Color >> 4) - 3) >= 0 ? ((shade.Color >> 4) - 3) : 0) << 4);
                        else if (distanceToWall >= tempRenderDistance - 1.4)
                            shade.Color = (short)((shade.Color & 15) | (((shade.Color >> 4) - 2) >= 0 ? ((shade.Color >> 4) - 2) : 0) << 4);
                        else if (distanceToWall >= tempRenderDistance - 1.5)
                            shade.Color = (short)((shade.Color & 15) | (((shade.Color >> 4) - 1) >= 0 ? ((shade.Color >> 4) - 1) : 0) << 4);
                    }
                }
            }

            result[y * FrameWidth + x] = shade;
            return result;
        }
        static Dictionary<int, ScreenCell> NewCastRay(int x, int y, double tempRenderDistance, double hFovCurrentDivBy2d, double vFovCurrentDivBy2d, double hFovCurrentDivByFrameWidth, double vFovCurrentDivByFrameHeight, double frameWidthDivBy2d, double frameHeightDivBy2d)
        {
            var result = new Dictionary<int, ScreenCell>();

            double dh;
            double dv;
            if (isFishEye)
            {
                dh = Math.Tan(hFovCurrentDivByFrameWidth * (frameWidthDivBy2d - x));
                dv = Math.Tan(vFovCurrentDivByFrameHeight * (frameHeightDivBy2d - y));
            }
            else
            {
                dh = Math.Tan(hFovCurrentDivBy2d) * (1 - x / frameWidthDivBy2d);
                dv = Math.Tan(vFovCurrentDivBy2d) * (1 - y / frameHeightDivBy2d);
            }

            double sinH = Math.Sin(PlayerAngleH);
            double cosH = Math.Cos(PlayerAngleH);

            double sinV = Math.Sin(PlayerAngleV + WeaponRecoilAngle);
            double cosV = Math.Cos(PlayerAngleV + WeaponRecoilAngle);

            double rayX = sinH * cosV + dh * cosH - dv * sinH * sinV;
            double rayY = cosH * cosV - dh * sinH - dv * cosH * sinV;
            double rayZ = sinV + dv * cosV;

            bool hitWall = false;

            ScreenCell shade;
            shade.ASCIIChar = 32;
            shade.Color = 0 | 0 << 4;

            //////////
            double distanceToWall = 0;

            double distanceToWallX = Int32.MaxValue;
            double distanceToWallY = Int32.MaxValue;
            double distanceToWallZ = Int32.MaxValue;

            double testX = PlayerX;
            double testY = PlayerY;
            double testZ = PlayerZ + PlayerHeight;
            
            //////////
            double newX = 0;
            double newY = 0;
            double newZ = 0;
            int DX = 0;
            int DY = 0;
            int DZ = 0;

            //x
            if (rayX >= 0)
            {
                newX = (int)testX + 1;
                DX = 1;
            }
            else
            {
                newX = (int)testX;
                DX = -1;
            }
            //x
            for (int i = 0; i < MapX; i++)
            {
                distanceToWallX = (newX - testX) / rayX;
                newY = testY + distanceToWallX * rayY;
                newZ = testZ + distanceToWallX * rayZ;
                if (newX + DX < 0 || newY < 0 || newZ < 0 || newX + DX > MapX - 1 || newY > MapY - 1 || newZ > MapZ - 1)
                    break;
                if (Map[(int)(newX + DX), (int)newY, (int)newZ] != 0)
                {
                    hitWall = true;
                    break;
                }
            }
            //y
            if (rayY >= 0)
            {
                newY = (int)testY + 1;
                DY = 1;
            }
            else
            {
                newY = (int)testY;
                DY = -1;
            }
            //y
            for (int i = 0; i < MapY; i++)
            {
                distanceToWallY = (newY - testY) / rayY;
                newX = testX + distanceToWallY * rayX;
                newZ = testZ + distanceToWallY * rayZ;
                if (newX < 0 || newY + DY < 0 || newZ < 0 || newX > MapX - 1 || newY + DY > MapY - 1 || newZ > MapZ - 1)
                    break;
                if (Map[(int)newX, (int)(newY + DY), (int)newZ] != 0)
                {
                    hitWall = true;
                    break;
                }
            }
            //z
            if (rayZ >= 0)
            {
                newZ = (int)testZ + 1;
                DZ = 1;
            }
            else
            {
                newZ = (int)testZ;
                DZ = -1;
            }
            //z
            for (int i = 0; i < MapZ; i++)
            {
                distanceToWallZ = (newZ - testZ) / rayZ;
                newX = testX + distanceToWallZ * rayX;
                newY = testY + distanceToWallZ * rayY;
                if (newX < 0 || newY < 0 || newZ + DZ < 0 || newX > MapX - 1 || newY > MapY - 1 || newZ + DZ > MapZ - 1)
                    break;
                if (Map[(int)newX, (int)newY, (int)(newZ + DZ)] != 0)
                {
                    hitWall = true;
                    break;
                }
            }

            //выбрать ближайшее пересечение
            if (distanceToWallX > distanceToWallY)
            {
                if (distanceToWallX > distanceToWallZ)
                    distanceToWall = distanceToWallX;
                else
                    distanceToWall = distanceToWallZ;
            }
            else
            {
                if (distanceToWallY > distanceToWallZ)
                    distanceToWall = distanceToWallY;
                else
                    distanceToWall = distanceToWallZ;
            }

            //////////
            if (hitWall)
            {

                if (distanceToWall < tempRenderDistance / 15d)
                    shade.Color = 0 | 15 << 4;
                else if (distanceToWall < tempRenderDistance / 14d)
                    shade.Color = 0 | 14 << 4;
                else if (distanceToWall < tempRenderDistance / 13d)
                    shade.Color = 0 | 13 << 4;
                else if (distanceToWall < tempRenderDistance / 12d)
                    shade.Color = 0 | 12 << 4;
                else if (distanceToWall < tempRenderDistance / 11d)
                    shade.Color = 0 | 11 << 4;
                else if (distanceToWall < tempRenderDistance / 10d)
                    shade.Color = 0 | 10 << 4;
                else if (distanceToWall < tempRenderDistance / 9d)
                    shade.Color = 0 | 9 << 4;
                else if (distanceToWall < tempRenderDistance / 8d)
                    shade.Color = 0 | 8 << 4;
                else if (distanceToWall < tempRenderDistance / 7d)
                    shade.Color = 0 | 7 << 4;
                else if (distanceToWall < tempRenderDistance / 6d)
                    shade.Color = 0 | 6 << 4;
                else if (distanceToWall < tempRenderDistance / 5d)
                    shade.Color = 0 | 5 << 4;
                else if (distanceToWall < tempRenderDistance / 4d)
                    shade.Color = 0 | 4 << 4;
                else if (distanceToWall < tempRenderDistance / 3d)
                    shade.Color = 0 | 3 << 4;
                else if (distanceToWall < tempRenderDistance / 2d)
                    shade.Color = 0 | 2 << 4;
                else
                    shade.Color = 0 | 1 << 4;

                // отрисовка рёбер блоков
                /*if (distanceToWall < tempRenderDistance &&
                    ((testZ - (PlayerZ + PlayerCurrentHeight + rayZ * distanceToWall) > -0.05) && (testX - (PlayerX + rayX * distanceToWall) > -0.05) ||
                    (testZ - (PlayerZ + PlayerCurrentHeight + rayZ * distanceToWall) > -0.05) && (testY - (PlayerY + rayY * distanceToWall) > -0.05) ||
                    (testZ - (PlayerZ + PlayerCurrentHeight + rayZ * distanceToWall) > -0.05) && (testX - (PlayerX + rayX * distanceToWall) + 1 < 0.05) ||
                    (testZ - (PlayerZ + PlayerCurrentHeight + rayZ * distanceToWall) > -0.05) && (testY - (PlayerY + rayY * distanceToWall) + 1 < 0.05) ||
                    (testZ - (PlayerZ + PlayerCurrentHeight + rayZ * distanceToWall) + 1 < 0.05) && (testX - (PlayerX + rayX * distanceToWall) > -0.05) ||
                    (testZ - (PlayerZ + PlayerCurrentHeight + rayZ * distanceToWall) + 1 < 0.05) && (testY - (PlayerY + rayY * distanceToWall) > -0.05) ||
                    (testZ - (PlayerZ + PlayerCurrentHeight + rayZ * distanceToWall) + 1 < 0.05) && (testX - (PlayerX + rayX * distanceToWall) + 1 < 0.05) ||
                    (testZ - (PlayerZ + PlayerCurrentHeight + rayZ * distanceToWall) + 1 < 0.05) && (testY - (PlayerY + rayY * distanceToWall) + 1 < 0.05)))
                    shade.Color = (short)((shade.Color & 15) | (((shade.Color >> 4) - 2) >= 0 ? ((shade.Color >> 4) - 2) : 0) << 4);
                else if (distanceToWall < tempRenderDistance &&
                        ((testX - (PlayerX + rayX * distanceToWall) > -0.05 && testY - (PlayerY + rayY * distanceToWall) > -0.05) ||
                            (testX - (PlayerX + rayX * distanceToWall) + 1 < 0.05 && testY - (PlayerY + rayY * distanceToWall) > -0.05) ||
                            (testX - (PlayerX + rayX * distanceToWall) > -0.05 && testY - (PlayerY + rayY * distanceToWall) + 1 < 0.05) ||
                            (testX - (PlayerX + rayX * distanceToWall) + 1 < 0.05 && testY - (PlayerY + rayY * distanceToWall) + 1 < 0.05)))
                    shade.Color = (short)((shade.Color & 15) | (((shade.Color >> 4) - 2) >= 0 ? ((shade.Color >> 4) - 2) : 0) << 4);*/
            }

            result[y * FrameWidth + x] = shade;
            return result;
        }
        static double GetDistance(double x1, double y1, double z1, double x2, double y2, double z2)
        {
            return Math.Sqrt((x2 - x1) * (x2 - x1) + (y2 - y1) * (y2 - y1) + (z2 - z1) * (z2 - z1));
        }
        static void Physics()
        {
            DateTime dateTimeFrom = DateTime.Now;

            while (true)
            {
                if (Load.ThreadState != ThreadState.Suspended) continue;

                DateTime dateTimeTo = DateTime.Now;
                double elapsedTime = (dateTimeTo - dateTimeFrom).TotalSeconds;
                dateTimeFrom = DateTime.Now;

                //защита от выхода за пределы карты
                if (PlayerX + Math.Sin(PlayerAngleH) * PlayerCurrentSpeed < 0.01) PlayerX -= Math.Sin(PlayerAngleH) * PlayerCurrentSpeed - 0.01;
                if (PlayerY + Math.Cos(PlayerAngleH) * PlayerCurrentSpeed < 0.01) PlayerY -= Math.Cos(PlayerAngleH) * PlayerCurrentSpeed - 0.01;
                if (PlayerZ - (PlayerFallingSpeed + Gravity * 0.001) * 0.01 < 0.01) PlayerZ -= (PlayerFallingSpeed + Gravity * 0.001) * 0.01 + 0.01;

                if (PlayerX + Math.Sin(PlayerAngleH) * PlayerCurrentSpeed > MapX - 0.01) PlayerX -= Math.Sin(PlayerAngleH) * PlayerCurrentSpeed + 0.01;
                if (PlayerY + Math.Cos(PlayerAngleH) * PlayerCurrentSpeed > MapY - 0.01) PlayerY -= Math.Cos(PlayerAngleH) * PlayerCurrentSpeed + 0.01;
                if (PlayerZ - (PlayerFallingSpeed + Gravity * 0.001) * 0.01 + PlayerCurrentHeight + PlayerRadius > MapZ - 0.01) PlayerZ -= (PlayerFallingSpeed + Gravity * 0.001) * 0.01 + PlayerCurrentHeight + PlayerRadius - 0.01;

                //игрок не должен в плотную подходить к блокам
                //double tempPlayerRadiusX = PlayerRadius;
                //double tempPlayerRadiusY = PlayerRadius;
                //double tempPlayerRadiusSideX =  PlayerRadius;
                //double tempPlayerRadiusSideY =  -PlayerRadius;

                /*if (PlayerAngleH > Math.PI && PlayerAngleH < Math.PI * 2)
                    tempPlayerRadiusX = -PlayerRadius;

                if (PlayerAngleH > Math.PI * 0.5 && PlayerAngleH < Math.PI * 1.5)
                    tempPlayerRadiusY = -PlayerRadius;*/

                /*if (PlayerAngleH > Math.PI * 0.5 && PlayerAngleH < Math.PI * 1.5)
                    tempPlayerRadiusSideX = -PlayerRadius;

                if (PlayerAngleH > Math.PI && PlayerAngleH < Math.PI * 2)
                    tempPlayerRadiusSideY = PlayerRadius;*/

                /*if (PlayerSpeed < 0)
                {
                    tempPlayerRadiusX = -tempPlayerRadiusX;
                    tempPlayerRadiusY = -tempPlayerRadiusY;
                }*/

                /*if (PlayerSideSpeed < 0)
                {
                    tempPlayerRadiusSideX = -tempPlayerRadiusSideX;
                    tempPlayerRadiusSideY = -tempPlayerRadiusSideY;
                }*/

                //движение вперед назад
                bool isBlockX = false;
                bool isBlockY = false;

                for (double z = PlayerZ; z <= PlayerZ + PlayerCurrentHeight + PlayerRadius; z++)
                {
                    if (Map[(int)(PlayerX + Math.Sin(PlayerAngleH) * PlayerCurrentSpeed/* + tempPlayerRadiusX*/), (int)(PlayerY), (int)(z)] != 0)
                        isBlockX = true;
                    if (Map[(int)(PlayerX), (int)(PlayerY + Math.Cos(PlayerAngleH) * PlayerCurrentSpeed/* + tempPlayerRadiusY*/), (int)(z)] != 0)
                        isBlockY = true;
                }

                if (!isBlockX)
                    PlayerX += Math.Sin(PlayerAngleH) * PlayerCurrentSpeed;
                else //отскок
                    PlayerX += Math.Sin(PlayerAngleH) * -PlayerCurrentSpeed * PlayerBounce;
                if (!isBlockY)
                    PlayerY += Math.Cos(PlayerAngleH) * PlayerCurrentSpeed;
                else
                    PlayerY += Math.Cos(PlayerAngleH) * -PlayerCurrentSpeed * PlayerBounce;

                //движение влево вправо
                isBlockX = false;
                isBlockY = false;

                for (double z = PlayerZ; z <= PlayerZ + PlayerCurrentHeight + PlayerRadius; z++)
                {
                    if (Map[(int)(PlayerX + Math.Sin(PlayerAngleH + Math.PI / 2d) * PlayerCurrentSideSpeed/* + tempPlayerRadiusSideX*/), (int)(PlayerY), (int)(z)] != 0)
                        isBlockX = true;
                    if (Map[(int)(PlayerX), (int)(PlayerY + Math.Cos(PlayerAngleH + Math.PI / 2d) * PlayerCurrentSideSpeed/* + tempPlayerRadiusSideY*/), (int)(z)] != 0)
                        isBlockY = true;
                }

                if (!isBlockX)
                    PlayerX += Math.Sin(PlayerAngleH + Math.PI / 2d) * PlayerCurrentSideSpeed;
                else //отскок
                    PlayerX += Math.Sin(PlayerAngleH + Math.PI / 2d) * -PlayerCurrentSideSpeed * PlayerBounce;
                if (!isBlockY)
                    PlayerY += Math.Cos(PlayerAngleH + Math.PI / 2d) * PlayerCurrentSideSpeed;
                else
                    PlayerY += Math.Cos(PlayerAngleH + Math.PI / 2d) * -PlayerCurrentSideSpeed * PlayerBounce;

                //замедление вперед назад
                if (PlayerCurrentSpeed > 0 && Map[(int)PlayerX, (int)PlayerY, (int)(PlayerZ - 1)] != 0)
                    PlayerCurrentSpeed -= !isScopeActivated ?
                        !isCrouch ? PlayerCurrentSpeed * 0.01 : PlayerCurrentSpeed * 0.01 * PlayerCrouchSpeedMult :
                        !isCrouch ? PlayerCurrentSpeed * 0.01 * PlayerScopeSpeedMult : PlayerCurrentSpeed * 0.01 * PlayerCrouchSpeedMult * PlayerScopeSpeedMult;
                else if (PlayerCurrentSpeed < 0 && Map[(int)PlayerX, (int)PlayerY, (int)(PlayerZ - 1)] != 0)
                    PlayerCurrentSpeed -= !isScopeActivated ?
                        !isCrouch ? PlayerCurrentSpeed * 0.01 * PlayerBackSpeedMult : PlayerCurrentSpeed * 0.01 * PlayerBackSpeedMult * PlayerCrouchSpeedMult :
                        !isCrouch ? PlayerCurrentSpeed * 0.01 * PlayerBackSpeedMult * PlayerScopeSpeedMult : PlayerCurrentSpeed * 0.01 * PlayerBackSpeedMult * PlayerCrouchSpeedMult * PlayerScopeSpeedMult;
                else if (Math.Abs(PlayerCurrentSpeed) > 0.1)
                    PlayerCurrentSpeed = 0;

                //замедление влево вправо
                if (PlayerCurrentSideSpeed > 0 && Map[(int)PlayerX, (int)PlayerY, (int)(PlayerZ - 1)] != 0)
                    PlayerCurrentSideSpeed -= !isScopeActivated ?
                        !isCrouch ? PlayerCurrentSideSpeed * 0.01 : PlayerCurrentSideSpeed * 0.01 * PlayerCrouchSpeedMult :
                        !isCrouch ? PlayerCurrentSideSpeed * 0.01 * PlayerScopeSpeedMult : PlayerCurrentSideSpeed * 0.01 * PlayerCrouchSpeedMult * PlayerScopeSpeedMult;
                else if (PlayerCurrentSideSpeed < 0 && Map[(int)PlayerX, (int)PlayerY, (int)(PlayerZ - 1)] != 0)
                    PlayerCurrentSideSpeed -= !isScopeActivated ?
                        !isCrouch ? PlayerCurrentSideSpeed * 0.01 : PlayerCurrentSideSpeed * 0.01 * PlayerCrouchSpeedMult :
                        !isCrouch ? PlayerCurrentSideSpeed * 0.01 * PlayerScopeSpeedMult : PlayerCurrentSideSpeed * 0.01 * PlayerCrouchSpeedMult * PlayerScopeSpeedMult;
                else if (Math.Abs(PlayerCurrentSideSpeed) > 0.1)
                    PlayerCurrentSideSpeed = 0;

                //падение и взлёт
                if (PlayerFallingSpeed >= 0 && Map[(int)PlayerX, (int)PlayerY, (int)(PlayerZ - (PlayerFallingSpeed + Gravity * 0.001) * 0.01)] == 0)
                {
                    PlayerFallingSpeed += Gravity * 0.001;
                    PlayerZ -= PlayerFallingSpeed * 0.01;
                }
                else if (PlayerFallingSpeed < 0 && Map[(int)PlayerX, (int)PlayerY, (int)(PlayerZ - (PlayerFallingSpeed + Gravity * 0.001) * 0.01 + PlayerCurrentHeight + PlayerRadius)] == 0)
                {
                    PlayerFallingSpeed += Gravity * 0.001;
                    PlayerZ -= PlayerFallingSpeed * 0.01;
                }
                else //прекращение падения или отскок
                {
                    if (Math.Abs(PlayerFallingSpeed) > 0.1)
                        PlayerFallingSpeed = -PlayerFallingSpeed * Gravity * PlayerBounce;
                    else
                        PlayerFallingSpeed = 0;
                }

                //приседание или подъём
                if (isCrouch && PlayerCurrentHeight > PlayerCrouchHeihgt)
                {
                    if (PlayerCurrentHeight - PlayerCrouchHeihgt > 0.01)
                        PlayerCurrentHeight -= (PlayerHeight - PlayerCrouchHeihgt) * 0.01;
                    else
                        PlayerCurrentHeight = PlayerCrouchHeihgt;
                }
                else if (!isCrouch && PlayerCurrentHeight < PlayerHeight && Map[(int)PlayerX, (int)PlayerY, (int)(PlayerZ + PlayerCurrentHeight + (PlayerHeight - PlayerCrouchHeihgt) * 0.01 + PlayerRadius)] == 0)
                {
                    if (PlayerHeight - PlayerCurrentHeight > 0.01)
                        PlayerCurrentHeight += (PlayerHeight - PlayerCrouchHeihgt) * 0.01;
                    else
                        PlayerCurrentHeight = PlayerHeight;
                }

                //отдача и возврат оружия
                if (isRecoil && Math.Abs(WeaponRecoilSpeedCurrent) > 0.1)
                {
                    WeaponRecoilAngle += WeaponRecoilSpeedCurrent * 0.1;
                    WeaponRecoilSpeedCurrent -= WeaponRecoilSpeedCurrent * 0.1;
                }
                else
                {
                    WeaponRecoilSpeedCurrent = 0;
                    isRecoil = false;
                }

                if (!isRecoil && Math.Abs(WeaponRecoilBackSpeedCurrent) > 0.01)
                {
                    WeaponRecoilAngle += WeaponRecoilBackSpeedCurrent * 0.01;
                    WeaponRecoilBackSpeedCurrent -= WeaponRecoilBackSpeedCurrent * 0.01;
                }
                else
                {
                    if (!isRecoil)
                    {
                        WeaponRecoilBackSpeedCurrent = 0;
                        PlayerAngleV += WeaponRecoilAngle;
                        WeaponRecoilAngle = 0;
                    }
                }

                Thread.Sleep(elapsedTime < 1 ? 1 : 0);
            }
        }
        static void Mouse()
        {
            Point centre = new Point(100, 100);
            Cursor.Position = centre;
            while (true)
            {
                if (isReleaseMouse)
                {
                    continue;
                }

                int mouseXAfter = Cursor.Position.X;
                int mouseYAfter = Cursor.Position.Y;
                int mouseMoveX = centre.X - mouseXAfter;
                int mouseMoveY = centre.Y - mouseYAfter;

                Cursor.Position = centre;

                if (PlayerAngleH + (!isScopeActivated ? mouseMoveX * MouseSensitivityH : mouseMoveX * MouseSensitivityH / ScopeCameraSpeedMult) < 0)
                    PlayerAngleH += (!isScopeActivated ? mouseMoveX * MouseSensitivityH : mouseMoveX * MouseSensitivityH / ScopeCameraSpeedMult) + Math.PI * 2;
                else if (PlayerAngleH + (!isScopeActivated ? mouseMoveX * MouseSensitivityH : mouseMoveX * MouseSensitivityH / ScopeCameraSpeedMult) > Math.PI * 2)
                    PlayerAngleH += (!isScopeActivated ? mouseMoveX * MouseSensitivityH : mouseMoveX * MouseSensitivityH / ScopeCameraSpeedMult) - Math.PI * 2;
                else
                    PlayerAngleH += !isScopeActivated ? mouseMoveX * MouseSensitivityH : mouseMoveX * MouseSensitivityH / ScopeCameraSpeedMult;


                if (PlayerAngleV > -Math.PI / 2d)
                    PlayerAngleV += !isScopeActivated ? mouseMoveY * MouseSensitivityV : mouseMoveY * MouseSensitivityV / ScopeCameraSpeedMult;
                else
                    PlayerAngleV = -Math.PI / 2d;

                if (PlayerAngleV < Math.PI / 2d)
                    PlayerAngleV += !isScopeActivated ? mouseMoveY * MouseSensitivityV : mouseMoveY * MouseSensitivityV / ScopeCameraSpeedMult;
                else
                    PlayerAngleV = Math.PI / 2d;
            }
        }
        static void Keyboard0()
        {
            /*switch (GraphicsLevel)
            {
                case 1: // Ultra
                    break;
                case 2: // High
                    break;
                case 3: // Medium
                    break;
                case 4: // Low
                    break;
                case 5: // Awful
                    break;
            }*/

            while (true)
            {
                if (GetAsyncKeyState(27) != 0) // Esc Выход
                {
                    System.Environment.Exit(0);
                }
                if (GetAsyncKeyState(116) != 0) // F5 Перезапуск
                {
                    if (Load.ThreadState == ThreadState.Suspended)
                    {
                        Restart();
                    }
                    Thread.Sleep(1000);
                }
                if (GetAsyncKeyState(117) != 0) // F6 Переключение формул расчёта лучей
                {
                    isFishEye = !isFishEye;
                    Thread.Sleep(500);
                }
                if (GetAsyncKeyState(118) != 0) // F7 Переключение движка обнаружения стен
                {
                    isNewEngine = !isNewEngine;
                    Thread.Sleep(500);
                }
                if (GetAsyncKeyState(115) != 0) // F4  Освободить курсор
                {
                    isReleaseMouse = !isReleaseMouse;
                    Thread.Sleep(500);
                }
                if (GetAsyncKeyState(192) != 0 && GraphicsLevel != 1) // Тильда Режим апскейла
                {
                    GraphicsMode = GraphicsMode == 3 ? 1 : GraphicsMode + 1;
                    Thread.Sleep(500);
                }
                if (GetAsyncKeyState(112) != 0) // F1 Уровень качества графики
                {
                    /*switch (GraphicsLevel)
                    {
                        case 5: // Ultra
                            GraphicsLevel = 1;
                            break;
                        case 1: // High
                            GraphicsLevel = 2;
                            break;
                        case 2: // Medium
                            GraphicsLevel = 3;
                            break;
                        case 3: // Low
                            GraphicsLevel = 4;
                            break;
                        case 4: // Awful
                            GraphicsLevel = 5;
                            break;
                    }*/

                    GraphicsLevel = GraphicsLevel == 2 ? 1 : GraphicsLevel + 1;
                    Thread.Sleep(500);
                }
                if (GetAsyncKeyState(122) != 0) // F11 Полноэкранный режим
                {
                    isFullScreenChanged = !isFullScreenChanged;
                }
                if (!isScopeActivated & GetAsyncKeyState(113) != 0) // F2 Уменьшить угол обзора
                {
                    if (hFov > Math.PI / 6d)
                    {
                        hFov -= 0.001;
                        vFov = hFov / AspectRatio;
                        isFovChanged = true;
                        Thread.Sleep(1);
                    }
                }
                if (!isScopeActivated & GetAsyncKeyState(114) != 0) // F3 Увеличить угол обзора
                {
                    if (hFov < Math.PI / 1.5d)
                    {
                        hFov += 0.001;
                        vFov = hFov / AspectRatio;
                        isFovChanged = true;
                        Thread.Sleep(1);
                    }
                }
            }
        }
        static void Keyboard1()
        {
            while (true)
            {
                if (GetAsyncKeyState(87) != 0) // W Вперед
                {
                    if (PlayerCurrentSpeed < PlayerSpeed && Map[(int)PlayerX, (int)PlayerY, (int)PlayerZ - 1] != 0)
                        PlayerCurrentSpeed += PlayerSpeed * 0.01;
                }
                if (GetAsyncKeyState(83) != 0) // S Назад
                {
                    if (PlayerCurrentSpeed > -PlayerSpeed && Map[(int)PlayerX, (int)PlayerY, (int)PlayerZ - 1] != 0)
                        PlayerCurrentSpeed -= PlayerSpeed * 0.01;
                }
                if (GetAsyncKeyState(65) != 0) // A Влево
                {
                    if (PlayerCurrentSideSpeed < PlayerSideSpeed && Map[(int)PlayerX, (int)PlayerY, (int)PlayerZ - 1] != 0)
                        PlayerCurrentSideSpeed += PlayerSideSpeed * 0.01;
                }
                if (GetAsyncKeyState(68) != 0) // D Вправо
                {
                    if (PlayerCurrentSideSpeed > -PlayerSideSpeed && Map[(int)PlayerX, (int)PlayerY, (int)PlayerZ - 1] != 0)
                        PlayerCurrentSideSpeed -= PlayerSideSpeed * 0.01;
                }

                if (GetAsyncKeyState(38) != 0) // (камера) Стрелка вверх
                {
                    if (PlayerAngleV > -Math.PI / 2d)
                        PlayerAngleV += !isScopeActivated ? 0.0005 : 0.0005 / ScopeCameraSpeedMult;
                    else
                        PlayerAngleV = -Math.PI / 2d;

                    if (PlayerAngleV < Math.PI / 2d)
                        PlayerAngleV += !isScopeActivated ? 0.0005 : 0.0005 / ScopeCameraSpeedMult;
                    else
                        PlayerAngleV = Math.PI / 2d;
                }
                if (GetAsyncKeyState(40) != 0) // (камера) Стрелка вниз
                {
                    if (PlayerAngleV > -Math.PI / 2d)
                        PlayerAngleV -= !isScopeActivated ? 0.0005 : 0.0005 / ScopeCameraSpeedMult;
                    else
                        PlayerAngleV = -Math.PI / 2d;

                    if (PlayerAngleV < Math.PI / 2d)
                        PlayerAngleV -= !isScopeActivated ? 0.0005 : 0.0005 / ScopeCameraSpeedMult;
                    else
                        PlayerAngleV = Math.PI / 2d;
                }
                if (GetAsyncKeyState(37) != 0) // (камера) Стрелка влево
                {
                    if (PlayerAngleH + (!isScopeActivated ? 0.001 : 0.001 / ScopeCameraSpeedMult) < 0)
                        PlayerAngleH += (!isScopeActivated ? 0.001 : 0.001 / ScopeCameraSpeedMult) + Math.PI * 2;
                    else if (PlayerAngleH + (!isScopeActivated ? 0.001 : 0.001 / ScopeCameraSpeedMult) > Math.PI * 2)
                        PlayerAngleH += (!isScopeActivated ? 0.001 : 0.001 / ScopeCameraSpeedMult) - Math.PI * 2;
                    else
                        PlayerAngleH += !isScopeActivated ? 0.001 : 0.001 / ScopeCameraSpeedMult;
                }
                if (GetAsyncKeyState(39) != 0) // (камера) Стрелка вправо
                {
                    if (PlayerAngleH - (!isScopeActivated ? 0.001 : 0.001 / ScopeCameraSpeedMult) < 0)
                        PlayerAngleH -= (!isScopeActivated ? 0.001 : 0.001 / ScopeCameraSpeedMult) + Math.PI * 2;
                    else if (PlayerAngleH - (!isScopeActivated ? 0.001 : 0.001 / ScopeCameraSpeedMult) > Math.PI * 2)
                        PlayerAngleH -= (!isScopeActivated ? 0.001 : 0.001 / ScopeCameraSpeedMult) - Math.PI * 2;
                    else
                        PlayerAngleH -= !isScopeActivated ? 0.001 : 0.001 / ScopeCameraSpeedMult;
                }

                Thread.Sleep(1);
            }
        }
        static void Keyboard2()
        {
            while (true)
            {
                if (GetAsyncKeyState(32) != 0) // Space Прыжок
                {
                    if (PlayerCurrentHeight == PlayerHeight && Map[(int)PlayerX, (int)PlayerY, (int)PlayerZ - 1] != 0)
                    {
                        PlayerFallingSpeed = -PlayerJumpStrength;
                        Thread.Sleep(500);
                    }
                    else
                    {
                        isCrouch = false;
                        Thread.Sleep(300);
                    }
                }
                if (GetAsyncKeyState(17) != 0) // Control Приседание
                {
                    isCrouch = !isCrouch;
                    Thread.Sleep(300);
                }
            }
        }
        static void Keyboard3()
        {
            while (true)
            {
                if (GetAsyncKeyState(2) != 0 && !isReleaseMouse) // ПКМ Прицел
                {
                    isScope = !isScope;
                    Thread.Sleep(300);
                }
            }
        }
        static void Keyboard4()
        {
            while (true)
            {
                if (GetAsyncKeyState(1) != 0 && !isReleaseMouse) // ЛКМ Убрать блок
                {
                    double dh;
                    double dv;
                    if (isFishEye)
                    {
                        dh = Math.Tan(hFovCurrent / FrameWidth * (FrameWidth / 2d - (FrameWidth / 2d)));
                        dv = Math.Tan(vFovCurrent / FrameHeight * (FrameHeight / 2d - (FrameHeight / 2d)));
                    }
                    else
                    {
                        dh = Math.Tan(hFovCurrent / 2d) * (1 - (FrameWidth / 2d) / (FrameWidth / 2d));
                        dv = Math.Tan(vFovCurrent / 2d) * (1 - (FrameHeight / 2d) / (FrameHeight / 2d));
                    }

                    double sinH = Math.Sin(PlayerAngleH);
                    double cosH = Math.Cos(PlayerAngleH);

                    double sinV = Math.Sin(PlayerAngleV + WeaponRecoilAngle);
                    double cosV = Math.Cos(PlayerAngleV + WeaponRecoilAngle);

                    double rayX = sinH * cosV + dh * cosH - dv * sinH * sinV;
                    double rayY = cosH * cosV - dh * sinH - dv * cosH * sinV;
                    double rayZ = sinV + dv * cosV;

                    double distanceToWall = 0;
                    bool hitWall = false;

                    while (!hitWall && distanceToWall < RenderDistance * ScopeMult)
                    {
                        distanceToWall += RenderQuality;

                        int testX = (int)(PlayerX + rayX * distanceToWall);
                        int testY = (int)(PlayerY + rayY * distanceToWall);
                        int testZ = (int)(PlayerZ + PlayerCurrentHeight + rayZ * distanceToWall);

                        if (Math.Round(PlayerX + rayX * distanceToWall - 0.5) < 0 || testX >= RenderDistance * ScopeMult + PlayerX || testX >= MapX ||
                            Math.Round(PlayerY + rayY * distanceToWall - 0.5) < 0 || testY >= RenderDistance * ScopeMult + PlayerY || testY >= MapY ||
                            Math.Round(PlayerZ + PlayerCurrentHeight + rayZ * distanceToWall) < 0 || testZ >= RenderDistance * ScopeMult + PlayerZ || testZ >= MapZ)
                        {
                            break;
                        }
                        else
                        {
                            if (Map[testX, testY, testZ] != 0 && Map[testX, testY, testZ] != 1)
                            {
                                Map[testX, testY, testZ] = 0;
                                break;
                            }
                        }
                    }

                    WeaponRecoilSpeedCurrent += WeaponRecoil * Rand.Next(200, 501) * 0.001;
                    WeaponRecoilBackSpeedCurrent += -WeaponRecoilSpeedCurrent * 0.75;
                    isRecoil = true;

                    Thread.Sleep(500);
                }

                if (GetAsyncKeyState(4) != 0 && !isReleaseMouse) // СКМ Поставить блок
                {
                    double dh;
                    double dv;
                    if (isFishEye)
                    {
                        dh = Math.Tan(hFovCurrent / FrameWidth * (FrameWidth / 2d - (FrameWidth / 2d)));
                        dv = Math.Tan(vFovCurrent / FrameHeight * (FrameHeight / 2d - (FrameHeight / 2d)));
                    }
                    else
                    {
                        dh = Math.Tan(hFovCurrent / 2d) * (1 - (FrameWidth / 2d) / (FrameWidth / 2d));
                        dv = Math.Tan(vFovCurrent / 2d) * (1 - (FrameHeight / 2d) / (FrameHeight / 2d));
                    }

                    double sinH = Math.Sin(PlayerAngleH);
                    double cosH = Math.Cos(PlayerAngleH);

                    double sinV = Math.Sin(PlayerAngleV + WeaponRecoilAngle);
                    double cosV = Math.Cos(PlayerAngleV + WeaponRecoilAngle);

                    double rayX = sinH * cosV + dh * cosH - dv * sinH * sinV;
                    double rayY = cosH * cosV - dh * sinH - dv * cosH * sinV;
                    double rayZ = sinV + dv * cosV;

                    double distanceToWall = 0;
                    bool hitWall = false;

                    while (!hitWall && distanceToWall < RenderDistance * ScopeMult)
                    {
                        distanceToWall += RenderQuality;

                        int testX = (int)(PlayerX + rayX * distanceToWall);
                        int testY = (int)(PlayerY + rayY * distanceToWall);
                        int testZ = (int)(PlayerZ + PlayerCurrentHeight + rayZ * distanceToWall);

                        if (Math.Round(PlayerX + rayX * distanceToWall - 0.5) < 0 || testX >= RenderDistance * ScopeMult + PlayerX || testX >= MapX ||
                            Math.Round(PlayerY + rayY * distanceToWall - 0.5) < 0 || testY >= RenderDistance * ScopeMult + PlayerY || testY >= MapY ||
                            Math.Round(PlayerZ + PlayerCurrentHeight + rayZ * distanceToWall) < 0 || testZ >= RenderDistance * ScopeMult + PlayerZ || testZ >= MapZ)
                        {
                            break;
                        }
                        else
                        {
                            if (Map[testX, testY, testZ] != 0/* && ((int)PlayerX != (int)(PlayerX + rayX * (distanceToWall - 1)) && (int)PlayerY != (int)(PlayerY + rayY * (distanceToWall - 1)) && (int)PlayerZ != (int)(PlayerZ + PlayerCurrentHeight + rayZ * (distanceToWall - 1)))*/)
                            {
                                Map[(int)(PlayerX + rayX * (distanceToWall - 0.5)), (int)(PlayerY + rayY * (distanceToWall - 0.5)), (int)(PlayerZ + PlayerCurrentHeight + rayZ * (distanceToWall - 0.5))] = 3;
                                break;
                            }
                        }
                    }

                    /*WeaponRecoilSpeedCurrent += WeaponRecoil * Rand.Next(500, 1001) * 0.001;
                    WeaponRecoilBackSpeedCurrent += -WeaponRecoilSpeedCurrent * 0.75;
                    isRecoil = true;*/

                    Thread.Sleep(500);
                }
            }
        }
        static void GenerateMap()
        {
            Map = new byte[MapX, MapY, MapZ];
            for (int x = 0; x < MapX; x++)
            {
                for (int y = 0; y < MapY; y++)
                {
                    for (int z = 0; z < MapZ; z++)
                    {
                        if (z == 0)
                            Map[x, y, z] = 1; // Пол

                        /*else if ((x == 0 && y == 0) || (x == MapX - 1 && y == 0) || (x == 0 && y == MapY - 1) || (x == MapX - 1 && y == MapY - 1))
                            Map[x, y, z] = 1; // Угловые колонны до потолка

                        else if (z == MapZ - 1)
                            Map[x, y, z] = 1; // Потолок*/

                        /*else if ((x == 0 || x == MapX - 1 || y == 0 || y == MapY - 1) && z <= 2)
                            Map[x, y, z] = 1; // Стены из нерушимых блоков в 2 ряда*/
                        else if (Rand.Next(101) <= 1 && Map[x, y, z - 1] == 1)
                            Map[x, y, z] = 2; // Случайный блок на полу

                        else if (Rand.Next(101) <= 1 && Map[x, y, z - 1] == 1)
                            Map[x, y, z] = 4; // Случайный цилиндр на полу

                        else if (Rand.Next(101) <= 99 * (1.5 - (z / (double)MapZ)) && Map[x, y, z - 1] == 4)
                            Map[x, y, z] = 4; // Случайный цилиндр на цилиндре

                        else
                            Map[x, y, z] = 0; // Иначе свободное пространство
                    }
                }
            }
            for (int x = 0; x < MapX; x++)
            {
                for (int y = 0; y < MapY; y++)
                {
                    for (int z = 3; z < MapZ; z++)
                    {
                        if (Map[x, y, z] == 4)
                        {
                            for (int i = 0; i < 8; i++)
                            {
                                if (Rand.Next(101) <= (z / (double)MapZ) * 95) // Случайный шар в сторону от цилиндра
                                {
                                    try
                                    {
                                        switch (i)
                                        {
                                            case 0:
                                                {
                                                    if (Map[x - 1, y - 1, z] == 0)
                                                        Map[x - 1, y - 1, z] = 3;
                                                    break;
                                                }
                                            case 1:
                                                {
                                                    if (Map[x, y - 1, z] == 0)
                                                        Map[x, y - 1, z] = 3;
                                                    break;
                                                }
                                            case 2:
                                                {
                                                    if (Map[x + 1, y - 1, z] == 0)
                                                        Map[x + 1, y - 1, z] = 3;
                                                    break;
                                                }
                                            case 3:
                                                {
                                                    if (Map[x + 1, y, z] == 0)
                                                        Map[x + 1, y, z] = 3;
                                                    break;
                                                }
                                            case 4:
                                                {
                                                    if (Map[x + 1, y + 1, z] == 0)
                                                        Map[x + 1, y + 1, z] = 3;
                                                    break;
                                                }
                                            case 5:
                                                {
                                                    if (Map[x, y + 1, z] == 0)
                                                        Map[x, y + 1, z] = 3;
                                                    break;
                                                }
                                            case 6:
                                                {
                                                    if (Map[x - 1, y + 1, z] == 0)
                                                        Map[x - 1, y + 1, z] = 3;
                                                    break;
                                                }
                                            case 7:
                                                {
                                                    if (Map[x - 1, y, z] == 0)
                                                        Map[x - 1, y, z] = 3;
                                                    break;
                                                }
                                        }
                                    }
                                    catch { continue; }
                                }
                            }
                            try
                            {
                                if (Map[x, y, z + 1] == 0)
                                {
                                    Map[x, y, z + 1] = 3; // Шар сверху столба цилиндров
                                }
                            }
                            catch { continue; }
                        }
                    }
                }
            }
        }
        static void LocatePlayer()
        {
            bool isLocated = false;
            while (!isLocated)
            {
                bool isBlock = false;

                double tempX = Rand.Next(1, MapX * 10) / 10d;
                double tempY = Rand.Next(1, MapY * 10) / 10d;
                double tempZ = Rand.Next(1, (int)((MapZ - PlayerCurrentHeight - PlayerRadius) * 10)) / 10d;

                for (double z = tempZ; z <= tempZ + PlayerHeight + PlayerRadius; z++)
                {
                    if (Map[(int)(tempX), (int)(tempY), (int)(z)] != 0)
                    {
                        isBlock = true;
                        break;
                    }
                }
                if (isBlock) continue;
                else
                {
                    isLocated = true;
                    PlayerX = tempX;
                    PlayerY = tempY;
                    PlayerZ = tempZ;
                }
            }

            PlayerAngleH = Rand.Next(0, 360) * Math.PI / 180d;
            PlayerAngleV = Rand.Next(-16, 15) * Math.PI / 180d;
            PlayerCurrentSpeed = 0;
            PlayerCurrentSideSpeed = 0;
            PlayerFallingSpeed = 0;
        }
        static void GenerateSkyFar()
        {
            skyTextureFar = new byte[FrameHeight * 3, FrameWidth * 4];
            for (int y = 0; y < skyTextureFar.GetLength(0); y++)
            {
                for (int x = 0; x < skyTextureFar.GetLength(1); x++)
                {
                    if (Rand.Next(0, 100) == 1)
                        skyTextureFar[y, x] = 39;
                    else if (Rand.Next(0, 1000) == 1)
                        skyTextureFar[y, x] = 42;
                    else
                        skyTextureFar[y, x] = 32;
                }
            }
        }
        static void GenerateSkyNear()
        {
            skyTextureNear = new byte[FrameHeight * 3, FrameWidth * 4];
            for (int y = 0; y < skyTextureNear.GetLength(0); y++)
            {
                for (int x = 0; x < skyTextureNear.GetLength(1); x++)
                {
                    skyTextureNear[y, x] = 0;
                }
            } 

            try
            {
                byte[,] moon = {{0,0,0,0,0,0,0,0,1,1,1,1,1,0,0,0},
                                {0,0,0,0,0,0,1,1,1,1,1,1,1,1,0,0},
                                {0,0,0,0,0,1,1,1,1,1,1,1,1,0,0,0},
                                {0,0,0,0,1,1,1,1,1,1,1,1,0,0,0,0},
                                {0,0,0,1,1,1,1,1,1,1,1,0,0,0,0,0},
                                {0,0,1,1,1,1,1,1,1,1,0,0,0,0,0,0},
                                {0,0,1,1,1,1,1,1,1,1,0,0,0,0,0,0},
                                {0,0,1,1,1,1,1,1,1,1,0,0,0,0,0,0},
                                {0,0,1,1,1,1,1,1,1,1,0,0,0,0,0,0},
                                {0,0,1,1,1,1,1,1,1,1,0,0,0,0,0,0},
                                {0,0,1,1,1,1,1,1,1,1,0,0,0,0,0,0},
                                {0,0,0,1,1,1,1,1,1,1,1,0,0,0,0,0},
                                {0,0,0,0,1,1,1,1,1,1,1,1,0,0,0,0},
                                {0,0,0,0,0,1,1,1,1,1,1,1,1,0,0,0},
                                {0,0,0,0,0,0,1,1,1,1,1,1,1,1,0,0},
                                {0,0,0,0,0,0,0,0,1,1,1,1,1,0,0,0}};

                int i = Rand.Next((int)(skyTextureNear.GetLength(0) * 0.15), (int)(skyTextureNear.GetLength(0) * 0.25)) - 16;
                int j = Rand.Next(16, skyTextureNear.GetLength(1) - 16);

                for (int y = i; y < i + 16; y++)
                {
                    for (int x = j; x < j + 16; x++)
                    {
                        skyTextureNear[y, x] = moon[y - i, x - j];
                    }
                }
            }
            catch {/*ну и ладно*/}

            //тест: орбита луны
            /*for (int y = 0; y < skyTextureNear.GetLength(0); y++)
            {
                for (int x = 0; x < skyTextureNear.GetLength(1); x++)
                {
                    if (skyTextureNear[y, x] == 32 && (x == j + 8 || y == i + 8))
                        skyTextureNear[y, x] = 177;
                }
            }*/
        }
        static void SkyRotateFar()
        {
            while (true)
            {
                SkyFarTry:
                try
                {
                    skyTextureFarTemp = new byte[FrameHeight * 3, FrameWidth * 4];
                    for (int y = 0; y < skyTextureFarTemp.GetLength(0); y++)
                    {
                        for (int x = 0; x < skyTextureFarTemp.GetLength(1); x++)
                        {
                            if (x + 1 < skyTextureFar.GetLength(1))
                                skyTextureFarTemp[y, x] = skyTextureFar[y, x + 1];
                            else
                                skyTextureFarTemp[y, x] = skyTextureFar[y, 0];
                        }
                    }
                    skyTextureFar = skyTextureFarTemp;
                    Thread.Sleep(600);
                }
                catch
                {
                    goto SkyFarTry;
                }
            }
        }
        static void SkyRotateNear()
        {
            while (true)
            {
                SkyNearTry:
                try
                {
                    skyTextureNearTemp = new byte[FrameHeight * 3, FrameWidth * 4];
                    for (int y = 0; y < skyTextureNearTemp.GetLength(0); y++)
                    {
                        for (int x = 0; x < skyTextureNearTemp.GetLength(1); x++)
                        {
                            if (x + 1 < skyTextureNear.GetLength(1))
                                skyTextureNearTemp[y, x] = skyTextureNear[y, x + 1];
                            else
                                skyTextureNearTemp[y, x] = skyTextureNear[y, 0];
                        }
                    }
                    skyTextureNear = skyTextureNearTemp;
                    Thread.Sleep(100);
                }
                catch
                {
                    goto SkyNearTry;
                }
            }
        }
        static void LightRotate()
        {
            vector3 mapCentre = new vector3(MapX / 2d, MapY / 2d, MapZ / 2d);

            double a = 0;

            while (true)
            {
                LightPosition.X = mapCentre.X + LightRotationRadius * Math.Cos(a);
                LightPosition.Y = mapCentre.Y + LightRotationRadius * Math.Sin(a);
                a += LightRotationSpeed;

                Thread.Sleep(1);
            }
        }

        static void Restart()
        {
            Load.Resume();
            Thread.Sleep(100);
            Console.Clear();

            GenerateMap();
            GenerateSkyFar();
            GenerateSkyNear();

            LocatePlayer();
            PlayerCurrentHeight = PlayerHeight;
            isCrouch = false;
            isScope = false;
            isScopeActivated = false;
            isRecoil = false;

            Load.Suspend();
        }
        static void Loading()
        {
            while (true)
            {
                Console.SetCursorPosition(0, 0);
                Console.CursorVisible = false;
                Console.Write("\n" +
                              "    ' *  \n" +
                              "  '      \n" +
                              "         \n" +
                              "         ");
                Thread.Sleep(100);
                Console.SetCursorPosition(0, 0);
                Console.CursorVisible = false;
                Console.Write("\n" +
                              "    ' '  \n" +
                              "        *\n" +
                              "         \n" +
                              "         ");
                Thread.Sleep(100);
                Console.SetCursorPosition(0, 0);
                Console.CursorVisible = false;
                Console.Write("\n" +
                              "      '  \n" +
                              "        '\n" +
                              "        *\n" +
                              "         ");
                Thread.Sleep(100);
                Console.SetCursorPosition(0, 0);
                Console.CursorVisible = false;
                Console.Write("\n" +
                              "         \n" +
                              "        '\n" +
                              "        '\n" +
                              "      *  ");
                Thread.Sleep(100);
                Console.SetCursorPosition(0, 0);
                Console.CursorVisible = false;
                Console.Write("\n" +
                              "         \n" +
                              "         \n" +
                              "        '\n" +
                              "    * '  ");
                Thread.Sleep(100);
                Console.SetCursorPosition(0, 0);
                Console.CursorVisible = false;
                Console.Write("\n" +
                              "         \n" +
                              "         \n" +
                              "  *      \n" +
                              "    ' '  ");
                Thread.Sleep(100);
                Console.SetCursorPosition(0, 0);
                Console.CursorVisible = false;
                Console.Write("\n" +
                              "         \n" +
                              "  *      \n" +
                              "  '      \n" +
                              "    '    ");
                Thread.Sleep(100);
                Console.SetCursorPosition(0, 0);
                Console.CursorVisible = false;
                Console.Write("\n" +
                              "    *    \n" +
                              "  '      \n" +
                              "  '      \n" +
                              "         ");
                Thread.Sleep(100);
            }
        }
        static void SetConsoleFont(string fontName, short fontSizeY)
        {
            unsafe
            {
                IntPtr hnd = GetStdHandle(STD_OUTPUT_HANDLE);
                if (hnd != INVALID_HANDLE_VALUE)
                {
                    CONSOLE_FONT_INFO_EX info = new CONSOLE_FONT_INFO_EX();
                    info.cbSize = (uint)Marshal.SizeOf(info);

                    CONSOLE_FONT_INFO_EX newInfo = new CONSOLE_FONT_INFO_EX();
                    newInfo.cbSize = (uint)Marshal.SizeOf(newInfo);
                    newInfo.FontFamily = TMPF_TRUETYPE;
                    IntPtr ptr = new IntPtr(newInfo.FaceName);
                    Marshal.Copy(fontName.ToCharArray(), 0, ptr, fontName.Length);

                    newInfo.dwFontSize = new COORD(info.dwFontSize.X, fontSizeY);
                    newInfo.FontWeight = info.FontWeight;
                    SetCurrentConsoleFontEx(hnd, false, ref newInfo);
                }
            }
        }
        static void SetConsoleColorGardient(int r, int g, int b, int stepR, int stepG, int stepB)
        {
            uint R = (uint)r;
            uint G = (uint)g;
            uint B = (uint)b;

            //Гардиент от тёмного к светлому
            SetScreenColorsApp.SetColor((ConsoleColor)0, R, G, B); r += stepR; g += stepG; b += stepB; R = (uint)r; G = (uint)g; B = (uint)b;
            SetScreenColorsApp.SetColor((ConsoleColor)1, R, G, B); r += stepR; g += stepG; b += stepB; R = (uint)r; G = (uint)g; B = (uint)b;
            SetScreenColorsApp.SetColor((ConsoleColor)2, R, G, B); r += stepR; g += stepG; b += stepB; R = (uint)r; G = (uint)g; B = (uint)b;
            SetScreenColorsApp.SetColor((ConsoleColor)3, R, G, B); r += stepR; g += stepG; b += stepB; R = (uint)r; G = (uint)g; B = (uint)b;
            SetScreenColorsApp.SetColor((ConsoleColor)4, R, G, B); r += stepR; g += stepG; b += stepB; R = (uint)r; G = (uint)g; B = (uint)b;
            SetScreenColorsApp.SetColor((ConsoleColor)5, R, G, B); r += stepR; g += stepG; b += stepB; R = (uint)r; G = (uint)g; B = (uint)b;
            SetScreenColorsApp.SetColor((ConsoleColor)6, R, G, B); r += stepR; g += stepG; b += stepB; R = (uint)r; G = (uint)g; B = (uint)b;
            SetScreenColorsApp.SetColor((ConsoleColor)7, R, G, B); r += stepR; g += stepG; b += stepB; R = (uint)r; G = (uint)g; B = (uint)b;
            SetScreenColorsApp.SetColor((ConsoleColor)8, R, G, B); r += stepR; g += stepG; b += stepB; R = (uint)r; G = (uint)g; B = (uint)b;
            SetScreenColorsApp.SetColor((ConsoleColor)9, R, G, B); r += stepR; g += stepG; b += stepB; R = (uint)r; G = (uint)g; B = (uint)b;
            SetScreenColorsApp.SetColor((ConsoleColor)10, R, G, B); r += stepR; g += stepG; b += stepB; R = (uint)r; G = (uint)g; B = (uint)b;
            SetScreenColorsApp.SetColor((ConsoleColor)11, R, G, B); r += stepR; g += stepG; b += stepB; R = (uint)r; G = (uint)g; B = (uint)b;
            SetScreenColorsApp.SetColor((ConsoleColor)12, R, G, B); r += stepR; g += stepG; b += stepB; R = (uint)r; G = (uint)g; B = (uint)b;
            SetScreenColorsApp.SetColor((ConsoleColor)13, R, G, B); r += stepR; g += stepG; b += stepB; R = (uint)r; G = (uint)g; B = (uint)b;
            SetScreenColorsApp.SetColor((ConsoleColor)14, R, G, B); r += stepR; g += stepG; b += stepB; R = (uint)r; G = (uint)g; B = (uint)b;
            SetScreenColorsApp.SetColor((ConsoleColor)15, R, G, B);
        }
    }
}