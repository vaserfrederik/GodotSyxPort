using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using launcher.GUI;
using snake2d;
using snake2d.util.gui;
using snake2d.util.gui.clickable;
using snake2d.util.misc;
using util.text;

namespace launcher
{
    class ScreenMain : GuiSection
    {
        public ScreenMain(Launcher l, ScreenLang lang)
        {
            add(l.res.smallPanel[0], 0, 0);
            for (int i = 0; i <= 5; i++)
                addDown(0, l.res.smallPanel[1]);
            addDown(0, l.res.smallPanel[2]);

            // BUTTONS

            GuiSection buttons = new GuiSection();

            CLICKABLE b;

            int ww = 150;
            D.gInit(this);

            b = new BText(l.res, D.g("Launch"), ww);
            b.clickActionSet(() =>
            {
                l.setMods();
            });
            buttons.add(b);

            b = new BText(l.res, D.g("Settings"), ww).clickActionSet(() =>
            {
                l.setSetts();
            });
            buttons.addRightC(2, b);

            b = new BText(l.res, D.g("Info"), ww).clickActionSet(() =>
            {
                l.setInfo();
            });
            buttons.addRightC(2, b);

            b = new BText(l.res, D.g("Exit"), ww).clickActionSet(() =>
            {
                Launcher.startGame = false;
                CORE.annihilate();
            });
            buttons.addRightC(2, b);

            b = new BSpriteBig(l.res.social[3]).clickActionSet(() =>
            {
                try
                {
                    openBrowser("https://songsofsyx.mod.io/");
                }
                catch (IOException e)
                {
                    e.printStackTrace();
                }
            });
            buttons.addRightC(2, b);

            b = new BSpriteBig(l.res.social[2]).clickActionSet(() =>
            {
                try
                {
                    openBrowser("https://discord.gg/eacfCuE");
                }
                catch (IOException e)
                {
                    e.printStackTrace();
                }
            });
            buttons.addRightC(2, b);

            b = new BSpriteBig(l.res.social[1]).clickActionSet(() =>
            {
                try
                {
                    openBrowser("https://twitter.com/songsofsyx");
                }
                catch (IOException e)
                {
                    e.printStackTrace();
                }
            });
            buttons.addRightC(2, b);

            b = new BSpriteBig(l.res.social[0]).clickActionSet(() =>
            {
                try
                {
                    openBrowser("https://www.youtube.com/channel/UCuWzoe8gnqI1brHv-k3oVyA");
                }
                catch (IOException e)
                {
                    e.printStackTrace();
                }
            });
            buttons.addRightC(2, b);

            body().centerY(0, Sett.HEIGHT);
            body().centerX(0, Sett.WIDTH);

            add(l.res.logo, (Sett.WIDTH - l.res.logo.width()) / 2, body().y1() + 60);

            buttons.body().centerX(0, Sett.WIDTH);
            buttons.body().moveY1(getLastY2() + 10);

            add(buttons);

            add(lang.butt(), 820, 160);
        }

        public static void openBrowser(string url) throws IOException
        {
            string os = System.getProperty("os.name").ToLower();
            if (os.IndexOf("win") >= 0)
            {
                Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
            }
            else if (os.IndexOf("mac") >= 0)
            {
                Process.Start("open", url);
            }
            else if (os.IndexOf("nix") >= 0 || os.IndexOf("nux") >= 0)
            {
                string[] browsers = { "epiphany", "firefox", "mozilla", "konqueror", "netscape", "opera", "links", "lynx" };
                foreach (string browser in browsers)
                {
                    try
                    {
                        Process.Start(new ProcessStartInfo(browser, url) { UseShellExecute = true });
                        return;
                    }
                    catch (Exception) { }
                }
                throw new IOException("No browser found");
            }
        }
    }
}