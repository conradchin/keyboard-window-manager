using NHotkey;
using NHotkey.Wpf;
using System;
using System.Windows.Input;

namespace KeypadLayout
{
    class HotkeyBinder
    {
        private Action _callback;
        private const string HOTKEY_IDENTIFIER = "keypadlayout-main-trigger";

        public HotkeyBinder()
        {
        }

        private void OnGlobalHotkey(object sender, HotkeyEventArgs e)
        {
            e.Handled = true;
            _callback?.Invoke();
        }

        internal void Register(Action callback, string hotkey)
        {
            this._callback = callback;
            if (!string.IsNullOrEmpty(hotkey))
            {
                var parts = hotkey.Split('+');
                Key? key = GetKey(parts[parts.Length-1]);
                ModifierKeys? modifiers = GetModifiers(parts);
                if(key != null && modifiers != null)
                {
                    HotkeyManager.Current.AddOrReplace(HOTKEY_IDENTIFIER, key.Value, modifiers.Value, OnGlobalHotkey);
                }
            }
        }

        private ModifierKeys GetModifiers(string[] keyStrings)
        {
            ModifierKeys combined = ModifierKeys.None;
            var converter = new ModifierKeysConverter();
            foreach (var keyString in keyStrings)
            {
                try
                {
                    var key = (ModifierKeys)converter.ConvertFromString(keyString);
                    combined = combined | key;
                }
                catch (Exception /*e*/)
                {
                    // The key was not a modifier key, just skip it.
                }
            }

            return combined;
        }

        private Key GetKey(string keyString)
        {
            var converter = new KeyConverter();
            return (Key)converter.ConvertFromString(keyString);
        }

        public void Unregister()
        {
            HotkeyManager.Current.Remove(HOTKEY_IDENTIFIER);
        }
    }
}
