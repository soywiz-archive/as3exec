package as3exec 
{
	import as3exec.utils.Utils;
	import flash.utils.setTimeout;
	public class TaskRunner 
	{
		internal var tasks:Vector.<Function>;
		internal var onComplete:Function;
		
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

		public function add(task:Function, ...args):void {
			this.tasks.push(function():void {
				task.apply(null, args);
			});
		}
		
		public function next(...args):void {
			if (this.tasks.length) {
				Utils.delayedExec(this.tasks.shift());
			} else {
				this.onComplete();
			}
		}
		
		public function execute(...args):void {
			next();
		}
	}

}