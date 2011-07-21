package asunit {
	public class TestCase {
		public function TestCase() {
			
		}
		
		public var errorCount:int = 0;
		
		protected function assertTrue(result:Boolean):void {
			if (!result) {
				errorCount++;
				try {
					try {
						throw(new Error("Assert"));
					} catch (e:Error) {
						//Stdio.writefln(e.getStackTrace().toString());
						var str:String = e.getStackTrace().toString();
						Stdio.writefln("Assert Failed :: " + str);
					}
				} catch (e:*) {
					Stdio.writefln("Assert Failed");
				}
			}
		}
	}
}