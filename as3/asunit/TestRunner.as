package asunit {
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
		
		public function addTestCase(testCase:TestCase):void {
			var xml:XML = describeType(testCase);
			for each (var method:* in xml.method) {
				queue.push([testCase, method]);
				//trace(method);
			}
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