using System;
using System.IO;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace snake2d
{
    public static class GlDebugger
    {
        private GlDebugger()
        {
        }

        public static IntPtr SetupDebugMessageCallback()
        {
            return SetupDebugMessageCallback(Console.OpenStandardOutput());
        }

        public static IntPtr SetupDebugMessageCallback(Stream stream)
        {
            var caps = GL.GetString(StringName.Extensions);

            if (caps.Contains("GL_ARB_debug_output"))
            {
                Console.WriteLine("[GL] Using ARB_debug_output for error logging.");
                GL.DebugMessageCallback((source, type, id, severity, length, message, userParam) =>
                {
                    if (severity != DebugSeverity.Notification)
                    {
                        stream.WriteLine("[LWJGL] ARB_debug_output message");
                        PrintDetail(stream, "ID", $"0x{id:X}");
                        PrintDetail(stream, "Source", GetSourceARB(source));
                        PrintDetail(stream, "Type", GetTypeARB(type));
                        PrintDetail(stream, "Severity", GetSeverityARB(severity));
                        PrintDetail(stream, "Message", message);
                    }
                }, IntPtr.Zero);
                return IntPtr.Zero;
            }

            if (caps.Contains("GL_AMD_debug_output"))
            {
                Console.WriteLine("[GL] Using AMD_debug_output for error logging.");
                GL.DebugMessageCallbackAMD((id, category, severity, length, message, userParam) =>
                {
                    if (severity != DebugSeverity.Notification)
                    {
                        stream.WriteLine("[LWJGL] AMD_debug_output message");
                        PrintDetail(stream, "ID", $"0x{id:X}");
                        PrintDetail(stream, "Category", GetCategoryAMD(category));
                        PrintDetail(stream, "Severity", GetSeverityAMD(severity));
                        PrintDetail(stream, "Message", message);
                    }
                }, IntPtr.Zero);
                return IntPtr.Zero;
            }

            Console.WriteLine("[GL] No debug output implementation is available.");
            return IntPtr.Zero;
        }

        private static void PrintDetail(Stream stream, string type, string message)
        {
            using (var writer = new StreamWriter(stream))
            {
                writer.WriteLine($"\t{type}: {message}");
            }
        }

        private static string GetSourceARB(DebugSource source)
        {
            switch (source)
            {
                case DebugSource.Api:
                    return "API";
                case DebugSource.WindowSystem:
                    return "WINDOW SYSTEM";
                case DebugSource.ShaderCompiler:
                    return "SHADER COMPILER";
                case DebugSource.ThirdParty:
                    return "THIRD PARTY";
                case DebugSource.Application:
                    return "APPLICATION";
                case DebugSource.Other:
                    return "OTHER";
                default:
                    return "UNKNOWN";
            }
        }

        private static string GetTypeARB(DebugType type)
        {
            switch (type)
            {
                case DebugType.Error:
                    return "ERROR";
                case DebugType.DeprecatedBehavior:
                    return "DEPRECATED BEHAVIOR";
                case DebugType.UndefinedBehavior:
                    return "UNDEFINED BEHAVIOR";
                case DebugType.Portability:
                    return "PORTABILITY";
                case DebugType.Performance:
                    return "PERFORMANCE";
                case DebugType.Other:
                    return "OTHER";
                default:
                    return "UNKNOWN";
            }
        }

        private static string GetSeverityARB(DebugSeverity severity)
        {
            switch (severity)
            {
                case DebugSeverity.High:
                    return "HIGH";
                case DebugSeverity.Medium:
                    return "MEDIUM";
                case DebugSeverity.Low:
                    return "LOW";
                default:
                    return "UNKNOWN";
            }
        }

        private static string GetCategoryAMD(int category)
        {
            switch (category)
            {
                case (int)DebugCategory.ApiError:
                    return "API ERROR";
                case (int)DebugCategory.WindowSystem:
                    return "WINDOW SYSTEM";
                case (int)DebugCategory.Deprecation:
                    return "DEPRECATION";
                case (int)DebugCategory.UndefinedBehavior:
                    return "UNDEFINED BEHAVIOR";
                case (int)DebugCategory.Performance:
                    return "PERFORMANCE";
                case (int)DebugCategory.ShaderCompiler:
                    return "SHADER COMPILER";
                case (int)DebugCategory.Application:
                    return "APPLICATION";
                case (int)DebugCategory.Other:
                    return "OTHER";
                default:
                    return "UNKNOWN";
            }
        }

        private static string GetSeverityAMD(int severity)
        {
            switch (severity)
            {
                case (int)DebugSeverity.High:
                    return "HIGH";
                case (int)DebugSeverity.Medium:
                    return "MEDIUM";
                case (int)DebugSeverity.Low:
                    return "LOW";
                default:
                    return "UNKNOWN";
            }
        }
    }
}