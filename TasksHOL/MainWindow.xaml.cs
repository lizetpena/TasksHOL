using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;


namespace HOL
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        BackgroundWorker _worker;
        const int _numTasks = 5;

        public static List<Task> Tasks = new List<Task>();

        public MainWindow()
        {
            InitializeComponent();

            _worker = new BackgroundWorker();
            _worker.DoWork += new DoWorkEventHandler(worker_DoWork);
            _worker.WorkerReportsProgress = true;
            _worker.ProgressChanged += new ProgressChangedEventHandler(worker_ProgressChanged);
            _worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(w_RunWorkerCompleted);
        }

        void w_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            buttonStart.IsEnabled = true;
            buttonStop.IsEnabled = false;
        }

        private void buttonStart_Click(object sender, RoutedEventArgs e)
        {
            buttonStart.IsEnabled = false;
            buttonStop.IsEnabled = true;

            _worker.RunWorkerAsync();
        }

        int prog = 0;

        void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            prog = 0;

            _worker.ReportProgress(prog++);

            CreateTasks();

            _worker.ReportProgress(prog++);

            try
            {
                while (!Task.WaitAll(Tasks.ToArray(), 300))
                {
                    _worker.ReportProgress(prog++);
                }
            }
            catch { } // Cancelling causes an exception to be througn

            _worker.ReportProgress(-1);
        }

        private void CreateTasks()
        {
            Tasks.Clear();

            for (int i = 0; i < _numTasks; i++)
            {
                OrderForm f = new OrderForm() { Name = "Test #" + i, PostalCode = "00000" };

                Tasks.Add(new Task(Work.ProcessForm, f, Work.CancelToken.Token));
                _worker.ReportProgress(prog++);
            }

            foreach (Task t in Tasks)
            {
                t.Start();
                _worker.ReportProgress(prog++);
            }
        }


        void worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            listBoxStatus.ItemsSource = from ct in Tasks
                                        select new
                                        {
                                            ct.Id,
                                            Status = ct.Status.ToString(),
                                            OrderStatus = ct.AsyncState ?? "<Unassigned>"
                                        };
        }

        private void buttonStop_Click(object sender, RoutedEventArgs e)
        {
            Work.CancelToken.Cancel();
        }
    }
}
