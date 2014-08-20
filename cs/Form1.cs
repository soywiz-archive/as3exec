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

		private string flashCall(string Name, object[] Args)
		{
			return flash.CallFunction(Serializer.SerializeInvoke(Name, Args));
		}

		private int lastWatcherId = 0;
		private Dictionary<int, FileSystemWatcher> watchers = new Dictionary<int, FileSystemWatcher>();

		dynamic as3_fs_watch(dynamic Params)
		{
			var Path = Params[0];
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

		dynamic as3_fs_unwatch(dynamic Params)
		{
			var watcherId = (int)Params[0];
			if (watchers.ContainsKey(watcherId))
			{
				var Watcher = watchers[watcherId];
				Watcher.EnableRaisingEvents = false;
				Watcher.Dispose();
				watchers.Remove(watcherId);
			}
			return null;
		}

		dynamic as3_writefln(dynamic Params)
		{
			//Console.WriteLine(ExAxShockwaveFlash.ToJson(Params));
			Console.WriteLine("{0}", ExAxShockwaveFlash.ToOutputString(Params));
			Console.Out.Flush();
			return "";
		}

		dynamic as3_fs_mkdir(dynamic Params)
		{
			try
			{
				Directory.CreateDirectory(Params[0]);
			}
			catch (Exception)
			{

			}
			return null;
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

		dynamic as3_fs_exists(dynamic Params)
		{
			String FileName = Params[0];
			return File.Exists(FileName) ? 1 : 0;
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

class Serializer
{
	static public string SerializeInvoke(String FunctionName, Object[] Arguments)
	{
		return "<invoke name=\"" + FunctionName + "\" returntype=\"xml\"><arguments>" + String.Join("", Arguments.Select(Item => Serialize(Item))) + "</arguments></invoke>";
	}

	static public string Serialize(Object Object) {
		if (Object == null) return "<null />";
		if (Object is bool) return ((bool)Object) ? "<true />" : "<false />";
		if (Object is string) return "<string>" + Object + "</string>";
		if (Object is sbyte || Object is byte || Object is short || Object is ushort || Object is int || Object is uint || Object is long || Object is ulong || Object is float || Object is double) return "<number>" + Object + "</number>";
		if (Object.GetType().IsArray)
		{
			var ArrayObject = (Array)Object;
			return "<array>" + Enumerable.Range(0, ArrayObject.Length).Select(Index => "<property id=\"" + Index + "\">" + ArrayObject.GetValue(Index) + "</property>") + "</array>";
		}
		throw(new Exception("Can't handle '" + Object + "'"));
	}
}


/*
 * http://help.adobe.com/en_US/ActionScript/3.0_ProgrammingAS3/WS5b3ccc516d4fbf351e63e3d118a9b90204-7caf.html
The external API’s XML format

Communication between ActionScript and an application hosting the Shockwave Flash ActiveX control uses a specific XML format to encode function calls and values.
 * There are two parts to the XML format used by the external API. One format is used to represent function calls. Another format is used to represent individual values;
 * this format is used for parameters in functions as well as function return values. The XML format for function calls is used for calls to and from ActionScript.
 * For a function call from ActionScript, Flash Player passes the XML to the container; for a call from the container, Flash Player expects the container application
 * to pass it an XML string in this format. The following XML fragment shows an example XML-formatted function call:

<invoke name="functionName" returntype="xml"> 
    <arguments> 
        ... (individual argument values) 
    </arguments> 
</invoke>
The root node is the invoke node. It has two attributes: name indicates the name of the function to call, and returntype is always xml. If the function call includes parameters,
 * the invoke node has a child arguments node, whose child nodes will be the parameter values formatted using the individual value format explained next.

Individual values, including function parameters and function return values, use a formatting scheme that includes data type information in addition to the actual values.
 * The following table lists ActionScript classes and the XML format used to encode values of that data type:

ActionScript class/value

C# class/value

Format

Comments

null

null

<null/>

 
Boolean true

bool true

<true/>

 
Boolean false

bool false

<false/>

 
String

string

<string>string value</string>

 
Number, int, uint

single, double, int, uint

<number>27.5</number> 
<number>-12</number>
 
Array (elements can be mixed types)

A collection that allows mixed-type elements, such as ArrayList or object[]

<array> 
    <property id="0"> 
        <number>27.5</number> 
    </property> 
    <property id="1"> 
        <string>Hello there!</string> 
    </property> 
    ... 
</array>
The property node defines individual elements, and the id attribute is the numeric, zero-based index.

Object

A dictionary with string keys and object values, such as a HashTable with string keys

<object> 
    <property id="name"> 
        <string>John Doe</string> 
    </property> 
    <property id="age"> 
        <string>33</string> 
    </property> 
    ... 
</object>
The property node defines individual properties, and the id attribute is the property name (a string).

Other built-in or custom classes

 	
<null/> or  
<object></object>
ActionScript encodes other objects as null or as an empty object. In either case any property values are lost.

Note: By way of example, this table shows equivalent C# classes in addition to ActionScript classes; however, the external API can be used to communicate with any programming language or run time that supports ActiveX controls, and is not limited to C# applications.
When you are building your own applications using the external API with an ActiveX container application, you’ll probably find it convenient to write a proxy that will perform the task of converting native function calls to the serialized XML format. For an example of a proxy class written in C#, see Inside the ExternalInterfaceProxy class.
*/