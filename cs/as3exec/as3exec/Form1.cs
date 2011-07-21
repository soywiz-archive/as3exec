using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ExControls;
using System.IO;

namespace as3exec
{
	public partial class Form1 : Form
	{
		static Form1 ThisForm;
		string[] args;

		Timer Timer1;
		ExAxShockwaveFlash flash;
		bool ShouldExit;

		public Form1(string[] args)
		{
			ThisForm = this;
			this.args = args;
			this.StartPosition = FormStartPosition.Manual;
			this.SetDesktopBounds(0, 0, 2, 2);

			InitializeComponent();

			//var image = ExAxShockwaveFlash.StaticTakeScreenshot(@"D:\OurClientV2\src\UnitTests\bin\UnitTests.swf");

			//OnLoad

			//Load += new EventHandler(Form1_Load);

			ExecuteFlash();

			Timer1 = new Timer();
			Timer1.Interval = 20;
			Timer1.Tick += new EventHandler(Timer1_Tick);
			Timer1.Start();
			//Timer1.Container = this;
		}

		void Timer1_Tick(object sender, EventArgs e)
		{
			if (ShouldExit)
			{
				Exit();
				return;
			}
			if (flash != null)
			{
				// Console.WriteLine(flash.Playing);
				if (!flash.Playing)
				{
					//Exit();
					return;
				}
			}
		}

		void ExecuteFlash()
		{
			flash = new ExAxShockwaveFlash();

			flash.BeginInit();
			{
				this.Controls.Add(flash);
				flash.Size = Size;
				//flash.Visible = false;
			}
			flash.EndInit();
			//flash.PreferredSize = Size;

			//flash.SetVariable("Arguments", flash.SerializeObject(args));

			flash.RegisterCallback("getargs", as3_getargs);
			flash.RegisterCallback("writef", as3_writef);
			flash.RegisterCallback("writefln", as3_writefln);
			flash.RegisterCallback("exit", as3_exit);

			//Console.WriteLine(ExAxShockwaveFlash.ToJson(args));

			//args = new string[] { @"D:\OurClientV2\src\UnitTests\bin\UnitTests.swf" };

			if (args.Length < 1)
			{
				Console.WriteLine("as3exec <file.swf>");
				ShouldExit = true;
				return;
				//throw (new Exception("Exiting"));
				//Application.ExitThread();
				//Application.Exit();
				//return;
			}
			var MoviePath = Path.GetFullPath(args[0]);
			if (!File.Exists(MoviePath)) throw(new Exception(String.Format("File '{0}' doesn't exist", MoviePath)));

			flash.LoadMovie(0, MoviePath);
			try
			{
				flash.Size = Size;
				flash.Quality = 1;
				flash.GotoFrame(0);
				flash.Play();
				if (flash.TotalFrames == 0)
				{
					throw(new Exception(""));
				}
			}
			catch (Exception Exception)
			{
				throw (new Exception(String.Format("Can't load movie '{0}'", MoviePath), Exception));
			}

			Controls.Add(flash);

			//Console.WriteLine(flash.Playing);


			//flash.ScaleMode = (int)ScaleMode;

			//flash.LoadMovie(0, );
			//ExternalInterfaceCall

			//Console.WriteLine(flash);
		}

		void Exit()
		{
			//Console.WriteLine("Exiting...");
			//ThisForm.Close();
			Application.Exit();
		}

		dynamic as3_writefln(dynamic Params)
		{
			//Console.WriteLine(ExAxShockwaveFlash.ToJson(Params));
			Console.WriteLine("{0}", ExAxShockwaveFlash.ToOutputString(Params));
			Console.Out.Flush();
			return "";
		}

		dynamic as3_writef(dynamic Params)
		{
			//Console.WriteLine(ExAxShockwaveFlash.ToJson(Params));
			Console.Write("{0}", ExAxShockwaveFlash.ToOutputString(Params));
			Console.Out.Flush();
			return "";
		}

		dynamic as3_getargs(dynamic Params)
		{
			return args;
		}

		dynamic as3_exit(dynamic Params)
		{
			ShouldExit = true;
			//throw (new Exception("Exiting"));
			return null;
		}
	}
}
