using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using snake2d;

namespace snake2d.util.process
{
    public static class Proccesser
    {
        private Proccesser()
        {
        }

        public static Process ExecuteLwjgl(Type clazz, string[] jvmArgs, string[] args, string[] classPaths)
        {
            if (OS.Get() == OS.MAC)
            {
                string[] ja = new string[jvmArgs.Length + 1];
                for (int i = 0; i < jvmArgs.Length; i++)
                    ja[i] = jvmArgs[i];
                ja[ja.Length - 1] = "-XstartOnFirstThread";
                jvmArgs = ja;
            }
            return Exec(clazz, jvmArgs, args, classPaths, true);
        }

        public static Process Exec(Type clazz, string[] jvmArgs, string[] args, string[] classPaths)
        {
            return Exec(clazz, jvmArgs, args, classPaths, false);
        }

        public static Process Exec(Type clazz, string[] jvmArgs, string[] args, string[] classPaths, bool addGameTweaks)
        {
            string javaHome = Environment.GetEnvironmentVariable("java.home");
            string javaBin = Path.Combine(javaHome, "bin", "java");
            string classpath = Environment.GetEnvironmentVariable("java.class.path");

            LOG.Ln("java exe is: " + javaBin);

            if (string.IsNullOrEmpty(classpath))
            {
                LOG.Ln("java.class.path is: '" + classpath + "'" + " bin");
                classpath = "SongsOfSyx.jar";
            }
            string className = clazz.FullName;

            List<string> command = new List<string>();
            command.Add(javaBin);
            command.AddRange(jvmArgs);
            command.Add("-cp");

            string cp = "";
            string sep = Path.PathSeparator.ToString();

            foreach (string c in classPaths)
            {
                cp += c + sep;
            }

            cp += classpath;
            command.Add(cp);
            command.Add(className);
            command.AddRange(args);

            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = javaBin,
                Arguments = string.Join(" ", command),
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            if (addGameTweaks)
            {
                startInfo.EnvironmentVariables["PULSE_PROP_application.media_role"] = "Game";
                startInfo.EnvironmentVariables["DRI_PRIME"] = "1";
                startInfo.EnvironmentVariables["__NV_PRIME_RENDER_OFFLOAD"] = "1";
                startInfo.EnvironmentVariables["__GLX_VENDOR_LIBRARY_NAME"] = "nvidia";
                startInfo.EnvironmentVariables["__VK_LAYER_NV_optimus"] = "NVIDIA_only";
            }

            Process process;
            try
            {
                process = new Process { StartInfo = startInfo };
                process.Start();
                return process;
            }
            catch (Exception e)
            {
                e.printStackTrace();
                return null;
            }
        }
    }
}