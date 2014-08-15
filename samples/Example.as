package {
	import flash.display.MovieClip;
	import flash.events.Event;
	import flash.utils.setTimeout;
	import flash.utils.ByteArray;
	import as3exec.*;

	[SWF(width = "640", height = "480", backgroundColor = "#ffffff", frameRate = "24", pageTitle = "Example")]
	public class Example extends MovieClip {
		public function Example() {
			if (stage) {
				init();
			} else {
				addEventListener(Event.ADDED_TO_STAGE, init);
			}
		}
		
		private function main():void {
			removeEventListener(Event.ADDED_TO_STAGE, init);

			graphics.beginFill(0xFF0000);
			graphics.drawRect(0, 0, 100, 100);
			graphics.endFill();

			Stdio.init(this, true);
			{
				Stdio.writefln("Hello World!");
				var ba:ByteArray = new ByteArray();
				ba.writeByte(77);
				ba.writeByte(78);
				ba.writeByte(79);
				Stdio.fs_write('temp.bin', ba);
				Stdio.writefln("readed again:" + Stdio.fs_read('temp.bin'));
				Stdio.writefln(JSON.stringify(Stdio.fs_stat('temp.bin')));
				Stdio.writefln("Hello World 2!");
				Stdio.writefln(JSON.stringify(Stdio.fs_list('..')));
				
				throw(new Error("Error!"));
			}
			Stdio.exit(0);
		}
		
		private function init(e:Event = null):void  {
			setTimeout(main, 0);
		}
	}
}