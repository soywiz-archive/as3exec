package {
	import flash.external.ExternalInterface;
	import flash.events.UncaughtErrorEvent;
	import flash.display.Stage;

	public class Stdio {
		static public function init(stage:Stage, loaderInfo:*):void {
			stage.loaderInfo.uncaughtErrorEvents.addEventListener(UncaughtErrorEvent.UNCAUGHT_ERROR, onUncaughtError);
			loaderInfo.uncaughtErrorEvents.addEventListener(UncaughtErrorEvent.UNCAUGHT_ERROR, onUncaughtError);
		}
	
		static public function writefln(str:*):void {
			if (ExternalInterface.available) {
				ExternalInterface.call("writefln", str);
			} else {
				trace(str);
			}
		}

		static public function getargs():* {
			if (ExternalInterface.available) {
				return ExternalInterface.call("getargs");
			} else {
				return [];
			}
		}
		
		static public function exit(errorCode:int = 0):void {
			if (ExternalInterface.available) {
				ExternalInterface.call("exit", errorCode);
			} else {
				trace("Exiting...", errorCode);
			}
		}
		
		static public function onUncaughtError(e:UncaughtErrorEvent):void {
			Stdio.writefln("onUncaughtError");
			Stdio.writefln(e.toString());
			Stdio.writefln(e.error);
			Stdio.exit(-1);
		}
	}
}