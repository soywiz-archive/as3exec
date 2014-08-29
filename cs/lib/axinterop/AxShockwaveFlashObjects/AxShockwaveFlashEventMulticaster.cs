using ShockwaveFlashObjects;
using System;
using System.Runtime.InteropServices;
namespace AxShockwaveFlashObjects
{
	[ClassInterface(ClassInterfaceType.None)]
	public class AxShockwaveFlashEventMulticaster : _IShockwaveFlashEvents
	{
		private AxShockwaveFlash parent;
		public AxShockwaveFlashEventMulticaster(AxShockwaveFlash parent)
		{
			this.parent = parent;
		}
		public virtual void OnReadyStateChange(int newState)
		{
			_IShockwaveFlashEvents_OnReadyStateChangeEvent e = new _IShockwaveFlashEvents_OnReadyStateChangeEvent(newState);
			this.parent.RaiseOnOnReadyStateChange(this.parent, e);
		}
		public virtual void OnProgress(int percentDone)
		{
			_IShockwaveFlashEvents_OnProgressEvent e = new _IShockwaveFlashEvents_OnProgressEvent(percentDone);
			this.parent.RaiseOnOnProgress(this.parent, e);
		}
		public virtual void FSCommand(string command, string args)
		{
			_IShockwaveFlashEvents_FSCommandEvent e = new _IShockwaveFlashEvents_FSCommandEvent(command, args);
			this.parent.RaiseOnFSCommand(this.parent, e);
		}
		public virtual void FlashCall(string request)
		{
			_IShockwaveFlashEvents_FlashCallEvent e = new _IShockwaveFlashEvents_FlashCallEvent(request);
			this.parent.RaiseOnFlashCall(this.parent, e);
		}
	}
}
