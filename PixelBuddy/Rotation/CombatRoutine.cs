//////////////////////////////////////////////////
//                                              //
//   See License.txt for Licensing information  //
//                                              //
//////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Media;
using System.Speech.Synthesis;
using System.Threading;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml.Serialization;
using Helpers;
using SaveFileDialog = Microsoft.Win32.SaveFileDialog;

namespace Rotation
{
    [SuppressMessage("ReSharper", "ParameterHidesMember")]
    public class CombatRoutine
    {
        public enum RotationState
        {
            Stopped = 0,
            Running = 1
        }

        public enum RotationType
        {
            SingleTarget = 0,
            AOE = 2
        }

        private volatile RotationType _rotationType = RotationType.SingleTarget;
        public RotationType Type => _rotationType;

        private readonly ManualResetEvent pause = new ManualResetEvent(false);
        private readonly Random random;

        private Thread mainThread;
        private int PulseFrequency = 250;

        private static SpeechSynthesizer synthesizer;

        public List<Spell> Spells;

        public CombatRoutine()
        {
            random = new Random(DateTime.Now.Second);
        }

        public RotationState State { get; private set; } = RotationState.Stopped;

        private void MainThreadTick()
        {
            try
            {
                while (true)
                {
                    pause.WaitOne();
                    Pulse();
                    Thread.Sleep(PulseFrequency + random.Next(50));
                }
            }
            catch (Exception ex)
            {
                Log.Write(ex.Message, Brushes.Red);
            }
        }

        private static void Speak(string words)
        {
            synthesizer.SpeakAsync(words);
        }

        public void ChangeType(RotationType rotationType)
        {
            if (_rotationType == rotationType) return;

            _rotationType = rotationType;

            Log.Write("Rotation type: " + rotationType, Brushes.GreenYellow);

            Speak(rotationType.ToString());
        }

        public void Load()
        {
            Spells = new List<Spell>();

            PulseFrequency = 250;
            //Log.Write("Using Pulse Frequency (ms) = " + PulseFrequency, Brushes.Aqua);

            mainThread = new Thread(MainThreadTick) {IsBackground = true};
            mainThread.Start();

            Initialize();
        }

        private static T ParseEnum<T>(string value)
        {
            return (T) Enum.Parse(typeof(T), value, true);
        }

        public string FileName { get; private set; } = "";

        public void LoadFromFile()
        {
            Log.Clear();
            Spells.Clear();

            var dlg = new OpenFileDialog();
            dlg.DefaultExt = ".txt";
            dlg.Filter = "Text documents (.txt)|*.txt";
            var result = dlg.ShowDialog();

            if (result != DialogResult.OK) return;

            using (var sr = new StreamReader(dlg.FileName))
            {
                FileName = dlg.FileName;
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    if (line.Trim() == "")
                        continue;

                    var split = line.Split('|');
                    var r = split[0].Split(',')[0];
                    var g = split[0].Split(',')[1];
                    var b = split[0].Split(',')[2];
                    var c = new Color {R = byte.Parse(r), G = byte.Parse(g), B = byte.Parse(b)};

                    var key = ParseEnum<Key>(split[1]);
                    var spellName = split[2];
                    var x = split[3].Split(',')[0];
                    var y = split[3].Split(',')[1];
                    var eq = "=";
                    try
                    {
                        eq = split[4];
                    }
                    catch
                    {
                        // Do nothing upgrade fix
                    }
                    var type = "Single Target";
                    try 
                    {
                        type = split[5];
                    }
                    catch 
                    {
                        // Do nothing upgrade fix
                    }
                    var point = new POINT(int.Parse(x), int.Parse(y));
                    var loadedSpell = new Spell(c, key, point, spellName, eq, type);
                    Spells.Add(loadedSpell);
                    Log.Write("Loaded Spell: [" + loadedSpell.type + "] - " + loadedSpell.Name + " - Key: [" + loadedSpell.Key + "]", Brushes.LightGreen);
                }
                sr.Close();
            }
        }


        internal void Dispose()
        {
            Log.Write("Stopping Pulse() timer...");
            Pause();
            Thread.Sleep(100); // Wait for it to close entirely so that all bitmap reading is done
        }

        public void Start()
        {
            try
            {
                if (State == RotationState.Stopped)
                {
                    Speak("Start");
                    Log.Write("Starting PixelBuddy...", Brushes.Yellow);
                    pause.Set();
                    State = RotationState.Running;
                }
            }
            catch (Exception ex)
            {
                Log.Write("Error Starting Combat Routine", Brushes.Red);
                Log.Write(ex.Message, Brushes.Red);
            }
        }

        public void Pause()
        {
            try
            {
                if (State != RotationState.Running) return;

                Speak("Stop");

                Log.Write("PixelBuddy has stopped.", Brushes.Red);
                Stop();
                pause.Reset();
                State = RotationState.Stopped;
            }
            catch (Exception ex)
            {
                Log.Write("Error Stopping PixelBuddy", Brushes.Red);
                Log.Write(ex.Message, Brushes.Red);
            }
        }

        private static void Initialize()
        {
            synthesizer = new SpeechSynthesizer();
            synthesizer.Volume = 100; // 0...100
            synthesizer.Rate = 2; // -10...10
            synthesizer.SelectVoiceByHints(VoiceGender.Female, VoiceAge.Adult);
        }

        private static void Stop()
        {
        }

        public static void WriteToXmlFile<T>(string filePath, T objectToWrite, bool append = false) where T : new()
        {
            TextWriter writer = null;
            try
            {
                var serializer = new XmlSerializer(typeof(T));
                writer = new StreamWriter(filePath, append);
                serializer.Serialize(writer, objectToWrite);
            }
            finally
            {
                if (writer != null)
                    writer.Close();
            }
        }

        public void Save()
        {
            var dlg = new SaveFileDialog();
            dlg.DefaultExt = ".txt";
            dlg.Filter = "Text documents (.txt)|*.txt";
            var result = dlg.ShowDialog();

            if (result != true) return;

            // Save document
            using (var streamWriter = new StreamWriter(dlg.FileName))
            {
                streamWriter.AutoFlush = true;
                foreach (var spell in Spells)
                {
                    Log.Write("Saving Spell: " + spell.Name, Brushes.Cyan);
                    streamWriter.WriteLine(spell.Color.R + "," + spell.Color.G + "," + spell.Color.B + "|" + spell.Key + "|" + spell.Name + "|" + spell.p.X + "," + spell.p.Y + "|" +
                                           spell.equals + "|" + spell.type);
                }
                streamWriter.Close();
            }
        }

        private void Pulse()
        {
            if (Type == RotationType.SingleTarget) 
            {
                foreach (var spell in Spells.Where(s => s.type == "Single Target")) 
                {
                    if (spell.MustPress()) 
                    {
                        spell.Press();
                    }
                }
            }
            else
            {
                foreach (var spell in Spells.Where(s => s.type == "AOE")) 
                {
                    if (spell.MustPress()) 
                    {
                        spell.Press();
                    }
                }
            }
        }
    }
}