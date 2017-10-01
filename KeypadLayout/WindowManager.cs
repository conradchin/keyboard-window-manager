using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows;

namespace KeypadLayout
{
    public class WindowManager
    {
        private static IntPtr? _hWndActive;
        private static User32Wrapper.RECT? _rect;

        private const short SWP_NOZORDER = 0X4;
        private const int SWP_SHOWWINDOW = 0x0040;
        private const int SWP_NOACTIVATE = 0x0010;

        public static void TransformWindow(TranslateWindowCommand direction, int rows, int cols)
        {
            if(_hWndActive == null)
            {
                throw new Exception("Error: ForegroundWindow not initialized!");
            }

            Object obj = new object();
            if (!User32Wrapper.GetWindowRect(new HandleRef(obj, _hWndActive.Value), out User32Wrapper.RECT rect))
            {
                Console.WriteLine("Error: Cannot get window dimensions.");
                return;
            }

            if (direction == TranslateWindowCommand.Minimize)
            {
                if(User32Wrapper.IsWindowVisible(_hWndActive.Value))
                {
                    User32Wrapper.MinimizeWindow(_hWndActive.Value);
                }
                else
                {
                    User32Wrapper.NormalizeWindow(_hWndActive.Value);
                }
            }
            else if (direction == TranslateWindowCommand.Maximize)
            {
                User32Wrapper.MaximizeWindow(_hWndActive.Value);
            }
            else
            {
                User32Wrapper.NormalizeWindow(_hWndActive.Value);

                int x = rect.Left;
                int width = rect.Right - rect.Left;
                int y = rect.Top;
                int height = rect.Bottom - rect.Top;

                int offset = CalculateOffsetToClosestStop(rows, cols, x, y, width, height, direction);
                if (offset == -1)
                {
                    return;
                }

                switch (direction)
                {
                    case TranslateWindowCommand.MoveUp:
                        y = y - offset;
                        break;

                    case TranslateWindowCommand.MoveDown:
                        y = y + offset;
                        break;

                    case TranslateWindowCommand.MoveLeft:
                        x = x - offset;
                        break;

                    case TranslateWindowCommand.MoveRight:
                        x = x + offset;
                        break;

                    case TranslateWindowCommand.MoveTopUp:
                        y = y - offset;
                        height = height + offset;
                        break;

                    case TranslateWindowCommand.MoveTopDown:
                        y = y + offset;
                        height = height - offset;
                        break;

                    case TranslateWindowCommand.MoveBottomUp:
                        height = height - offset;
                        break;

                    case TranslateWindowCommand.MoveBottomDown:
                        height = height + offset;
                        break;

                    case TranslateWindowCommand.MoveLeftLeft:
                        x = x - offset;
                        width = width + offset;
                        break;

                    case TranslateWindowCommand.MoveLeftRight:
                        x = x + offset;
                        width = width - offset;
                        break;

                    case TranslateWindowCommand.MoveRightLeft:
                        width = width - offset;
                        break;

                    case TranslateWindowCommand.MoveRightRight:
                        width = width + offset;
                        break;
                }

                User32Wrapper.SetWindowPos(_hWndActive.Value, 0, x, y, width, height, SWP_NOZORDER | SWP_SHOWWINDOW | SWP_NOACTIVATE);
            }
        }

        private static int CalculateOffsetToClosestStop(int rows, int cols, int x, int y, int width, int height, TranslateWindowCommand direction)
        {
            int screenWidth = (int)SystemParameters.WorkArea.Width;
            int screenHeight = (int)SystemParameters.WorkArea.Height;

            var columnWidths = new List<int>();
            var rowHeights = new List<int>();

            for (var ix = 0; ix <= cols; ix++)
            {
                columnWidths.Add((screenWidth / cols) * ix);
            }

            for (var ix = 0; ix <= rows; ix++)
            {
                rowHeights.Add((screenHeight / rows) * ix);
            }

            int stop;
            int i;
            bool stopFound = false;
            int ret = -1;
            switch (direction)
            {
                case TranslateWindowCommand.MoveUp:
                case TranslateWindowCommand.MoveTopUp:
                    i = 0;
                    stop = -1;
                    while (i < rowHeights.Count && rowHeights[i] < y)
                    {
                        stop = rowHeights[i++];
                        stopFound = true;
                    }
                    ret = y - stop;
                    break;

                case TranslateWindowCommand.MoveBottomUp:
                    i = 0;
                    stop = -1;
                    while (i < rowHeights.Count && rowHeights[i] < (y + height))
                    {
                        stop = rowHeights[i++];
                        stopFound = true;
                    }
                    ret = (y + height) - stop;
                    break;

                case TranslateWindowCommand.MoveDown:
                case TranslateWindowCommand.MoveTopDown:
                    i = rowHeights.Count - 1;
                    stop = Int32.MaxValue;
                    while (i >= 0 && rowHeights[i] > y)
                    {
                        stop = rowHeights[i--];
                        stopFound = true;
                    }
                    ret = stop - y;
                    break;


                case TranslateWindowCommand.MoveBottomDown:
                    i = rowHeights.Count - 1;
                    stop = Int32.MaxValue;
                    while (i >= 0 && rowHeights[i] > (y + height))
                    {
                        stop = rowHeights[i--];
                        stopFound = true;
                    }
                    ret = stop - (y + height);
                    break;

                case TranslateWindowCommand.MoveLeft:
                case TranslateWindowCommand.MoveLeftLeft:
                    i = 0;
                    stop = -1;
                    while (i < columnWidths.Count && columnWidths[i] < x)
                    {
                        stop = columnWidths[i++];
                        stopFound = true;
                    }
                    ret = x - stop;
                    break;

                case TranslateWindowCommand.MoveRightLeft:
                    i = 0;
                    stop = -1;
                    while (i < columnWidths.Count && columnWidths[i] < (x + width))
                    {
                        stop = columnWidths[i++];
                        stopFound = true;
                    }
                    ret = (x + width) - stop;
                    break;

                case TranslateWindowCommand.MoveRight:
                case TranslateWindowCommand.MoveLeftRight:
                    i = columnWidths.Count - 1;
                    stop = Int32.MaxValue;
                    while (i >= 0 && columnWidths[i] > x)
                    {
                        stop = columnWidths[i--];
                        stopFound = true;
                    }
                    ret = stop - x;
                    break;

                case TranslateWindowCommand.MoveRightRight:
                    i = columnWidths.Count - 1;
                    stop = Int32.MaxValue;
                    while (i >= 0 && columnWidths[i] > (x + width))
                    {
                        stop = columnWidths[i--];
                        stopFound = true;
                    }
                    ret = stop - (x + width);
                    break;
            }

            return stopFound ? ret : -1;
        }

        internal static void CaptureCurrentForeground()
        {
            _hWndActive = User32Wrapper.GetForegroundWindow();

            Object obj = new object();
            if (!User32Wrapper.GetWindowRect(new HandleRef(obj, _hWndActive.Value), out User32Wrapper.RECT rect))
            {
                Console.WriteLine("Error: Cannot get current foreground window dimensions.");
                return;
            }

            _rect = rect;
        }

        internal static void ClearState()
        {
            _hWndActive = null;
            _rect = null;
        }

        internal static void RestoreOriginalPosition()
        {
            User32Wrapper.SetWindowPos(
                _hWndActive.Value, 
                0, 
                _rect.Value.Left, 
                _rect.Value.Top, 
                _rect.Value.Right - _rect.Value.Left, 
                _rect.Value.Bottom - _rect.Value.Top, 
                SWP_NOZORDER | SWP_SHOWWINDOW | SWP_NOACTIVATE);
        }
    }
}
