﻿namespace Ll2NativeTests
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using Il2Native.Logic;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    public static class CompilerHelper
    {
#if _DISK_C_
        public const string SourcePath = @"C:\Dev\Gits\mono\mcs\tests\";
        public const string SourcePathCustom = @"C:\Temp\tests\";
        public const string OutputPath = @"C:\Temp\IlCTests\";
        public const string CoreLibCSProjPath = @"C:\Dev\Gits\Il2Native\CoreLib\CoreLib.csproj";
        public const string CoreLibDllPath = @"C:\Dev\Gits\Il2Native\CoreLib\bin\Release\CoreLib.dll";
        public const string CoreLibPdbPath = @"C:\Dev\Gits\Il2Native\CoreLib\bin\Release\CoreLib.pdb";
        public const string MscorlibCSProjPath = @"C:\Dev\Gits\Il2Native\mscorlib\mscorlib.csproj";
        public static string MscorlibDllPath = @"C:\Dev\Gits\Il2Native\mscorlib\bin\Release\mscorlib.dll";
        public const string MscorlibPdbPath = @"C:\Dev\Gits\Il2Native\mscorlib\bin\Release\mscorlib.pdb";
        public const string SscliSourcePath = @"C:\Dev\Unzipped\sscli20\tests\bcl\system\";
        public const string CoreCLRSourcePath = @"C:\Dev\Gits\coreclr\tests\src\";
        public const string CoreCLRDlls = @"C:\Dev\Gits\coreclr\tests\packages\dnx-coreclr-win-x86.1.0.0-beta5-12101\bin\";
        
        public const string OutputObjectFileExt = "obj";

        public const string GcHeaders = @"C:\Dev\Gits\bdwgc\include\";
#endif
#if _DISK_D_
        public const string SourcePath = @"D:\Temp\CSharpTranspilerExt\Mono-Class-Libraries\mcs\tests\";
        public const string SourcePathCustom = @"D:\Temp\tests\";
        public const string OutputPath = @"D:\Temp\IlCTests\";
        public const string CoreLibCSProjPath = @"..\..\..\CoreLib\CoreLib.csproj";
        public const string CoreLibDllPath = @"..\..\..\CoreLib\bin\Release\CoreLib.dll";
        public const string CoreLibPdbPath = @"..\..\..\CoreLib\bin\Release\CoreLib.pdb";
        public const string MscorlibCSProjPath = @"..\..\..\mscorlib\mscorlib.csproj";
        public static string MscorlibDllPath = @"..\..\..\mscorlib\bin\Release\mscorlib.dll";
        public const string MscorlibPdbPath = @"..\..\..\mscorlib\bin\Release\mscorlib.pdb";
        public const string SscliSourcePath = @"D:\Temp\CSharpTranspilerExt\sscli20\tests\bcl\system\";
        public const string CoreCLRSourcePath = @"C:\Dev\Gits\coreclr\tests\src\";
        public const string CoreCLRDlls = @"E:\Gits\coreclr\tests\packages\dnx-coreclr-win-x86.1.0.0-beta5-12101\bin\";
        
        public const string OutputObjectFileExt = "obj";
        
        public const string GcHeaders = @"E:\Gits\bdwgc\include";
#endif

        /// <summary>
        /// </summary>
        public static bool GcDebugEnabled = false;

        /// <summary>
        /// </summary>
        public static bool Mscorlib = false;

        /// <summary>
        /// </summary>
        public static bool AddSystemLinq = false;

        /// <summary>
        /// </summary>
        public static bool Stubs = false;

        /// <summary>
        /// </summary>
        /// <param name="includeCoreLib">
        /// </param>
        /// <param name="gc"></param>
        /// <param name="mt"></param>
        /// <param name="gctors"></param>
        /// <param name="debugInfo"></param>
        /// <param name="stubs"></param>
        /// <returns>
        /// </returns>
        public static string[] GetConverterArgs(bool includeCoreLib, bool stubs = false)
        {
            var args = new List<string>();
            if (includeCoreLib)
            {
                if (Mscorlib)
                {
                    args.Add("corelib:" + Path.GetFullPath(MscorlibDllPath));
                    
                    if (AddSystemLinq)
                    {
                        //args.Add("ref:System.Core");
                        args.Add(string.Format(@"ref:{0}System.Linq.dll", CoreCLRDlls));
                    }
                }
                else
                {
                    args.Add("corelib:" + Path.GetFullPath(CoreLibDllPath));
                }
            }

            if (!GcDebugEnabled)
            {
                args.Add("release");
            }

            if (stubs || Stubs)
            {
                args.Add("stubs");
            }

            return args.ToArray();
        }

        /// <summary>
        /// </summary>
        /// <param name="index">
        /// </param>
        /// <param name="assemblyName">
        /// </param>
        /// <param name="format">
        /// </param>
        /// <param name="justCompile">
        /// </param>
        public static void ExecCompile(string assemblyName, bool justCompile = false, bool opt = false, int returnCode = 0, bool debug = false)
        {
            Trace.WriteLine("==========================================================================");
            if (justCompile)
            {
                Trace.WriteLine("Compiling " + assemblyName);
            }
            else
            {
                Trace.WriteLine("Compiling/Executing " + assemblyName);
            }

            Trace.WriteLine("==========================================================================");
            Trace.WriteLine(string.Empty);

            // file exe
            ExecCmd(string.Format("build_vs2015_{0}.bat", debug ? "debug" : "release"), string.Empty, Path.Combine(OutputPath, assemblyName));

            if (!justCompile)
            {
                // test execution
                ExecCmd(
                    string.Format("{0}.exe", assemblyName.Replace("-", "_")),
                    string.Empty,
                    Path.Combine(OutputPath, string.Format("{0}\\__build_win32_{1}\\{2}\\", assemblyName, debug ? "debug" : "release", debug ? "Debug" : "Release")),
                    readOutput: true,
                    returnCode: returnCode);
            }
            else
            {
                Assert.IsTrue(File.Exists(string.Format("{0}\\__build_win32_{1}\\{2}\\{0}.exe", assemblyName, debug ? "debug" : "release", debug ? "Debug" : "Release")));
            }
        }

        private static string GetAllSourceFiles(string assemblyName)
        {
            var path = Path.Combine(OutputPath, assemblyName, "src");
            var allSources = string.Join(" ", Directory.GetFiles(path, "*.cpp", SearchOption.AllDirectories));
            return allSources;
        }

        public static void ExecCmd(
            string fileName,
            string arguments = "",
            string workingDir = OutputPath,
            bool readOutput = false,
            int returnCode = 0)
        {
            var processStartInfo = new ProcessStartInfo();
            processStartInfo.WorkingDirectory = workingDir;
            processStartInfo.FileName = readOutput ? Path.Combine(workingDir, fileName) : fileName;
            processStartInfo.Arguments = arguments;
            processStartInfo.CreateNoWindow = false;
            processStartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            processStartInfo.RedirectStandardOutput = readOutput;
            processStartInfo.UseShellExecute = !readOutput;

            var process = Process.Start(processStartInfo);
            var output = string.Empty;
            if (readOutput)
            {
                output = process.StandardOutput.ReadToEnd();
            }

            process.WaitForExit();

            if (readOutput)
            {
                Trace.WriteLine(output);
            }

            Assert.AreEqual(returnCode, process.ExitCode, string.Format("File: {0}, Arguments: {1}, Working Dir: {2}", fileName, arguments, workingDir));
        }

        /// <summary>
        /// </summary>
        public static void Compile(string fileName, string source = SourcePath, bool ignoreBadFiles = false, bool includeAll = true, int returnCode = 0, string additionalFilesFolder = "", string[] additionalFilesPattern = null)
        {
            try
            {
                if (includeAll)
                {
                    if (!ConvertAll(fileName, source, additionalFilesFolder, additionalFilesPattern))
                    {
                        return;
                    }
                }
                else
                {
                    Convert(fileName, source);
                }
            }
            catch (BadImageFormatException ex)
            {
                Trace.WriteLine(ex);

                if (!ignoreBadFiles)
                {
                    Assert.Inconclusive("Could not compile it");
                }

                return;
            }
            catch (FileNotFoundException ex)
            {
                Trace.WriteLine(ex);

                if (!ignoreBadFiles)
                {
                    Assert.Inconclusive("File not found");
                }

                return;
            }

            ExecCompile(fileName, true, returnCode: returnCode);
        }

        /// <summary>
        /// </summary>
        public static void CompileAndRun(string fileName, string source = SourcePath, bool ignoreBadFiles = false, bool includeAll = true, int returnCode = 0, string additionalFilesFolder = "", string[] additionalFilesPattern = null)
        {
            try
            {
                if (includeAll)
                {
                    if (!ConvertAll(fileName, source, additionalFilesFolder, additionalFilesPattern))
                    {
                        return;
                    }
                }
                else
                {
                    Convert(fileName, source);
                }
            }
            catch (BadImageFormatException ex)
            {
                Trace.WriteLine(ex);

                if (!ignoreBadFiles)
                {
                    Assert.Inconclusive("Could not compile it");
                }

                return;
            }
            catch (FileNotFoundException ex)
            {
                Trace.WriteLine(ex);

                if (!ignoreBadFiles)
                {
                    Assert.Inconclusive("File not found");
                }

                return;
            }

            ExecCompile(fileName, returnCode: returnCode);

            Thread.Sleep(400);

            // cleanup if success
            foreach (var fileToDelete in Directory.GetFiles(OutputPath, string.Format("{0}.*", fileName)).ToList())
            {
                File.Delete(fileToDelete);
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="number">
        /// </param>
        /// <param name="source">
        /// </param>
        /// <param name="fileName">
        /// </param>
        /// <param name="format">
        /// </param>
        public static void Convert(string fileName, string source = SourcePath)
        {
            Trace.WriteLine(string.Empty);
            Trace.WriteLine("==========================================================================");
            Trace.WriteLine("Generating C for " + fileName);
            Trace.WriteLine("==========================================================================");
            Trace.WriteLine(string.Empty);

            Il2Converter.Convert(
                string.Concat(source, string.Format("{0}.cs", fileName)),
                OutputPath,
                GetConverterArgs(true));
        }

        public static bool ConvertAll(string fileName, string source = SourcePath, string additionalFilesFolder = "", string[] additionalFilesPattern = null)
        {
            Trace.WriteLine(string.Empty);
            Trace.WriteLine("==========================================================================");
            Trace.WriteLine("Generating C for " + fileName);
            Trace.WriteLine("==========================================================================");
            Trace.WriteLine(string.Empty);

            var sources = Directory.GetFiles(source, string.Format("{0}-*.cs", fileName)).ToList();
            var filePath = Path.Combine(source, string.Format("{0}.cs", fileName));
            if (File.Exists(filePath))
            {
                sources.Insert(0, filePath);
            }

            if (!string.IsNullOrEmpty(additionalFilesFolder))
            {
                foreach (var pattern in additionalFilesPattern)
                {
                    sources = Directory.GetFiles(additionalFilesFolder, pattern).Union(sources).ToList();
                }
            }

            if (!sources.Any())
            {
                Trace.WriteLine(string.Empty);
                Trace.WriteLine("==========================================================================");
                Trace.WriteLine("MISSING! " + fileName);
                Trace.WriteLine("==========================================================================");
                Trace.WriteLine(string.Empty);
                return false;
            }

            Il2Converter.Convert(
                sources.ToArray(),
                OutputPath,
                GetConverterArgs(true));

            return true;
        }

        public static void AssertUiEnabled(bool enable)
        {
            foreach (var def in from object listener in Debug.Listeners
                let def = listener as DefaultTraceListener
                where listener is DefaultTraceListener
                select def)
            {
                def.AssertUiEnabled = enable;
            }
        }
    }
}