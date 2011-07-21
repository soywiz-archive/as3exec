using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AxShockwaveFlashObjects;
using System.Windows.Forms;
using System.Xml.Linq;
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

		public ExAxShockwaveFlash()
		{
			//this.FSCommand
			//this.CallFunction("alert(1);");
			//this.fsç
			//this.
			//this.drawTo
			//this.DrawToBitmap
			/*
			this.MouseRightClick += delegate(object o)
			{
				Console.WriteLine(this.ExternalInterfaceCall("testRetVoid"));
			};

			this.MouseLeftClick += delegate(object o)
			{
				//Console.WriteLine(this.CallFunction2("testRetStr", "test"));
				Console.WriteLine(this.ExternalInterfaceCall("testRetStr", 10));
				//Console.WriteLine(((dynamic)this.CallFunction2<String>("testRetArray"))["0"]);
				//Console.WriteLine(this.CallFunction2("testRetBool"));
			};
			*/

			/*
			this.MouseLeftClick += delegate(object o)
			{
				var bmp = TakeScreenshot();
				//var bmp = CaptureControl(new Button());
				bmp.Save(@"c:\temp\11.png");

				//bmp.LockBits();
				//Console.WriteLine(bmp.GetPixel(71, 71));
				//new Bitmap(
				//Console.WriteLine(g.);
			};*/
			
			//this.WMode = "Transparent";
			/*
			this.SetStyle(
			ControlStyles.SupportsTransparentBackColor |
			ControlStyles.OptimizedDoubleBuffer |
			ControlStyles.AllPaintingInWmPaint |
			ControlStyles.ResizeRedraw |
			ControlStyles.UserPaint, true);
			this.BackColor = Color.Transparent;

			this.BackgroundColor = Color.Transparent.ToArgb();
			*/
			SetStyle(ControlStyles.SupportsTransparentBackColor, true);
			SetStyle(ControlStyles.Opaque, true);
			SetStyle(ControlStyles.ResizeRedraw, true);

			Callbacks = new Dictionary<string, Func<dynamic, dynamic>>();
			FlashCall += new AxShockwaveFlashObjects._IShockwaveFlashEvents_FlashCallEventHandler(flash_FlashCall);
		}

		Dictionary<string, Func<dynamic, dynamic>> Callbacks;

		public void RegisterCallback(string Name, Func<dynamic, dynamic> Callback)
		{
			Callbacks[Name] = Callback;
		}

		void flash_FlashCall(object sender, AxShockwaveFlashObjects._IShockwaveFlashEvents_FlashCallEvent e)
		{
			//var flash = (ExAxShockwaveFlash)sender;
			//Console.WriteLine(sender);
			//Console.WriteLine(e.request);

			InvokeInfo InvokeInfo;
			try
			{
				//Console.WriteLine("Input:{0}", e.request);
				InvokeInfo = UnserializeInvoke(e.request);
			}
			catch (Exception Exception)
			{
				Console.WriteLine(Exception);
				return;
			}
			Func<dynamic, dynamic> Callback;
			try
			{
				Callback = Callbacks[InvokeInfo.Name];
			}
			catch (Exception Exception)
			{
				Console.WriteLine("Calling undefined callback '{0}'", InvokeInfo.Name);
				Console.WriteLine(Exception);
				return;
			}

			dynamic ReturnValue;

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
			return String.Join("", MD5.Create().ComputeHash(Data).Select((v) => v.ToString("x2")));
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
				var dynParam = (dynamic)Param;
				string ret = "";
				ret += "";
				for (int n = 0; n < dynParam.Length; n++)
				{
					if (n != 0) ret += ", ";
					ret += ToOutputString(dynParam[n]);
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
				var dynParam = (dynamic)Param;
				string ret = "";
				ret += "[";
				for (int n = 0; n < dynParam.Length; n++)
				{
					if (n != 0) ret += ", ";
					ret += ToJson(dynParam[n]);
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
				var dynParam = (dynamic)Param;
				string ret = "";
				ret += "<array>";
				for (int n = 0; n < dynParam.Length; n++)
				{
					ret += "<property id=\"" + n + "\">";
					ret += SerializeObject(dynParam[n]);
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

		protected object ExternalInterfaceCall(String Name, params object[] Params)
		{
			//typeof(T).
			String CallString = "";
			CallString += "<invoke name=\"" + Name + "\" returntype=\"string\">";
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
			public dynamic Params;
		}

		public InvokeInfo UnserializeInvoke(String InvokeXmlString)
		{
			XmlDocument XmlDocument = new XmlDocument();
			XmlDocument.LoadXml(InvokeXmlString);
			var FirstChild = XmlDocument.FirstChild;
			if (FirstChild.Name != "invoke")
			{
				throw(new Exception("Not an invoke"));
			}

			var objects = new List<object>();

			foreach (var Node in FirstChild.FirstChild.ChildNodes.Cast<XmlNode>())
			{
				objects.Add(UnserializeObject(Node));
			}

			return new InvokeInfo() { Name = FirstChild.Attributes["name"].Value, Params = objects.ToArray() };
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
