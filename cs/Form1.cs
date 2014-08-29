using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using ExControls;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;

namespace as3exec
{
	public partial class Form1 : Form
	{
		static Form1 ThisForm;
		string[] args;

		System.Windows.Forms.Timer Timer1;
		ExAxShockwaveFlash flash;
		bool ShouldExit;
		int ExitCode = 0;

		public Form1(string[] args)
		{
			ThisForm = this;
			this.args = args;
			this.StartPosition = FormStartPosition.Manual;
			this.MinimumSize = new Size(1, 1);
			this.MaximumSize = new Size(1, 1);
			this.SetDesktopBounds(0, 0, 1, 1);

			InitializeComponent();

			this.Size = new Size(1, 1);
			this.ClientSize = new System.Drawing.Size(1, 1);

			//var image = ExAxShockwaveFlash.StaticTakeScreenshot(@"D:\OurClientV2\src\UnitTests\bin\UnitTests.swf");

			//OnLoad

			//Load += new EventHandler(Form1_Load);

			ExecuteFlash();

			Timer1 = new System.Windows.Forms.Timer();
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
			flash.RegisterCallback("fs_mkdir", as3_fs_mkdir);
			flash.RegisterCallback("fs_watch", as3_fs_watch);
			flash.RegisterCallback("fs_unwatch", as3_fs_unwatch);
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

		private Object flashCall(string Name, object[] Args)
		{
            return flash.ExternalInterfaceCall(Name, Args);
		}

		private int lastWatcherId = 0;
		private Dictionary<int, FileSystemWatcher> watchers = new Dictionary<int, FileSystemWatcher>();

		Object as3_fs_watch(Array Params)
		{
			var Path = (String)Params.GetValue(0);
			var watcher = new FileSystemWatcher(Path);
			int watcherId = lastWatcherId++;
			watchers[watcherId] = watcher;
			//FileSystemWatcher.Created
			watcher.Changed += (sender, e) =>
			{
				flashCall("FsWatchNotify", new string[] { watcherId.ToString(), "Changed", e.FullPath, "" });
			};
			watcher.Created += (sender, e) =>
			{
				flashCall("FsWatchNotify", new string[] { watcherId.ToString(), "Created", e.FullPath, "" });
			};
			watcher.Deleted += (sender, e) =>
			{
				flashCall("FsWatchNotify", new string[] { watcherId.ToString(), "Deleted", e.FullPath, "" });
			};
			watcher.Renamed += (sender, e) =>
			{
				flashCall("FsWatchNotify", new string[] { watcherId.ToString(), "Renamed", e.FullPath, e.OldFullPath });
			};
			watcher.EnableRaisingEvents = true;

			return watcherId;
		}

		Object as3_fs_unwatch(Array Params)
		{
            var watcherId = (int)Params.GetValue(0);
			if (watchers.ContainsKey(watcherId))
			{
				var Watcher = watchers[watcherId];
				Watcher.EnableRaisingEvents = false;
				Watcher.Dispose();
				watchers.Remove(watcherId);
			}
			return null;
		}

		Object as3_writefln(Array Params)
		{
			//Console.WriteLine(ExAxShockwaveFlash.ToJson(Params));
			Console.WriteLine("{0}", ExAxShockwaveFlash.ToOutputString(Params));
			Console.Out.Flush();
			return "";
		}

		Object as3_fs_mkdir(Array Params)
		{
			try
			{
                Directory.CreateDirectory((String)Params.GetValue(0));
			}
			catch (Exception)
			{

			}
			return null;
		}

		Object as3_writef(Array Params)
		{
			//Console.WriteLine(ExAxShockwaveFlash.ToJson(Params));
			Console.Write("{0}", ExAxShockwaveFlash.ToOutputString(Params));
			Console.Out.Flush();
			return "";
		}

		Object as3_fs_write(Array Params)
		{
            String FileName = (String)Params.GetValue(0);
            String BinaryDataAsHex = (String)Params.GetValue(1);

			File.WriteAllBytes(FileName, HexToBin(BinaryDataAsHex));
			return "";
		}

		Object as3_fs_read(Array Params)
		{
            String FileName = (String)Params.GetValue(0);
			for (int n = 0; n < 10; n++)
			{
				try
				{
					return BinToHex(File.ReadAllBytes(FileName));
				}
				catch (IOException)
				{
					Thread.Sleep(10 * (n + 1));
				}
			}
			return "";
		}

		Object as3_fs_exists(Array Params)
		{
            String FileName = (String)Params.GetValue(0);
			return File.Exists(FileName) ? 1 : 0;
		}

		Object as3_fs_delete(Array Params)
		{
            String FileName = (String)Params.GetValue(0);
			File.Delete(FileName);
			return true;
		}

		Object as3_fs_list(Array Params)
		{
            String Path = (String)Params.GetValue(0);
            var Names = new List<string>();
            foreach (var Test in new DirectoryInfo(Path).GetFileSystemInfos()) {
                Names.Add(Test.Name);
            }
			return Names.ToArray();
		}

        Object as3_fs_stat(Array Params)
		{
            String FileName = (String)Params.GetValue(0);
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

		Object as3_getargs(Array Params)
		{
			return args;
		}

		Object as3_exit(Array Params)
		{
			ShouldExit = true;
			//throw (new Exception("Exiting"));

			//Console.WriteLine("as3_exit: " + Params[0]);

			ExitCode = Convert.ToInt32(Params.GetValue(0));
			//Environment.Exit();
			ShouldExit = true;
			return null;
		}
	}
}
