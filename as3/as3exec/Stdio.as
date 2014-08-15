package as3exec {
	import flash.display.MovieClip;
	import flash.display.Sprite;
	import flash.display.StageAlign;
	import flash.display.StageScaleMode;
	import flash.events.Event;
	import flash.external.ExternalInterface;
	import flash.events.UncaughtErrorEvent;
	import flash.display.Stage;
	import flash.system.Capabilities;
	import flash.text.TextField;
	import flash.text.TextFormat;
	import flash.utils.ByteArray;

	public class Stdio {
		static public var stage:Stage;
		static protected var textField:TextField;
		static protected var buffer:String = "";
		
		static public function get hasVisualConsole():Boolean {
			return Capabilities.playerType != "ActiveX";
		}
		
		static public function init(sprite:Sprite, showVersion:Boolean = true):void {
			stage = sprite.stage;
			sprite.stage.loaderInfo.uncaughtErrorEvents.addEventListener(UncaughtErrorEvent.UNCAUGHT_ERROR, onUncaughtError);
			sprite.loaderInfo.uncaughtErrorEvents.addEventListener(UncaughtErrorEvent.UNCAUGHT_ERROR, onUncaughtError);
			
			if (hasVisualConsole) {
				textField = new TextField();
				textField.defaultTextFormat = new TextFormat("Courier New", 12);
				textField.multiline = true;
				textField.border = false;
				textField.condenseWhite = false;
				textField.x = 0;
				textField.y = 0;
				textField.text = "";
				sprite.addChild(textField);
				
				sprite.stage.scaleMode = StageScaleMode.NO_SCALE;
				sprite.stage.align = StageAlign.TOP_LEFT;
				sprite.stage.addEventListener(Event.RESIZE, function(e:Event):void {
					textField.width = sprite.stage.stageWidth;
					textField.height = sprite.stage.stageHeight;
				});
				sprite.stage.dispatchEvent(new Event(Event.RESIZE));
			}
			
			if (showVersion) {
				Stdio.writefln(
					"Version: " + Capabilities.os + " - " + Capabilities.cpuArchitecture +
					" :: " + Capabilities.version + " :: " + Capabilities.playerType +
					" :: " + (Capabilities.isDebugger ? "Debugger" : "Retail")
				);
				Stdio.writefln("");
			}
		}
		
		static public function get consoleAvailable():Boolean {
			return ExternalInterface.available;
		}
		
		static public function format(format:String, ...rest):String {
			throw(new Error("TODO"));
			return "";
		}
		
		static protected function flush(complete:Boolean = false):void {
			if (hasVisualConsole) {
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
					textField.scrollV = textField.maxScrollV;
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

		static public function fs_list(path:String):Array {
			return ExternalInterface.call("fs_list", path);
		}
		
		static public function fs_stat(path:String):* {
			return JSON.parse(ExternalInterface.call("fs_stat", path));
		}
		
		static public function fs_exists(file:String):Boolean {
			return ExternalInterface.call("fs_exists", file);
		}
		
		static public function fs_delete(file:String):void {
			ExternalInterface.call("fs_delete", file);
		}

		static public function fs_write(file:String, out:ByteArray):void {
			var out_s:String = '';
			for (var n:int = 0; n < out.length; n++) {
				var o:String = out[n].toString(16);
				while (o.length < 2) o = '0' + o;
				out_s += o;
			}
			ExternalInterface.call("fs_write", file, out_s);
		}
		
		static public function fs_read(file:String):ByteArray {
			var result:String = ExternalInterface.call("fs_read", file);
			var out:ByteArray = new ByteArray();
			for (var n:int = 0; n < result.length; n += 2) {
				out.writeByte(parseInt(result.substr(n, 2), 16));
			}
			out.position = 0;
			return out;
		}

		static public function getargs():Array {
			if (ExternalInterface.available) {
				return ExternalInterface.call("getargs");
			} else {
				return ["movie.swf"];
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
		
		static public function getopt(args:Array = null, handler:Function = null /* function(key:String, value:String):void */):void {
			args = getargs().slice(1);
			
			if (args.length == 0) {
				handler('', '');
				return;
			} else {
				var key:String;
				var value:String;
				args = args.slice();
				while (args.length) {
					var arg:String = args.shift();
					if (arg.substr(0, 2) == '--') {
						var parts:Array = arg.substr(2).split('=');
						key = parts[0];
						value = parts.slice(1).join('=');
						handler(key, value);
					} else if (arg.substr(0, 1) == '-') {
						key   = arg.substr(1, 1)
						value = arg.substr(2);
						handler(key, value);
					} else {
						handler('', arg);
					}
				}
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