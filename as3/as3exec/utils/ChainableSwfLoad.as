package as3exec.utils 
{
	import flash.events.Event;
	import org.casalib.load.ISwfLoad;

	public class ChainableSwfLoad 
	{
		protected var swfLoad:ISwfLoad;
		
		public function ChainableSwfLoad(swfLoad:ISwfLoad) 
		{
			this.swfLoad = swfLoad;
		}
		
		public function addEventListener (type:String, listener:Function, useCapture:Boolean = false, priority:int = 0, useWeakReference:Boolean = false) : ChainableSwfLoad
		{
			this.swfLoad.addEventListener(type, listener, useCapture, priority, useWeakReference);
			return this;
		}
		
		public function start():ChainableSwfLoad
		{
			this.swfLoad.start();
			return this;
		}
	}
}