package {
	import flash.display.MovieClip;
	import flash.events.Event;
	import flash.utils.setTimeout;

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

			Stdio.init(stage);
			{
				Stdio.writefln("Hello World!");
				
				//throw(new Error("Error!"));
			}
			Stdio.exit(0);
		}
		
		private function init(e:Event = null):void  {
			setTimeout(main, 0);
		}
	}
}