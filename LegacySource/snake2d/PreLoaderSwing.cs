using System;
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace Snake2D
{
    public class PreLoaderSwing
    {
        public static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                args = new string[] {
                    "Songs of poop",
                    "C:\\Users\\mail__000\\Documents\\syx13\\Syx\\res\\base\\texture\\PreLoader.png",
                    "C:\\Users\\mail__000\\Documents\\syx13\\Syx\\res\\base\\texture\\Icon.png"
                };
            }
            new PreLoaderSwing(args[0], args[1], args[2]);
        }

        private Form frame;

        private PreLoaderSwing(string name, string path, string iconPath)
        {
            try
            {
                frame = new Form();
                frame.Text = name;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            frame.StartPosition = FormStartPosition.CenterScreen;
            frame.FormBorderStyle = FormBorderStyle.None;
            frame.TopMost = true;
            frame.BackColor = System.Drawing.Color.Black;

            string preloader = path;

            if (!File.Exists(preloader))
                throw new Exception("Unable to find file: " + preloader);
            frame.BackgroundImage = System.Drawing.Image.FromFile(preloader);
            frame.BackgroundImageLayout = ImageLayout.Stretch;

            //Label version = new Label();
            //version.Text = name;
            //version.Size = new System.Drawing.Size(frame.ClientSize.Width, 30);
            //version.BackColor = System.Drawing.Color.Transparent;
            //version.Location = new System.Drawing.Point(0, 0);
            //frame.Controls.Add(version);

            frame.Load += (sender, e) =>
            {
                frame.Width = frame.BackgroundImage.Width;
                frame.Height = frame.BackgroundImage.Height;
                frame.Left = frame.Left - (frame.Width / 2);
                frame.Top = frame.Top - (frame.Height / 2);
            };

            frame.Icon = new System.Drawing.Icon(iconPath);
            frame.ShowInTaskbar = false;
            frame.Show();

            for (int i = 0; i < 5000; i++)
            {
                try
                {
                    if (Console.In.Peek() > -1 && Console.In.Read() != -1)
                        break;
                    Thread.Sleep(1);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
            Dispose();
        }

        private void Dispose()
        {
            frame.Visible = false;
            frame.Dispose();
        }
    }
}