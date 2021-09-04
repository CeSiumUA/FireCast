﻿using FireCast.Server.Capture;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FireCast.Server.Network;

namespace FireCast.Server
{
    public class MainViewModel
    {
        private readonly IGraphicsProvider _graphicsProvider;
        private readonly INetworkManager _networkManager; 
        public MainViewModel()
        {
            this._graphicsProvider = new WindowsGraphicsProvider();
            this._networkManager = new UserDatagramSenderManager();
        }
        public async Task CaptureScreen()
        {
            while (true)
            {
                var rawBytes = await _graphicsProvider.GetRawInstantImage();
                await _networkManager.SendImage(rawBytes);
            }
        }
    }
}