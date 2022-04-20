using System.ComponentModel;
using System.Workflow.ComponentModel;
using System;

namespace MyAgencyVault.VM.BaseVM
{
    public class BaseViewModel : DependencyObject, INotifyPropertyChanged, IDisposable
    {

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string PropertyName)
        {
            if (this.PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(PropertyName));
            }
        }

        public static void PayorPropertyChanged(string PropertyName)
        {
            if (new BaseViewModel().PropertyChanged != null)
            {
                new BaseViewModel().PropertyChanged(new BaseViewModel(), new PropertyChangedEventArgs(PropertyName));

            }
        }
        #endregion

        public virtual void onDispose()
        {

        }

        #region IDisposable Members

        public void Dispose()
        {
            this.onDispose();

        }

        #endregion
    }
}
