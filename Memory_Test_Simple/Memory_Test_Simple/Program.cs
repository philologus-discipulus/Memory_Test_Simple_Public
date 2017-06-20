using System;
using Gtk;

namespace Memory_Test_Simple
{
	class MainClass
	{
		public static void Main (string[] args)
		{
			Application.Init ();
			MainWindow win = MainWindow.Create ();
			win.Show ();
			Application.Run ();
		}
	}
}
