using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Remoting;
using log4net;
using log4net.Config;
using PhotonHostRuntimeInterfaces;

namespace PhotonHostRuntime
{
    public class PhotonDomainManager : AppDomainManager, IPhotonDomainManager
    {
        // Fields
        private IUnloadApplicationDomains appDomainUnloader;
        private string applicationBinaryFullDirectory;
        private string applicationFullDirectory;
        private string applicationRootFullDirectory;
        private List<string> assemblyLookupPathes;
        private string clrBaseDirectory;
        private PhotonDomainManager defaultDomainManager;
        private string instanceName = "unknown";
        private string lastAssemblyLoadIntent;
        private string licenseFullPath;
        private static bool licenseIsValid = false;
        private static ILicensing licensingInstance;
        private static readonly ILog log = LogManager.GetLogger("PhotonHostRuntime.PhotonDomainManager");
        private static readonly string Log4NetConfigFilename = "Photon.local.log4net";
        private IPhotonApplicationsCounter photonApplicationsCounter;
        public IManageClientConnectionCount photonConnectionCountManager;
        private IPhotonControl photonControl;
        private IPhotonDomainBehavior photonDomainBehavior;
        private string photonExeFullDirectory;
        private bool photonIsRunning;
        private IPhotonServerShutdown photonServerShutdown;
        private string sdkVersion = "unknown";
        private ILogToUnmanagedLog unmanagedLog;
        private string unmanagedLogDirectory;
        private bool useCustomAssemblyLoader;

        // Methods
        public void AssemblyLoadHandler(object sender, AssemblyLoadEventArgs args)
        {
            this.LogDebug(string.Format("AssemblyLoadEvent: " + args.LoadedAssembly.FullName + " from " + args.LoadedAssembly.CodeBase, new object[0]));
            this.lastAssemblyLoadIntent = string.Empty;
        }

        public Assembly AssemblyResolveHandler(object sender, ResolveEventArgs args)
        {
            this.LogDebug(string.Format("AssemblyResolveEvent: ApplicationName = '{0}', DomainId='{1}', args.Name='{2}'", AppDomain.CurrentDomain.FriendlyName, AppDomain.CurrentDomain.Id, args.Name));
            this.lastAssemblyLoadIntent = args.Name;
            if (this.useCustomAssemblyLoader && (this.assemblyLookupPathes != null))
            {
                string path = string.Empty;
                try
                {
                    for (int i = 0; i < this.assemblyLookupPathes.Count; i++)
                    {
                        path = Path.Combine(this.assemblyLookupPathes[i], args.Name + ".dll");
                        if (File.Exists(path))
                        {
                            return Assembly.LoadFrom(path);
                        }
                    }
                }
                catch (Exception exception)
                {
                    this.LogError(string.Format("AssemblyResolve: Failed to load file {0}, error={1}", path, exception.Message));
                }
            }
            return null;
        }

        public int CreateAppDomain(string name, string assemblyName, string baseDirectoryRaw, string applicationRootDirectoryRaw, string applicationDirectoryRaw, string sharedDirectoryRaw, string applicationSharedDirectoryRaw, bool enableShadowCopy, bool createPerInstanceShadowCopyCaches, string instanceName)
        {
            int num;
            try
            {
                if (!licenseIsValid)
                {
                    throw new Exception("License is Expired!");
                }
                num = this.photonDomainBehavior.CreateAppDomain(name, assemblyName, baseDirectoryRaw, applicationRootDirectoryRaw, applicationDirectoryRaw, sharedDirectoryRaw, applicationSharedDirectoryRaw, enableShadowCopy, createPerInstanceShadowCopyCaches, instanceName);
            }
            catch (Exception exception)
            {
                log.Error("CreateDomain", exception);
                throw;
            }
            return num;
        }

        public void DomainUnloadHandler(object sender, EventArgs e)
        {
            try
            {
                this.unmanagedLog.LogMessage(string.Format("Unloading Domain: ApplicationName = '{0}', DomainId='{1}'", AppDomain.CurrentDomain.FriendlyName, AppDomain.CurrentDomain.Id));
                this.photonDomainBehavior.DomainUnloadHandler(sender, e);
            }
            catch (Exception exception)
            {
                this.LogAndAdjustException(string.Format("Unloading Domain: ApplicationName = '{0}', DomainId='{1}'", AppDomain.CurrentDomain.FriendlyName, AppDomain.CurrentDomain.Id), exception);
            }
        }

        private static string GetFullPath(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return path;
            }
            string fullPath = Path.GetFullPath(path);
            char ch = path[path.Length - 1];
            if ((ch != Path.DirectorySeparatorChar) && (ch != Path.AltDirectorySeparatorChar))
            {
                return fullPath;
            }
            return Path.GetDirectoryName(fullPath);
        }

        private static string GetFullPath(string basePath, string path)
        {
            if (Path.IsPathRooted(path))
            {
                return GetFullPath(path);
            }
            return GetFullPath(Path.Combine(basePath, path));
        }

        public static string GetHardwareID(bool cpu, bool hdd, bool mac, bool mainboard, bool bios, bool os)
        {
            ILicensing licensing = TryGetLicensingInstance();
            if (licensing == null)
            {
                return null;
            }
            return licensing.GetHardwareId(cpu, hdd, mac, mainboard, bios, os);
        }

        public static SortedList GetLicenseInformation()
        {
            ILicensing licensing = TryGetLicensingInstance();
            if (licensing == null)
            {
                return null;
            }
            return licensing.GetLicenseInformation(null);
        }

        public bool GetLicenseInformation(IPhotonServerShutdown shutdown, out int maxConcurrentConnections, out string[] validIps)
        {
            bool licenseIsValid;
            try
            {
                PhotonDomainManager.licenseIsValid = this.photonDomainBehavior.GetLicenseInformation(shutdown, out maxConcurrentConnections, out validIps);
                licenseIsValid = PhotonDomainManager.licenseIsValid;
            }
            catch (Exception exception)
            {
                throw this.LogAndAdjustException("GetLicenseInformation()", exception);
            }
            return licenseIsValid;
        }

        public void InitialiseDefaultAppDomain(UnhandledExceptionPolicy unhandledExceptionPolicy, IUnloadApplicationDomains domainUnloader, ILogToUnmanagedLog logger, string logDirectory, IControlListeners listenerControl, IManageClientConnectionCount connectionCountManager)
        {
            try
            {
                this.photonDomainBehavior.InitialiseDefaultAppDomain(unhandledExceptionPolicy, domainUnloader, logger, logDirectory, listenerControl, connectionCountManager);
            }
            catch (Exception exception)
            {
                throw this.LogAndAdjustException("InitialiseDefaultAppDomain()", exception);
            }
        }

        public override void InitializeNewDomain(AppDomainSetup appDomainInfo)
        {
            try
            {
                if (this.IsDefaultDomain)
                {
                    this.photonDomainBehavior = new PhotonDefaultAppDomainBehavior(this);
                }
                else
                {
                    this.photonDomainBehavior = new PhotonPlainAppDomainBehavior(this);
                }
                this.photonDomainBehavior.InitializeNewDomain(appDomainInfo);
                base.InitializationFlags = AppDomainManagerInitializationOptions.RegisterWithHost;
            }
            catch (Exception exception)
            {
                throw this.LogAndAdjustException("InitializeNewDomain()", exception);
            }
        }

        private Exception LogAndAdjustException(string where, Exception e)
        {
            string str;
            if (e.InnerException != null)
            {
                if (this.unmanagedLog != null)
                {
                    str = string.Concat(new object[] { this.DomainIdentifier, " - ", where, " failed. Exception:\r\n", e.InnerException, "\r\n at...\r\n", e });
                    if (this.unmanagedLog != null)
                    {
                        this.unmanagedLog.LogMessage(str);
                    }
                    else
                    {
                        OutputDebugString(str);
                    }
                }
                return e.InnerException;
            }
            str = string.Concat(new object[] { this.DomainIdentifier, " - ", where, " failed.\r\nException:\r\n", e });
            if (this.unmanagedLog != null)
            {
                this.unmanagedLog.LogMessage(str);
                return e;
            }
            OutputDebugString(str);
            return e;
        }

        public void LogDebug(string msg)
        {
            this.photonDomainBehavior.LogDebug(msg);
        }

        public void LogError(string msg)
        {
            this.photonDomainBehavior.LogError(msg);
        }

        public void LogError(string msg, Exception ex)
        {
            this.photonDomainBehavior.LogError(msg, ex);
        }

        public void LogInfo(string msg)
        {
            this.photonDomainBehavior.LogInfo(msg);
        }

        public void LogUnmanagedMsg(string msg)
        {
            if (this.unmanagedLog != null)
            {
                this.unmanagedLog.LogMessage(msg);
            }
            else
            {
                OutputDebugString(msg);
            }
        }

        public void LogWarning(string msg)
        {
            this.photonDomainBehavior.LogWarning(msg);
        }

        [DllImport("kernel32.dll")]
        private static extern void OutputDebugString(string lpOutputString);
        public void PhotonRunning()
        {
            this.photonIsRunning = true;
            try
            {
                this.photonDomainBehavior.PhotonRunning();
            }
            catch (Exception exception)
            {
                throw this.LogAndAdjustException("PhotonRunning()", exception);
            }
        }

        public Assembly ReflectionOnlyAssemblyResolveHandler(object sender, ResolveEventArgs args)
        {
            this.LogDebug(string.Format("ReflectionOnlyAssemblyResolveHandler: ApplicationName = '{0}', DomainId='{1}', args.Name='{2}'", AppDomain.CurrentDomain.FriendlyName, AppDomain.CurrentDomain.Id, args.Name));
            return null;
        }

        public void RequestStop()
        {
            try
            {
                this.photonDomainBehavior.RequestStop();
            }
            catch (Exception exception)
            {
                throw this.LogAndAdjustException("RequestStop()", exception);
            }
        }

        public Assembly ResourceResolveHandler(object sender, ResolveEventArgs args)
        {
            this.LogDebug(string.Format("ResourceResolveHandler: ApplicationName = '{0}', DomainId='{1}', args.Name='{2}'", AppDomain.CurrentDomain.FriendlyName, AppDomain.CurrentDomain.Id, args.Name));
            return null;
        }

        private void SetAppDomainEventHandlers(AppDomain appDomain, bool includeExceptions, UnhandledExceptionPolicy unhandledExceptionPolicy)
        {
            PhotonDomainManager domainManager = (PhotonDomainManager)appDomain.DomainManager;
            if (includeExceptions)
            {
                if (unhandledExceptionPolicy == UnhandledExceptionPolicy.UnhandledExceptionLogException)
                {
                    log.InfoFormat("AppDomains with unhandled exceptions are usually not unloaded and restarted.", new object[0]);
                    appDomain.UnhandledException += new UnhandledExceptionEventHandler(domainManager.UnhandledExceptionHandlerLogException);
                }
                else
                {
                    log.InfoFormat("AppDomains with unhandled exceptions are unloaded and restarted.", new object[0]);
                    appDomain.UnhandledException += new UnhandledExceptionEventHandler(domainManager.UnhandledExceptionHandlerUnloadAppDomain);
                }
            }
            appDomain.AssemblyLoad += new AssemblyLoadEventHandler(domainManager.AssemblyLoadHandler);
            appDomain.AssemblyResolve += new ResolveEventHandler(domainManager.AssemblyResolveHandler);
            appDomain.DomainUnload += new EventHandler(domainManager.DomainUnloadHandler);
            appDomain.ReflectionOnlyAssemblyResolve += new ResolveEventHandler(domainManager.ReflectionOnlyAssemblyResolveHandler);
            appDomain.ResourceResolve += new ResolveEventHandler(domainManager.ResourceResolveHandler);
            appDomain.TypeResolve += new ResolveEventHandler(domainManager.TypeResolveHandler);
        }

        public void SetPhotonConnectionCountManager(IntPtr intPtr)
        {
            this.photonConnectionCountManager = (IManageClientConnectionCount)Marshal.GetObjectForIUnknown(intPtr);
        }

        public IPhotonApplication Start(string assemblyName, string typeName, string instanceName, string applicationName, IPhotonApplicationSink sink, ILogToUnmanagedLog logger, IControlListeners listenerControl)
        {
            IPhotonApplication application;
            try
            {
                application = this.photonDomainBehavior.Start(assemblyName, typeName, instanceName, applicationName, sink, logger, listenerControl);
            }
            catch (Exception exception)
            {
                throw this.LogAndAdjustException("Start()", exception);
            }
            return application;
        }

        public void Stop()
        {
            try
            {
                this.photonDomainBehavior.Stop();
            }
            catch (Exception exception)
            {
                throw this.LogAndAdjustException("Stop()", exception);
            }
        }

        private static ILicensing TryGetLicensingInstance()
        {
            if (licensingInstance == null)
            {
                try
                {
                    licensingInstance = (ILicensing)Assembly.Load(AssemblyName.GetAssemblyName("PhotonLicensing.dll")).CreateInstance("PhotonHostRuntime.Licensing");
                }
                catch (Exception exception)
                {
                    log.ErrorFormat("Failed to get PhotonLicensing instance: {0}", exception.Message);
                    return null;
                }
            }
            return licensingInstance;
        }

        public Assembly TypeResolveHandler(object sender, ResolveEventArgs args)
        {
            this.LogDebug(string.Format("TypeResolveHandler: ApplicationName = '{0}', DomainId='{1}', args.Name='{2}'", AppDomain.CurrentDomain.FriendlyName, AppDomain.CurrentDomain.Id, args.Name));
            return null;
        }

        public void UnhandledExceptionHandlerLogException(object sender, UnhandledExceptionEventArgs e)
        {
            if (e.IsTerminating)
            {
                log.WarnFormat("Process is terminating.", new object[0]);
            }
            this.LogError("UnhandledException: ", (Exception)e.ExceptionObject);
        }

        public void UnhandledExceptionHandlerUnloadAppDomain(object sender, UnhandledExceptionEventArgs e)
        {
            this.UnhandledExceptionHandlerLogException(sender, e);
            if (sender != AppDomain.CurrentDomain)
            {
                AppDomain domain = (AppDomain)sender;
                if (!domain.IsFinalizingForUnload())
                {
                    log.InfoFormat("Unloading app domain {0} after unhandled exception", domain.FriendlyName);
                    string reason = "Unloading app domain " + domain.FriendlyName + " after unhandled exception: " + e.ExceptionObject.ToString();
                    this.appDomainUnloader.UnloadAppDomain(domain.Id, reason);
                }
            }
        }

        // Properties
        public string ApplicationBinaryFullDirectory
        {
            get
            {
                return this.applicationBinaryFullDirectory;
            }
        }

        public string ApplicationFullDirectory
        {
            get
            {
                return this.applicationFullDirectory;
            }
        }

        public string ApplicationRootFullDirectory
        {
            get
            {
                return this.applicationRootFullDirectory;
            }
        }

        public PhotonDomainManager DefaultDomainManager
        {
            get
            {
                return this.defaultDomainManager;
            }
            set
            {
                this.defaultDomainManager = value;
            }
        }

        public string DomainIdentifier
        {
            get
            {
                return (AppDomain.CurrentDomain.FriendlyName + ":" + AppDomain.CurrentDomain.Id);
            }
        }

        public string InstanceName
        {
            get
            {
                return this.instanceName;
            }
        }

        public bool IsDefaultDomain
        {
            get
            {
                return AppDomain.CurrentDomain.IsDefaultAppDomain();
            }
        }

        public string PhotonExeFullDirectory
        {
            get
            {
                return this.photonExeFullDirectory;
            }
        }

        public bool PhotonIsRunning
        {
            get
            {
                return this.photonIsRunning;
            }
        }

        public string SDKVersion
        {
            get
            {
                return this.sdkVersion;
            }
        }

        public string UnmanagedLogDirectory
        {
            get
            {
                return this.unmanagedLogDirectory;
            }
        }

        // Nested Types
        private enum AppDomainInitEnum
        {
            ApplicationDirectory,
            UnmanagedLogDirectory,
            PhotonExeDirectory,
            ApplicationRootDirectory,
            ApplicationBinaryDirectory,
            UseCustomAssemblyLoader
        }

        private interface IPhotonDomainBehavior
        {
            // Methods
            int CreateAppDomain(string name, string assemblyName, string baseDirectoryRaw, string applicationRootDirectoryRaw, string applicationDirectoryRaw, string sharedDirectoryRaw, string applicationSharedDirectoryRaw, bool enableShadowCopy, bool createPerInstanceShadowCopyCaches, string instanceName);
            void DomainUnloadHandler(object sender, EventArgs e);
            bool GetLicenseInformation(IPhotonServerShutdown shutdown, out int maxConcurrentConnections, out string[] validIps);
            void InitialiseDefaultAppDomain(UnhandledExceptionPolicy unhandledExceptionPolicy, IUnloadApplicationDomains domainUnloader, ILogToUnmanagedLog logger, string logDirectory, IControlListeners listenerControl, IManageClientConnectionCount connectionCountManager);
            void InitializeNewDomain(AppDomainSetup appDomainInfo);
            void LogDebug(string msg);
            void LogError(string msg);
            void LogError(string msg, Exception ex);
            void LogInfo(string msg);
            void LogWarning(string msg);
            void PhotonRunning();
            void RequestStop();
            IPhotonApplication Start(string assemblyName, string typeName, string instanceName, string applicationName, IPhotonApplicationSink sink, ILogToUnmanagedLog logger, IControlListeners listenerControl);
            void Stop();
        }

        private class PhotonDefaultAppDomainBehavior : PhotonDomainManager.IPhotonDomainBehavior
        {
            // Fields
            private readonly PhotonDomainManager photonDomainManagerState;

            // Methods
            public PhotonDefaultAppDomainBehavior(PhotonDomainManager state)
            {
                this.photonDomainManagerState = state;
            }

            public int CreateAppDomain(string name, string assemblyName, string baseDirectoryRaw, string applicationRootDirectoryRaw, string applicationDirectoryRaw, string sharedDirectoryRaw, string applicationSharedDirectoryRaw, bool enableShadowCopy, bool createPerInstanceShadowCopyCaches, string instanceName)
            {
                if (PhotonDomainManager.log.IsInfoEnabled)
                {
                    PhotonDomainManager.log.InfoFormat("CreateAppDomain: name = '{0}' , assemblyName = '{1}'", name, assemblyName);
                }
                this.photonDomainManagerState.instanceName = instanceName;
                string[] strArray = applicationSharedDirectoryRaw.Split(new char[] { ';' });
                for (int i = 0; i < strArray.Length; i++)
                {
                }
                this.photonDomainManagerState.clrBaseDirectory = PhotonDomainManager.GetFullPath(baseDirectoryRaw);
                this.photonDomainManagerState.applicationRootFullDirectory = PhotonDomainManager.GetFullPath(this.photonDomainManagerState.clrBaseDirectory, applicationRootDirectoryRaw);
                this.photonDomainManagerState.applicationFullDirectory = PhotonDomainManager.GetFullPath(this.photonDomainManagerState.applicationRootFullDirectory, applicationDirectoryRaw);
                string applicationFullDirectory = this.photonDomainManagerState.applicationFullDirectory;
                string path = Path.Combine(applicationFullDirectory, assemblyName + ".dll");
                if (!File.Exists(path))
                {
                    applicationFullDirectory = Path.Combine(applicationFullDirectory, "bin");
                    path = Path.Combine(applicationFullDirectory, assemblyName + ".dll");
                    if (!File.Exists(path))
                    {
                        throw new Exception(string.Format("Can't find {0}.dll in {1} or {2}", assemblyName, this.photonDomainManagerState.applicationBinaryFullDirectory, applicationFullDirectory));
                    }
                }
                this.photonDomainManagerState.applicationBinaryFullDirectory = applicationFullDirectory;
                string clrBaseDirectory = this.photonDomainManagerState.clrBaseDirectory;
                string str5 = string.Empty;
                string fullPath = PhotonDomainManager.GetFullPath(sharedDirectoryRaw);
                if (fullPath != this.photonDomainManagerState.photonExeFullDirectory)
                {
                    this.photonDomainManagerState.unmanagedLog.LogMessage("*******************************************************************************************************************************");
                    this.photonDomainManagerState.unmanagedLog.LogMessage(" Using CustomAssemblyLoader:");
                    this.photonDomainManagerState.unmanagedLog.LogMessage(string.Format("   The location of the started PhotonSocketServer.exe {0}", this.photonDomainManagerState.photonExeFullDirectory));
                    this.photonDomainManagerState.unmanagedLog.LogMessage(string.Format("   doesn't match the path to the installed service '{0}'.", fullPath));
                    this.photonDomainManagerState.unmanagedLog.LogMessage(string.Format("   Setting CLRbaseDirectory to '{0}'", this.photonDomainManagerState.photonExeFullDirectory));
                    this.photonDomainManagerState.unmanagedLog.LogMessage("   WARNING: Your Application logs might be found beside the Photon logs - to fix this check the log4net samples in the demos.");
                    this.photonDomainManagerState.unmanagedLog.LogMessage("   WARNING: Code using AppDomain.CurrentDomain.BaseDirectory, should use ApplicationBase.ApplicationRootPath instead.");
                    this.photonDomainManagerState.unmanagedLog.LogMessage("*******************************************************************************************************************************");
                    clrBaseDirectory = this.photonDomainManagerState.photonExeFullDirectory;
                    this.photonDomainManagerState.useCustomAssemblyLoader = true;
                }
                else if (!fullPath.Contains(this.photonDomainManagerState.clrBaseDirectory))
                {
                    this.photonDomainManagerState.unmanagedLog.LogMessage("*******************************************************************************************************************************");
                    this.photonDomainManagerState.unmanagedLog.LogMessage(" Using CustomAssemblyLoader:");
                    this.photonDomainManagerState.unmanagedLog.LogMessage(string.Format("   PhotonSocketServer.exe is not located in the sub-dir-tree of the configured CLRbaseDirectory '{0}'.", this.photonDomainManagerState.clrBaseDirectory));
                    this.photonDomainManagerState.unmanagedLog.LogMessage(string.Format("   Setting CLRbaseDirectory to '{0}'", this.photonDomainManagerState.photonExeFullDirectory));
                    this.photonDomainManagerState.unmanagedLog.LogMessage("   WARNING: Your Application logs might be found beside the Photon logs - to fix this check the log4net samples in the demos.");
                    this.photonDomainManagerState.unmanagedLog.LogMessage("   WARNING: Code using AppDomain.CurrentDomain.BaseDirectory, should use ApplicationBase.ApplicationRootPath instead.");
                    this.photonDomainManagerState.unmanagedLog.LogMessage("*******************************************************************************************************************************");
                    clrBaseDirectory = this.photonDomainManagerState.photonExeFullDirectory;
                    this.photonDomainManagerState.useCustomAssemblyLoader = true;
                }
                else if (!this.photonDomainManagerState.applicationBinaryFullDirectory.Contains(this.photonDomainManagerState.clrBaseDirectory))
                {
                    this.photonDomainManagerState.unmanagedLog.LogMessage("*******************************************************************************************************************************");
                    this.photonDomainManagerState.unmanagedLog.LogMessage(" Using CustomAssemblyLoader:");
                    this.photonDomainManagerState.unmanagedLog.LogMessage(string.Format("   The location of the application '{0}' is not in the sub-dir-tree", this.photonDomainManagerState.photonExeFullDirectory));
                    this.photonDomainManagerState.unmanagedLog.LogMessage(string.Format("   of the configured CLRbaseDirectory '{0}'.", this.photonDomainManagerState.clrBaseDirectory));
                    this.photonDomainManagerState.unmanagedLog.LogMessage("*******************************************************************************************************************************");
                    this.photonDomainManagerState.useCustomAssemblyLoader = true;
                    str5 = fullPath.Substring(this.photonDomainManagerState.clrBaseDirectory.Length + 1) + ";";
                }
                else
                {
                    str5 = fullPath.Substring(this.photonDomainManagerState.clrBaseDirectory.Length + 1) + ";";
                }
                for (int j = 0; j < strArray.Length; j++)
                {
                    str5 = str5 + Path.Combine(applicationRootDirectoryRaw, strArray[j]) + ";";
                }
                AppDomainSetup setup = new AppDomainSetup
                {
                    ApplicationName = name,
                    ApplicationBase = clrBaseDirectory,
                    PrivateBinPath = this.photonDomainManagerState.applicationBinaryFullDirectory + ";" + str5,
                    ConfigurationFile = path + ".config"
                };
                FileInfo info = new FileInfo(setup.ConfigurationFile);
                if (!info.Exists)
                {
                    if (PhotonDomainManager.log.IsDebugEnabled)
                    {
                        PhotonDomainManager.log.DebugFormat("  ConfigurationFile= {0} doesn't exist.", setup.ConfigurationFile);
                    }
                    setup.ConfigurationFile = string.Empty;
                }
                if (enableShadowCopy)
                {
                    setup.ShadowCopyFiles = "true";
                    setup.CachePath = Path.Combine(this.photonDomainManagerState.applicationBinaryFullDirectory, "Cache");
                    if (createPerInstanceShadowCopyCaches)
                    {
                        setup.CachePath = Path.Combine(setup.CachePath, instanceName);
                    }
                }
                if (PhotonDomainManager.log.IsInfoEnabled)
                {
                    PhotonDomainManager.log.InfoFormat("  ApplicationBase = '{0}'", setup.ApplicationBase);
                    PhotonDomainManager.log.InfoFormat("  PrivateBinPath = '{0}'", setup.PrivateBinPath);
                    PhotonDomainManager.log.InfoFormat("  ConfigurationFile = '{0}'", setup.ConfigurationFile);
                    PhotonDomainManager.log.InfoFormat("  CachePath = '{0}'", setup.CachePath);
                }
                if (PhotonDomainManager.log.IsDebugEnabled)
                {
                    PhotonDomainManager.log.DebugFormat("  DomainManager.applicationBinaryFullDirectory = '{0}'", this.photonDomainManagerState.applicationBinaryFullDirectory);
                    PhotonDomainManager.log.DebugFormat("  DomainManager.unmanagedLogDirectory = '{0}'", this.photonDomainManagerState.unmanagedLogDirectory);
                    PhotonDomainManager.log.DebugFormat("  DomainManager.photonExeFullDirectory = '{0}'", this.photonDomainManagerState.photonExeFullDirectory);
                    PhotonDomainManager.log.DebugFormat("  DomainManager.applicationRootFullDirectory = '{0}'", this.photonDomainManagerState.applicationRootFullDirectory);
                }
                string[] strArray2 = new string[6];
                strArray2[0] = this.photonDomainManagerState.applicationFullDirectory;
                strArray2[4] = this.photonDomainManagerState.applicationBinaryFullDirectory;
                strArray2[3] = this.photonDomainManagerState.applicationRootFullDirectory;
                strArray2[2] = this.photonDomainManagerState.photonExeFullDirectory;
                strArray2[1] = this.photonDomainManagerState.unmanagedLogDirectory;
                strArray2[5] = this.photonDomainManagerState.useCustomAssemblyLoader.ToString(CultureInfo.InvariantCulture);
                setup.AppDomainInitializerArguments = strArray2;
                AppDomain appDomain = AppDomain.CreateDomain(name, null, setup);
                this.photonDomainManagerState.SetAppDomainEventHandlers(appDomain, false, UnhandledExceptionPolicy.UnhandledExceptionLogException);
                try
                {
                    PhotonDomainManager domainManager = (PhotonDomainManager)appDomain.DomainManager;
                    domainManager.DefaultDomainManager = this.photonDomainManagerState;
                    IntPtr iUnknownForObject = Marshal.GetIUnknownForObject(this.photonDomainManagerState.photonConnectionCountManager);
                    domainManager.SetPhotonConnectionCountManager(iUnknownForObject);
                    Marshal.Release(iUnknownForObject);
                    if (PhotonDomainManager.log.IsInfoEnabled)
                    {
                        PhotonDomainManager.log.InfoFormat("  ApplicationPath = '{0}'", domainManager.ApplicationFullDirectory);
                        PhotonDomainManager.log.InfoFormat("  BinaryPath = '{0}'", domainManager.ApplicationBinaryFullDirectory);
                        PhotonDomainManager.log.InfoFormat("  ApplicationRootPath = '{0}'", domainManager.ApplicationRootFullDirectory);
                        PhotonDomainManager.log.InfoFormat("  UnmanagedLogPath = '{0}'", domainManager.UnmanagedLogDirectory);
                    }
                }
                catch (Exception exception)
                {
                    this.photonDomainManagerState.LogAndAdjustException("Due to crossdomain issues Photon managed logging might be reduced.", exception);
                }
                return appDomain.Id;
            }

            public void DomainUnloadHandler(object sender, EventArgs e)
            {
            }

            public bool GetLicenseInformation(IPhotonServerShutdown shutdown, out int maxConcurrentConnections, out string[] validIps)
            {
                bool flag;
                PhotonDomainManager.log.Info("Getting license information:");
                this.photonDomainManagerState.photonServerShutdown = shutdown;
                Version assemblyVersion = Assembly.GetAssembly(typeof(PhotonDomainManager)).GetName().Version;
                ILicensing licensing = PhotonDomainManager.TryGetLicensingInstance();
                if (licensing == null)
                {
                    string message = "ERROR: failed to load PhotonLicensing.dll. Going to shutdown.";
                    PhotonDomainManager.log.Error(message);
                    this.LogUnmanagedMsg(message);
                    maxConcurrentConnections = 20;
                    validIps = new string[0];
                    return false;
                }
                bool flag2 = licensing.GetLicenseInformation(shutdown, out maxConcurrentConnections, out validIps, out flag, assemblyVersion, this.photonDomainManagerState.unmanagedLog, this.photonDomainManagerState) == 1;
                if (flag)
                {
                    this.LogUnmanagedMsg("LICENSE: No license file was found. Starting with Bootstrap License.");
                }
                return flag2;
            }

            public void InitialiseDefaultAppDomain(UnhandledExceptionPolicy unhandledExceptionPolicy, IUnloadApplicationDomains domainUnloader, ILogToUnmanagedLog logger, string logDirectory, IControlListeners listenerControl, IManageClientConnectionCount connectionCountManager)
            {
                this.photonDomainManagerState.photonExeFullDirectory = PhotonDomainManager.GetFullPath(AppDomain.CurrentDomain.BaseDirectory);
                this.photonDomainManagerState.unmanagedLogDirectory = PhotonDomainManager.GetFullPath(logDirectory);
                this.photonDomainManagerState.unmanagedLog = logger;
                this.photonDomainManagerState.appDomainUnloader = domainUnloader;
                this.photonDomainManagerState.photonConnectionCountManager = connectionCountManager;
                GlobalContext.Properties["Photon:UnmanagedLogDirectory"] = this.photonDomainManagerState.unmanagedLogDirectory;
                this.SetupLog4net(this.photonDomainManagerState.photonExeFullDirectory);
                this.photonDomainManagerState.defaultDomainManager = this.photonDomainManagerState;
                this.photonDomainManagerState.SetAppDomainEventHandlers(AppDomain.CurrentDomain, true, unhandledExceptionPolicy);
            }

            public void InitializeNewDomain(AppDomainSetup appDomainInfo)
            {
                Environment.CurrentDirectory = appDomainInfo.ApplicationBase;
            }

            public void LogDebug(string msg)
            {
                if (PhotonDomainManager.log.IsDebugEnabled)
                {
                    PhotonDomainManager.log.Debug(msg);
                }
            }

            public void LogError(string msg)
            {
                if (PhotonDomainManager.log.IsErrorEnabled)
                {
                    PhotonDomainManager.log.Error(msg);
                }
            }

            public void LogError(string msg, Exception ex)
            {
                if (PhotonDomainManager.log.IsErrorEnabled)
                {
                    PhotonDomainManager.log.Error(msg, ex);
                }
            }

            public void LogInfo(string msg)
            {
                if (PhotonDomainManager.log.IsInfoEnabled)
                {
                    PhotonDomainManager.log.Info(msg);
                }
            }

            private void LogUnmanagedMsg(string msg)
            {
                this.photonDomainManagerState.LogUnmanagedMsg(msg);
            }

            public void LogWarning(string msg)
            {
                if (PhotonDomainManager.log.IsInfoEnabled)
                {
                    PhotonDomainManager.log.Info(msg);
                }
            }

            public void PhotonRunning()
            {
                this.LogInfo(string.Format("PhotonRunning: ApplicationName = '{0}', DomainId='{1}'", AppDomain.CurrentDomain.FriendlyName, AppDomain.CurrentDomain.Id));
            }

            public void RequestStop()
            {
                this.LogInfo(string.Format("RequestStop: ApplicationName = '{0}', DomainId='{1}'", AppDomain.CurrentDomain.FriendlyName, AppDomain.CurrentDomain.Id));
            }

            private void SetupLog4net(string photonlog4netpath)
            {
                if (string.IsNullOrEmpty(photonlog4netpath))
                {
                    this.LogUnmanagedMsg("Failed to setup PhotonClr.log - undefined path.");
                }
                else
                {
                    FileInfo configFile = new FileInfo(Path.Combine(photonlog4netpath, PhotonDomainManager.Log4NetConfigFilename));
                    if (configFile.Exists)
                    {
                        XmlConfigurator.ConfigureAndWatch(configFile);
                    }
                    else
                    {
                        this.LogUnmanagedMsg("Failed to setup PhotonClr.log - '{0}' does not exist.");
                    }
                    if (PhotonDomainManager.log.IsInfoEnabled)
                    {
                        PhotonDomainManager.log.InfoFormat("Initialize: ApplicationName = '{0}', DomainID = '{1}'", AppDomain.CurrentDomain.FriendlyName, AppDomain.CurrentDomain.Id);
                    }
                }
            }

            public IPhotonApplication Start(string assemblyName, string typeName, string instanceName, string applicationName, IPhotonApplicationSink sink, ILogToUnmanagedLog logger, IControlListeners listenerControl)
            {
                throw new NotImplementedException("This behavior is implemented by the plain app domain only.");
            }

            public void Stop()
            {
                throw new NotImplementedException("This behavior is implemented by the plain app domain only.");
            }
        }

        private class PhotonPlainAppDomainBehavior : PhotonDomainManager.IPhotonDomainBehavior
        {
            // Fields
            private readonly PhotonDomainManager photonDomainManagerState;

            // Methods
            public PhotonPlainAppDomainBehavior(PhotonDomainManager state)
            {
                this.photonDomainManagerState = state;
            }

            public int CreateAppDomain(string name, string assemblyName, string baseDirectoryRaw, string applicationRootDirectoryRaw, string applicationDirectoryRaw, string sharedDirectoryRaw, string applicationSharedDirectoryRaw, bool enableShadowCopy, bool createPerInstanceShadowCopyCaches, string instanceName)
            {
                throw new NotImplementedException("This behavior is implemented by the default domain only.");
            }

            public void DomainUnloadHandler(object sender, EventArgs e)
            {
            }

            public bool GetLicenseInformation(IPhotonServerShutdown shutdown, out int maxConcurrentConnections, out string[] validIps)
            {
                throw new NotImplementedException("This behavior is implemented by the default domain only.");
            }

            public void InitialiseDefaultAppDomain(UnhandledExceptionPolicy unhandledExceptionPolicy, IUnloadApplicationDomains domainUnloader, ILogToUnmanagedLog logger, string logDirectory, IControlListeners listenerControl, IManageClientConnectionCount connectionCountManager)
            {
                throw new NotImplementedException("This behavior is implemented by the default domain only.");
            }

            public void InitializeNewDomain(AppDomainSetup appDomainInfo)
            {
                if ((appDomainInfo.AppDomainInitializerArguments != null) && (appDomainInfo.AppDomainInitializerArguments.Length >= 6))
                {
                    this.photonDomainManagerState.applicationBinaryFullDirectory = appDomainInfo.AppDomainInitializerArguments[4];
                    this.photonDomainManagerState.unmanagedLogDirectory = appDomainInfo.AppDomainInitializerArguments[1];
                    this.photonDomainManagerState.photonExeFullDirectory = appDomainInfo.AppDomainInitializerArguments[2];
                    this.photonDomainManagerState.applicationRootFullDirectory = appDomainInfo.AppDomainInitializerArguments[3];
                    this.photonDomainManagerState.useCustomAssemblyLoader = bool.Parse(appDomainInfo.AppDomainInitializerArguments[5]);
                    this.photonDomainManagerState.applicationFullDirectory = appDomainInfo.AppDomainInitializerArguments[0];
                    if (this.photonDomainManagerState.useCustomAssemblyLoader)
                    {
                        this.photonDomainManagerState.assemblyLookupPathes = new List<string>();
                        foreach (string str in appDomainInfo.PrivateBinPath.Split(new char[] { ';' }))
                        {
                            if (!string.IsNullOrEmpty(str))
                            {
                                if (Path.IsPathRooted(str))
                                {
                                    this.photonDomainManagerState.assemblyLookupPathes.Add(str);
                                }
                                else
                                {
                                    this.photonDomainManagerState.assemblyLookupPathes.Add(Path.Combine(this.photonDomainManagerState.applicationRootFullDirectory, str));
                                }
                            }
                        }
                    }
                    Environment.CurrentDirectory = this.photonDomainManagerState.applicationFullDirectory;
                }
            }

            public void LogDebug(string msg)
            {
                if (this.photonDomainManagerState.defaultDomainManager != null)
                {
                    try
                    {
                        this.photonDomainManagerState.defaultDomainManager.LogWarning(msg);
                    }
                    catch (RemotingException)
                    {
                        PhotonDomainManager.OutputDebugString("PhotonCLR: DEBUG - " + msg);
                    }
                    catch (Exception exception)
                    {
                        this.LogUnmanagedMsg("PhotonCLR: DEBUG - " + msg);
                        this.photonDomainManagerState.LogAndAdjustException("PhotonCLR: DEBUG Exception", exception);
                    }
                }
                else
                {
                    this.LogUnmanagedMsg("PhotonCLR: DEBUG - " + msg);
                }
            }

            public void LogError(string msg)
            {
                if (this.photonDomainManagerState.defaultDomainManager != null)
                {
                    try
                    {
                        this.photonDomainManagerState.defaultDomainManager.LogError(msg);
                    }
                    catch (RemotingException)
                    {
                        PhotonDomainManager.OutputDebugString("PhotonCLR: ERROR - " + msg);
                    }
                    catch (Exception exception)
                    {
                        this.LogUnmanagedMsg("PhotonCLR: ERROR - " + msg);
                        this.photonDomainManagerState.LogAndAdjustException("PhotonCLR: ERROR Exception", exception);
                    }
                }
                else
                {
                    this.LogUnmanagedMsg("PhotonCLR: ERROR - " + msg);
                }
            }

            public void LogError(string msg, Exception ex)
            {
                if (this.photonDomainManagerState.defaultDomainManager != null)
                {
                    try
                    {
                        this.photonDomainManagerState.defaultDomainManager.LogError(msg, ex);
                    }
                    catch (RemotingException)
                    {
                        PhotonDomainManager.OutputDebugString("PhotonCLR: ERROR - " + msg + " - " + ex.Message);
                    }
                    catch (Exception)
                    {
                        this.LogUnmanagedMsg("PhotonCLR: ERROR - " + msg + " - " + ex.Message);
                        this.photonDomainManagerState.LogAndAdjustException("PhotonCLR: ERROR Exception", ex);
                    }
                }
                else
                {
                    this.LogUnmanagedMsg("PhotonCLR: ERROR - " + msg);
                }
            }

            public void LogInfo(string msg)
            {
                if (this.photonDomainManagerState.defaultDomainManager != null)
                {
                    try
                    {
                        this.photonDomainManagerState.defaultDomainManager.LogWarning(msg);
                    }
                    catch (RemotingException)
                    {
                        PhotonDomainManager.OutputDebugString("PhotonCLR: INFO - " + msg);
                    }
                    catch (Exception exception)
                    {
                        this.LogUnmanagedMsg("PhotonCLR: INFO - " + msg);
                        this.photonDomainManagerState.LogAndAdjustException("PhotonCLR: INFO Exception", exception);
                    }
                }
                else
                {
                    this.LogUnmanagedMsg("PhotonCLR: INFO - " + msg);
                }
            }

            private void LogUnmanagedMsg(string msg)
            {
                this.photonDomainManagerState.LogUnmanagedMsg(msg);
            }

            public void LogWarning(string msg)
            {
                if (this.photonDomainManagerState.defaultDomainManager != null)
                {
                    try
                    {
                        this.photonDomainManagerState.defaultDomainManager.LogWarning(msg);
                    }
                    catch (RemotingException)
                    {
                        PhotonDomainManager.OutputDebugString("PhotonCLR: WARNING - " + msg);
                    }
                    catch (Exception exception)
                    {
                        this.LogUnmanagedMsg("PhotonCLR: WARNING - " + msg);
                        this.photonDomainManagerState.LogAndAdjustException("PhotonCLR: WARNING Exception", exception);
                    }
                }
                else
                {
                    this.LogUnmanagedMsg("PhotonCLR: WARNING - " + msg);
                }
            }

            public void PhotonRunning()
            {
                if (this.photonDomainManagerState.photonControl == null)
                {
                    throw new Exception("PhotonRunning: unable to call the Application.PhotonRunning() - undefined photonControl.");
                }
                this.LogInfo(string.Format("PhotonRunning: ApplicationName = '{0}', DomainId='{1}' -- calling Application.Setup", AppDomain.CurrentDomain.FriendlyName, AppDomain.CurrentDomain.Id));
                this.photonDomainManagerState.photonControl.OnPhotonRunning();
            }

            public void RemoveAppDomain(string identifier)
            {
                throw new NotImplementedException("This behavior is implemented by the default app domain only.");
            }

            public void RequestStop()
            {
                if (this.photonDomainManagerState.photonControl == null)
                {
                    throw new Exception("RequestStop: unable to call the Application.OnStopRequested() - undefined photonControl.");
                }
                this.LogInfo(string.Format("RequestStop: ApplicationName = '{0}', DomainId='{1}' -- calling Application.OnStopRequested()", AppDomain.CurrentDomain.FriendlyName, AppDomain.CurrentDomain.Id));
                this.photonDomainManagerState.photonControl.OnStopRequested();
            }

            public IPhotonApplication Start(string assemblyName, string typeName, string instanceName, string applicationName, IPhotonApplicationSink sink, ILogToUnmanagedLog logger, IControlListeners listenerControl)
            {
                this.photonDomainManagerState.unmanagedLog = logger;
                Assembly assembly = Assembly.Load(assemblyName);
                try
                {
                    foreach (AssemblyName name in assembly.GetReferencedAssemblies())
                    {
                        if (name.Name == "Photon.SocketServer")
                        {
                            this.photonDomainManagerState.sdkVersion = name.Version.ToString();
                            this.LogInfo("Photon SDK:" + this.photonDomainManagerState.sdkVersion);
                        }
                    }
                }
                catch (Exception exception)
                {
                    this.LogDebug("SDK version check failed. " + exception.Message);
                }
                object obj2 = assembly.CreateInstance(typeName);
                if (obj2 == null)
                {
                    if (string.IsNullOrEmpty(this.photonDomainManagerState.lastAssemblyLoadIntent))
                    {
                        throw new TypeLoadException("Failed to create object \"" + typeName + "\" from assembly \"" + assemblyName + "\".");
                    }
                    throw new TypeLoadException("Failed to create object \"" + typeName + "\" from assembly \"" + assemblyName + "\". \"" + this.photonDomainManagerState.lastAssemblyLoadIntent + "\" is last assembly without successfull load event.");
                }
                IPhotonControl control = (IPhotonControl)obj2;
                this.photonDomainManagerState.photonControl = control;
                this.LogInfo(string.Format("Starting: name = '{0}', assemblyName = '{1}', typeName = '{2}'", applicationName, assemblyName, typeName));
                this.photonDomainManagerState.photonApplicationsCounter = this.photonDomainManagerState.photonConnectionCountManager;
                return control.OnStart(instanceName, applicationName, sink, listenerControl, this.photonDomainManagerState.photonApplicationsCounter, this.photonDomainManagerState.unmanagedLogDirectory);
            }

            public void Stop()
            {
                if (this.photonDomainManagerState.photonControl == null)
                {
                    throw new Exception("Stop: unable to call the Application.TearDown() - undefined photonControl.");
                }
                this.LogInfo(string.Format("Stop: ApplicationName = '{0}', DomainId='{1}' -- calling Application.TearDown()", AppDomain.CurrentDomain.FriendlyName, AppDomain.CurrentDomain.Id));
                this.photonDomainManagerState.photonControl.OnStop();
            }
        }
    }
}
