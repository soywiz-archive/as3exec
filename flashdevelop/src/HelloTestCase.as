package  
{
	import as3exec.asunit.TestCase;
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
			sprite.graphics.lineTo(5, 0);
			bitmapData.draw(sprite);
			assertEquals(0xffff0000, bitmapData.getPixel32(0, 0));
			assertEquals(0x00000000, bitmapData.getPixel32(0, 1));
		}

		public function testEqualsArrayIgnoringOrderWorksOnDisordered():void {
			assertEqualsArrayIgnoringOrder([1, 2, 3, 4], [4, 3, 2, 1]);
		}

		public function testEqualsArrayIgnoringOrderFailsOnLengthMismatch():void {
			expectException(Error, function():void {
				assertEqualsArrayIgnoringOrder([1, 2, 3, 4], [1, 2, 3, 4, 5]);
			});

			expectException(Error, function():void {
				assertEqualsArrayIgnoringOrder([1, 2, 3, 4, 5], [1, 2, 3, 4]);
			});
		}
		
		public function testEqualsArrayFailsOnDisordered():void {
			expectException(Error, function():void {
				assertEqualsArray([1, 2, 3, 4], [4, 3, 2, 1]);
			});
		}

		public function testEqualsArraySuccessOnOrdered():void {
			assertEqualsArray([1, 2, 3, 4], [1, 2, 3, 4]);
		}
	}

}