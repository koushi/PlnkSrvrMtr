using System;
using System.Management;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using System.ServiceProcess;
using System.ComponentModel;

namespace PowerlinkServiceFixer
{
    class NotifiableServiceController : INotifyPropertyChanged
    {
        #region INotifyPropertyChanged Memembers
        /// <summary>Occurs when a property value changes.</summary>
        [NonSerialized]
        private PropertyChangedEventHandler fPropertyChanged;
        /// <summary>Occurs when a property value changes.</summary>
        public event PropertyChangedEventHandler PropertyChanged
        {
            add { fPropertyChanged += value; }
            remove { fPropertyChanged -= value; }
        }

        /// <summary>Invoked whenever the effective value of any dependency property on this DependencyObject has been updated. The specific dependency property that changed is reported in the event data.</summary>
        /// <param name="sender">The source of the event data.</param>
        /// <param name="e">The event data that describes the property that changed, as well as old and new values.</param>
        protected virtual void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            PropertyChangedEventHandler handler = fPropertyChanged;
            if (handler != null)
                handler(sender, e);
        }

        /// <summary>Invoked whenever the effective value of any dependency property on this DependencyObject has been updated. The specific dependency property that changed is reported in the event data.</summary>
        /// <param name="property">The event data that describes the property that changed.  <see cref="System.Linq.Expressions.Expression"/> type.</param>
        protected virtual void OnPropertyChanged<T>(Expression<Func<T>> property, T newValue)
        {
            if (property.Body.NodeType == ExpressionType.MemberAccess)
                OnPropertyChanged(this, new PropertyChangedEventArgs<T>((property.Body as MemberExpression).Member.Name, newValue));
        }
        #endregion

        #region Private Backing Fields
        private readonly Guid fId;
        private readonly System.Timers.Timer fRefreshTimer = new System.Timers.Timer();
        private readonly ServiceControllerEx fServiceController;
        private readonly List<ServiceControllerEx> fDependentServiceControllers = new List<ServiceControllerEx>();
        #endregion

        #region Public Properties
        /// <summary>Gets a value indicating whether this instance can stop.</summary>
        /// <value><c>true</c> if this instance can stop; otherwise, <c>false</c>.</value>
        public bool CanStop
        {
            get { return fServiceController.CanStop; }
        }

        /// <summary>Gets the display name.</summary>
        /// <value>The display name.</value>
        public string DisplayName
        {
            get { return fServiceController.DisplayName; }
        }

        /// <summary>Gets the identifier.</summary>
        /// <value>The identifier.</value>
        public Guid Id
        {
            get { return fId; }
        }

        /// <summary>Gets the name of the service.</summary>
        /// <value>The name of the service.</value>
        public string ServiceName
        {
            get { return fServiceController.ServiceName; }
        }

        /// <summary>Gets the status.</summary>
        /// <value>The status.</value>
        public ServiceControllerStatus Status
        {
            get { return fServiceController.Status; }
        }
        #endregion

        #region Constructors
        /// <summary>Initializes a new instance of the <see cref="NotifiableServiceController"/> class.</summary>
        /// <param name="Service">The service.</param>
        public NotifiableServiceController(ServiceController Service) : this(Service, new Guid(), TimeSpan.FromSeconds(.5), null) {; }

        /// <summary>Initializes a new instance of the <see cref="NotifiableServiceController"/> class.</summary>
        /// <param name="Service">The service.</param>
        /// <param name="Id">The identifier.</param>
        public NotifiableServiceController(ServiceController Service, Guid Id) : this(Service, Id, TimeSpan.FromSeconds(.5), null) {; }

        /// <summary>Initializes a new instance of the <see cref="NotifiableServiceController"/> class.</summary>
        /// <param name="Service">The service.</param>
        /// <param name="Id">The identifier.</param>
        /// <param name="Interval">The interval.</param>
        public NotifiableServiceController(ServiceController Service, Guid Id, TimeSpan Interval) : this(Service, Id, Interval, null) {; }

        /// <summary>Initializes a new instance of the <see cref="NotifiableServiceController"/> class.</summary>
        /// <param name="Service">The service.</param>
        /// <param name="Id">The identifier.</param>
        /// <param name="DependentServices">The dependent services.</param>
        public NotifiableServiceController(ServiceController Service, Guid Id, IEnumerable<ServiceController> DependentServices) : this(Service, Id, TimeSpan.FromSeconds(.5), DependentServices) {; }

        /// <summary>Initializes a new instance of the <see cref="NotifiableServiceController"/> class.</summary>
        /// <param name="Service">The service.</param>
        /// <param name="Interval">The interval.</param>
        public NotifiableServiceController(ServiceController Service, TimeSpan Interval) : this(Service, new Guid(), Interval, null) {; }

        /// <summary>Initializes a new instance of the <see cref="NotifiableServiceController"/> class.</summary>
        /// <param name="Service">The service.</param>
        /// <param name="Interval">The interval.</param>
        /// <param name="DependentServices">The dependent services.</param>
        public NotifiableServiceController(ServiceController Service, TimeSpan Interval, IEnumerable<ServiceController> DependentServices) : this(Service, new Guid(), Interval, DependentServices) {; }

        /// <summary>Initializes a new instance of the <see cref="NotifiableServiceController"/> class.</summary>
        /// <param name="Service">The service.</param>
        /// <param name="DependentServices">The dependent services.</param>
        public NotifiableServiceController(ServiceController Service, IEnumerable<ServiceController> DependentServices) : this(Service, new Guid(), TimeSpan.FromSeconds(.5), DependentServices) {; }

        /// <summary>Initializes a new instance of the <see cref="NotifiableServiceController"/> class.</summary>
        /// <param name="Service">The service.</param>
        /// <param name="Id">The identifier.</param>
        /// <param name="Interval">The interval.</param>
        /// <param name="DependentServices">The dependent services.</param>
        /// <exception cref="System.ArgumentNullException">Service;Service cannot be a null value.</exception>
        public NotifiableServiceController(ServiceController Service, Guid Id, TimeSpan Interval, IEnumerable<ServiceController> DependentServices)
        {
            if (Service == null)
                throw new ArgumentNullException("Service", "Service cannot be a null value.");

            fId = Id;
            fServiceController = new ServiceControllerEx(Service.ServiceName);
            if (DependentServices != null)
            {
                foreach (var service in DependentServices)
                    fDependentServiceControllers.Add(new ServiceControllerEx(service.ServiceName));
            }

            fRefreshTimer.Interval = Interval.TotalMilliseconds;
            fRefreshTimer.Elapsed += OnTimerElapsed;
            fRefreshTimer.Start();
        }
        #endregion

        #region Public Methods
        /// <summary>Gets the controllable services.</summary>
        /// <returns>IEnumerable&lt;NotifiableServiceController&gt;.</returns>
        public static IEnumerable<NotifiableServiceController> GetControllableServices()
        {
            var controllers = new List<NotifiableServiceController>();
            foreach (var controller in ServiceController.GetServices())
            {
                try
                {
                    controllers.Add(new NotifiableServiceController(controller));
                }
                catch (Exception) { /* This service is cannot be controlled */; }
            }
            return controllers;
        }

        /// <summary>Restarts this instance.</summary>
        public void Restart()
        {
            try
            {
                if (fServiceController.CanStop && (fServiceController.Status == ServiceControllerStatus.Running || fServiceController.Status == ServiceControllerStatus.Paused))
                {
                    Stop();
                    fServiceController.WaitForStatus(ServiceControllerStatus.Stopped);
                }

                if (fServiceController.Status == ServiceControllerStatus.Stopped)
                {
                    Start();
                    fServiceController.WaitForStatus(ServiceControllerStatus.Running);
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>Starts this instance.</summary>
        public void Start()
        {
            try
            {
                if (fServiceController.Status == ServiceControllerStatus.Stopped)
                {
                    fServiceController.Start();
                    fServiceController.WaitForStatus(ServiceControllerStatus.Running);

                    foreach (var dependent in fDependentServiceControllers)
                    {
                        if (dependent != null && dependent.StartupType != ServiceStartMode.Disabled && dependent.Status == ServiceControllerStatus.Stopped)
                        {
                            dependent.Start();
                            dependent.WaitForStatus(ServiceControllerStatus.Running);
                        }
                    }

                    OnPropertyChanged(() => Status, fServiceController.Status);
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>Stops this instance.</summary>
        public void Stop()
        {
            try
            {
                if (fServiceController.CanStop && (fServiceController.Status == ServiceControllerStatus.Running || fServiceController.Status == ServiceControllerStatus.Paused))
                {
                    foreach (var dependent in fDependentServiceControllers)
                    {
                        if (dependent != null && dependent.StartupType != ServiceStartMode.Disabled && dependent.CanStop && (dependent.Status == ServiceControllerStatus.Running || dependent.Status == ServiceControllerStatus.Paused))
                        {
                            dependent.Stop();
                            dependent.WaitForStatus(ServiceControllerStatus.Stopped);
                        }
                    }

                    fServiceController.Stop();
                    fServiceController.WaitForStatus(ServiceControllerStatus.Stopped);
                    OnPropertyChanged(() => Status, fServiceController.Status);
                }
            }
            catch (Exception)
            {
                throw;
            }
        }
        #endregion

        #region Private Methods
        void OnTimerElapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            var status = fServiceController.Status;
            fServiceController.Refresh();
            if (status != fServiceController.Status)
                OnPropertyChanged(() => Status, fServiceController.Status);
        }
        #endregion

        #region Overriden Methods
        /// <summary>Returns a <see cref="System.String" /> that represents this instance.</summary>
        /// <returns>A <see cref="System.String" /> that represents this instance.</returns>
        public override string ToString()
        {
            return string.Format("{0} ({1})", DisplayName, Status);
        }
        #endregion
    }

    class ServiceControllerEx : ServiceController
    {
        /// <summary>Gets the description.</summary>
        /// <value>The description.</value>
        public string Description
        {
            get
            {
                if (ServiceName != null)
                {
                    // Construct the management path
                    ManagementPath path = new ManagementPath(string.Format("\\\\{0}\\root\\cimv2:Win32_Service.Name='{1}'", MachineName, ServiceName));
                    // Construct the management object
                    ManagementObject obj = new ManagementObject(path);
                    return obj["Description"] != null ? obj["Description"].ToString() : null;
                }
                else
                    return null;
            }
        }

        /// <summary>Gets or sets the type of the startup.</summary>
        /// <value>The type of the startup.</value>
        public ServiceStartMode StartupType
        {
            get
            {
                if (ServiceName != null)
                {
                    // Construct the management path
                    ManagementPath path = new ManagementPath(string.Format("\\\\{0}\\root\\cimv2:Win32_Service.Name='{1}'", MachineName, ServiceName));
                    // Construct the management object
                    ManagementObject obj = new ManagementObject(path);
                    string mode = obj["StartMode"].ToString();
                    switch (mode)
                    {
                        case "Auto":
                            return ServiceStartMode.Automatic;
                        case "Manual":
                            return ServiceStartMode.Manual;
                        case "Disabled":
                        default:
                            return ServiceStartMode.Disabled;
                    }
                }
                else
                    return ServiceStartMode.Disabled;
            }
            set
            {
                if (ServiceName != null)
                {
                    // Construct the management path
                    ManagementPath path = new ManagementPath(string.Format("\\\\{0}\\root\\cimv2:Win32_Service.Name='{1}'", MachineName, ServiceName));
                    // Construct the management object
                    ManagementObject obj = new ManagementObject(path);
                    obj.InvokeMethod("ChangeStartMode", new object[] { value.ToString() });
                }
            }
        }

        /// <summary>Initializes a new instance of the <see cref="ServiceControllerEx"/> class.</summary>
        public ServiceControllerEx() {; }

        /// <summary>Initializes a new instance of the <see cref="T:System.ServiceProcess.ServiceController" /> class that is associated with an existing service on the local computer.</summary>
        /// <param name="name">The name that identifies the service to the system. This can also be the display name for the service.</param>
        public ServiceControllerEx(string name) : base(name) {; }

        /// <summary>Initializes a new instance of the <see cref="T:System.ServiceProcess.ServiceController" /> class that is associated with an existing service on the specified computer.</summary>
        /// <param name="name">The name that identifies the service to the system. This can also be the display name for the service..</param>
        /// <param name="machineName">The computer on which the service resides.</param>
        public ServiceControllerEx(string name, string machineName) : base(name, machineName) {; }
    }

    class PropertyChangedEventArgs<T> : PropertyChangedEventArgs
    {
        /// <summary>Gets the new value of the <paramref name="PropertyName"/> parameter.</summary>
        public T NewValue { get; private set; }

        /// <summary>Initializes a new instance of the <see cref="PropertyChangedEventArgs{T}"/> class.</summary>
        /// <param name="PropertyName">Name of the property.</param>
        public PropertyChangedEventArgs(string PropertyName) : base(PropertyName)
        {
            NewValue = default(T);
        }

        /// <summary>Initializes a new instance of the <see cref="PropertyChangedEventArgs{T}"/> class.</summary>
        /// <param name="PropertyName">Name of the property.</param>
        /// <param name="NewValue">The new value.</param>
        public PropertyChangedEventArgs(string PropertyName, T NewValue) : base(PropertyName)
        {
            this.NewValue = NewValue;
        }
    }
}
