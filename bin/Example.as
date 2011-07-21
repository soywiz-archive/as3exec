package {
	import flash.display.MovieClip;

	[SWF(width = "640", height = "480", backgroundColor = "#ffffff", frameRate = "24", pageTitle = "Example")]
	public class Example extends MovieClip {
		public function Example() {
			graphics.beginFill(0xFF0000);
			graphics.drawRect(0, 0, 100, 100);
			graphics.endFill();

			Stdio.writefln("Hello World!");
			Stdio.exit(0);
		}
	}
}