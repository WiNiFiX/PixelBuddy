using System;
using System.CodeDom;
using System.Windows.Media;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Input;
using Helpers;

namespace Rotation
{
    public class Spell
    {
        public Color Color;
        public Key Key;
        public string Name;
        public POINT p;
        public string equals;
        public string type;

        public Spell(Color color, Key key, POINT p, string spellName, string equals, string type)
        {
            Name = spellName;
            Color = color;
            Key = key;
            this.p = p;
            this.equals = equals;
            this.type = type;
        }

        public bool MustPress()
        {
            if (equals == "=") 
            {
                var color = Pixels.GetPixelColor(p);
                return color.R == Color.R && color.G == Color.G && color.B == Color.B;
            }
            else // (equals == "!=") 
            {
                var color = Pixels.GetPixelColor(p);
                return color.R != Color.R && color.G != Color.G && color.B != Color.B;
            }
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = false)]
        private static extern IntPtr SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);

        private void KeyDown(Key key)
        {
            SendMessage(Globals.Process.MainWindowHandle, 0x100, KeyInterop.VirtualKeyFromKey(key), 0);
        }

        private void KeyUp(Key key)
        {
            SendMessage(Globals.Process.MainWindowHandle, 0x101, KeyInterop.VirtualKeyFromKey(key), 0);
        }

        private void KeyPressRelease(Key key)
        {
            KeyDown(key);
            Thread.Sleep(50);
            KeyUp(key);
        }

        public void Press()
        {
            Log.Write("Casting: " + Name + " Key: " + Key, Brushes.Aquamarine);
            KeyPressRelease(Key);
        }
    }
}