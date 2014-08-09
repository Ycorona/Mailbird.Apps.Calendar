using DevExpress.Xpf.Scheduler;
using System.Windows;
using DevExpress.XtraScheduler;
using Mailbird.Apps.Calendar.ViewModels;
using Appointment = Mailbird.Apps.Calendar.Engine.Metadata.Appointment;
using Mailbird.Apps.Calendar.ViewModels;
using System.Windows.Threading;
using System;
using System.ComponentModel;
using System.Windows;

namespace Mailbird.Apps.Calendar
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        int nRefresh = 1000 * 60 * 5;// 5 miniutes
        MainWindowViewModel WndViewModel = null;
        private BackgroundWorker RereshBackground = new BackgroundWorker();
        delegate void MethodDelegate( );
        System.Windows.Threading.Dispatcher pDispatcher = null;
        public MainWindow()
        {
            InitializeComponent();
            Scheduler.DayView.MoreButtonDownStyle = (Style)FindResource("MoreButtonDownStyle");
            Scheduler.DayView.MoreButtonUpStyle = (Style)FindResource("MoreButtonUpStyle");
            
            pDispatcher = LayoutRoot.Dispatcher;
            RereshBackground.DoWork += new DoWorkEventHandler(DoRefreshWork);
            RereshBackground.RunWorkerAsync();
        }

        //Sync background work
        private void DoRefreshWork(object sender, DoWorkEventArgs e)
        {
            WndViewModel = new MainWindowViewModel();

            for ( ;; )
            {
                WndViewModel.GetCalendarAppointments();
                pDispatcher.BeginInvoke(
                    new MethodDelegate(SetDataContext),
                        DispatcherPriority.Background, null);

                System.Threading.Thread.Sleep( nRefresh );
            }
        }

        public void SetDataContext()
        {
            StorageData.DataSource = WndViewModel.AppointmentCollection;
        }

        public MainWindowViewModel ViewModel
        {
            get { return WndViewModel; }
        }
        
        private void Scheduler_OnEditAppointmentFormShowing(object sender, EditAppointmentFormEventArgs e)
        {
            e.Cancel = true;
            ViewModel.OpenInnerFlyout(Scheduler);
            FlyoutControl.Focus();
        }

        private void UIElement_GotFocus(object sender, RoutedEventArgs e)
        {
            if (!FlyoutControl.IsOpen) return;
            ViewModel.CloseInnerFlyout();
        }

        private void SchedulerStorage_AppointmentsChanged(object sender, DevExpress.XtraScheduler.PersistentObjectsEventArgs e)
        {
            foreach (DevExpress.XtraScheduler.Appointment obj in e.Objects)
            {
                var app = new Appointment
                {
                    Id = obj.Id,
                    AllDayEvent = obj.AllDay,
                    Description = obj.Description,
                    EndTime = obj.End,
                    LabelId = obj.LabelId,
                    Location = obj.Location,
                    ResourceId = obj.ResourceId,
                    StartTime = obj.Start,
                    StatusId = obj.StatusId,
                    Subject = obj.Subject
                };
                ViewModel.AppointmentOnViewChanged(app);
            }
        }

        private void SchedulerStorage_OnAppointmentsDeleted(object sender, PersistentObjectsEventArgs e)
        {
            foreach (DevExpress.XtraScheduler.Appointment obj in e.Objects)
            {
                ViewModel.RemoveAppointment(obj.Id);
            }
        }
    }
}
