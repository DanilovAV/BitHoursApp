using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace BitHoursApp.Updater
{
    public class ViewModelBase : INotifyPropertyChanged, IDisposable
    {
        protected virtual void RaisePropertyChanged<T>(Expression<Func<T>> property)
        {
            OnPropertyChanged(ReflectionHelper.GetPropertyNameFromExpression(property));
        }      

        private bool disposed;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {                 
                    // Dispose managed resources here.                 
                    Disposing();
                }

                // Clear unmanaged resources here

                disposed = true;
            }
        }

        protected virtual void Disposing()
        {
        }

        ~ViewModelBase()
        {
            Dispose(false);
        }    
       
        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {           
            PropertyChangedEventHandler handler = this.PropertyChanged;

            if (handler != null)
            {
                var e = new PropertyChangedEventArgs(propertyName);
                handler(this, e);
            }
        }

        #endregion              
    }
}