package asunit {
	import flash.utils.describeType;
	import flash.utils.setTimeout;
	public class TestRunner {
		protected var queue:Array;
		
		public function TestRunner() {
			queue = [];
		}
		
		public function addTest(testCase:TestCase):void {
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
				Stdio.writef(methodPath + "...");
				{
					testCase[method.@name]();
				}
				Stdio.writefln(testCase.errorCount ? "Fail" : "Ok");
				//trace(method);
				setTimeout(executeTestMethod, 0);
			} else {
				Stdio.exit();
			}
		}
		
		public function run():void {
			executeTestMethod();
		}
	}
}