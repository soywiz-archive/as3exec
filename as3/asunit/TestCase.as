package asunit {
	import flash.utils.clearTimeout;
	import flash.utils.setTimeout;
	public class TestCase {
		public function TestCase() {
			
		}
		
		public var totalCount:int = 0;
		public var errorCount:int = 0;
		public var waitAsyncCount:int = 0;
		public var waitAsyncCallback:Function;
		
		public function __init(waitAsyncCallback:Function):void {
			this.totalCount = 0;
			this.errorCount = 0;
			this.waitAsyncCount = 0;
			this.waitAsyncCallback = waitAsyncCallback;
		}

		private function assert(type:String, result:Boolean):void {
			totalCount++;
			if (!result) {
				var errorStr:String;
				errorCount++;
				try {
					try {
						throw(new Error("Assert"));
					} catch (e:Error) {
						//Stdio.writefln(e.getStackTrace().toString());
						errorStr = e.getStackTrace().toString();
						errorStr = errorStr.split("\n").slice(3).join("\n");
						Stdio.writefln("Assert '" + type + "' Failed ::\n" + errorStr);
					}
				} catch (e:*) {
					Stdio.writefln("Assert Failed");
				}
			}
		}
		
		protected function assertTrue(result:Boolean):void {
			assert("assertTrue", result);
		}

		protected function assertFalse(result:Boolean):void {
			assert("assertFalse", !result);
		}
		
		protected function assertEquals(expected:*, returned:*):void {
			assert("assertEquals", expected == returned);
		}

		protected function assertNotEquals(expected:*, returned:*):void {
			assert("assertNotEquals", expected != returned);
		}
		
		protected function addAsync(callback:Function, timeout:int = 1000):Function {
			var that:TestCase = this;
			waitAsyncCount++;
			var timeoutId:uint = setTimeout(function():void {
				throw(new Error("Async timeout!"));
			}, timeout);
			return function(...rest):void {
				clearTimeout(timeoutId);
				callback.apply(that, rest);
				waitAsyncCount--;
				if (waitAsyncCount <= 0) {
					setTimeout(waitAsyncCallback, 0);
				}
			};
		}
	}
}