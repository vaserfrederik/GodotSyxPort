using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Diagnostics;

namespace init
{
    public class Main
    {
        public static void Main(string[] args)
        {
            MacWarning();

            try
            {
                if (args != null && args.Length > 0 && args[0].Equals("launcher", StringComparison.OrdinalIgnoreCase))
                {
                    LOG.Ln("*************************************");
                    LOG.Ln("* LAUNCHER " + VERSION.VERSION_STRING);
                    LOG.Ln("*************************************");

                    string[] jvmArgs = Array.Empty<string>();

                    try
                    {
                        string filePath = Path.Combine(Directory.GetCurrentDirectory(), "jvmargs-launcher.txt");
                        if (File.Exists(filePath))
                        {
                            jvmArgs = File.ReadAllLines(filePath);
                            foreach (var arg in jvmArgs)
                            {
                                LOG.Ln("Launcher arg: " + arg);
                            }
                        }
                        else
                        {
                            LOG.Ln("could not read launcher arguments: file does not exist");
                        }
                    }
                    catch (Exception e)
                    {
                        e.StackTrace?.Dump();
                        LOG.Ln("could not read launcher arguments");
                    }

                    Process process = Proccesser.ExecuteLwjgl(typeof(Launcher), Array.Empty<string>(), jvmArgs, Array.Empty<string>());

                    if (process != null)
                    {
                        while (!process.HasExited)
                        {
                            Thread.Sleep(0);
                        }

                        if (process.ExitCode != 0)
                            return;
                    }
                }
                LOG.Ln("*************************************");
                LOG.Ln("* STARTING " + VERSION.VERSION_STRING);
                LOG.Ln("*************************************");

                LSettings s = new LSettings();

                PATHS.Init(s.Mods.Get(), null, false);

                List<string> jars = PATHS.SCRIPT().ModClasspaths();
                string[] cps = jars.ToArray();

                string[] jvmArgs = s.JvmArguments.Get();

                if (s.Debug.Get() == 1)
                {
                    string[] aa = new string[jvmArgs.Length + 3];
                    jvmArgs.CopyTo(aa, 3);
                    aa[0] = "-Dorg.lwjgl.util.Debug=true";
                    aa[1] = "-Dorg.lwjgl.util.DebugAllocator=true";
                    aa[2] = "-Dorg.lwjgl.util.DebugStack=true";
                    jvmArgs = aa;
                }

                Proccesser.ExecuteLwjgl(typeof(MainProcess), jvmArgs, Array.Empty<string>(), cps);

                if (OS.Get() == OS.MAC)
                {
                    string filePath = Path.Combine(Directory.GetCurrentDirectory(), "hasRunOnceOnMac.txt");
                    if (File.Exists(filePath))
                        return;
                    else
                        File.Create(filePath).Close();
                }
            }
            catch (Exception e)
            {
                e.StackTrace?.Dump();

                try
                {
                    using (var writer = new StreamWriter("SEVERE_ERROR.txt", false, Encoding.UTF8))
                    {
                        string timeStamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm");
                        writer.WriteLine(timeStamp);
                        e.StackTrace?.Dump(writer);
                    }
                }
                catch (Exception ex)
                {
                    ex.StackTrace?.Dump();
                }
            }
        }

        private static void MacWarning()
        {
            try
            {
                if ((OS.Get() == OS.MAC && PATHS.IsSteam()))
                {
                    string filePath = Path.Combine(Directory.GetCurrentDirectory(), "hasRunOnceOnMac.txt");
                    if (File.Exists(filePath))
                        return;

                    string message = "Dear Mac User\n\n" +
                                      "You are playing Songs of Syx through Mac, this is good.\n" +
                                      "Unfortunately, the steam overlay breaks the visuals of the game.\n" +
                                      "The overlay can not be disabled by us developers, it has to be done manually by the user.\n\n" +
                                      "Steam > Right click Songs of Syx > Properties > General > Uncheck 'Enable the Steam Overlay while in-game'\n\n" +
                                      "If having trouble: www.reddit.com/r/songsofsyx/comments/umzi1t/deactivate_steam_overlay_to_run_game_on_mac\n\n" +
                                      "Please also report this as a bug so that steam will fix this issue.\n" +
                                      "https://help.steampowered.com/en/\n\n" +
                                      "The game also works fine to run like a normal app from the installation directory, being completely DRM free.\n\n" +
                                      "Apologies for the inconvenience, the alternative is to delist the game for mac, which would be a travesty.\n\n" +
                                      "CLOSE THIS MESSAGE TO CONTINUE TO THE GAME";

                    string escapedMessage = message.Replace("\"", "\\\"");

                    string script = $"tell application \"System Events\" to display dialog \"{escapedMessage}\" with title \"Steam Overlay Warning\" buttons {{\"OK\"}} default button \"OK\"";

                    ProcessStartInfo startInfo = new ProcessStartInfo("osascript", $"-e \"{script}\"");
                    startInfo.RedirectStandardOutput = true;
                    startInfo.UseShellExecute = false;
                    startInfo.CreateNoWindow = true;

                    Process process = new Process { StartInfo = startInfo };
                    process.Start();
                    process.WaitForExit(); // This will block until the user clicks OK
                }
            }
            catch (Exception e)
            {
                e.StackTrace?.Dump();
            }
        }
    }
}