using System;
using System.Drawing;
using System.Windows.Forms;

namespace Util.Error
{
    public class SwingMessage
    {
        [STAThread]
        public static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                args = new string[]
                {
                    "Dear Mac User",
                    "You are playing Songs of Syx through Mac, this is good." + Environment.NewLine
                    + "Unfortunately, the steam overlay breaks the visuals of the game." + Environment.NewLine
                    + "The overlay can not be disabled by us developers, it has to be done manually by the user." + Environment.NewLine
                    + "Steam > Right click Songs of Syx > Properties > General > Uncheck \"Enable the Steam Overlay while in-game\"" + Environment.NewLine
                    + "if having trouble: www.reddit.com/r/songsofsyx/comments/umzi1t/deactivate_steam_overlay_to_run_game_on_mac" + Environment.NewLine
                    + "Please also report this as a bug so that steam will fix this issue." + Environment.NewLine
                    + "https://help.steampowered.com/en/" + Environment.NewLine
                    + "The game also works fine to run like a normal app from the installation directory, being completely DRM free." + Environment.NewLine
                    + "Apologies for the inconvenience, the alternative is to delist the game for mac, which would be a travesty, since it should run fine natively on it. The goal is to apply enough pressure on steam so that they fix it." + Environment.NewLine
                };
            }

            string title = args[0];
            string mess = args[1];

            new SwingMessage(title, mess);
        }

        private SwingMessage(string title, string message)
        {
            Form frame = new Form();
            frame.Text = title;
            frame.FormClosing += (s, e) => Application.Exit();
            frame.FormBorderStyle = FormBorderStyle.FixedSingle;
            frame.MinimizeBox = false;
            frame.MaximizeBox = false;
            frame.Size = new Size(700, 500);
            frame.StartPosition = FormStartPosition.CenterScreen;

            Panel container = new Panel();
            container.Dock = DockStyle.Fill;
            container.AutoScroll = true;
            container.AutoSize = true;
            container.AutoSizeMode = AutoSizeMode.GrowAndShrink;

            setMessage(container, message);
            frame.Controls.Add(container);

            frame.Show();
        }

        private void setMessage(Panel container, string message)
        {
            TextBox text = new TextBox();
            text.Multiline = true;
            text.ScrollBars = ScrollBars.Vertical;
            text.WordWrap = true;
            text.Font = new Font(text.Font.FontFamily, 18f);
            text.TextAlign = HorizontalAlignment.Left;
            text.BorderStyle = BorderStyle.None;
            text.BackColor = container.BackColor;
            text.ForeColor = container.ForeColor;
            text.Text = message;
            text.Dock = DockStyle.Fill;
            text.Margin = new Padding(25, 25, 5, 25);

            container.Controls.Add(text);
        }
    }
}