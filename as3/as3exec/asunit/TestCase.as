package as3exec.asunit {
	import as3exec.Stdio;
	import as3exec.utils.tasks.TaskRunner;
	import as3exec.utils.Utils;

	import flash.utils.ByteArray;
	import flash.utils.clearTimeout;
	import flash.utils.Dictionary;
	import flash.utils.getQualifiedClassName;
	import flash.utils.setTimeout;

	public class TestCase {
		public function TestCase() {
		}
		
		final public function __setUp():void {
			executionStep = 0;
		}
		
		public function setUp():void {
		}

		public function tearDown():void {
		}
		
		public var totalCount:int = 0;
		public var incompleteCount:int = 0;
		public var errorCount:int = 0;
		public var waitAsyncCount:int = 0;
		public var waitAsyncCallback:Function;

		protected var executionStep:int = 0;

		public function __init(waitAsyncCallback:Function):void {
			this.totalCount = 0;
			this.errorCount = 0;
			this.incompleteCount = 0;
			this.waitAsyncCount = 0;
			this.waitAsyncCallback = waitAsyncCallback;
		}
		
		private var _setUpAsyncOnceFlag:Boolean = false;
		
		final public function _setUpAsyncOnce(onReady:Function):void {
			if (!_setUpAsyncOnceFlag) {
				_setUpAsyncOnceFlag = true;
				var taskRunner:TaskRunner = new TaskRunner(onReady);
				{
					setUpAsyncOnce(taskRunner);
				}
				taskRunner.execute();
			} else {
				Utils.delayedExec(onReady);
			}
		}
		
		protected function setUpAsyncOnce(taskRunner:TaskRunner):void {
		}

		final public function _setUpAsync(onReady:Function):void {
			setUpAsync(onReady);
		}

		protected function setUpAsync(onReady:Function):void {
			Utils.delayedExec(onReady);
			//onReady();
		}
		
		final public function __captureAsserts(callback:Function):void {
			if (Stdio.hasVisualConsole) {
				callback();
				return;
			}
			try {				
				try {
					callback();
					return;
				} catch (e:Error) {
					var errorStr:String;
					//Stdio.writefln(e.getStackTrace().toString());
					errorStr = e.getStackTrace().toString();
					errorStr = errorStr.split("\n").slice(3).join("\n");
					Stdio.writefln(e + "\n" + errorStr);					
				}
			} catch (e:*) {
				Stdio.writefln("Assert Failed");				
			}
			errorCount++;
		}

		final private function assert(type:String, result:Boolean, message:String = ""):void {
			totalCount++;
			if (!result) {
				throw(new Error("Assert '" + type + "' Failed :: " + message));
			}
		}
		
		final protected function fail(message:String = ""):void {
			assert("fail", false, message);
		}
		
		final protected function assertIncomplete(message:String = ""):void {
			incompleteCount++;
		}
		
		final protected function assertExecutionStep(index:int = 0, message:String = ""):void {
			assertEquals(index, executionStep++);
		}
		
		final protected function success(message:String = ""):void {
			assert("success", true, message);
		}

		final protected function assertFail(message:String = ""):void {
			fail(message);
		}

		final protected function assertSuccess(message:String = ""):void {
			success(message);
		}

		final protected function assertTrue(actual:Boolean):void {
			assert("assertTrue", actual);
		}

		final protected function assertFalse(actual:Boolean):void {
			assert("assertFalse", !actual);
		}

		final protected function assertNull(actual:*):void {
			assert("assertNull", actual === null);
		}
		
		final protected function assertNotNull(actual:*):void {
			assert("assertNotNull", actual !== null);
		}
		
		final protected function assertEquals(expected:*, actual:*):void {
			assert("assertEquals", expected == actual, "!(" + expected + " == " + actual + ")");
		}

		final protected function assertNotEquals(expected:*, actual:*):void {
			assert("assertNotEquals", expected != actual, "!(" + expected + " != " + actual + ")");
		}

		final protected function assertSame(expected:*, actual:*):void {
			assert("assertEquals", expected === actual, "!(" + expected + " === " + actual + ")");
		}

		final protected function assertNotSame(expected:*, actual:*):void {
			assert("assertEquals", expected !== actual, "!(" + expected + " !== " + actual + ")");
		}

		final protected function assertEqualsFloat(expected:Number, actual:Number, tolerance:Number = 0):void {
			assert("assertEqualsFloat", (Math.abs(actual - expected) <= tolerance), "Float(" + expected + " == " + actual + ")");
		}
		
		final protected function assertDeepEquals(expected:*, actual:*):void {
			_assertDeepEquals("assertDeepEquals", expected, actual);
		}
		
	
		final private function _deepEquals(expected:*, actual:*):Boolean {
			var expectedBA:ByteArray = new ByteArray();
			var actualBA:ByteArray = new ByteArray();
			with (expectedBA) { writeObject(expected); position = 0; }
			with (actualBA) { writeObject(actual); position = 0; }
			if (expectedBA.length != actualBA.length) return false;
			//return expectedBA.toString() == actualBA.toString();
			for (var n:int = 0; n < expectedBA.length; n++) if (expectedBA[n] != actualBA[n]) return false;
			return true;
		}

		final private function _assertDeepEquals(type:String, expected:*, actual:*):void {
			assert(type, _deepEquals(expected, actual), "Object(" + expected + " == " + actual + ")");
		}

		final private function _assertEqualsArray(type:String, expected:Array, actual:Array):void {
			var result:Boolean = true;
			if (expected.length == actual.length) {
				result = true;
				var len:int = expected.length;
				for (var n:int = 0; n < len; n++) {
					if (expected[n] != actual[n]) {
						result = false;
						break;
					}
				}
			} else {
				result = false;
			}
			
			assert(type, result, "Array(" + expected + " == " + actual + ")");
		}
		
		final protected function assertEqualsArray(expected:Array, actual:Array):void {
			_assertEqualsArray("assertEqualsArray", expected, actual);
		}

		final protected function assertEqualsArrayIgnoringOrder(expected:Array, actual:Array):void {
			_assertEqualsArray("assertEqualsArrayIgnoringOrder", expected.sort(), actual.sort());
		}
		
		final protected function expectException(exceptionType:Class, code:Function /* ():void */):void {
			try {
				code();
			} catch (e:*) {
				if (!(e is exceptionType)) {
					fail("Exception thrown doesn't match. Expected: '" + getQualifiedClassName(exceptionType) + "', but get '" + getQualifiedClassName(e) + "'");
				}
				return;
			}
			fail("assertThrows didn't throw any exception, but expected: '" + getQualifiedClassName(exceptionType) + "'");
		}

		/**
		 * Checks that a function is called with a parameter several times with a set of values.
		 *
		 * @example
		 * var callback:Function = addAsyncExpectParameter([1, 2, 3]);
		 * Utils.delayedExec(function() {
		 *     callback(1);
		 *     callback(2);
		 *     callback(3);
		 * });
		 *
		 * @param	expectedValues            An array with a list of expected values
		 * @param	parameterIndexToCheck     Index of the parameter to check
		 * @param	timeout                   Milliseconds to wait for all the calls
		 * @return
		 */
		final protected function addAsyncExpectParameter(expectedValues:Array, parameterIndexToCheck:int = 0, timeout:int = 1000):Function {
			return addAsync(function():void {
				assertEquals(expectedValues.shift(), arguments[parameterIndexToCheck]);
			}, timeout, expectedValues.length);
		}
		
		final protected function addAsyncCheckList(expectedCallbacks:Array, timeout:int = 1000):Function {
			var that:TestCase = this;
			return addAsync(function():void {
				var expectedCallback:Function = expectedCallbacks.shift() as Function;
				expectedCallback.apply(that, arguments);
			}, timeout, expectedCallbacks.length);
		}

		final protected function addAsync(callback:Function, timeout:int = 1000, expectedExecutedTimes:uint = 1):Function {
			var that:TestCase = this;
			var timeoutId:uint;

			waitAsyncCount += expectedExecutedTimes;
			
			function continueExecution():void {
				clearTimeout(timeoutId);
				waitAsyncCount--;
				if (waitAsyncCount <= 0) {
					setTimeout(waitAsyncCallback, 0);
				}
			}
			
			timeoutId = setTimeout(function():void {
				__captureAsserts(function():void {
					fail("Async timeout " + timeout + "!");
				});
				Utils.delayedExec(waitAsyncCallback);
			}, timeout);
			
			return function(...rest):void {
				callback.apply(that, rest);
				continueExecution();
			};
		}
	}
}
