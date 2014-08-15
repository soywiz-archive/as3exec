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
using System.Runtime.InteropServices;
using System.Dynamic;

namespace as3exec
{
	public partial class Form1 : Form
	{
		static Form1 ThisForm;
		string[] args;

		Timer Timer1;
		ExAxShockwaveFlash flash;
		bool ShouldExit;
		int ExitCode = 0;

		public Form1(string[] args)
		{
			ThisForm = this;
			this.args = args;
			this.StartPosition = FormStartPosition.Manual;
			this.SetDesktopBounds(0, 0, 1, 1);

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

		private const int SW_SHOWNOACTIVATE = 4;
		private const int HWND_TOPMOST = -1;
		private const uint SWP_NOACTIVATE = 0x0010;

		[DllImport("user32.dll", EntryPoint = "SetWindowPos")]
		static extern bool SetWindowPos(
			 int hWnd,           // window handle
			 int hWndInsertAfter,    // placement-order handle
			 int X,          // horizontal position
			 int Y,          // vertical position
			 int cx,         // width
			 int cy,         // height
			 uint uFlags);       // window positioning flags

		[DllImport("user32.dll")]
		static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

		// http://stackoverflow.com/questions/156046/show-a-form-without-stealing-focus-in-c
		public void ShowInactiveTopmost()
		{
			StaticShowInactiveTopmost(this);
		}

		static void StaticShowInactiveTopmost(Form frm)
		{
			ShowWindow(frm.Handle, SW_SHOWNOACTIVATE);
			SetWindowPos(frm.Handle.ToInt32(), HWND_TOPMOST,
			frm.Left, frm.Top, frm.Width, frm.Height,
			SWP_NOACTIVATE);
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

		/// C:\Windows\SysWOW64\Macromed\Flash
		void ExecuteFlash()
		{
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

			var ocxNames = new string[] { "Flash11a.ocx", "Flash10u.ocx", "Flash.ocx" };

			String OcxPathBase = Directory.GetParent(Application.ExecutablePath).FullName;

			foreach (var ocxName in ocxNames)
			{
				String ocxPath = String.Format(@"{0}\{1}", OcxPathBase, ocxName);
				if (File.Exists(ocxPath))
				{
					flash = new ExAxShockwaveFlash(ocxPath);
					break;
				}
			}

			if (flash == null)
			{
				flash = new ExAxShockwaveFlash();
			}

			flash.BeginInit();
			{
				//flash.Size = Size;
				flash.Size = new Size(1, 1);
				//flash.Visible = false;
				this.Controls.Add(flash);
			}
			flash.EndInit();
			//flash.PreferredSize = Size;

			//flash.SetVariable("Arguments", flash.SerializeObject(args));

			flash.RegisterCallback("getargs" , as3_getargs);
			flash.RegisterCallback("writef"  , as3_writef);
			flash.RegisterCallback("writefln", as3_writefln);
			flash.RegisterCallback("fs_write", as3_fs_write);
			flash.RegisterCallback("fs_read", as3_fs_read);
			flash.RegisterCallback("fs_exists", as3_fs_exists);
			flash.RegisterCallback("fs_delete", as3_fs_delete);
			flash.RegisterCallback("fs_list", as3_fs_list);
			flash.RegisterCallback("fs_stat", as3_fs_stat);
			flash.RegisterCallback("exit"    , as3_exit);

			//Console.WriteLine(ExAxShockwaveFlash.ToJson(args));

			//args = new string[] { @"D:\OurClientV2\src\UnitTests\bin\UnitTests.swf" };

			var MoviePath = Path.GetFullPath(args[0]);
			if (!File.Exists(MoviePath)) throw(new Exception(String.Format("File '{0}' doesn't exist", MoviePath)));

			flash.Movie = MoviePath;
			//flash.LoadMovie(0, MoviePath);
			try
			{
				//flash.Size = Size;
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
			Environment.Exit(ExitCode);
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

		dynamic as3_fs_write(dynamic Params)
		{
			String FileName = Params[0];
			String BinaryDataAsHex = Params[1];

			File.WriteAllBytes(FileName, HexToBin(BinaryDataAsHex));
			return "";
		}

		dynamic as3_fs_read(dynamic Params)
		{
			String FileName = Params[0];
			return BinToHex(File.ReadAllBytes(FileName));
		}

		dynamic as3_fs_exists(dynamic Params)
		{
			String FileName = Params[0];
			return File.Exists(FileName);
		}

		dynamic as3_fs_delete(dynamic Params)
		{
			String FileName = Params[0];
			File.Delete(FileName);
			return true;
		}

		dynamic as3_fs_list(dynamic Params)
		{
			String Path = Params[0];
			return new DirectoryInfo(Path).GetFileSystemInfos().Select(Item => Item.Name).ToArray();
		}

		dynamic as3_fs_stat(dynamic Params)
		{
			String FileName = Params[0];
			var info = new FileInfo(FileName);
			var dynamic = new Dictionary<string, Object>();
			//dynamic["size"] = info.Length;
			return "{\"size\":" + info.Length + ",\"exists\":" + (info.Exists ? "true" : "false") + ",\"name\":\"" + info.Name + "\"}";
		}

		static private String BinToHex(byte[] Bin)
		{
			var Out = "";
			for (int n = 0; n < Bin.Length; n++)
			{
				Out += Bin[n].ToString("X").PadLeft(2, '0');
			}
			return Out;
		}

		static private byte[] HexToBin(string Hex)
		{
			var Out = new byte[Hex.Length / 2];
			for (int n = 0, m = 0; n < Hex.Length; n += 2, m++) {
				Out[m] = (byte)Convert.ToInt32(Hex.Substring(n, 2), 16);
			}
			return Out;
		}

		dynamic as3_getargs(dynamic Params)
		{
			return args;
		}

		dynamic as3_exit(dynamic _Params)
		{
			Object[] Params = _Params;
			ShouldExit = true;
			//throw (new Exception("Exiting"));

			//Console.WriteLine("as3_exit: " + Params[0]);

			ExitCode = Convert.ToInt32(Params[0]);
			//Environment.Exit();
			ShouldExit = true;
			return null;
		}
	}
}
