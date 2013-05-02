package as3exec.asunit {
	import as3exec.Stdio;
	import as3exec.utils.Utils;
	import flash.display.Sprite;
	import flash.display.StageScaleMode;
	import flash.events.Event;
	import flash.utils.describeType;
	import flash.utils.Dictionary;
	import flash.utils.getQualifiedClassName;

	public class TestRunner {
		protected var queue:Array;
		protected var testCaseFailed:int;
		protected var testCaseIncomplete:int;
		protected var testCaseTotal:int;
		protected var assertsIncomplete:int;
		protected var assertsFailed:int;
		protected var assertsTotal:int;
		protected var addedClasses:Dictionary = new Dictionary();
		
		public function TestRunner() {
			queue = [];
		}
		
		static public function fromSprite(sprite:Sprite, callback:Function /*(testRunner:TestRunner):void*/):void {
			var that:TestRunner = new TestRunner();
			
			var init:Function = function():void {
				sprite.removeEventListener(Event.ADDED_TO_STAGE, init);
				Stdio.init(sprite);

				Utils.delayedExec(function():void {
					callback(that);
					that.run();
				});
			};
			
			if (sprite.stage) init(); else sprite.addEventListener(Event.ADDED_TO_STAGE, init);
		}
		
		public function addTestCase(testCase:TestCase):TestRunner {
			var testCaseClassName:String = getQualifiedClassName(testCase);
			
			if (!(testCaseClassName in addedClasses)) {
				addedClasses[testCaseClassName] = true;
				
				var xml:XML = describeType(testCase);
				var count:int = 0;
				var methods:Array = [];
				
				for each (var method:* in xml.method) {
					if (/^test/.test(method.@name)) {
						methods.push([testCase, method, testCaseClassName + "." + method.@name]);
						count++;
					}
					//trace(method);
				}
				
				methods = methods.sort(function(_a:*, _b:*):int {
					var a:String = _a[2], b:String = _b[2];
					if (a == b) return 0;
					if (a < b) {
						return -1;
					} else {
						return +1;
					}
				});
				
				for each (var row:* in methods) {
					queue.push(row);
				}

				if (count == 0) {
					Stdio.writefln("WARNING: Class '" + xml.@name + "' doesn't have public methods starting by 'test'");
				}
			}

			return this;
		}
		
		public function executeTestMethod(endedCallback:Function):void {
			if (queue.length > 0) {
				var row:* = queue.shift();
				var testCase:TestCase = row[0];
				var method:* = row[1];
				var methodPath:String = row[2];
				var calledAlready:Boolean = false;

				var completedCallback:Function = function():void {
					if (calledAlready) return;
					calledAlready = true;
					
					Stdio.writef("...");
					if (testCase.errorCount > 0) {
						Stdio.writefln("Fail");
					} else if (testCase.totalCount == 0) {
						Stdio.writefln("No Asserts");
						assertsIncomplete++;
						testCaseIncomplete++;
					} else {
						Stdio.writefln("Ok");
					}
					
					Utils.delayedExec(function():void {
						executeTestMethod(endedCallback);
					});
				};
				
				Stdio.writef("## " + methodPath);
				testCase.__init(completedCallback);
				testCase._setUpAsyncOnce(function():void {
					testCase._setUpAsync(function():void {
						testCase.__setUp();
						testCase.setUp();
						{
							testCase.__captureAsserts(function():void {
								testCase[method.@name]();
							});
						}
						testCase.tearDown();
						
						testCaseTotal++;
						if (testCase.errorCount) testCaseFailed++;

						assertsIncomplete += testCase.incompleteCount;
						assertsTotal  += testCase.totalCount;
						assertsFailed += testCase.errorCount;
						
						//trace(method);
						if (testCase.waitAsyncCount <= 0) {
							Utils.delayedExec(completedCallback);
						}
					}
					)
				});
			} else {
				endedCallback();
			}
		}
		
		public function run():void {
			executeTestMethod(function():void {
				if (assertsTotal > 0) {
					Stdio.writefln("");
					Stdio.writefln("Results: " + (assertsTotal - assertsFailed) + " succeded / " + assertsTotal + " total");
					Stdio.writefln("Failed: " + assertsFailed + " / Incomplete: " + assertsIncomplete);
					Stdio.writefln("");
				}
				Stdio.exit();
			});
		}
	}
}