using FireCast.Server.Capture;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FireCast.Server.Network;
using System.Diagnostics;
using System.Threading;
using System.ComponentModel;

namespace FireCast.Server
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly IGraphicsProvider _graphicsProvider;
        private readonly INetworkManager _networkManager;
        private CancellationTokenSource _captureCancellationToken = new CancellationTokenSource();
        private bool isCaptureRunning = false;

        public event PropertyChangedEventHandler? PropertyChanged;

        public MainViewModel()
        {
            this._graphicsProvider = new WindowsGraphicsProvider();
            this._networkManager = new UserDatagramSenderManager("192.168.0.154", 1488);
        }
        public bool IsCaptureRunable
        {
            get
            {
                return !isCaptureRunning;
            }
            set
            {
                this.isCaptureRunning = !value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("IsCaptureRunable"));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("IsCaptureCancellable"));
            }
        }

        public bool IsCaptureCancellable
        {
            get
            {
                return isCaptureRunning;
            }
            set
            {
                this.isCaptureRunning = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("IsCaptureCancellable"));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("IsCaptureRunable"));
            }
        }

        public void CaptureScreen()
        {
            new Task(async () =>
            {
                _captureCancellationToken = new CancellationTokenSource();

                IsCaptureRunable = false;
                while (!_captureCancellationToken.IsCancellationRequested)
                {
                    var rawBytes = _graphicsProvider.GetRawInstantImage();
                    await _networkManager.SendImage(rawBytes);
                    await Task.Delay(10);
                }
                IsCaptureCancellable = false;
            }).Start();
        }
        public void CancelStreaming()
        {
            this._captureCancellationToken.Cancel();
        }
    }
}
