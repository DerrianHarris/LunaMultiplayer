﻿using Server.Context;
using Server.Log;
using Server.Settings.Structures;
using Server.Web.Handlers;
using Server.Web.Structures;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using uhttpsharp;
using uhttpsharp.Handlers;
using uhttpsharp.Handlers.Compression;
using uhttpsharp.Listeners;
using uhttpsharp.RequestProviders;

namespace Server.Web
{
    /// <summary>
    /// This class controls the information shown in JSON format when you go to http://yourip:8900
    /// </summary>
    public class WebServer
    {
        private static readonly HttpServer Server = new HttpServer(new HttpRequestProvider());
        public static readonly ServerInformation ServerInformation = new ServerInformation();

        /// <summary>
        /// Starts the web server
        /// </summary>
        public static void StartWebServer()
        {
            if (WebsiteSettings.SettingsStore.EnableWebsite)
            {
                try
                {
                    Server.Use(new TcpListenerAdapter(new TcpListener(IPAddress.Any, WebsiteSettings.SettingsStore.Port)));

                    Server.Use(new ExceptionHandler());
                    Server.Use(new CompressionHandler(DeflateCompressor.Default, GZipCompressor.Default));
                    Server.Use(new FileHandler());
                    Server.Use(new HttpRouter().With(string.Empty, new RestHandler<ServerInformation>(new ServerInformationRestController(), JsonResponseProvider.Default)));

                    Server.Start();
                }
                catch (Exception e)
                {
                    LunaLog.Error($"Could not start web server. Details: {e}");
                }
            }
        }

        /// <summary>
        /// Starts the web server
        /// </summary>
        public static void StopWebServer()
        {
            if (WebsiteSettings.SettingsStore.EnableWebsite)
            {
                try
                {
                    Server.Dispose();
                }
                catch (Exception e)
                {
                    LunaLog.Error($"Could not stop web server. Details: {e}");
                }
            }
        }

        /// <summary>
        /// Refresh the server information
        /// </summary>
        public static async void RefreshWebServerInformation()
        {
            if (WebsiteSettings.SettingsStore.EnableWebsite)
            {
                while (ServerContext.ServerRunning)
                {
                    ServerInformation.Refresh();
                    await Task.Delay((int)TimeSpan.FromMilliseconds(WebsiteSettings.SettingsStore.RefreshIntervalMs).TotalMilliseconds);
                }
            }
        }
    }
}