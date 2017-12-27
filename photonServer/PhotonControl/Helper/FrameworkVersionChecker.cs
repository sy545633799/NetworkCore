using System;
using System.Collections.Generic;
using Microsoft.Win32;
using PhotonControl.Resources;

namespace PhotonControl.Helper
{
    public class FrameworkVersionChecker
    {
        // Methods
        public static bool CheckRequiredFrameworkVersion(Version requiredVersion, out string errorMessage)
        {
            string str;
            List<FrameworkVersion> list;
            errorMessage = null;
            if (GetInstalledFrameworkVersions(out list, out str))
            {
                bool flag = false;
                foreach (FrameworkVersion version in list)
                {
                    if (version.Version >= requiredVersion)
                    {
                        flag = true;
                        break;
                    }
                }
                if (!flag)
                {
                    errorMessage = string.Format(PhotonControlStrings.frameworkVersionRequiredMsg, requiredVersion);
                    return false;
                }
            }
            return true;
        }

        public static bool GetInstalledFrameworkVersions(out List<FrameworkVersion> frameworkVersions, out string errorMessage)
        {
            errorMessage = null;
            frameworkVersions = new List<FrameworkVersion>();
            try
            {
                RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\NET Framework Setup\NDP");
                if (key != null)
                {
                    string[] subKeyNames = key.GetSubKeyNames();
                    for (int i = 0; i < subKeyNames.Length; i++)
                    {
                        try
                        {
                            string name = subKeyNames[i];
                            if (name.StartsWith("v"))
                            {
                                FrameworkVersion item = new FrameworkVersion
                                {
                                    VersionName = name
                                };
                                string[] strArray2 = name.Remove(0, 1).Split(new char[] { '.' });
                                int major = int.Parse(strArray2[0]);
                                int minor = (strArray2.Length > 1) ? int.Parse(strArray2[1]) : 0;
                                item.Version = new Version(major, minor);
                                RegistryKey key2 = key.OpenSubKey(name);
                                if (key2 != null)
                                {
                                    item.ServicePack = Convert.ToInt32(key2.GetValue("SP", 0));
                                }
                                frameworkVersions.Add(item);
                            }
                        }
                        catch (Exception)
                        {
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                errorMessage = "Could not determine installed .NET Framework versions:" + exception;
                return false;
            }
            return true;
        }
    }
}
