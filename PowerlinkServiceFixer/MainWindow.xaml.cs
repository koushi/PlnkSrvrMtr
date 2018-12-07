using System;
using System.ServiceProcess;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Runtime.InteropServices;
using System.ComponentModel;
using System.Diagnostics;

namespace PowerlinkServiceFixer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 


    public partial class MainWindow : Window
    {

        public ServiceClass foo;
        public ServiceClass bar;
        public ServiceClass magic;
        public bool serviceNeedsRestart = false;
        static NotifiableServiceController plService, sqlService, valeService;
        

        public MainWindow()
        {
            InitializeComponent();

            restartButton.Visibility = Visibility.Hidden;
            initializeTestClass();
                        
            //this.DataContext = foo;



        }

        private void initializeTestClass()
        {

            foo = new ServiceClass("PLWinService.exe");
            bar = new ServiceClass("MSSQLSERVER");
            magic = new ServiceClass("vswSQLEJobServer");

            powerlink.DataContext = foo;
            SQL.DataContext = bar;
            vale.DataContext = magic;

            plService = new NotifiableServiceController(foo.SrvcController);
            sqlService = new NotifiableServiceController(bar.SrvcController);
            valeService = new NotifiableServiceController(magic.SrvcController);

            //plService.PropertyChanged += OnServiceModified;
            //sqlService.PropertyChanged += OnServiceModified;
            //valeService.PropertyChanged += OnServiceModified;

            foo.PropertyChanged += OnPropertyChanged;
            bar.PropertyChanged += OnPropertyChanged;
            magic.PropertyChanged += OnPropertyChanged;

        }

        private void restartButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if(foo.VisibleFlag == true)
                {
                    plService.Start();
                } else if(bar.VisibleFlag == true)
                {
                    sqlService.Start();
                } else if(magic.VisibleFlag == true)
                {
                    valeService.Start();
                }
            } catch(Exception ex)
            {
                box.Text = ex.ToString();
                Debug.Write(ex.ToString());
            }
        }


        private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {

            if (sender is ServiceClass)
            {
                ServiceClass controller = sender as ServiceClass;
                if (e.PropertyName == "NotifiableController")
                {
                    
                    if (controller.ServiceName == foo.ServiceName)
                    {
                        foo.StatusToColor(controller.SrvcController.Status);

                    }
                    else if (controller.ServiceName == bar.ServiceName)
                    {
                        bar.StatusToColor(controller.SrvcController.Status);

                    }
                    else if (controller.ServiceName == magic.ServiceName)
                    {
                        magic.StatusToColor(controller.SrvcController.Status);

                    }
                    MessageBox.Show("Mooo");
                }
                
                if (e.PropertyName == "StatusColor")
                {
                    toggleButton();
                }
            }
        }

        private void toggleButton()
        {
            if((foo.VisibleFlag == false) && (bar.VisibleFlag == false) && (magic.VisibleFlag == false))
            {
                restartButton.Visibility = Visibility.Hidden;
            }
            else if((foo.VisibleFlag == true) || (bar.VisibleFlag == true) || (magic.VisibleFlag == true))
            {
                restartButton.Visibility = Visibility.Visible;
            }
        }
        
        private void OnServiceModified(object sender, PropertyChangedEventArgs e)
        {
            
            if (sender is NotifiableServiceController)
            {
                
                NotifiableServiceController controller = sender as NotifiableServiceController;
                if(controller.ServiceName == plService.ServiceName)
                {
                    foo.StatusToColor(controller.Status);
                    
                } else if (controller.ServiceName == sqlService.ServiceName)
                {
                    bar.StatusToColor(controller.Status);
                    
                } else if (controller.ServiceName == valeService.ServiceName)
                {
                    magic.StatusToColor(controller.Status);
                    
                }

            }
        }
        

    }
}
