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

        public ServiceClass PL;
        public ServiceClass MSSQL;
        public ServiceClass Vale;
        public bool serviceNeedsRestart = false;
        static NotifiableServiceController plService, sqlService, valeService;
        

        public MainWindow()
        {
            InitializeComponent();

            restartButton.Visibility = Visibility.Hidden;
            initializeData();
                        
            
        }

        private void initializeData()
        {
            


            plService = new NotifiableServiceController(new ServiceController("PLWinService.exe"));
            plService.PropertyChanged += OnPropertyChanged;
            PL = new ServiceClass(plService.DisplayName, "White");
            PL.StatusToColor(plService.Status);
            PL.PropertyChanged += OnPropertyChanged;

            sqlService = new NotifiableServiceController(new ServiceController("MSSQLSERVER"));
            sqlService.PropertyChanged += OnPropertyChanged;
            MSSQL = new ServiceClass(sqlService.DisplayName, "White");
            MSSQL.StatusToColor(sqlService.Status);
            PL.PropertyChanged += OnPropertyChanged;

            valeService = new NotifiableServiceController(new ServiceController("vswSQLEJobServer"));
            valeService.PropertyChanged += OnPropertyChanged;
            Vale = new ServiceClass(valeService.DisplayName, "White");
            Vale.StatusToColor(valeService.Status);
            Vale.PropertyChanged += OnPropertyChanged;

            powerlink.DataContext = PL;
            SQL.DataContext = MSSQL;
            vale.DataContext = Vale;
            box.Text = "";

        }

        private void restartButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                

                
            } catch(Exception ex)
            {
                
            }
        }


        private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if(sender is ServiceClass)
            {
                if(e.PropertyName == "StatusColor")
                {
                    toggleButton();
                }
            } else if (sender is NotifiableServiceController)
            {
                NotifiableServiceController temp = sender as NotifiableServiceController;
                if(Vale.ServiceName == temp.DisplayName)
                {
                    Vale.StatusToColor(temp.Status);
                } else if (PL.ServiceName == temp.DisplayName)
                {
                    PL.StatusToColor(temp.Status);
                } else if (MSSQL.ServiceName == temp.DisplayName)
                {
                    MSSQL.StatusToColor(temp.Status);
                }
            }
        }

        private void toggleButton()
        {
            if(Vale.IsStopped)
            {
                MessageBox.Show("Stopped");
            }
        }

    }
}
