using Steamworks;

namespace Integrations
{
    internal class SteamScreenshot
    {
        private SteamScreenshots ss;

        private SteamScreenshotsCallback ssCallback = new SteamScreenshotsCallback
        {
            OnScreenshotReady = (SteamScreenshotHandle local, EResult result) =>
            {
                INTEGRATIONS.Log("Screenshot saved on disk!");
                INTEGRATIONS.Log("Result: " + result);
            },

            OnScreenshotRequested = () =>
            {
                INTEGRATIONS.Log("Steam wants to take a screenshot!");
            }
        };

        public SteamScreenshot()
        {
            Init();
        }

        private void Init()
        {
            INTEGRATIONS.Log("Register SteamScreenshot ...");
            ss = new SteamScreenshots(ssCallback);
            // Turning off Steam's default screenshot functionality
            // ss.HookScreenshots(true);
        }

        public void AddScreenshot(string path, string thumbnailPath, int width, int height)
        {
            ss.AddScreenshotToLibrary(path, thumbnailPath, width, height);
        }

        public void Dispose()
        {
            ss.Dispose();
        }
    }
}