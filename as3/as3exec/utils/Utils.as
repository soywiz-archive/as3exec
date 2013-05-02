package as3exec.utils 
{
	import flash.utils.setTimeout;
	/**
	 * ...
	 * @author ...
	 */
	public class Utils 
	{
		static private var enqueued:Boolean = false;
		static private var delayed:Vector.<Function> = new Vector.<Function>();
		
		static private function executeDelayed():void {
			enqueued = false;
			for each (var done:Function in delayed) {
				done();
			}
			delayed = new Vector.<Function>();
		}
		
		static public function delayedExec(done:Function):void {
			delayed.push(done);
			//done();
			if (!enqueued) {
				enqueued = true;
				setTimeout(executeDelayed, 0);
			}
		}
	}

}