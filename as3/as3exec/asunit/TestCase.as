package as3exec.asunit {
	import as3exec.Stdio;
	import flash.utils.clearTimeout;
	import flash.utils.getQualifiedClassName;
	import flash.utils.setTimeout;

	public class TestCase {
		public function TestCase() {
		}
		
		public function setUp():void {
		}

		public function tearDown():void {
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
		
		final public function __captureAsserts(callback:Function):void {
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
		 * setTimeout(function() {
		 *     callback(1);
		 *     callback(2);
		 *     callback(3);
		 * }, 0);
		 *
		 * @param	expectedValues            An array with a list of expected values
		 * @param	parameterIndexToCheck     Index of the parameter to check
		 * @param	timeout                   Milliseconds to wait for all the calls
		 * @return
		 */
		final protected function addAsyncExpectParameter(expectedValues:Array, parameterIndexToCheck:int = 0, timeout:int = 1000):Function {
			return addAsync(function() {
				assertEquals(expectedValues.shift(), arguments[parameterIndexToCheck]);
			}, timeout, expectedValues.length);
		}
		
		final protected function addAsyncCheckList(expectedCallbacks:Array, timeout:int = 1000):Function {
			var that:TestCase = this;
			return addAsync(function() {
				var expectedCallback:Function = expectedCallbacks.shift() as Function;
				expectedCallback.apply(that, arguments);
			}, timeout, expectedCallbacks.length);
		}

		final protected function addAsync(callback:Function, timeout:int = 1000, expectedExecutedTimes:uint = 1):Function {
			var that:TestCase = this;
			var timeoutId:uint;

			waitAsyncCount += expectedExecutedTimes;
			
			function continueExecution():void {
				waitAsyncCount--;
				if (waitAsyncCount <= 0) {
					clearTimeout(timeoutId);
					setTimeout(waitAsyncCallback, 0);
				}
			}
			
			timeoutId = setTimeout(function():void {
				__captureAsserts(function():void {
					fail("Async timeout " + timeout + "!");
				});
				setTimeout(waitAsyncCallback, 0);
			}, timeout);
			
			return function(...rest):void {
				callback.apply(that, rest);
				continueExecution();
			};
		}
	}
}