using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reactive.Disposables;
using System.Text;
using System.Threading.Tasks;
using BitHoursApp.Common.Reflection;
using ReactiveUI;

namespace BitHoursApp.Mvvm
{
    public abstract class ViewModelBase : ReactiveObject
    {
        protected virtual void RaisePropertyChanged<T>(Expression<Func<T>> property)
        {
            this.RaisePropertyChanged(ReflectionHelper.GetPropertyNameFromExpression(property));
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
                    DisposeList.Dispose();
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

        private CompositeDisposable disposeList = new CompositeDisposable();
        protected CompositeDisposable DisposeList
        {
            get { return disposeList; }
        }

    }
}
