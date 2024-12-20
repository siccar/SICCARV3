﻿using Microsoft.Extensions.DependencyInjection;
using Siccar.Common.ServiceClients;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SiccarApplicationTests;

namespace Siccar.IntegrationTests
{
    public class WalletTests : IDisposable
    {
        WalletServiceClient _walletServiceClient;
        private TestData testData = new TestData();


        public WalletTests(IServiceProvider serviceProvider, string bearer)
        {
            _walletServiceClient = (WalletServiceClient)serviceProvider.GetService<IWalletServiceClient>();
            if (_walletServiceClient == null)
                throw new Exception("Cannot instanciate service client [WalletServiceClient]");
        }


        public TimeSpan Go_Basic()
        {
            var walletStopwatch = new Stopwatch();

            walletStopwatch.Start();
            
            var wallet1 = _walletServiceClient.CreateWallet(testData.walletName).Result;

            walletStopwatch.Stop();

            if (wallet1 == null)
                return TimeSpan.Zero;
            Console.WriteLine($"Created new wallet : {wallet1.Address}");

            return walletStopwatch.Elapsed;

        }

        public TimeSpan Go_Advanced()
        {
            var walletStopwatch = new Stopwatch();


            return walletStopwatch.Elapsed;

        }

        public TimeSpan Go_Load()
        {
            Console.WriteLine("Creating 1000 Wallets - 10ms Delay");
            var walletStopwatch = new Stopwatch();

            List<string> walletAddresses = new List<string>();

            for (int i = 0; i < 1000; i++)
            {

            }


            return walletStopwatch.Elapsed;

        }

        public void Dispose()
        {
            _walletServiceClient = null;
        }
    }
}
