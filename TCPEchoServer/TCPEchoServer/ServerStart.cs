﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace TCPEchoServer
{
    public class ServerStart
    {
        private readonly List<ServerThread> _echoServices = new List<ServerThread>();
        private readonly TcpListener _serverSocket;
        private bool _isRunning = true;
        public static string RootCatalog = "../../../../RootFolder";
        public static string DefaultRootCatalog = "../../../../RootFolder";
        private int port = 80;

        public static void Main(string[] args)
        {
            ServerStart serviceStart = new ServerStart();
            Console.WriteLine("CFG port: " + serviceStart.port);
            Console.WriteLine("CFG root: " + RootCatalog);
            serviceStart.Run();
        }

        public ServerStart()
        {
            loadConfigFile();
            _serverSocket = new TcpListener(IPAddress.Parse("127.0.0.1"), port); //65080
        }

        public void Run()
        {
            _serverSocket.Start();

            //Wait for closing...
            Task.Run(new Action(WaitForEnter));

            var checkThread = new Thread(CheckConnections);
            checkThread.Start();

            while (_isRunning)
            {
                try
                {
                    Console.WriteLine("Waiting for a new client...");
                    var connectionSocket = _serverSocket.AcceptTcpClient();
                    Console.WriteLine("Server activated");
                    ServerThread echoService = new ServerThread(connectionSocket);
                    _echoServices.Add(echoService);
//                    Thread thread = new Thread(echoService.DoIt);
//                    thread.Start();
// OR A FACTORY
//                    Task.Factory.StartNew(echoService.DoIt);
                    Task.Run(new Action(echoService.DoIt));
                }
                catch (SocketException socketException)
                {
                    Debug.Write(socketException.StackTrace);
                }
                catch (IOException ioException)
                {
                    Debug.Write(ioException.StackTrace);
                }
            }

            _serverSocket.Stop();
        }

        public void WaitForEnter()
        {
            Console.ReadLine();
            Kill();
        }

        public void Kill()
        {
            _isRunning = false;
            _serverSocket.Server.Close();
        }

        public void CheckConnections()
        {
            while (_isRunning)
            {
                List<ServerThread> echoServicesRemove = new List<ServerThread>();
                foreach (var echoService in _echoServices)
                {
                    if (echoService.ConnectionSocket.Connected == false)
                    {
                        Console.WriteLine("Client " + echoService.ClientNumber + " closed.");
                        echoServicesRemove.Add(echoService);
                    }
                }
                foreach (ServerThread echoService in echoServicesRemove)
                {
                    _echoServices.Remove(echoService);
                }
                Thread.Sleep(100);
            }
        }

        public void loadConfigFile()
        {
            try
            {
                StreamReader sr = new StreamReader(new FileStream("config.cfg", FileMode.Open));

                String line = "";
                do
                {
                    line = sr.ReadLine();
                    String[] splitted = line.Split(':');
                    if (splitted[0].ToUpper() == "PORT")
                    {
                        port = Convert.ToInt32(splitted[1]);

                    }
                    else if (splitted[0].ToUpper() == "ROOT")
                    {
                        RootCatalog = splitted[1];
                    }
                    
                } while (!sr.EndOfStream);
            }
            catch (FileNotFoundException e)
            {
                Console.WriteLine("Config not found: " + e.Message);
            }
        }

        public void saveConfigFile()
        {
            
        }
    }
}