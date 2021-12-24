﻿using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using ProxyKit;
using System.IO;
using System;
using Microsoft.Extensions.Hosting;
using System.Diagnostics;
using System.Net;
using System.Threading;
using System.Security.Principal;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;

namespace ToFStart
{
    internal class Program
    {

        public class Startup
        {
            public void ConfigureServices(IServiceCollection services)
            {
                services.AddProxy();
            }
            public static bool IsAdministrator()
            {
                var identity = WindowsIdentity.GetCurrent();
                var principal = new WindowsPrincipal(identity);
                return principal.IsInRole(WindowsBuiltInRole.Administrator);
            }
            private void Exit()
            {

                Thread.Sleep(2000);
                Environment.Exit(1);
            }
            public void Configure(IApplicationBuilder app)
            {

                app.UseWhen(
                    context => context.Request.Path.ToString().Contains("initConfig"),
                     appInner => appInner.RunProxy(context =>

                     {
                         var config = "{'code':0,'message':'获取成功','result':{'DisableIAAd':0,'ace':null,'bbsSwitch':0,'clientLogSwitch':0,'imei':0,'kefuSwitch':0,'loginOptionConfig':[0],'oneKeySmsLogin':{},'qq_app_id':'100456354','realNameStatus':0,'realNameTips':'按照相关法律和监管要求，请提供您的下述真实有效的身份信息，其中您的境内居民身份证号属于敏感个人信息。\n\n完美世界将为履行法定义务收集下述信息并提供给新闻出版署网络游戏防沉迷实名验证系统，以便完成实名认证。','receipt':1,'request_idfa':0,'rvc':true,'serviceTerms':null,'weibo_app_key':'-1','weixin_app_id':'-1','wmOauth2ClientId':6}}".Replace("'", "\"");
                         Console.WriteLine("initconfig");
                         context.Response.Headers["Transfer-Encoding"] = "identity";
                         context.Response.ContentType = "application/json";
                         context.Response.WriteAsync(config);
                         string text = File.ReadAllText(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "drivers/etc/hosts"));
                         Console.WriteLine("Deleting 127.0.0.1 user.laohu.com");
                         text = text.Replace("127.0.0.1 user.laohu.com", "#127.0.0.1 user.laohu.com");
                         File.WriteAllText(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "drivers/etc/hosts"), text);
                         Console.WriteLine("Done.");
                         Thread thread1 = new Thread(Exit);
                         thread1.Start();
                         return context
                             .ForwardTo("")
                             .Send();
                     }
                     ));
            }
            public static void Main(string[] args)
            {
                var GameFolder = Directory.GetCurrentDirectory();
                string text = File.ReadAllText(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "drivers/etc/hosts"));
                var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("ToFStart.localhost.pfx");
                var bytes = new byte[stream.Length];
                stream.Read(bytes, 0, bytes.Length);
                var cert = new X509Certificate2(bytes, "9ZDPcmskTt3uW3PE");
                var host = new WebHostBuilder()
                    .UseKestrel(serverOptions =>
                    {
                        serverOptions.Listen(IPAddress.Loopback, 443,
                            listenOptions =>
                            {
                                listenOptions.UseHttps(cert);
                            });

                    })
                    .UseUrls(urls: "https://127.0.0.1:443")
                    .UseIISIntegration()
                    .UseStartup<Startup>()
                    .Build();
                if (!IsAdministrator())
                {
                    Console.WriteLine("Запустите программу от имени администратора.");
                    Thread.Sleep(5000);
                    Environment.Exit(1);
                }
                if (File.Exists(Path.Combine(GameFolder, "gameLauncher.exe")))
                {
                    Process.Start("gameLauncher.exe");
                    if (text.Contains("#127.0.0.1 user.laohu.com"))
                    {
                        Console.WriteLine("Adding 127.0.0.1 user.laohu.com");
                        text = text.Replace("#127.0.0.1 user.laohu.com", "127.0.0.1 user.laohu.com");
                        File.WriteAllText(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "drivers/etc/hosts"), text);
                    }
                    else if (!text.Contains("127.0.0.1 user.laohu.com"))
                    {
                        Console.WriteLine("Adding 127.0.0.1 user.laohu.com");
                        File.WriteAllText(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "drivers/etc/hosts"), text + "\n127.0.0.1 user.laohu.com");
                    }
                    else if (text.Contains("127.0.0.1 user.laohu.com") & !text.Contains("#127.0.0.1 user.laohu.com"))
                    {
                        Console.WriteLine("Already added, continue");
                    }
                    host.Start();
                    host.WaitForShutdown();
                }
                else
                {
                    Console.WriteLine("Переместите программу в папку с игрой.");
                    Thread.Sleep(5000);
                }
            }
        }
    }
}