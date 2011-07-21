package {
	import flash.external.ExternalInterface;

	public class Stdio {
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
	}
}