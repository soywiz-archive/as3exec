package  
{
	import asunit.TestCase;
	import flash.display.Bitmap;
	import flash.display.BitmapData;
	import flash.display.Sprite;
	import flash.utils.setTimeout;

	public class HelloTestCase extends TestCase {
		public function testTrue():void {
			assertTrue(true);
		}
		
		public function testFalse():void {
			assertFalse(false);
		}

		public function testTimeout():void {
			setTimeout(addAsync(function(...rest):void {
				assertTrue(true);
			}, 1000), 20);
			//assertTrue(false);
		}
		
		public function testDraw():void {
			var bitmapData:BitmapData = new BitmapData(5, 5, true, 0x00000000);
			var sprite:Sprite = new Sprite();
			sprite.graphics.lineStyle(1, 0xFF0000);
			sprite.graphics.moveTo(0, 0);
			sprite.graphics.lineTo(5, 5);
			bitmapData.draw(sprite);
			Stdio.writefln(bitmapData.getPixel32(0, 0));
			Stdio.writefln(bitmapData.getPixel32(0, 5));
		}
	}

}