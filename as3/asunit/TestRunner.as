package asunit {
	import flash.display.Sprite;
	import flash.events.Event;
	import flash.utils.describeType;
	import flash.utils.setTimeout;
	public class TestRunner {
		protected var queue:Array;
		protected var testCaseFailed:int;
		protected var testCaseTotal:int;
		protected var assertsFailed:int;
		protected var assertsTotal:int;
		
		public function TestRunner() {
			queue = [];
		}
		
		static public function fromSprite(sprite:Sprite, callback:Function /*(testRunner:TestRunner):void*/):void {
			var that:TestRunner = new TestRunner();
			Stdio.init(sprite.stage, sprite.loaderInfo);
			
			var init:Function = function():void {
				sprite.removeEventListener(Event.ADDED_TO_STAGE, init);
				setTimeout(function():void {
					callback(that);
					that.run();
				}, 0);
			};
			
			if (sprite.stage) init(); else sprite.addEventListener(Event.ADDED_TO_STAGE, init);
		}
		
		public function addTestCase(testCase:TestCase):TestRunner {
			var xml:XML = describeType(testCase);
			for each (var method:* in xml.method) {
				queue.push([testCase, method]);
				//trace(method);
			}
			return this;
		}
		
		public function executeTestMethod(endedCallback:Function):void {
			if (queue.length > 0) {
				var row:* = queue.shift();
				var testCase:TestCase = row[0];
				var method:* = row[1];
				var methodPath:String = method.@declaredBy + "." + method.@name;
				if (/^test/.test(method.@name)) {
					var completedCallback:Function = function():void {
						Stdio.writefln(testCase.errorCount ? "Fail" : "Ok");
						executeTestMethod(endedCallback);
					};
					
					Stdio.writef(methodPath + "...");
					{
						testCase.__init(completedCallback);
						testCase[method.@name]();
					}
					
					assertsTotal  += testCase.totalCount;
					assertsFailed += testCase.errorCount;
					
					//trace(method);
					if (testCase.waitAsyncCount <= 0) {
						setTimeout(completedCallback, 0);
					}
				} else {
					setTimeout(function():void {
						executeTestMethod(endedCallback);
					}, 0)
				}
			} else {
				endedCallback();
			}
		}
		
		public function run():void {
			executeTestMethod(function():void {
				Stdio.writefln("");
				Stdio.writefln("Results: " + (assertsTotal - assertsFailed) + " succeded / " + assertsTotal + " total");
				Stdio.writefln("Failed: " + assertsFailed);
				Stdio.exit();
			});
		}
	}
}