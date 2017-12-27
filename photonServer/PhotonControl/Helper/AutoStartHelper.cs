using System.Reflection;
using Microsoft.Win32;

namespace PhotonControl.Helper
{
    internal class AutoStartHelper
    {
        // Fields
        private const string RunLocation = @"Software\Microsoft\Windows\CurrentVersion\Run";

        // Methods
        public static bool IsAutoStartEnabled(string keyName, string assemblyLocation)
        {
            RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run");
            if (key == null)
            {
                return false;
            }
            string str = (string)key.GetValue(keyName);
            if (str == null)
            {
                return false;
            }
            return (str == assemblyLocation);
        }

        public static void RemoveAutoStart(string keyName)
        {
            RegistryKey key = Registry.CurrentUser.CreateSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run");
            if (key != null)
            {
                key.DeleteValue(keyName);
            }
        }

        public static void SetAutoStart(string keyName, string assemblyLocation)
        {
            RegistryKey key = Registry.CurrentUser.CreateSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run");
            if (key != null)
            {
                key.SetValue(keyName, assemblyLocation);
            }
        }

        public static void ToggleAutostarting(string registryKey)
        {
            if (IsAutoStartEnabled(registryKey, Assembly.GetExecutingAssembly().Location))
            {
                RemoveAutoStart(registryKey);
            }
            else
            {
                SetAutoStart(registryKey, Assembly.GetExecutingAssembly().Location);
            }
        }
    }
}
