using System;
using System.Collections.Generic;
using System.Linq;
using OpenAL.Bindings;
using OpenAL.Core;
using OpenTK.Mathematics;

namespace Snake2D
{
    public static class SoundDevices
    {
        private static List<string> available;

        static SoundDevices()
        {
            available = new List<string>(ALC.GetStringList(IntPtr.Zero, ALC.AllDevicesSpecifier));
        }

        private static SoundDevices self;

        public static List<string> Get()
        {
            if (self == null)
                self = new SoundDevices();
            return available;
        }

        public static void Refresh()
        {
            self = new SoundDevices();
        }

        public void Check(string name)
        {
            Console.WriteLine(name);

            IntPtr device = ALC.OpenDevice(name);
            if (device == IntPtr.Zero)
            {
                throw new InvalidOperationException("Failed to open an OpenAL device.");
            }

            var deviceCaps = ALC.CreateCapabilities(device);

            IntPtr context = ALC.CreateContext(device, IntPtr.Zero);
            CheckALCError(device);

            bool useTLC = deviceCaps.ALC_EXT_thread_local_context && ALC.SetThreadContext(context);
            if (!useTLC)
            {
                if (!ALC.MakeContextCurrent(context))
                {
                    throw new InvalidOperationException();
                }
            }
            CheckALCError(device);

            AL.CreateCapabilities(deviceCaps);

            PrintALCInfo(device, deviceCaps);
            PrintALInfo();

            ALC.MakeContextCurrent(IntPtr.Zero);
            if (useTLC)
            {
                AL.CurrentThread = null;
            }
            else
            {
                AL.CurrentProcess = null;
            }

            ALC.DestroyContext(context);
            ALC.CloseDevice(device);

            Console.WriteLine();
        }

        private static void PrintALCInfo(IntPtr device, ALCDeviceCapabilities caps)
        {
            Console.WriteLine("Default capture device: " + ALC.GetString(IntPtr.Zero, ALC.CaptureDefaultDeviceSpecifier));

            Console.WriteLine("ALC device specifier: " + ALC.GetString(device, ALC.DeviceSpecifier));

            int majorVersion = ALC.GetInteger(device, ALC.MajorVersion);
            int minorVersion = ALC.GetInteger(device, ALC.MinorVersion);
            CheckALCError(device);

            Console.WriteLine("ALC version: " + majorVersion + "." + minorVersion);

            string[] extensions = ALC.GetString(device, ALC.Extensions).Split(' ');
            Console.WriteLine("ALC extensions: " + string.Join(", ", extensions));
            CheckALCError(device);
        }

        private static void PrintALInfo()
        {
            Console.WriteLine("OpenAL vendor string: " + AL.GetString(AL.Vendor));
            Console.WriteLine("OpenAL renderer string: " + AL.GetString(AL.Renderer));
            Console.WriteLine("OpenAL version string: " + AL.GetString(AL.Version));

            string[] extensions = AL.GetString(AL.Extensions).Split(' ');
            Console.WriteLine("AL extensions: " + string.Join(", ", extensions));
            CheckALError();
        }

        static void CheckALCError(IntPtr device)
        {
            int err = ALC.GetError(device);
            if (err != ALC.NoError)
            {
                throw new Exception(ALC.GetString(device, err));
            }
        }

        static void CheckALError()
        {
            int err = AL.GetError();
            if (err != AL.NoError)
            {
                throw new Exception(AL.GetString(err));
            }
        }

        public static void Main(string[] args)
        {
            new SoundDevices();
        }
    }
}