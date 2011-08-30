package as3exec 
{
	import flash.utils.setTimeout;
	public class TaskRunner 
	{
		var tasks:Vector.<Function>;
		var onComplete:Function;
		
		public function TaskRunner(onComplete:Function) 
		{
			this.tasks = new Vector.<Function>();
			this.onComplete = onComplete;
		}

		public function add(task:Function, ...args) {
			this.tasks.push(function():void {
				task.apply(null, args);
			});
		}
		
		public function next(...args):void {
			if (this.tasks.length) {
				setTimeout(this.tasks.shift(), 0);
			} else {
				this.onComplete();
			}
		}
		
		public function execute(...args):void {
			next();
		}
	}

}