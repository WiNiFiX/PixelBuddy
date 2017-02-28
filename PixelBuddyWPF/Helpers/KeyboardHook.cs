// Credits: http://stackoverflow.com/questions/2450373/set-global-hotkeys-using-c-sharp

using System;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Windows.Media;

// ReSharper disable MemberCanBePrivate.Global

namespace Helpers
{
    public sealed class KeyboardHook : IDisposable
    {
        private readonly Window _window = new Window();
        private int _currentId;

        public KeyboardHook()
        {
            // register the event of the inner native window.
            _window.KeyPressed += delegate(object sender, KeyPressedEventArgs args) { KeyPressed?.Invoke(this, args); };
        }

        #region IDisposable Members

        public void Dispose()
        {
            // unregister all the registered hot keys.
            for (var i = _currentId; i > 0; i--)
            {
                UnregisterHotKey(_window.Handle, i);
            }

            // dispose the inner native window.
            _window.Dispose();
        }

        #endregion

        // Registers a hot key with Windows.
        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        // Unregisters the hot key with Windows.
        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        /// <summary>
        ///     Registers a hot key in the system.
        /// </summary>
        /// <param name="modifier">The modifiers that are associated with the hot key.</param>
        /// <param name="key">The key itself that is associated with the hot key.</param>
        /// <param name="forWhat"></param>
        public void RegisterHotKey(ModifierKeys modifier, Keys key, string forWhat)
        {
            // increment the counter.
            _currentId = _currentId + 1;

            Log.WriteNoTime($"Hotkey [{modifier}] + [{key}] registered for [{forWhat}]", new SolidColorBrush {Color = Colors.LightGreen });

            // register the hot key.
            if (!RegisterHotKey(_window.Handle, _currentId, (uint) modifier, (uint) key))
                throw new InvalidOperationException("Couldn’t register the hot key.");
        }

        /// <summary>
        ///     A hot key has been pressed.
        /// </summary>
        public event EventHandler<KeyPressedEventArgs> KeyPressed;

        /// <summary>
        ///     Represents the window that is used internally to get the messages.
        /// </summary>
        private sealed class Window : NativeWindow, IDisposable
        {
            private static readonly int WM_HOTKEY = 0x0312;

            public Window()
            {
                // create the handle for the window.
                CreateHandle(new CreateParams());
            }

            #region IDisposable Members

            public void Dispose()
            {
                DestroyHandle();
            }

            #endregion

            /// <summary>
            ///     Overridden to get the notifications.
            /// </summary>
            /// <param name="m"></param>
            protected override void WndProc(ref Message m)
            {
                base.WndProc(ref m);

                //Log.Write("WM_ = " + m.Msg);

                // check if we got a hot key pressed.
                if (m.Msg == WM_HOTKEY)
                {
                    // get the keys.
                    var key = (Keys) (((int) m.LParam >> 16) & 0xFFFF);
                    var modifier = (ModifierKeys) ((int) m.LParam & 0xFFFF);

                    // invoke the event to notify the parent.
                    KeyPressed?.Invoke(this, new KeyPressedEventArgs(modifier, key));
                }
            }

            public event EventHandler<KeyPressedEventArgs> KeyPressed;
        }
    }

    /// <summary>
    ///     Event Args for the event that is fired after the hot key has been pressed.
    /// </summary>
    public class KeyPressedEventArgs : EventArgs
    {
        internal KeyPressedEventArgs(ModifierKeys modifier, Keys key)
        {
            Modifier = modifier;
            Key = key;
        }

        public ModifierKeys Modifier { get; }

        public Keys Key { get; }
    }

    public static class Keyboard
    {
        public static Keys toKey(string keystr)
        {
            return (Keys) Enum.Parse(typeof(Keys), keystr);
        }

        public static ModifierKeys toModifier(string keystr)
        {
            return (ModifierKeys) Enum.Parse(typeof(ModifierKeys), keystr);
        }
    }

    /// <summary>
    ///     The enumeration of possible modifiers.
    /// </summary>
    [Flags]
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public enum ModifierKeys : uint
    {
        None = 0,
        Alt = 1,
        Ctrl = 2,
        Shift = 4
    }
}