package  {
	import as3exec.asunit.TestRunner;
	import flash.display.Sprite;
	
	public class Main extends Sprite {
		public function Main():void {
			TestRunner.fromSprite(this, function(testRunner:TestRunner):void {
				testRunner.addTestCase(new HelloTestCase());
			});
		}
	}
}