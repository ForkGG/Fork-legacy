using nihilus.Logic.Manager;
using nihilus.ViewModel;

namespace nihilus.Logic.ApplicationConsole
{
    public class ConsoleManager
    {
        #region Singleton
        //Lock to ensure Singleton pattern
        private static object myLock = new object();
        private static ConsoleManager instance;
        public static ConsoleManager Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (myLock)
                    {
                        if (instance == null)
                        {
                            instance = new ConsoleManager();
                        }
                    }
                }
                return instance;
            }
        }
        private ConsoleManager()
        {
            
        }
        #endregion

        private ConsoleViewModel consoleViewModel = ApplicationManager.Instance.ConsoleViewModel;
        
        public void WriteLine(string line)
        {
            //TODO
        }
    }
}