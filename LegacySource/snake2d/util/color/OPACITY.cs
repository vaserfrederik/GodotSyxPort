using System;

namespace Snake2D.Util.Color
{
    public interface IOpacity
    {
        byte Get();

        default void Bind()
        {
            CORE.Renderer().SetOpacity(this);
        }

        public static void Unbind()
        {
            CORE.Renderer().SetNormalOpacity();
        }
    }

    internal class OpacityImp : IOpacity
    {
        private readonly byte _opacity;

        public OpacityImp(byte opacity)
        {
            _opacity = opacity;
        }

        public byte Get() => _opacity;
    }

    internal class OpaPuls : IOpacity
    {
        private readonly int _start;
        private readonly int _end;

        public OpaPuls(int start, int end)
        {
            _start = start;
            _end = end;
        }

        public byte Get()
        {
            // Placeholder implementation for pulsing opacity
            // Replace with actual logic
            return (byte)(_start + (_end - _start) / 2);
        }
    }

    public static class CORE
    {
        public static IRenderer Renderer()
        {
            // Placeholder implementation for Renderer
            // Replace with actual logic
            return new Renderer();
        }
    }

    public interface IRenderer
    {
        void SetOpacity(IOpacity opacity);
        void SetNormalOpacity();
    }

    public class Renderer : IRenderer
    {
        public void SetOpacity(IOpacity opacity)
        {
            // Placeholder implementation for SetOpacity
            // Replace with actual logic
        }

        public void SetNormalOpacity()
        {
            // Placeholder implementation for SetNormalOpacity
            // Replace with actual logic
        }
    }
}