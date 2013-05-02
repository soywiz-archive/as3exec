package as3exec.utils 
{
	import flash.utils.setTimeout;
	/**
	 * ...
	 * @author ...
	 */
	public class Utils 
	{
		static private var executeDelayedEnqueued:Boolean = false;
		static private var delayedFunctionList:Vector.<Function> = new Vector.<Function>();
		
		static private function executeDelayed():void {
			executeDelayedEnqueued = false;
			for each (var delayedFunction:Function in delayedFunctionList) delayedFunction();
			delayedFunctionList = new Vector.<Function>();
		}
		
		static public function delayedExec(delayedFunction:Function):void {
			delayedFunctionList.push(delayedFunction);
			if (!executeDelayedEnqueued) {
				executeDelayedEnqueued = true;
				setTimeout(executeDelayed, 0);
			}
		}
	}

}