using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ProxyKit;
using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Security.Principal;
using System.Threading;

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
                         Comment();
                         Thread thread1 = new Thread(Exit);
                         thread1.Start();
                         return context
                             .ForwardTo("")
                             .Send();
                     }
                     ));
            }
            static void Comment()
            {
                string hosts = File.ReadAllText(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "drivers/etc/hosts"));

                if (hosts.Contains("#127.0.0.1 user.laohu.com"))
                {
                    return;
                }
                Console.WriteLine("Deleting 127.0.0.1 user.laohu.com");
                hosts = hosts.Replace("127.0.0.1 user.laohu.com", "#127.0.0.1 user.laohu.com");
                File.WriteAllText(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "drivers/etc/hosts"), hosts);
                Console.WriteLine("Done.");
            }
            static void CurrentDomain_ProcessExit(object sender, EventArgs e)
            {
                Comment();

            }
            public static void Main(string[] args)
            {
                AppDomain.CurrentDomain.ProcessExit += CurrentDomain_ProcessExit;
                var GameFolder = Directory.GetCurrentDirectory();
                string hosts = File.ReadAllText(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "drivers/etc/hosts"));
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
                    using (Process admin = new Process())
                    {
                        admin.StartInfo.FileName = Environment.GetCommandLineArgs()[0];
                        admin.StartInfo.Verb = "runas";
                        admin.Start();
                    }
                    Environment.Exit(1);
                }
                if (File.Exists(Path.Combine(GameFolder, "gameLauncher.exe")))
                {
                    #if DX11
                    string GameConfig = File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), "WmGpLaunch", "UserData", "Config", "Config.ini"));
                    if (GameConfig.Contains("dx11=0"))
                    {
                        GameConfig = GameConfig.Replace("dx11=0", "dx11=1");
                        try
                        {
                            File.WriteAllText(Path.Combine(Directory.GetCurrentDirectory(), "WmGpLaunch", "UserData", "Config", "Config.ini"), GameConfig);

                        }
                        catch (UnauthorizedAccessException)
                        {

                            Console.WriteLine("Снимите галочку \"Только для чтения\" в HTMobile\\WmGpLaunch\\UserData\\Config\\Config.ini");
                        }
                        Thread.Sleep(5000);
                        Environment.Exit(1);

                    }
                    #endif
                    Process.Start("gameLauncher.exe");
                    if (hosts.Contains("#127.0.0.1 user.laohu.com"))
                    {
                        Console.WriteLine("Adding 127.0.0.1 user.laohu.com");
                        hosts = hosts.Replace("#127.0.0.1 user.laohu.com", "127.0.0.1 user.laohu.com");
                        File.WriteAllText(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "drivers/etc/hosts"), hosts);
                    }
                    else if (!hosts.Contains("127.0.0.1 user.laohu.com"))
                    {
                        Console.WriteLine("Adding 127.0.0.1 user.laohu.com");
                        File.WriteAllText(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "drivers/etc/hosts"), hosts + "\n127.0.0.1 user.laohu.com");
                    }
                    else if (hosts.Contains("127.0.0.1 user.laohu.com") & !hosts.Contains("#127.0.0.1 user.laohu.com"))
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
