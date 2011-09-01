package as3exec.utils.events 
{
	internal class EventCallback 
	{
		protected var event:Event;
		protected var callback:Function;
		protected var executionCount:int = 0;
		protected var maxExecutionCount:int = 0;
		
		public function EventCallback(event:Event, callback:Function, maxExecutionCount:int = -1):void {
			this.event = event;
			this.callback = callback;
			this.maxExecutionCount = maxExecutionCount;
		}
		
		public function destroy():void {
			event.removeEventCallback(this);
			this.event = null;
		}
		
		public function dispatch(target:Object):void {
			this.callback.call(null, target);
			executionCount++;
			
			if (maxExecutionCount != -1) {
				if (executionCount >= maxExecutionCount) {
					destroy();
				}
			}
		}
	}

}