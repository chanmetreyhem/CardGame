using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets._Game.Scripts
{
    public class SecuritySurveillanceMain : MonoBehaviour
    {
        private void Start()
        {
            SecuritySurveillanceHub securitySurveillanceHub = new SecuritySurveillanceHub();

            EmployeeNotify employee1 = new EmployeeNotify(new Employee
            {
                Id = 1,
                Name = "KanSa",
                JobTitle = "Employee Service"
            });

            EmployeeNotify employee2 = new EmployeeNotify(new Employee
            {
                Id = 2,
                Name = "Akin Zar",
                JobTitle = "Employee Service"
            });

            SecurityNotify security = new SecurityNotify();

            security.Subscribe(securitySurveillanceHub);
            employee1.Subscribe(securitySurveillanceHub);
            employee2.Subscribe(securitySurveillanceHub);

            ExternalVisitor externalVisitor1 = new ExternalVisitor
            {
                Id = 1,
                Name = "Ty Na",
                CompanyName = "LMS"
            };
            ExternalVisitor externalVisitor2 = new ExternalVisitor
            {
                Id = 2,
                Name = "Ty Na",
                CompanyName = "LUN"
            };


            
            securitySurveillanceHub.ConfirmExternalVisitorEnterBuilding(externalVisitor1.Id,externalVisitor1.Name,externalVisitor1.CompanyName,DateTime.Parse("27 Nov 2025 13:00"),1);
            securitySurveillanceHub.ConfirmExternalVisitorEnterBuilding(externalVisitor2.Id, externalVisitor2.Name, externalVisitor2.CompanyName, DateTime.Parse("27 Nov 2025 14:00"),2);
              
        }
    }

    public class SecuritySurveillanceHub : IObservable<ExternalVisitor>
    {
        private List<ExternalVisitor> _externalVisitors;
        private List<IObserver<ExternalVisitor>> _observers;
        public SecuritySurveillanceHub()
        {
            _externalVisitors = new List<ExternalVisitor>();
            _observers = new List<IObserver<ExternalVisitor>>();
        }

        // member of IObservable
        public IDisposable Subscribe(IObserver<ExternalVisitor> observer)
        {
            Debug.Log($"{nameof(SecuritySurveillanceHub) }: On Subscribe");
            if (!_observers.Contains(observer))
                _observers.Add(observer);
            foreach (ExternalVisitor visitor in _externalVisitors)
                observer.OnNext(visitor);
            return new UnSubscriber<ExternalVisitor>(_observers, observer);
        }

        public void ConfirmExternalVisitorEnterBuilding(int id, string name, string companyName, DateTime visitTime, int employeeContactId)
        {
            ExternalVisitor externalVisitor = new ExternalVisitor()
            {
                Id = id,
                Name = name,
                CompanyName = companyName,
                VisitTime = visitTime,
                InBuild = true,
                EmployeeContactId = employeeContactId
            };
            _externalVisitors.Add(externalVisitor);
            foreach (var observer in _observers)
            {
                observer.OnNext(externalVisitor);
            }
        }
        public void ConfirmExternalVisitorExitsBuilding(int externalId, DateTime exitDateTime)
        {
            var externalVisitor = _externalVisitors.FirstOrDefault(e => e.Id == externalId);
            if (externalVisitor != null)
            {
                externalVisitor.ExitTime = exitDateTime;
                externalVisitor.InBuild = false;
                foreach (var observer in _observers)
                {
                    observer.OnNext(externalVisitor);
                }
            }
        }
        public void BuildingEntryCutoffTimeReached()
        {
            List<ExternalVisitor> inBuildingExternalVisitors = _externalVisitors.Where(e => e.InBuild).ToList();
            if (inBuildingExternalVisitors.Count == 0)
            {
                foreach (var observer in _observers)
                {
                    observer.OnCompleted();
                }
            }
        }
    }

    public abstract class Observer : IObserver<ExternalVisitor>
    {
        IDisposable _cancellation;
        protected List<ExternalVisitor> _externalVisitors = new List<ExternalVisitor>();
        public abstract void OnCompleted();

        public abstract void OnError(Exception error);

        public abstract void OnNext(ExternalVisitor value);


        public void Subscribe(IObservable<ExternalVisitor> provider)
        {
            _cancellation = provider.Subscribe(this);

        }
        public void UnSubscribe()
        {
            _cancellation.Dispose();
            _externalVisitors.Clear(); 
        }
    }

    public class SecurityNotify : Observer
    {


        public override void OnCompleted()
        {

            string heading = $"Security Daily Visitor's Report.";
            Debug.Log(heading);
            Debug.Log($"{new string('=', heading.Length)}");
            foreach (var externalVisitor in _externalVisitors)
            {
                externalVisitor.InBuild = false;
                Debug.Log(externalVisitor.ToString());
            }
        }

        public override void OnError(Exception error)
        {
            throw new NotImplementedException();
        }

        public override void OnNext(ExternalVisitor value)
        {
            var externalVisitor = value ;
            var externalVisitorItem = _externalVisitors.FirstOrDefault(e => e.Id == externalVisitor.Id);

            if(externalVisitorItem == null)
            {
                _externalVisitors.Add(externalVisitor);
                Debug.Log($"Security notification : Visitor visit : {externalVisitor.ToString()}");
                Debug.Log("");
            }
            else
            {
                if(externalVisitorItem.InBuild == false)
                {
                    externalVisitorItem.InBuild = false;
                    externalVisitorItem.ExitTime = externalVisitor.ExitTime;
                    Debug.Log($"Security notification :  Visitor leave : {externalVisitor.ToString()}");
                    Debug.Log("");
                }
            }
        }
    }
    public class Employee : IEmployee
    {
        public int Id {  get; set; }
        public string Name { get; set; }
        public string JobTitle { get; set ; }
    }

    public interface IEmployee
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string JobTitle { get; set; }
    }

    public class EmployeeNotify : Observer
    {
        private IEmployee _employee = null;
        public EmployeeNotify(IEmployee employee)
        {
            _employee = employee;
        }
        #region Member of IObserver
        public override void OnCompleted()
        {
            string heading = $"{_employee.Name} Daily Visitor's Report.";
            Debug.Log(heading);
            Debug.Log($"{new string('=',heading.Length)}");
            foreach(var externalVisitor in _externalVisitors)
            {
                externalVisitor.InBuild = false;
                Debug.Log(externalVisitor.ToString());
            }
           
        }

        public override void OnError(Exception error)
        {
            throw new NotImplementedException();
        }

        public override void OnNext(ExternalVisitor value)
        {
            var externalVisitor = value;
            if(externalVisitor.EmployeeContactId == _employee.Id)
            {
                var externalVisitorItem = _externalVisitors.FirstOrDefault(e => e.EmployeeContactId == externalVisitor.EmployeeContactId);
                if(externalVisitorItem == null)
                {
                    _externalVisitors.Add(externalVisitor);
                    Debug.Log($"{_employee.Name} , your visitor has arrived. ${externalVisitor.ToString()}");
                }
                else
                {
                    if(externalVisitor.InBuild == false)
                    {
                        externalVisitorItem.InBuild = false;
                        externalVisitorItem.ExitTime = externalVisitor.ExitTime;
                    }
                }
            }
        }
        #endregion
       
    }

    public class UnSubscriber<ExternalVisitor> : IDisposable
    {
        private List<IObserver<ExternalVisitor>> _observers;
        private IObserver<ExternalVisitor> _observer;
        public UnSubscriber(List<IObserver<ExternalVisitor>> observers , IObserver<ExternalVisitor> observer)
        {
            _observers = observers;
            _observer = observer;
        }

        // member method of IDisposable
        public void Dispose()
        {
            Debug.Log($"{nameof(UnSubscriber<ExternalVisitor>)}: On Dispose");
            if (_observers.Contains(_observer))
            {
                _observers.Remove(_observer);
            }
        }
    }
  

    public class ExternalVisitor
    {
       

        public int Id { get; set; }
        public string Name { get; set; }
        public string CompanyName {  get; set; }
        public DateTime VisitTime { get; set; }
        public DateTime ExitTime { get; set; }
        public bool InBuild { get; set; }
        public int EmployeeContactId { get; set; }

        public override string ToString()
        {
            return $"Visitor :[Id:{Id},Name:{Name},CompanyName:{CompanyName},VisitTime:{VisitTime.ToString("dd MMM yyyy hh:mm:ss tt")},ExitTime:{ExitTime.ToString("dd MMM yyyy hh:mm:ss tt")}";
        }
    }
}