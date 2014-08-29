using System;
using System.Collections.Generic;
using System.Text;
using AxShockwaveFlashObjects;
using System.Windows.Forms;
using System.Xml;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Threading;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Collections;
using System.Reflection;

delegate TR Func<T, TR>(T Param);

namespace ExControls
{
	// http://bojordan.com/log/?p=269
	// http://msfast.googlecode.com/svn/trunk/src/MySpace.MSFast.GUI.Engine/Panels/GraphView/Controls/FlashPlayer.cs
	class ExAxShockwaveFlash : AxShockwaveFlash
	{
		private const int WM_LBUTTONDOWN = 0x0201;
		private const int WM_RBUTTONDOWN = 0x0204;

		public enum FlashScaleMode
		{
			/// <summary>
			/// (Default) makes the entire Flash content visible in the specified area without distortion while maintaining the original aspect ratio of the. Borders can appear on two sides of the application.
			/// </summary>
			showAll = 0,
			/// <summary>
			/// scales the Flash content to fill the specified area, without distortion but possibly with some cropping, while maintaining the original aspect ratio of the application.
			/// </summary>
			noBorder = 1,
			/// <summary>
			/// makes the entire Flash content visible in the specified area without trying to preserve the original aspect ratio. Distortion can occur.
			/// </summary>
			exactFit = 2,
			/// <summary>
			/// makes the size of the Flash content fixed, so that it remains unchanged even as the size of the player window changes. Cropping may occur if the player window is smaller than the Flash content.
			/// </summary>
			noScale = 3,
		}

		//DllGetClassObject fuction pointer signature
		private delegate int DllGetClassObject(ref Guid ClassId, ref Guid InterfaceId, [Out, MarshalAs(UnmanagedType.Interface)] out object ppunk);

		//Some win32 methods to load\unload dlls and get a function pointer
		private class Win32NativeMethods
		{
			[DllImport("kernel32.dll")]
			public static extern IntPtr LoadLibrary(string lpFileName);

			[DllImport("kernel32.dll", CharSet = CharSet.Ansi)]
			public static extern IntPtr GetProcAddress(IntPtr hModule, string lpProcName);

			[DllImport("kernel32.dll")]
			public static extern bool FreeLibrary(IntPtr hModule);
		}

		/*
		private static IUnknown GetClassFactoryFromDll(string dllName, string filterPersistClass)
		{
			//Load the dll
			IntPtr dllHandle = Win32NativeMethods.LoadLibrary(dllName);
			if (dllHandle == IntPtr.Zero)
				return null;

			//Keep a reference to the dll until the process\AppDomain dies
			_dllList.AddDllHandle(dllHandle);

			//Get a pointer to the DllGetClassObject function
			IntPtr dllGetClassObjectPtr = Win32NativeMethods.GetProcAddress(dllHandle, "DllGetClassObject");
			if (dllGetClassObjectPtr == IntPtr.Zero)
				return null;

			//Convert the function pointer to a .net delegate
			DllGetClassObject dllGetClassObject = (DllGetClassObject)Marshal.GetDelegateForFunctionPointer(dllGetClassObjectPtr, typeof(DllGetClassObject));

			//Call the DllGetClassObject to retreive a class factory for out Filter class
			Guid filterPersistGUID = new Guid(filterPersistClass);
			Guid IClassFactoryGUID = new Guid("00000001-0000-0000-C000-000000000046"); //IClassFactory class id
			Object unk;
			if (dllGetClassObject(ref filterPersistGUID, ref IClassFactoryGUID, out unk) != 0)
				return null;

			//Yippie! cast the returned object to IClassFactory
			return (unk as IClassFactory);
		}
		*/

		/// <summary>
		/// Holds a list of dll handles and unloads the dlls 
		/// in the destructor
		/// </summary>
		private class DllList {
			private List<IntPtr> _dllList=new List<IntPtr>();
			public void AddDllHandle(IntPtr dllHandle) {
				lock (_dllList) {
					_dllList.Add(dllHandle);
				}
			}

			~DllList() {
				foreach (IntPtr dllHandle in _dllList) {
					try {
						Win32NativeMethods.FreeLibrary(dllHandle);
					} catch {
					};
				}
			}
		}

		static DllList _dllList=new DllList();

		[ComVisible(false)]
		[ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("00000001-0000-0000-C000-000000000046")]
		internal interface IClassFactory
		{
			void CreateInstance([MarshalAs(UnmanagedType.Interface)] object pUnkOuter, ref Guid refiid, [MarshalAs(UnmanagedType.Interface)] out object ppunk);
			void LockServer(bool fLock);
		}

		internal static IClassFactory GetClassFactory(string dllName, string filterPersistClass) {
			//Load the class factory from the dll
			IClassFactory classFactory=GetClassFactoryFromDll(dllName, filterPersistClass);
			return classFactory;
		}

		private static IClassFactory GetClassFactoryFromDll(string dllName, string filterPersistClass) {
			//Load the dll
			IntPtr dllHandle=Win32NativeMethods.LoadLibrary(dllName);
			if (dllHandle==IntPtr.Zero)
			return null;

			//Keep a reference to the dll until the process\AppDomain dies
			//_dllList.AddDllHandle(dllHandle);

			//Get a pointer to the DllGetClassObject function
			IntPtr dllGetClassObjectPtr=Win32NativeMethods.GetProcAddress(dllHandle, "DllGetClassObject");
			if (dllGetClassObjectPtr==IntPtr.Zero)
			return null;

			//Convert the function pointer to a .net delegate
			DllGetClassObject dllGetClassObject=(DllGetClassObject)Marshal.GetDelegateForFunctionPointer(dllGetClassObjectPtr, typeof(DllGetClassObject));

			//Call the DllGetClassObject to retreive a class factory for out Filter class
			Guid filterPersistGUID=new Guid(filterPersistClass);
			Guid IClassFactoryGUID=new Guid("00000001-0000-0000-C000-000000000046"); //IClassFactory class id
			Object unk;
			if (dllGetClassObject(ref filterPersistGUID, ref IClassFactoryGUID, out unk) != 0)
			{
				return null;
			}

			//Yippie! cast the returned object to IClassFactory
			return (unk as IClassFactory);
		}

		protected override object CreateInstanceCore(Guid clsid)
		{
			// Temp
			//this.ocxPath = "Flash10u.ocx";

			if (this.ocxPath != null)
			{
				IntPtr dllOcxPtr = Win32NativeMethods.LoadLibrary(this.ocxPath);
				if (dllOcxPtr.ToInt64() == 0) throw (new Exception(String.Format("Can't find '" + dllOcxPtr + "'")));
				IntPtr dllGetClassObjectPtr = Win32NativeMethods.GetProcAddress(dllOcxPtr, "DllGetClassObject");

				DllGetClassObject dllGetClassObject = (DllGetClassObject)Marshal.GetDelegateForFunctionPointer(dllGetClassObjectPtr, typeof(DllGetClassObject));
				Object unk;

				// UnsafeNativeMethods.CoCreateInstance(ref clsid, null, 1, ref NativeMethods.ActiveX.IID_IUnknown);

				Guid IID_IUnknown = new Guid("{00000000-0000-0000-C000-000000000046}");
				var ClassFactory = GetClassFactoryFromDll(this.ocxPath, clsid.ToString());
				ClassFactory.CreateInstance(null, ref IID_IUnknown, out unk);

				//object obj = UnsafeNativeMethods.CoCreateInstance(ref clsid, null, 1, ref NativeMethods.ActiveX.IID_IUnknown);
				//this.instance = obj;
				typeof(AxHost).GetField("instance", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(this, unk);

				//clsid.
				//Console.WriteLine("CreateInstanceCore(" + clsid + ") :: " + dllOcxPtr + " :: " + dllGetClassObjectPtr);
				Console.WriteLine("Created from '" + this.ocxPath + "'");
				//return unk;
				//GetProcAddress
				return unk;
			}
			else
			{
				return base.CreateInstanceCore(clsid);
			}
		}

		string ocxPath;

		public ExAxShockwaveFlash(string ocxPath)
		{

			this.ocxPath = ocxPath;
			init();
		}

		public ExAxShockwaveFlash()
		{
			this.ocxPath = null;
			init();
		}

		protected void init()
		{
			SetStyle(ControlStyles.SupportsTransparentBackColor, true);
			SetStyle(ControlStyles.Opaque, true);
			SetStyle(ControlStyles.ResizeRedraw, true);

            Callbacks = new Dictionary<string, Func<Array, Object>>();
			FlashCall += new AxShockwaveFlashObjects._IShockwaveFlashEvents_FlashCallEventHandler(flash_FlashCall);
		}

        Dictionary<string, Func<Array, Object>> Callbacks;

		public void RegisterCallback(string Name, Func<Array, Object> Callback)
		{
			Callbacks[Name] = Callback;
		}

		void flash_FlashCall(object sender, AxShockwaveFlashObjects._IShockwaveFlashEvents_FlashCallEvent e)
		{
			//var flash = (ExAxShockwaveFlash)sender;
			//Console.WriteLine(sender);
			//Console.WriteLine(e.request);

			//Console.WriteLine("**********************");

			InvokeInfo InvokeInfo;
			try
			{
				//Console.WriteLine("Input:{0}", e.request);
				InvokeInfo = UnserializeInvoke(e.request);
			}
			catch (XmlException)
			{
				return;
			}
			catch (Exception Exception)
			{
				Console.WriteLine(Exception);
				return;
			}
            Func<Array, Object> Callback;
			try
			{
				Callback = Callbacks[InvokeInfo.Name];
			}
			catch (Exception Exception)
			{
				if (InvokeInfo.Name.Substring(0, 8) == "function")
				{
					return;
				}
				Console.WriteLine("Calling undefined callback '{0}'", InvokeInfo.Name);
				Console.WriteLine(Exception);
				return;
			}

			Object ReturnValue;

			try
			{
				ReturnValue = Callback(InvokeInfo.Params);
			}
			catch (Exception Exception)
			{
				Console.WriteLine("Error on Callback: " + Exception);
				return;
			}


			try
			{
				var ResultStringXml = SerializeObject(ReturnValue);
				//Console.WriteLine("Result('{0}') :: {1}", ReturnValue, ResultStringXml);
				SetReturnValue(ResultStringXml);
			}
			catch (Exception Exception)
			{
				Console.WriteLine("Result('{0}')", ReturnValue);
				Console.WriteLine("Error setting result: " + Exception);
				return;
			}
		}

		[Flags]
		private enum DrawingOptions
		{
			PRF_CHECKVISIBLE = 0x00000001,
			PRF_NONCLIENT = 0x00000002,
			PRF_CLIENT = 0x00000004,
			PRF_ERASEBKGND = 0x00000008,
			PRF_CHILDREN = 0x00000010,
			PRF_OWNED = 0x00000020
		}

		private const uint WM_PAINT = 0xF;

		[DllImport("user32.dll", CharSet = CharSet.Auto)]
		private static extern IntPtr SendMessage(IntPtr hWnd, uint msg, IntPtr dc, DrawingOptions opts);

		static private String MD5Hex(byte[] Data) {
            var Data2 = MD5.Create().ComputeHash(Data);
            var Out = "";
            for (int n = 0; n < Data2.Length; n++) {
                Out += Data2[n].ToString("x2");
            }
			return Out;
		}

		static private String MD5Hex(String Data, Encoding Encoding)
		{
			return MD5Hex(Encoding.GetBytes(Data));
		}

		static private String MD5Hex(String Data)
		{
			return MD5Hex(Data, Encoding.Unicode);
		}

		static public Image StaticTakeScreenshot(String MovieURL, int FrameNum = 0, FlashScaleMode ScaleMode = FlashScaleMode.showAll, Color? BackgroundColor = null)
		{
			return StaticTakeScreenshot(MovieURL, new Size(100, 100), FrameNum, ScaleMode, BackgroundColor);
		}

		static public Image StaticTakeScreenshot(String MovieURL, Size Size, int FrameNum = 0, FlashScaleMode ScaleMode = FlashScaleMode.showAll, Color? BackgroundColor = null)
		{
			var flash = new ExAxShockwaveFlash();

			//IFormatter formatter = new BinaryFormatter();
			//var state = (AxHost.State)formatter.Deserialize(new MemoryStream(OcxData));
			//Console.WriteLine(state);

			if (!File.Exists(MovieURL)) throw(new Exception("Flash Movie '" + MovieURL + "' doesn't exists."));

			String Hash = "ExAxShockwaveFlashCache_" + MD5Hex(MovieURL) + "_" + MD5Hex(File.GetLastWriteTimeUtc(MovieURL).ToString()) + "_" + Size.Width + "x" + Size.Height + "_" + FrameNum + "_" + ScaleMode + "_" + MD5Hex(BackgroundColor.ToString());
			String CacheTempFile = Path.GetTempPath() + @"\" + Hash + ".bmp";

			if (File.Exists(CacheTempFile))
			{
				return Image.FromFile(CacheTempFile);
			}

			var tempForm = new Form();
			tempForm.BackColor = Color.White;

			flash.BeginInit();
			{
				tempForm.Controls.Add(flash);
				flash.Size = Size;
				//flash.Visible = false;
			}
			flash.EndInit();
			//flash.PreferredSize = Size;

			flash.LoadMovie(0, MovieURL);
			flash.Size = Size;
			flash.Quality = 1;
			flash.GotoFrame(FrameNum);
			flash.ScaleMode = (int)ScaleMode;

			//flash.BGColor = "ffffff";
			
			if (BackgroundColor.HasValue)
			{
				//Console.WriteLine(BackgroundColor);
				//Console.WriteLine(BackgroundColor.Value.R);
				//flash.BGColor = "ffffff";
				//flash.WMode = "Window";
				//flash.BackColor = Color.White;
				//flash.BackgroundColor = BackgroundColor.Value.ToArgb();
				flash.BackgroundColor = (BackgroundColor.Value.R << 0) | (BackgroundColor.Value.G << 8) | (BackgroundColor.Value.B << 16);
			}
			else
			{
				flash.WMode = "Transparent";
				//Console.WriteLine("Transparent");
			}

			//Thread.Sleep(20);
			Bitmap Screenshot = flash.TakeScreenshot(Size);

			flash.Dispose();
			tempForm.Dispose();

			Screenshot.Save(CacheTempFile);

			return Screenshot;
		}

		public Bitmap TakeScreenshot()
		{
			return this.TakeScreenshot(new Size(100, 100));
		}

		public Bitmap TakeScreenshot(Size Size)
		{
			return CaptureControl(this);
		}

		// http://stackoverflow.com/questions/4135599/c-activex-drawtobitmap-equivalent
		static private Bitmap CaptureControl(Control control)
		{
			Bitmap bm = new Bitmap(control.Width, control.Height);
			using (Graphics g = Graphics.FromImage(bm))
			{
				IntPtr dc = g.GetHdc();
				try
				{
					SendMessage(
						control.Handle,
						WM_PAINT,
						dc,
						DrawingOptions.PRF_CLIENT | DrawingOptions.PRF_NONCLIENT | DrawingOptions.PRF_CHILDREN
					);
				}
				finally
				{
					
					g.ReleaseHdc();
				}
			}
			return bm;
		}

		public void SetReturnValueObject(object ReturnValue)
		{
			SetReturnValue(SerializeObject(ReturnValue));
		}

		static public string ToOutputString(object Param)
		{
			if (Param == null) return "null";

			var Type = Param.GetType();
			var TypeString = Type.ToString();
			if (TypeString.Substring(TypeString.Length - 2) == "[]")
			{
				var dynParam = (Array)Param;
				string ret = "";
				ret += "";
				for (int n = 0; n < dynParam.Length; n++)
				{
					if (n != 0) ret += ", ";
					ret += ToOutputString(dynParam.GetValue(n));
				}
				ret += "";
				return ret;
			}

			switch (TypeString)
			{
				case "System.String":
				case "System.Int16":
				case "System.Int32":
				case "System.Int64":
				case "System.UInt16":
				case "System.UInt32":
				case "System.UInt64":
					return String.Format("{0}", Param);
				case "System.Collections.Hashtable":
					Hashtable Table = (Hashtable)Param;
					{
						string ret = "";
						ret += "{";
						bool First = true;
						foreach (var Key in Table.Keys)
						{
							if (First)
							{
								First = false;
							}
							else
							{
								ret += ", ";
							}
							ret += ToOutputString(Key) + ":" + ToOutputString(Table[Key]);
						}
						ret += "}";
						return ret;
					}
				case "System.Collections.ArrayList":
					ArrayList Array = (ArrayList)Param;
					{
						string ret = "";
						ret += "";
						bool First = true;
						foreach (var Value in Array)
						{
							if (First)
							{
								First = false;
							}
							else
							{
								ret += ", ";
							}
							ret += ToOutputString(Value);
						}
						ret += "";
						return ret;
					}
				default:
					//throw (new Exception("Unknown param type '" + Param.GetType() + "'"));
					throw (new Exception(String.Format("Unknown param type '{0}'", TypeString)));
				//return "\"" + Param + "\"";
			}
		}

		static public string ToJson(object Param)
		{
			var Type = Param.GetType();
			var TypeString = Type.ToString();
			if (TypeString.Substring(TypeString.Length - 2) == "[]")
			{
				var dynParam = (Array)Param;
				string ret = "";
				ret += "[";
				for (int n = 0; n < dynParam.Length; n++)
				{
					if (n != 0) ret += ", ";
					ret += ToJson(dynParam.GetValue(n));
				}
				ret += "]";
				return ret;
			}

			switch (TypeString)
			{
				case "System.String":
					return "\"" + Param + "\"";
				case "System.Int16":
				case "System.Int32":
				case "System.Int64":
				case "System.UInt16":
				case "System.UInt32":
				case "System.UInt64":
					return String.Format("{0}", Param);
				case "System.Collections.Hashtable":
					Hashtable Table = (Hashtable)Param;
					{
						string ret = "";
						ret += "{";
						bool First = true;
						foreach (var Key in Table.Keys)
						{
							if (First)
							{
								First = false;
							}
							else
							{
								ret += ", ";
							}
							ret += ToJson(Key) + ":" + ToJson(Table[Key]);
						}
						ret += "}";
						return ret;
					}
				case "System.Collections.ArrayList":
					ArrayList Array = (ArrayList)Param;
					{
						string ret = "";
						ret += "[";
						bool First = true;
						foreach (var Value in Array)
						{
							if (First)
							{
								First = false;
							}
							else
							{
								ret += ", ";
							}
							ret += ToJson(Value);
						}
						ret += "]";
						return ret;
					}
				default:
					//throw (new Exception("Unknown param type '" + Param.GetType() + "'"));
					throw(new Exception(String.Format("Unknown param type '{0}'", TypeString)));
					//return "\"" + Param + "\"";
			}
		}


		public string SerializeObject(object Param)
		{
			if (Param == null) return "<null></null>";
			var Type = Param.GetType();
			var TypeString = Type.ToString();
			if (TypeString.Substring(TypeString.Length - 2) == "[]")
			{
				var dynParam = (Array)Param;
				string ret = "";
				ret += "<array>";
				for (int n = 0; n < dynParam.Length; n++)
				{
					ret += "<property id=\"" + n + "\">";
					ret += SerializeObject(dynParam.GetValue(n));
					ret += "</property>";
				}
				ret += "</array>";
				return ret;
			}

			switch (TypeString)
			{
				case "System.String":
					return "<string>" + Param + "</string>";
				case "System.Int16":
				case "System.Int32":
				case "System.Int64":
				case "System.UInt16":
				case "System.UInt32":
				case "System.UInt64":
					return "<number>" + Param + "</number>";
				case "System.Collections.Hashtable":
					Hashtable Table = (Hashtable)Param;
					{
						string ret = "";
						ret += "<object>";
						foreach (var Key in Table.Keys)
						{
							ret += "<property id=\"" + Key + "\">";
							ret += SerializeObject(Table[Key]);
							ret += "</property>";

						}
						ret += "</object>";
						return ret;
					}
				case "System.Collections.ArrayList":
					ArrayList Array = (ArrayList)Param;
					{
						string ret = "";
						int n = 0;
						ret += "<array>";
						foreach (var Value in Array)
						{
							ret += "<property id=\"" + n + "\">";
							ret += SerializeObject(Value);
							ret += "</property>";
							n++;
						}
						ret += "</array>";
						return ret;
					}
				default:
					//throw (new Exception("Unknown param type '" + Param.GetType() + "'"));
					throw(new Exception(String.Format("Unknown param type '{0}'", TypeString)));
					//return "<string>" + Param + "</string>";
			}
		}

		public object ExternalInterfaceCall(String Name, params object[] Params)
		{
			//typeof(T).
			String CallString = "";
			CallString += "<invoke name=\"" + Name + "\" returntype=\"xml\">";
			CallString += "<arguments>";

			foreach (var Param in Params)
			{
				CallString += SerializeObject(Param);
			}

			CallString += "</arguments>";
			CallString += "</invoke>";

			return UnserializeObject(CallFunction(CallString));
		}

		public class InvokeInfo
		{
			public string Name;
			public Array Params;
		}

		public InvokeInfo UnserializeInvoke(String InvokeXmlString)
		{
			try
			{
				XmlDocument XmlDocument = new XmlDocument();
				XmlDocument.LoadXml(InvokeXmlString);
				var FirstChild = XmlDocument.FirstChild;
				if (FirstChild.Name != "invoke")
				{
					throw (new Exception("Not an invoke"));
				}

				var objects = new List<object>();

				foreach (var Node in FirstChild.FirstChild.ChildNodes)
				{
					objects.Add(UnserializeObject((XmlNode)Node));
				}

				return new InvokeInfo() { Name = FirstChild.Attributes["name"].Value, Params = objects.ToArray() };
			}
			catch (XmlException exception)
			{
				throw (exception);
			} 
			catch (Exception exception)
			{
				Console.WriteLine("");
				Console.WriteLine(InvokeXmlString);
				Console.WriteLine("");
				throw (exception);
			}
		}

		public object UnserializeObject(String XmlDocumentString)
		{
			XmlDocument XmlDocument = new XmlDocument();
			XmlDocument.LoadXml(XmlDocumentString);
			try
			{
				return UnserializeObject(XmlDocument.FirstChild);
			}
			catch (Exception e)
			{
				Console.WriteLine("::'" + XmlDocumentString + "'");
				throw(new Exception("Error unserializing", e));
			}
		}

		public object UnserializeObject(XmlNode RootNode)
		{
			String Type = RootNode.Name;
			String ValueStr = RootNode.InnerText;

			switch (Type)
			{
				case "string": return ValueStr;
				case "number": return ValueStr;
				case "undefined": return null;
				case "true": return true;
				case "false": return false;
				case "null": return null;
				case "array":
					{
						var Array = new ArrayList();
						//var Array = new Hashtable();
						//var Array = new Dictionary<object, object>();
						int ExpectedIndex = 0;
						foreach (XmlNode PropertyNode in RootNode.ChildNodes)
						{
							//Console.WriteLine("INNER: " + PropertyNode.InnerXml);
							String Index = PropertyNode.Attributes["id"].Value;
							object Value = UnserializeObject(PropertyNode.FirstChild);
							if (ExpectedIndex.ToString() != Index)
							{
								throw(new Exception(String.Format("Invalid ArrayIndex : Expected:{0}, Found:{1}", ExpectedIndex, Index)));
							}
							Array.Add(Value);
							//Array[Convert.ToInt32(Key)] = Value;
							ExpectedIndex++;
						}
						return Array;
					}
				case "object":
				{
					var Array = new Hashtable();
					foreach (XmlNode PropertyNode in RootNode.ChildNodes)
					{
						//Console.WriteLine("INNER: " + PropertyNode.InnerXml);
						String Key = PropertyNode.Attributes["id"].Value;
						object Value = UnserializeObject(PropertyNode.FirstChild);
						Array[Key] = Value;
					}
					return Array;
				}
				default: throw (new Exception("Unknown flash return type: '" + Type + "'"));
			}

			//return null;
		}

		//public event _IShockwaveFlashEvents_OnProgressEventHandler OnClick;
		public delegate void MouseDelegate(object o);

		public event MouseDelegate MouseLeftClick;
		public event MouseDelegate MouseRightClick;

		protected override void WndProc(ref Message m)
		{
			if (!this.EditMode)
			{
				switch (m.Msg)
				{
					case WM_LBUTTONDOWN:
						if (MouseLeftClick != null)
						{
							MouseLeftClick(this);
							return;
						}
						break;
					case WM_RBUTTONDOWN:
						if (MouseRightClick != null)
						{
							MouseRightClick(this);
						}
						//Console.WriteLine(CallFunction2<String>("testFunction", "hello world"));
						//Console.WriteLine(CallFunction("<invoke name=\"testFunction\" returntype=\"string\"><arguments><string>hello world</string></arguments></invoke>"));
						m.Result = IntPtr.Zero;
						return;
				}
			}
			base.WndProc(ref m);
		}
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