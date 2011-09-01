package as3exec.utils.events 
{
	import flash.utils.setTimeout;
	public class Event 
	{
		protected var eventCallbacks:Vector.<EventCallback>;
		
		public function Event():void {
			eventCallbacks = new Vector.<EventCallback>();
		}
		
		public function register(callback:Function):void {
			eventCallbacks.push(new EventCallback(this, callback));
		}

		public function registerOnce(callback:Function):void {
			eventCallbacks.push(new EventCallback(this, callback, 1));
		}
		
		internal function removeEventCallback(eventCallback:EventCallback):void {
			var index:int = eventCallbacks.indexOf(eventCallback);
			if (index != -1) {
				eventCallbacks.splice(index, 1);
			}
		}

		public function dispatchNow(target:Object, ...args):void {
			args.unshift(target);

			eventCallbacks.slice().forEach(function(item:EventCallback, index:int, eventCallbacks:Vector.<EventCallback>):void {
				item.dispatch.apply(null, args);
			});
		}
		
		public function dispatchLater(target:Object, ...args):void {
			args.unshift(target);

			setTimeout(function():void {
				dispatchNow.apply(null, args);
			}, 0);
		}
		
		public function destroy():void {
			eventCallbacks.forEach(function(eventCallback:EventCallback):void {
				eventCallback.destroy();
			});
			eventCallbacks = null;
		}
	}

}