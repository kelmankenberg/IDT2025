using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace IDT2025
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private bool _isVpnConnected;

        public bool IsVpnConnected
        {
            get => _isVpnConnected;
            set
            {
                if (_isVpnConnected != value)
                {
                    _isVpnConnected = value;
                    Debug.WriteLine($"IsVpnConnected changed to: {_isVpnConnected}");
                    OnPropertyChanged();
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
