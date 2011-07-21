package asunit {
	import flash.utils.describeType;
	import flash.utils.setTimeout;
	public class TestRunner {
		protected var queue:Array;
		
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
		
		public function executeTestMethod():void {
			if (queue.length > 0) {
				var row:* = queue.shift();
				var testCase:TestCase = row[0];
				var method:* = row[1];
				var methodPath:String = method.@declaredBy + "." + method.@name;
				if (/^test/.test(method.@name)) {
					var completedCallback:Function = function():void {
						Stdio.writefln(testCase.errorCount ? "Fail" : "Ok");
						executeTestMethod();
					};
					
					Stdio.writef(methodPath + "...");
					{
						
						testCase.__init(completedCallback);
						testCase[method.@name]();
					}
					
					//trace(method);
					if (testCase.waitAsyncCount <= 0) {
						setTimeout(completedCallback, 0);
					}
				} else {
					setTimeout(executeTestMethod, 0)
				}
			} else {
				Stdio.exit();
			}
		}
		
		public function run():void {
			executeTestMethod();
		}
	}
}