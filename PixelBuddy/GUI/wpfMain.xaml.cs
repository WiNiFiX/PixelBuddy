using System;
using System.Diagnostics;
using System.Globalization;
using System.Net;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media;
using Helpers;
using PixelBuddyWPF.GUI;
using Rotation;
using MessageBox = System.Windows.MessageBox;

namespace PixelBuddyWPF
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private readonly CombatRoutine combatRoutine = new CombatRoutine();
        private KeyboardHook hook;
        

        public MainWindow()
        {
            InitializeComponent();
        }

        private static DateTime NistTime
        {
            get
            {
                var myHttpWebRequest = (HttpWebRequest)WebRequest.Create("http://www.microsoft.com");
                var response = myHttpWebRequest.GetResponse();
                string todaysDates = response.Headers["date"];
                DateTime dateTime = DateTime.ParseExact(todaysDates, "ddd, dd MMM yyyy HH:mm:ss 'GMT'", CultureInfo.InvariantCulture.DateTimeFormat, DateTimeStyles.AssumeUniversal);
                return dateTime;
            }
        }

        private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            richTextBox.IsReadOnly = true;
            Log.Initialize(richTextBox, this);

            try 
            {
                var Nist = NistTime;
                Log.Write("Current NIST time: " + Nist.ToString("yyyy/MM/dd HH:mm:ss"), Brushes.DeepSkyBlue);
            }
            catch (Exception ex) 
            {
                MessageBox.Show("Failed to get internet time\r\nError " + ex.Message + ", application will exit.");
                Log.Write("Failed to get internet time, application will exit.");
                Close();
            }

            while (Globals.Process == null) 
            {
                wpfSelectProcess f = new wpfSelectProcess();
                f.ShowDialog();
            }

            Log.WriteNoTime("Sucessfully connected to process: " + Globals.Process.MainModule.ModuleName, Brushes.Yellow);

            if (Environment.MachineName == "DESKTOP-QKRB42C")
            {
                Log.Write("Im a noob !!!", Brushes.Red);
            }
            
            hook = new KeyboardHook();
            hook.RegisterHotKey(ModifierKeys.Ctrl, Keys.None, "Get Pixel Color");
            hook.RegisterHotKey(ModifierKeys.Alt, Keys.X, "Start/Stop Rotation");
            hook.RegisterHotKey(ModifierKeys.Alt, Keys.S, "Single Target/AOE Rotation");
            hook.RegisterHotKey(ModifierKeys.Alt, Keys.B, "Burst");

            cmdStartStop.Background = Brushes.LightGreen;
            cmdStartStop.Foreground = Brushes.Black;
            
            hook.KeyPressed += Hook_KeyPressed;
            combatRoutine.Load();

            if (GameDVR.IsAppCapturedEnabled || GameDVR.IsGameDVREnabled) {
                var dialogResult = MessageBox.Show("Game DVR is currently ENABLED on this machine. Would you like to disable it? PixelBuddy will NOT function correctly with it enabled.",
                    "DisableGameDVR", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (dialogResult == MessageBoxResult.Yes) {
                    GameDVR.SetAppCapturedEnabled(0);
                    GameDVR.SetGameDVREnabled(0);
                    MessageBox.Show("Game DVR has been disabled. A restart maybe required to take effect.", "PixelBuddy", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else {
                    Log.Write("PixelBuddy cannot run until GameDVR is disabled", Brushes.Red);
                    return;
                }
            }
            else {
                Log.Write("GameDVR is disabled in Xbox app", Brushes.Yellow);
            }
        }

        private void Hook_KeyPressed(object sender, KeyPressedEventArgs e)
        {
            if (e.Modifier == ModifierKeys.None && e.Key == Keys.ControlKey) 
            {
                if (chkEditMode.IsChecked == false)
                {
                    Log.Write("You are not in edit mode, if you wish to record a new routine ensure to tick the edit mode checkbox.", Brushes.Red);
                   return;    
                }
                POINT point;
                Mouse.GetCursorPos(out point);
                Pixels.ScreenToClient(Globals.Process.MainWindowHandle, ref point);
                var color = Pixels.GetPixelColor(point);

                combatRoutine.Pause();

                Log.Write($"RGB = {color.R},{color.G},{color.B}", Brushes.WhiteSmoke);

                wpfSpell f = new wpfSpell(color, point, combatRoutine);
                f.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                f.Topmost = true;
                f.ShowDialog();

                if (combatRoutine.Spells.Count > 0)
                    combatRoutine.Start();
            }
            if (e.Modifier == ModifierKeys.Alt && e.Key == Keys.X)
            {
                cmdStartStop_Click(null, null);
            }
            if (e.Modifier == ModifierKeys.Alt && e.Key == Keys.S) 
            {
                if (combatRoutine.Type == CombatRoutine.RotationType.SingleTarget) {
                    combatRoutine.ChangeType(CombatRoutine.RotationType.AOE);
                    return;
                }
                if (combatRoutine.Type == CombatRoutine.RotationType.AOE) {
                    combatRoutine.ChangeType(CombatRoutine.RotationType.SingleTarget);
                    return;
                }
            }
        }

        private void cmdStartStop_Click(object sender, RoutedEventArgs e)
        {
            if (combatRoutine.Spells.Count == 0)
            {
                Log.Write("Add atleast 1 spell.", new SolidColorBrush {Color = Colors.Red});
                return;
            }

            if (combatRoutine.State == CombatRoutine.RotationState.Stopped)
            {
                combatRoutine.Start();

                if (combatRoutine.State != CombatRoutine.RotationState.Running) return;

                cmdStartStop.Content = "Stop";
                cmdStartStop.Background = Brushes.Red;
            }
            else
            {
                combatRoutine.Pause();
                cmdStartStop.Content = "Start";
                cmdStartStop.Background = Brushes.LightGreen;
            }
        }

        private void cmdLoad_OnClick(object sender, RoutedEventArgs e)
        {
            combatRoutine.LoadFromFile();   
        }

        private void cmdSave_OnClick(object sender, RoutedEventArgs e)
        {
            combatRoutine.Save();
        }

        private void cmdDPSTest_Click(object sender, RoutedEventArgs e)
        {

        }

        private void MenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            if (combatRoutine.FileName != "") 
                Process.Start(combatRoutine.FileName);
        }

        private void chkEditMode_Checked(object sender, RoutedEventArgs e)
        {
            
        }
    }
}