using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace KeypadLayout
{
    static class SettingDefinitions
    {
        public const string GRID_COL_COUNT = "GridColumnCount";
        public const string GRID_ROW_COUNT = "GridRowCount";
        public const string HOTKEY = "Hotkey";
    }

    public partial class Preferences : Window, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private MainWindow _mainWindow;
        private Configuration _conf;

        public Preferences(MainWindow mainWindow, Configuration configuration)
        {
            InitializeComponent();
            DataContext = this;
            myGrid.DataContext = this;
            _mainWindow = mainWindow;
            _conf = configuration;

            if(!ReadSettings())
            {
                // TODO: show error
            }

            Closing += Preferences_Closing;

            hotkeyControl.CaretBrush = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0));
        }

        private bool ReadSettings()
        {
            try
            {
                GridWidth = int.Parse(_conf.GetConfigurationValue(SettingDefinitions.GRID_COL_COUNT));
                GridHeight = int.Parse(_conf.GetConfigurationValue(SettingDefinitions.GRID_ROW_COUNT));
                hotkeyControl.Text = _conf.GetConfigurationValue(SettingDefinitions.HOTKEY);
                return true;
            }
            catch(Exception /*e*/)
            {
                return false;
            }
        }

        private void Preferences_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _conf.SetConfigurationValue(SettingDefinitions.GRID_COL_COUNT, _gridWidth.ToString());
            _conf.SetConfigurationValue(SettingDefinitions.GRID_ROW_COUNT, _gridHeight.ToString());
            _conf.SetConfigurationValue(SettingDefinitions.HOTKEY, hotkeyControl.Text);
        }

        private void Quit_Click(object sender, RoutedEventArgs e)
        {
            Close();
            _mainWindow.CloseApp();
        }

        public static Dictionary<string, string> GetDefaultConfigurationData()
        {
            return new Dictionary<string, string>()
            {
                { SettingDefinitions.GRID_COL_COUNT, "6" },
                { SettingDefinitions.GRID_ROW_COUNT, "6" },
                { SettingDefinitions.HOTKEY, "" }
            };
        }
        private int _gridWidth;
        public int GridWidth
        {
            get { return _gridWidth; }
            set
            {
                _gridWidth = value;
                NotifyPropertyChanged();
            }
        }

        private int _gridHeight;
        public int GridHeight
        {
            get { return _gridHeight; }
            set
            {
                _gridHeight = value;
                NotifyPropertyChanged();
            }
        }

        private string _previousHotkey = "";
        private bool _winKeyDown = false;

        private void HotkeyControl_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            e.Handled = true;
            
            Key key = (e.Key == Key.System ? e.SystemKey : e.Key);

            // Windows key is not shown in the default modifiers list,
            // therefore requires special handling.
            if (key == Key.LWin || key == Key.RWin)
            {
                _winKeyDown = true;
                return;
            }

            if (key == Key.LeftShift || key == Key.RightShift
              || key == Key.LeftCtrl || key == Key.RightCtrl
              || key == Key.LeftAlt || key == Key.RightAlt)
            {
                return;
            }

            StringBuilder shortcutText = new StringBuilder();
            if ((Keyboard.Modifiers & ModifierKeys.Control) != 0)
            {
                shortcutText.Append("Ctrl+");
            }
            if ((Keyboard.Modifiers & ModifierKeys.Shift) != 0)
            {
                shortcutText.Append("Shift+");
            }
            if (_winKeyDown)
            {
                shortcutText.Append("Win+");
            }
            if ((Keyboard.Modifiers & ModifierKeys.Alt) != 0)
            {
                shortcutText.Append("Alt+");
            }

            shortcutText.Append(key.ToString());
            hotkeyControl.Text = shortcutText.ToString();
        }

        private void HotkeyControl_KeyUp(object sender, KeyEventArgs e)
        {
            e.Handled = true;
            Key key = (e.Key == Key.System ? e.SystemKey : e.Key);
            if (key == Key.LWin || key == Key.RWin)
            {
                _winKeyDown = false;
                return;
            }
        }

        private void RemoveHotkeyButton_Click(object sender, RoutedEventArgs e)
        {
            hotkeyControl.Text = "";
        }

        private void DiscardChangeButton_Click(object sender, RoutedEventArgs e)
        {
            ReadSettings();
        }

        private void hotkeyControl_GotFocus(object sender, RoutedEventArgs e)
        {
            _previousHotkey = hotkeyControl.Text;
            hotkeyControl.Text = "Type a combo";
            hotkeyControl.Foreground = new SolidColorBrush(Color.FromArgb(70, 0, 0, 0));
        }

        private void hotkeyControl_LostFocus(object sender, RoutedEventArgs e)
        {
            if(string.IsNullOrWhiteSpace(hotkeyControl.Text) || hotkeyControl.Text == "Type a combo")
            {
                hotkeyControl.Text = _previousHotkey;
            }

            hotkeyControl.Foreground = new SolidColorBrush(Color.FromRgb(0, 0, 0));
        }
        private void preferencesBackground_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if(e.OriginalSource.GetType() == typeof(System.Windows.Controls.Border))
            {
                closeWindowButton.Focus();
            }
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
