using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace as3exec
{
	static class Program
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main(string[] args)
		{
			try
			{
				Application.EnableVisualStyles();
				Application.SetCompatibleTextRenderingDefault(false);
				Form1 form = new Form1(args);

				form.ShowInactiveTopmost();
				//Application.Run(form);
				Application.Run();
			}
			catch (Exception e)
			{
				Console.WriteLine(String.Format("Exception: {0}", e));
			}
		}
	}
}
