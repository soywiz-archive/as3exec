package  
{
	import asunit.TestCase;
	import flash.utils.setTimeout;

	public class HelloTestCase extends TestCase {
		public function testHello():void {
			assertTrue(true);
		}
		
		public function test2():void {
			assertTrue(false);
		}

		public function testTimeout():void {
			setTimeout(addAsync(function(...rest):void {
				assertTrue(true);
			}, 1000), 20);
			//assertTrue(false);
		}
	}

}