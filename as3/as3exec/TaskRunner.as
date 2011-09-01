package as3exec 
{
	import flash.utils.setTimeout;
	public class TaskRunner 
	{
		var tasks:Vector.<Function>;
		var onComplete:Function;
		
		/**
		 * 
		 * @example
		 * function taskRunnerCompleted() {
		 * 	// Se ejecutaría al completar todas las tareas añadidas.
		 * }
		 * 
		 * var taskRunner:TaskRunner = new TaskRunner(taskRunnerCompleted);
		 * taskRunner.add(function():void {
		 *     // Tarea asíncrona aquí.
		 *     setTimeout(taskRunner.next, 500);
		 * });
		 * taskRunner.add(function():void {
		 * 	var obj = ...;
		 * 	obj.addEventListener(Event.COMPLETE, taskRunner.next);
		 * });
		 * taskRunner.start();
		 * 
		 * @param	onComplete
		 */
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