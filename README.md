as3exec - Execute ActionScript3 on a console (and simple ASUnit for doing continuous integration)

This project provides an executable that will run a .swf in a console and an API with simple stuff to read parameters
write to the console, and exit. It also provides an API to do unittesting easily.

as3exec.Stdio:
- hasVisualConsole()
- init(sprite:Sprite)
- consoleAvailable()
- writef(str:String)
- writefln(str:String)
- getargs()
- exit(errorCode:Int = 0)
- getopt(args:Array = null, handler:Function = null /* function(key:String, value:String):void */)
- onUncaughtError(e:UncaughtErrorEvent)

as3exec.asunit.TestCase:
- fail(message:String = ""):void
- assertIncomplete(message:String = ""):void
- assertExecutionStep(index:int = 0, message:String = ""):void
- success(message:String = ""):void
- assertFail(message:String = ""):void
- assertSuccess(message:String = ""):void
- assertTrue(actual:Boolean):void
- assertFalse(actual:Boolean):void
- assertNull(actual:*):void
- assertNotNull(actual:*):void
- assertEquals(expected:*, actual:*):void
- assertNotEquals(expected:*, actual:*):void
- assertSame(expected:*, actual:*):void
- assertNotSame(expected:*, actual:*):void
- assertEqualsFloat(expected:Number, actual:Number, tolerance:Number = 0):void
- assertEqualsArray(expected:Array, actual:Array):void
- assertEqualsArrayIgnoringOrder(expected:Array, actual:Array):void
- expectException(exceptionType:Class, code:Function /* ():void */):void
- addAsyncExpectParameter(expectedValues:Array, parameterIndexToCheck:int = 0, timeout:int = 1000):Function
- addAsyncCheckList(expectedCallbacks:Array, timeout:int = 1000):Function
- addAsync(callback:Function, timeout:int = 1000, expectedExecutedTimes:uint = 1):Function

```
TestRunner.fromSprite(this, function(testRunner:TestRunner):void {
  testRunner.addTestCase(new TestTestCaseExtended());
});
```
