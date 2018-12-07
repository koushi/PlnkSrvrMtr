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
        private ServiceController _srvcController;
        public bool _isVisible = false;
        private NotifiableServiceController _srvcNot;
        
        public ServiceClass()
        {
            this._srvcController = null;
        }

        public ServiceClass(string srvcName)
        {
            this._srvcController = new ServiceController(srvcName);
            this._serviceName = _srvcController.DisplayName;
            this._srvcNot = new NotifiableServiceController(this._srvcController);
            this._srvcNot.PropertyChanged += srvcChanged;
            this.StatusToColor(this._srvcController.Status);
        }

        public bool VisibleFlag
        {
            get { return this._isVisible; }
            set
            {
                this._isVisible = value;
            }
        }

        public ServiceController SrvcController
        {
            get { return this._srvcController; }
            set
            {
                this._srvcController = value;
                //this.NotifyPropertyChanged("SrvcController");
                
                this._serviceName = value.DisplayName;
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
                

        public void UpdateNameFromSrvc()
        {
            this._serviceName = this._srvcController.DisplayName;
        }

        public void StatusToColor(ServiceControllerStatus sc)
        {

            switch(sc)
            {
                case ServiceControllerStatus.Stopped:
                    this.StatusColor = "Red";
                    this.VisibleFlag = true;
                    break;
                case ServiceControllerStatus.Running:
                    this.StatusColor = "Green";
                    this.VisibleFlag = false;
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

        private void srvcChanged(object sender, PropertyChangedEventArgs e)
        {
            this.NotifyPropertyChanged("NotifiableController");
        }
    }
}
