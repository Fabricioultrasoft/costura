using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;

internal static class ILTemplateWithTempAssembly
{
    [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
    public static extern bool MoveFileEx(string lpExistingFileName, string lpNewFileName, int dwFlags);

    //public const int MOVEFILE_DELAY_UNTIL_REBOOT = 0x4;

    private static string tempBasePath;

    public static void Attach()
    {
        AppDomain currentDomain = AppDomain.CurrentDomain;
        currentDomain.AssemblyResolve += ResolveAssembly;

        //Create a unique Temp directory for the application path.
        tempBasePath = Path.Combine(Path.Combine(Path.GetTempPath(), "Costura"),
                                    CreateMd5Hash(Assembly.GetExecutingAssembly().CodeBase));
        CreateDirectory();
    }


    public static Assembly ResolveAssembly(object sender, ResolveEventArgs args)
    {
        var name = new AssemblyName(args.Name).Name;
        var assemblyResourceName = string.Format("Costura.{0}.dll", name);
        var executingAssembly = Assembly.GetExecutingAssembly();

        var assemblyTempFilePath = Path.Combine(tempBasePath, assemblyResourceName);
        if (!File.Exists(assemblyTempFilePath))
        {
            using (var assemblyStream = executingAssembly.GetManifestResourceStream(assemblyResourceName))
            {
                if (assemblyStream == null)
                {
                    return null;
                }
                var assemblyData = new Byte[assemblyStream.Length];
                assemblyStream.Read(assemblyData, 0, assemblyData.Length);
                File.WriteAllBytes(assemblyTempFilePath, assemblyData);
                var pdbName = Path.ChangeExtension(assemblyResourceName, "pdb");
                using (var pdbStream = executingAssembly.GetManifestResourceStream(pdbName))
                {
                    if (pdbStream != null)
                    {
                        var pdbData = new Byte[pdbStream.Length];
                        pdbStream.Read(pdbData, 0, pdbData.Length);
                        var assemblyPdbTempFilePath = Path.Combine(tempBasePath, pdbName);
                        File.WriteAllBytes(assemblyPdbTempFilePath, pdbData);
                    }
                }
            }
        }
        return Assembly.LoadFile(assemblyTempFilePath);
    }

    private static void CreateDirectory()
    {
        if (Directory.Exists(tempBasePath))
        {
            try
            {
                Directory.Delete(tempBasePath, true);
            }
            catch
            {}
        }
        Directory.CreateDirectory(tempBasePath);
        MoveFileEx(tempBasePath, null, 0x4);
    }

    private static string CreateMd5Hash(string input)
    {
        using (MD5 md5 = MD5.Create())
        {
            byte[] inputBytes = Encoding.ASCII.GetBytes(input);
            byte[] hashBytes = md5.ComputeHash(inputBytes);

            var sb = new StringBuilder();
            for (int i = 0; i < hashBytes.Length; i++)
            {
                sb.Append(hashBytes[i].ToString("X2"));
            }
            return sb.ToString();
        }
    }
}