package {
	import flash.external.ExternalInterface;
	import flash.events.UncaughtErrorEvent;
	import flash.display.Stage;
	import flash.system.Capabilities;

	public class Stdio {
		static public function init(stage:Stage, loaderInfo:*):void {
			stage.loaderInfo.uncaughtErrorEvents.addEventListener(UncaughtErrorEvent.UNCAUGHT_ERROR, onUncaughtError);
			loaderInfo.uncaughtErrorEvents.addEventListener(UncaughtErrorEvent.UNCAUGHT_ERROR, onUncaughtError);
			
			Stdio.writefln(
				"Version: " + Capabilities.os + " - " + Capabilities.cpuArchitecture +
				" :: " + Capabilities.version + " :: " + Capabilities.playerType +
				" :: " + (Capabilities.isDebugger ? "Debugger" : "Retail")
			);
			Stdio.writefln("");
		}
		
		static public function format(format:String, ...rest):String {
			throw(new Error("TODO"));
			return "";
		}

		static public function writef(str:*):void {
			if (ExternalInterface.available) {
				ExternalInterface.call("writef", '' + str);
			} else {
				trace(str);
			}
		}
		
		static public function writefln(str:*):void {
			if (ExternalInterface.available) {
				ExternalInterface.call("writefln", '' + str);
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
			try {
				//Stdio.writefln("onUncaughtError");
				Stdio.writefln(e);
				//Stdio.writefln(e.error);
				Stdio.writefln(e.error.getStackTrace().toString());
			} catch (e:*) {
				
			}
			Stdio.exit(-1);
		}
	}
}