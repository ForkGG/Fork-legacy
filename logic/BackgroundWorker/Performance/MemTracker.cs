using System.Diagnostics;
using System.Threading.Tasks;
using Fork.ViewModel;

namespace Fork.Logic.BackgroundWorker.Performance
{
    public class MemTracker
    {
        private bool interrupted;

        public void TrackP(Process p, EntityViewModel viewModel)
        {
            //TODO this is not working with Java 15 bc of subprocess spawned
            Task.Run(async () =>
            {
                while (!interrupted == !p.HasExited)
                    try
                    {
                        p.Refresh();
                        viewModel.MemValueUpdate(p.WorkingSet64 / (1024d * 1024d));
                        await Task.Delay(500);
                    }
                    catch
                    {
                        break;
                    }

                viewModel.MemValueUpdate(0.0);
                viewModel.MemValueUpdate(0.0);
                viewModel.MemValueUpdate(0.0);
            });
        }

        public void StopThreads()
        {
            interrupted = true;
        }
    }
}