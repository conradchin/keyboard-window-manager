using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Shapes;

namespace KeypadLayout
{
    public partial class MainWindow : Window
    {
        private HotkeyBinder _hotkeyBinder;
        private Preferences _preferencesWindow;
        private Configuration _configuration;
        private const string CONFIGURATION_FILE = "conf.ini";

        private ICommand upCommand;
        private ICommand shiftUpCommand;
        private ICommand ctrlUpCommand;

        private ICommand downCommand;
        private ICommand shiftDownCommand;
        private ICommand ctrlDownCommand;

        private ICommand leftCommand;
        private ICommand shiftLeftCommand;
        private ICommand ctrlLeftCommand;

        private ICommand rightCommand;
        private ICommand shiftRightCommand;
        private ICommand ctrlRightCommand;

        private ICommand minimizeCommand;
        private ICommand maximizeCommand;

        private ICommand changeToNextWindowCommand;
        private ICommand changeToPreviousWindowCommand;
        private ICommand cancelCommand;
        private ICommand applyCommand;
        private ICommand taskbarIconClickCommand;

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;

            taskbarIconClickCommand = new ActionCommand(() => { ShowPreferences(); });
            upCommand = new ActionCommand(() => { _moveWindow(TranslateWindowCommand.MoveTopUp); });
            shiftUpCommand = new ActionCommand(() => { _moveWindow(TranslateWindowCommand.MoveBottomUp); });
            ctrlUpCommand = new ActionCommand(() => { _moveWindow(TranslateWindowCommand.MoveUp); });

            downCommand = new ActionCommand(() => { _moveWindow(TranslateWindowCommand.MoveTopDown); });
            shiftDownCommand = new ActionCommand(() => { _moveWindow(TranslateWindowCommand.MoveBottomDown); });
            ctrlDownCommand = new ActionCommand(() => { _moveWindow(TranslateWindowCommand.MoveDown); });

            leftCommand = new ActionCommand(() => { _moveWindow(TranslateWindowCommand.MoveLeftLeft); });
            shiftLeftCommand = new ActionCommand(() => { _moveWindow(TranslateWindowCommand.MoveRightLeft); });
            ctrlLeftCommand = new ActionCommand(() => { _moveWindow(TranslateWindowCommand.MoveLeft); });

            rightCommand = new ActionCommand(() => { _moveWindow(TranslateWindowCommand.MoveLeftRight); });
            shiftRightCommand = new ActionCommand(() => { _moveWindow(TranslateWindowCommand.MoveRightRight); });
            ctrlRightCommand = new ActionCommand(() => { _moveWindow(TranslateWindowCommand.MoveRight); });

            minimizeCommand = new ActionCommand(() => { _moveWindow(TranslateWindowCommand.Minimize); });
            maximizeCommand = new ActionCommand(() => { _moveWindow(TranslateWindowCommand.Maximize); });

            changeToNextWindowCommand = new ActionCommand(() => { _selectNextWindow(); });
            changeToPreviousWindowCommand = new ActionCommand(() => { _selectPreviousWindow(); });
            cancelCommand = new ActionCommand(() => { _cancelMoving(true); });
            applyCommand = new ActionCommand(() => { _applyMoving(); });

            _configuration = new Configuration(CONFIGURATION_FILE, Preferences.GetDefaultConfigurationData());
            _hotkeyBinder = new HotkeyBinder();
            _hotkeyBinder.Register(OnHotkeyPress, _configuration.GetConfigurationValue(SettingDefinitions.HOTKEY));

            ApplyConfigurationChanges();

            Deactivated += MainWindow_Deactivated;
        }

        private void MainWindow_Deactivated(object sender, EventArgs e)
        {
            _cancelMoving(false);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
        }

        public ICommand UpCommand { get { return upCommand; } }
        public ICommand ShiftUpCommand { get { return shiftUpCommand; } }
        public ICommand CtrlUpCommand { get { return ctrlUpCommand; } }
        public ICommand DownCommand { get { return downCommand; } }
        public ICommand ShiftDownCommand { get { return shiftDownCommand; } }
        public ICommand CtrlDownCommand { get { return ctrlDownCommand; } }
        public ICommand LeftCommand { get { return leftCommand; } }
        public ICommand ShiftLeftCommand { get { return shiftLeftCommand; } }
        public ICommand CtrlLeftCommand { get { return ctrlLeftCommand; } }
        public ICommand RightCommand { get { return rightCommand; } }
        public ICommand ShiftRightCommand { get { return shiftRightCommand; } }
        public ICommand CtrlRightCommand { get { return ctrlRightCommand; } }
        public ICommand MinimizeCommand { get { return minimizeCommand; } }
        public ICommand MaximizeCommand { get { return maximizeCommand; } }
        public ICommand ChangeToNextWindowCommand { get { return changeToNextWindowCommand; } }
        public ICommand ChangeToPreviousWindowCommand { get { return changeToPreviousWindowCommand; } }
        public ICommand CancelCommand { get { return cancelCommand; } }
        public ICommand ApplyCommand { get { return applyCommand; } }
        public ICommand TaskbarIconClickCommand { get { return taskbarIconClickCommand; } }

        internal void RegisterHotkey(string hotkey)
        {
            this._hotkeyBinder.Register(this.OnHotkeyPress, hotkey);
        }

        private void _moveWindow(TranslateWindowCommand direction)
        {
            WindowManager.TransformWindow(
                direction,
                int.Parse(_configuration.GetConfigurationValue(SettingDefinitions.GRID_ROW_COUNT)),
                int.Parse(_configuration.GetConfigurationValue(SettingDefinitions.GRID_COL_COUNT))
            );
        }

        private void _selectNextWindow()
        {

        }
        private void _selectPreviousWindow()
        {

        }

        private void _cancelMoving(Boolean restoreOriginalPosition)
        {
            if(restoreOriginalPosition)
            {
                WindowManager.RestoreOriginalPosition();
            }
            WindowManager.ClearState();
            Hide();
        }

        private void _applyMoving()
        {
            WindowManager.ClearState();
            Hide();
        }

        private void OnHotkeyPress()
        {
            WindowManager.CaptureCurrentForeground();
            Visibility = Visibility.Visible;
            WindowState = WindowState.Maximized;
            Topmost = true;
        }

        private void ApplyConfigurationChanges()
        {
            int cols = int.Parse(_configuration.GetConfigurationValue(SettingDefinitions.GRID_COL_COUNT));
            int rows = int.Parse(_configuration.GetConfigurationValue(SettingDefinitions.GRID_ROW_COUNT));
            if(cols > 0 && rows > 0)
            {
                RedrawGrid(cols, rows);
            }
            else
            {
                throw new Exception("Error: Invalid configuration: only positive non-zero values are accepted for row and column count.");
            }

            _hotkeyBinder.Register(OnHotkeyPress, _configuration.GetConfigurationValue(SettingDefinitions.HOTKEY));
        }

        private void RedrawGrid(int cols, int rows)
        {
            canvas.Children.Clear();


            int screenWidth = (int)SystemParameters.WorkArea.Width;
            int screenHeight = (int)SystemParameters.WorkArea.Height;

            var gridBrushColor = System.Windows.Media.Brushes.Black;

            for (var i = 0; i <= cols; i++)
            {
                int offset = ((screenWidth / cols) * i);
                Line line = new Line()
                {
                    StrokeThickness = 1,
                    Stroke = gridBrushColor,
                    X1 = offset,
                    X2 = offset,
                    Y1 = 0,
                    Y2 = screenHeight
                };

                canvas.Children.Add(line);
            }

            for (var i = 0; i <= rows; i++)
            {
                int offset = ((screenHeight / rows) * i);
                Line line = new Line()
                {
                    StrokeThickness = 1,
                    Stroke = gridBrushColor,
                    X1 = 0,
                    X2 = screenWidth,
                    Y1 = offset,
                    Y2 = offset
                };

                canvas.Children.Add(line);
            }
        }

        private void Preferences_Click(object sender, RoutedEventArgs e)
        {
            ShowPreferences();
        }

        private void ShowPreferences()
        {
            this._hotkeyBinder.Unregister();
            if (_preferencesWindow == null)
            {
                _preferencesWindow = new Preferences(this, _configuration);
                _preferencesWindow.Closed += _preferencesWindow_Closed;
            }
            _preferencesWindow.Show();
        }

        private void _preferencesWindow_Closed(object sender, EventArgs e)
        {
            _preferencesWindow = null;
            _configuration.WriteChanges();
            ApplyConfigurationChanges();
        }
        
        private void Quit_Click(object sender, RoutedEventArgs e)
        {
            CloseApp();
        }

        public void CloseApp()
        {
            if (_preferencesWindow != null)
            {
                _preferencesWindow.Close();
            }

            _hotkeyBinder.Unregister();
            Close();
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
        }

    }

    
}
