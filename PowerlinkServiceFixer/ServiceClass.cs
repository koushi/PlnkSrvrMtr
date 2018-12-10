using System;
using System.ServiceProcess;
using System.ComponentModel;
using System.Diagnostics;

namespace PowerlinkServiceFixer
{
    public class ServiceClass : INotifyPropertyChanged
    {
        private string _serviceName = String.Empty;
        private string _statusColor = "WhiteSmoke";
        public bool _isStopped = false;
        
        public ServiceClass()
        {
            this.ServiceName = "Blank";
            this.StatusColor = "Black";
        }

        public ServiceClass(string srvcName, string color)
        {
            
            this.ServiceName = srvcName;
            this.StatusColor = color;
        }

        public bool IsStopped
        {
            get { return this._isStopped; }
            set
            {
                this._isStopped = value;
                this.NotifyPropertyChanged("IsStopped");
            }
        }
                
        public string ServiceName {
            get { return this._serviceName; }
            set
            {
                if (this._serviceName != null)
                {
                    this._serviceName = value;
                    this.NotifyPropertyChanged("ServiceName");
                }
            }

        }

        public string StatusColor
        {
            get { return this._statusColor; }
            set
            {
                if(this._statusColor != null)
                {
                    this._statusColor = value;
                    this.NotifyPropertyChanged("StatusColor");
                }
            }
        }

        public void StatusToColor(ServiceControllerStatus sc)
        {

            switch(sc)
            {
                case ServiceControllerStatus.Stopped:
                    this.StatusColor = "Red";
                    this.IsStopped = true;
                    break;
                case ServiceControllerStatus.Running:
                    this.StatusColor = "Green";
                    this.IsStopped = false;
                    break;
                case ServiceControllerStatus.StartPending:
                    this.StatusColor = "Yellow";
                    break;
                case ServiceControllerStatus.StopPending:
                    this.StatusColor = "Yellow";
                    break;
               
            }
        }
                       

        public event PropertyChangedEventHandler PropertyChanged;

        public void NotifyPropertyChanged(string propName)
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(propName));
            }
        }

    }
}
