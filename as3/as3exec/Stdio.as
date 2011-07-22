package as3exec {
	import flash.display.MovieClip;
	import flash.display.Sprite;
	import flash.external.ExternalInterface;
	import flash.events.UncaughtErrorEvent;
	import flash.display.Stage;
	import flash.system.Capabilities;
	import flash.text.TextField;
	import flash.text.TextFormat;

	public class Stdio {
		static protected var textField:TextField;
		
		static public function init(sprite:Sprite):void {
			sprite.stage.loaderInfo.uncaughtErrorEvents.addEventListener(UncaughtErrorEvent.UNCAUGHT_ERROR, onUncaughtError);
			sprite.loaderInfo.uncaughtErrorEvents.addEventListener(UncaughtErrorEvent.UNCAUGHT_ERROR, onUncaughtError);
			
			if (!ExternalInterface.available) {
				textField = new TextField();
				textField.defaultTextFormat = new TextFormat("Courier New", 12);
				textField.multiline = true;
				textField.border = false;
				textField.condenseWhite = false;
				textField.x = 0;
				textField.y = 0;
				textField.width = sprite.stage.stageWidth;
				textField.height = sprite.stage.stageHeight;
				textField.text = "";
				sprite.addChild(textField);
			}
			
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
		
		static protected var buffer:String;
		
		static protected function flush(complete:Boolean = false):void {
			if (!ExternalInterface.available) {
				if (buffer.length) {
					var lines:Array = buffer.split("\n");
					if (!complete) {
						buffer = lines[lines.length - 1];
						lines = lines.slice(0, lines.length - 1);
					}
					for each (var line:String in lines) {
						trace(line);
						textField.appendText(line + "\n");
					}
				}
			}
		}
		
		static protected function appendBuffer(str:String):void {
			buffer += str;
			if (buffer.indexOf("\n") != -1) flush();
			if (buffer.length > 4096) flush();
		}

		static public function writef(str:*):void {
			if (ExternalInterface.available) {
				ExternalInterface.call("writef", '' + str);
			} else {
				appendBuffer(str);
			}
		}
		
		static public function writefln(str:*):void {
			if (ExternalInterface.available) {
				ExternalInterface.call("writefln", '' + str);
			} else {
				appendBuffer(str + "\n");
				flush();
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
			flush(true);
			
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