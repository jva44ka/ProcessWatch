using System;

namespace ProcessWatcher
{
    class Program
    {
        static void Main(string[] args)
        {
            Watcher watcher = new Watcher(args[0], args[1], args[2]);
            //Watcher watcher = new Watcher();
        }
    }
}
