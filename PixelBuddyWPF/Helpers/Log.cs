//////////////////////////////////////////////////
//                                              //
//   See License.txt for Licensing information  //
//                                              //
//////////////////////////////////////////////////

using System;
using System.Diagnostics;
using System.Windows;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using MahApps.Metro.Controls;
using PixelBuddyWPF;

namespace Helpers
{
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    public static class Log
    {
        private static bool Initialized;
        private static StreamWriter _sw;
        private static RichTextBox _rtbLogWindow;
        private static MainWindow _parent;
        public static string HorizontalLine = "------------";
        private static int LineCount = 0;
        
        public static void Initialize(RichTextBox rtbLogWindow, MainWindow parent, bool clearHistory = true)
        {
            if (!Directory.Exists(Environment.CurrentDirectory + "\\Logs\\" + DateTime.Now.ToString("yyyy-MMM")))
                Directory.CreateDirectory(Environment.CurrentDirectory + "\\Logs\\" + DateTime.Now.ToString("yyyy-MMM"));

            _sw = new StreamWriter(Environment.CurrentDirectory + "\\Logs\\" + DateTime.Now.ToString("yyyy-MMM") + "\\" + DateTime.Now.ToString("yyyy.MM.dd HH.mm.ss") + ".txt") {AutoFlush = true};

            _rtbLogWindow = rtbLogWindow;
            _parent = parent;

            Initialized = true;
        }

        public static void Clear()
        {
            _rtbLogWindow.Document.Blocks.Clear();
        }

        public static void WriteNoTime(string activity)
        {
            _parent.Invoke(() =>
            {
                InternalWrite(Brushes.Black, activity, true);
                WriteDirectlyToLogFile(activity);
            });
        }

        public static void WriteNoTime(string activity, SolidColorBrush c)
        {
            _parent.Invoke(() =>
            {
                InternalWrite(c, activity, true);
                WriteDirectlyToLogFile(activity);
            });
        }

        private static void LogActivity(string activity, SolidColorBrush c)
        {
            if (!Initialized)
                return;

            try
            {
                if (activity == string.Empty)
                {
                    DrawHorizontalLine();
                }
                else if (activity.Trim() == string.Empty)
                {
                    WriteNewLine();
                }
                else
                {
                    Write(activity, c);
                }
            }
            catch (Exception execp)
            {
                LogActivity("Exception in LogActivity function\r\nError: " + execp.Message, Brushes.Red);
            }
        }

        public static void LogActivity(string activity)
        {
            if (!Initialized)
                return;

            try
            {
                if (activity == string.Empty)
                {
                    DrawHorizontalLine();
                }
                else if (activity.Trim() == string.Empty)
                {
                    WriteNewLine();
                }
                else
                {
                    Write(activity, Brushes.Red);
                }
            }
            catch (Exception execp)
            {
                LogActivity("Exception in LogActivity function\r\nError: " + execp.Message, Brushes.Red);
            }
        }

        public static void WriteDirectlyToLogFile(string format, params object[] args)
        {
            try
            {
                _sw?.WriteLine("[" + DateTime.Now.ToString(CultureInfo.InvariantCulture) + "] " + format, args);
            }
            catch (Exception ex)
            {
                if (ex.Message == "Index (zero based) must be greater than or equal to zero and less than the size of the argument list." ||
                    ex.Message == "Input string was not in a correct format.")
                {
                    try
                    {
                        _sw?.WriteLine("[" + DateTime.Now.ToString(CultureInfo.InvariantCulture) + "] " + format);
                    }
                    catch
                    {
                        LogActivity("Failed to write to log file [2] - " + ex.Message, Brushes.Red);
                    }
                }
                else
                {
                    LogActivity("Failed to write to log file [1] - " + ex.Message, Brushes.Red);
                }
            }
        }

        public static void Write(string text)
        {
            Write(text, Brushes.Black);
        }



        public static void Write(string text, SolidColorBrush c)
        {
            if (_parent == null)
            {
                MessageBox.Show("Please ensure you call Log.Initialize()");
                Application.Current.Shutdown();
            }

            try
            {
                _parent?.Invoke(() =>
                {
                    InternalWrite(c, text);
                    WriteDirectlyToLogFile(text);
                });
            }
            catch
            {
                // ignored
            }
        }

        public static void WriteNewLine()
        {
            _parent.Invoke(() =>
            {
                InternalWrite(Brushes.Black, "", true);
                WriteDirectlyToLogFile("");
            });
        }

        public static void DrawHorizontalLine()
        {
            _parent.Invoke(() =>
            {
                InternalWrite(Brushes.WhiteSmoke, HorizontalLine, true);
                WriteDirectlyToLogFile(HorizontalLine);
            });
        }

        public static void Write(SolidColorBrush color, string format, params object[] args)
        {
            _parent.Invoke(() =>
            {
                InternalWrite(color, string.Format(format, args));
                WriteDirectlyToLogFile(format, args);
            });
        }
        
        private static void InternalWrite(SolidColorBrush color, string text, bool noTime = false, bool lineFeed = true)
        {
            try
            {
                LineCount++;
                var rtb = _rtbLogWindow;

                if (LineCount == 1000) {
                    LineCount = 0;
                    Clear();
                }

                if (!noTime) 
                {
                    var rangeOfTime = new TextRange(rtb.Document.ContentEnd, rtb.Document.ContentEnd);
                    rangeOfTime.Text = $"[{DateTime.Now.ToString("HH:mm:ss")}] ";
                    rangeOfTime.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.WhiteSmoke);
                    rangeOfTime.ApplyPropertyValue(TextElement.FontWeightProperty, FontWeights.Normal);
                }

                var rangeOfText1 = new TextRange(rtb.Document.ContentEnd, rtb.Document.ContentEnd);
                rangeOfText1.Text = lineFeed ? $"{text}\r" : text;
                rangeOfText1.ApplyPropertyValue(TextElement.ForegroundProperty, color);
                rangeOfText1.ApplyPropertyValue(TextElement.FontWeightProperty, FontWeights.Normal);

                rtb.ScrollToEnd();
            }
            catch
            {
                // ignored
            }
        }
    }
}