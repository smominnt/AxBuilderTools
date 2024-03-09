using System;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using ksNet;

namespace SimpleKn5Converter
{
	internal static class Program
	{
		private static void Main(string[] args)
		{
			Program.SetUnhandledExceptionHandler();
			string destinationFile = null;
			bool convertModeTrack = false;
			string inputFile = null;
			string conversionType = (args.Length != 0) ? args[0] : null;

			if (args.Length != 3 || (conversionType != "kn5" && conversionType != "kn5track"))
            {
				MessageBox.Show(
					"Converting cars: kn5 output.kn5 path/to/your/input.fbx \n" +
					"Converting tracks: kn5track output.kn5 path/to/your/input.fbx \n",
					"Command line arguments to use",
					MessageBoxButtons.OK, MessageBoxIcon.Hand);
				return;
			}

			if (conversionType == "kn5")
			{
				destinationFile = args[1];
				inputFile = args[2];
			}
			if (conversionType == "kn5track")
			{
				destinationFile = args[1];
				inputFile = args[2];
				convertModeTrack = true;
			}

			Program.ExecuteConversion(inputFile, destinationFile, convertModeTrack);
		}

        // Form must be created as the KsNet call to ksGraphics.loadFBX and ksGraphics.saveKN5 will not work without it
        private static void ExecuteConversion(string inputFile, string destinationFile, bool convertModeTrack)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Form form = new Form();
			form.Size = new Size(400, 200);
            Panel panel = new Panel();
            panel.Dock = DockStyle.Fill;
            panel.Location = new Point(440, 28);
            panel.Margin = new Padding(4);
            panel.Name = "panScene";
            panel.Size = new Size(1131, 432);
            panel.TabIndex = 5;
            panel.BackColor = SystemColors.ButtonFace;

            form.Controls.Add(panel);
            form.Load += delegate (object sender, EventArgs e)
            {
                IntPtr handle = panel.Handle;
            };
            form.Shown += delegate (object sender, EventArgs e)
            {
                Program.LoadFbxAndSaveKn5(inputFile, destinationFile, convertModeTrack, panel);
            };

            form.ShowInTaskbar = false;

            Application.Run(form);
        }

        private static async void LoadFbxAndSaveKn5(string inputFile, string destinationFile, bool convertModeTrack, Panel panel)
		{
            Label loadingLabel = new Label();
            loadingLabel.Font = new Font("Arial", 16, FontStyle.Bold);
            loadingLabel.TextAlign = ContentAlignment.MiddleCenter;
            loadingLabel.Dock = DockStyle.Fill;
            loadingLabel.Text = $"Building KN5 {(convertModeTrack ? "track" : "car")} file...";
            panel.Controls.Add(loadingLabel);
            
            ksGraphics ksGraphics = new ksGraphics(panel.Handle);
            if (!File.Exists(inputFile))
			{
				MessageBox.Show("The input file was not found: " + inputFile, "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                Environment.Exit(1);
			}

            await Task.Factory.StartNew(delegate ()
			{
                ksGraphics.loadFBX(inputFile);
            });

            ksGraphics.saveKN5(destinationFile, convertModeTrack, -1, true);
            Environment.Exit(1);
		}


		public static void SetUnhandledExceptionHandler()
		{
			AppDomain.CurrentDomain.UnhandledException += Program.UnhandledExceptionHandler;
		}


		private static void UnhandledExceptionHandler(object sender, UnhandledExceptionEventArgs args)
		{
			Exception ex = args.ExceptionObject as Exception;
            string text = "Unhandled exception:\n\n" + ((ex?.ToString()) ?? "null");
			try
			{
				MessageBox.Show(ex.Message, "Oops!", MessageBoxButtons.OK, MessageBoxIcon.Hand);
			}
			catch (Exception)
			{
			}
			try
			{
				File.WriteAllText(string.Concat(new object[]
				{
					AppDomain.CurrentDomain.BaseDirectory,
					"/crash_",
					DateTime.Now.Ticks,
					".txt"
				}), text);
			}
			catch (Exception)
			{
			}
			Environment.Exit(1);
		}
	}
}
