using System;
using System.IO;
using System.Windows.Forms;

namespace Util.Error
{
    public class ErrorMessage
    {
        [STAThread]
        public static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                args = new string[] {
                    "test thing",
                    "bugs@gugs.com",
                    "2",
                    "oh no!",
                    "C:\\Users\\Jake\\AppData\\Roaming\\songsofsyx\\logs\\error03-22-2021-15-15-02-157.txt",
                    "C:",
                    "Runtime...",
                };
            }

            string pgmname = args[0];
            string bugmail = args[1];
            int type = int.Parse(args[2]);
            string message = args[3];
            string dump = args[4];
            string path = args.Length > 5 ? args[5] : null;
            string key = args[6];

            Application.Run(new ErrorMessageForm(pgmname, bugmail, type, message, dump, path, key));
        }
    }

    public class ErrorMessageForm : Form
    {
        private string pgmname;
        private string bugMail;
        private int type;
        private string message;
        private string dump;
        private string path;
        private string key;

        public ErrorMessageForm(string pgmname, string bugMail, int type, string message, string dump, string path, string key)
        {
            this.pgmname = pgmname;
            this.bugMail = bugMail;
            this.type = type;
            this.message = message;
            this.dump = dump;
            this.path = path;
            this.key = key;

            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = pgmname + " Error Message";
            this.FormClosing += (s, e) => Application.Exit();

            this.Width = 700;
            this.Height = 100;
            this.MinimumSize = new System.Drawing.Size(700, 100);
            this.MaximumSize = new System.Drawing.Size(700, 700);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.AutoSize = false;
            this.AutoScroll = true;

            var container = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown,
                AutoScroll = true
            };

            if (type == 0) // dataError
            {
                SetHeader(container, System.Drawing.Color.FromArgb(230, 200, 128), "Failed loading assets!");
                SetFile(container, path);
                SetMessage(container, message);
            }
            else if (type == 1) // gameerror
            {
                SetHeader(container, System.Drawing.Color.FromArgb(230, 200, 128), "Game Notification");
                SetMessage(container, message);
                //SetDump(container, dump, bugMail, key);
            }
            else if (type == 2) // gameerror
            {
                SetHeader(container, System.Drawing.Color.FromArgb(230, 200, 128), "Output");
                if (File.Exists(path))
                    SetFile(container, path);
                SetMessage(container, message);

                //SetDump(container, dump, bugMail, key);
            }
            else
            {
                SetHeader(container, System.Drawing.Color.FromArgb(240, 20, 20), "Unexpected Problems!");
                SetMessage(container, message);
                SetDump(container, dump, bugMail, key);
            }

            this.Controls.Add(container);
        }

        private void SetHeader(FlowLayoutPanel container, System.Drawing.Color col, string m)
        {
            var header = new Label
            {
                Text = m,
                Font = new System.Drawing.Font(header.Font, 24f),
                ForeColor = col,
                AutoSize = true
            };

            container.Controls.Add(header);
        }

        private void SetFile(FlowLayoutPanel container, string filepath)
        {
            var file = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.LeftToRight,
                AutoScroll = true
            };

            string p;
            if (filepath == null)
                filepath = "null";

            Label label;
            if (filepath != null && File.Exists(filepath))
            {
                label = new Label { Text = "file corrupt:", Font = new System.Drawing.Font(label.Font, 16f) };
                p = filepath;
            }
            else
            {
                if (filepath.EndsWith(Path.DirectorySeparatorChar.ToString()))
                {
                    string pa = filepath.Substring(0, filepath.Length - 1);
                    int last = pa.LastIndexOf(Path.DirectorySeparatorChar);
                    string fol = pa.Substring(last, pa.Length - last);
                    label = new Label { Text = "directory missing: " + fol, Font = new System.Drawing.Font(label.Font, 16f) };
                    p = pa.Substring(0, last);
                }
                else
                {
                    string pa = filepath;
                    int last = pa.LastIndexOf(Path.DirectorySeparatorChar) + 1;
                    string fol = pa.Substring(last, pa.Length - last);
                    label = new Label { Text = "file missing: " + fol, Font = new System.Drawing.Font(label.Font, 16f) };
                    p = pa.Substring(0, last);
                }
            }

            label.AutoSize = true;
            file.Controls.Add(label);

            var pathLabel = new LinkLabel { Text = p, Font = new System.Drawing.Font(label.Font, 16f) };
            pathLabel.LinkClicked += (s, e) => FileManager.OpenDesktop(p);
            pathLabel.AutoSize = true;
            file.Controls.Add(pathLabel);

            container.Controls.Add(file);
        }

        private void SetMessage(FlowLayoutPanel container, string message)
        {
            var text = new TextBox
            {
                Text = message,
                Multiline = true,
                WordWrap = true,
                Font = new System.Drawing.Font(text.Font, 18f),
                AutoSize = true,
                ReadOnly = true,
                BorderStyle = BorderStyle.None
            };

            container.Controls.Add(text);
        }

        private void SetDump(FlowLayoutPanel container, string dumpfile, string mail, string key)
        {
            if (dumpfile == null || dumpfile.Equals("none"))
                return;

            var panel = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.TopDown,
                AutoScroll = true
            };

            var label = new Label { Text = "Please report this terrible incident!", Font = new System.Drawing.Font(label.Font, 20f), AutoSize = true };
            panel.Controls.Add(label);

            label = new Label { Text = "(By doing so you will share your computers specs with the developer, used for debugging)", Font = new System.Drawing.Font(label.Font, 12f), AutoSize = true };
            panel.Controls.Add(label);

            var f = new TextBox { Text = "please send file to: " + mail, ReadOnly = true, BorderStyle = BorderStyle.None, AutoSize = true };
            panel.Controls.Add(f);

            container.Controls.Add(panel);

            panel = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.LeftToRight,
                AutoScroll = true
            };

            label = new Label { Text = "Dump file: ", Font = new System.Drawing.Font(label.Font, 16f), AutoSize = true };
            panel.Controls.Add(label);

            var dumpLabel = new LinkLabel { Text = dumpfile, Font = new System.Drawing.Font(label.Font, 12f), AutoSize = true };
            dumpLabel.LinkClicked += (s, e) => FileManager.OpenDesktop(dumpfile);
            panel.Controls.Add(dumpLabel);

            container.Controls.Add(panel);

            var textArea = new TextBox
            {
                Text = "It just happened, man!",
                Multiline = true,
                WordWrap = true,
                Font = new System.Drawing.Font(textArea.Font, 18f),
                AutoSize = true,
                BorderStyle = BorderStyle.None
            };

            container.Controls.Add(textArea);

            var reportButton = new Button { Text = "REPORT", Font = new System.Drawing.Font(reportButton.Font, 24f), AutoSize = true };
            reportButton.Click += (s, e) => ReportBug(dumpfile, textArea.Text, key, mail);
            reportButton.BackColor = System.Drawing.Color.FromArgb(200, 200, 200);
            container.Controls.Add(reportButton);
        }

        private void ReportBug(string dumpfile, string message, string key, string mail)
        {
            try
            {
                if (new ErrorSender().Send(key, message, File.ReadAllText(dumpfile)))
                {
                    Application.Exit();
                }
                else
                {
                    message += "\n" + File.ReadAllText(dumpfile);
                    if (!FileManager.SendEmail(mail, message, "Bug"))
                    {
                        MessageBox.Show("ERROR..");
                    }
                    else
                    {
                        Application.Exit();
                    }
                }
            }
            catch
            {
                message += "\n" + File.ReadAllText(dumpfile);
                if (!FileManager.SendEmail(mail, message, "Bug"))
                {
                    MessageBox.Show("ERROR..");
                }
                else
                {
                    Application.Exit();
                }
            }
        }
    }

    public static class FileManager
    {
        public static void OpenDesktop(string path)
        {
            System.Diagnostics.Process.Start("explorer.exe", path);
        }

        public static bool SendEmail(string mail, string message, string subject)
        {
            // Implement your email sending logic here
            return true;
        }
    }

    public class ErrorSender
    {
        public bool Send(string key, string message, string dump)
        {
            // Implement your error sending logic here
            return true;
        }
    }
}