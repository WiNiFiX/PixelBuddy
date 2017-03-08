using System;
using System.Diagnostics;
using System.Windows;
using Helpers;

namespace PixelBuddyWPF.GUI
{
    /// <summary>
    ///     Interaction logic for wpfSelectProcess.xaml
    /// </summary>
    public partial class wpfSelectProcess
    {
        public wpfSelectProcess()
        {
            InitializeComponent();
        }

        private void WpfSelectProcess_OnLoaded(object sender, RoutedEventArgs e)
        {
            foreach (var process in Process.GetProcesses())
            {
                if (process.MainWindowTitle != "")
                    comboBox.Items.Add($"{process.MainModule.ModuleName} => {process.Id}");
            }
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var PID = int.Parse(comboBox.Text.Split('>')[1]);
                Globals.Process = Process.GetProcessById(PID);
                Close();
            }
            catch
            {
                // ignored
            }
        }

        private void cmdCancel_Click(object sender, RoutedEventArgs e)
        {
            Environment.Exit(0);
        }
    }
}