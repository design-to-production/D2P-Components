using Microsoft.Win32;
using System.Reflection;

[assembly: Parallelize(Scope = ExecutionScope.MethodLevel)]

namespace D2P_CoreTests
{
    [TestClass]
    public static class TestInit
    {
        private static bool _initialized = false;
        private static string _rhinoDir = string.Empty;

        [AssemblyInitialize]
        public static void AssemblyInitialize(TestContext context)
        {
            if (_initialized)
            {
                throw new InvalidOperationException("Rhino.Inside should only be initialized once.");
            }

            _rhinoDir = GetRhinoInstallationDirectory();
            AssertRhinoDirectoryExists(_rhinoDir);
            context.WriteLine("Current Rhino 7 installation: " + _rhinoDir);

            EnsureRunningIn64Bit();
            UpdateEnvironmentPath(_rhinoDir);

            RhinoInside.Resolver.Initialize();
            _initialized = true;
            context.WriteLine("Rhino.Inside initialization started.");

            StartRhino();
            AppDomain.CurrentDomain.AssemblyResolve += ResolveGrasshopper;
        }

        private static string GetRhinoInstallationDirectory()
        {
            return Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\McNeel\Rhinoceros\7.0\Install", "Path", null) as string ?? string.Empty;
        }

        private static void AssertRhinoDirectoryExists(string rhinoDir)
        {
            Assert.IsTrue(Directory.Exists(rhinoDir), $"Rhino system directory not found: {rhinoDir}");
        }

        private static void EnsureRunningIn64Bit()
        {
            Assert.IsTrue(Environment.Is64BitProcess, "Tests must be run as x64.");
        }

        private static void UpdateEnvironmentPath(string rhinoDir)
        {
            string currentPath = Environment.GetEnvironmentVariable("PATH") ?? string.Empty;
            Environment.SetEnvironmentVariable("PATH", currentPath + ";" + rhinoDir);
        }

        [STAThread]
        public static void StartRhino()
        {
            var rhinoCore = new Rhino.Runtime.InProcess.RhinoCore(null, Rhino.Runtime.InProcess.WindowStyle.NoWindow);
        }

        private static Assembly ResolveGrasshopper(object sender, ResolveEventArgs args)
        {
            var name = args.Name;

            if (!name.StartsWith("Grasshopper"))
            {
                return null;
            }

            var path = Path.Combine(Path.GetFullPath(Path.Combine(_rhinoDir, @"..\")), "Plug-ins\\Grasshopper\\Grasshopper.dll");
            return Assembly.LoadFrom(path);
        }
    }
}
