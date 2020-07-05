using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using PricelistMailReader.Console.Configuration;
using PricelistMailReader.DataBase;
using PricelistMailReader.Mail;
using AppContext = PricelistMailReader.DataBase.AppContext;

namespace PricelistMailReader.Console
{
    class Program
    {
        static async Task Main(string[] args)
        {
            try
            {
                await LoadPriceLists();
                System.Console.WriteLine("Для завершения работы нажмите любую кнопку...");

            }
            catch (Exception e)
            {
                System.Console.WriteLine(e);
            }
            finally
            {
                System.Console.ReadKey();
            }

        }

        public static async Task LoadPriceLists()
        {
            ProvidersSection providersSection = (ProvidersSection)ConfigurationManager.GetSection("PricelistProviders");
            MailBoxSection mailSection = (MailBoxSection)ConfigurationManager.GetSection("MailConfig");

            var mailBox = mailSection.MailBoxes.Count > 1
                ? throw new ConfigurationErrorsException("Указано более одного почтового ящика")
                : mailSection.MailBoxes[0];


            System.Console.WriteLine("Чтение прайс листов с электронной  почты.");
            System.Console.WriteLine("Сисок постащиков.");
            foreach (ProviderElement provider in providersSection.Providers)
            {
                System.Console.WriteLine(provider.Name);
            }
            System.Console.WriteLine($"Адрес почтового сервера: {mailBox.ServerUrl}");

            var mailBoxOptions = new MailBoxOptions();
            FillUserCredentials(mailBoxOptions);
            mailBoxOptions.Port = mailBox.Port;
            mailBoxOptions.ImapServerUrl = mailBox.ServerUrl;

            Action<ProgressOptions> progressOptionsFunc = progressOptions => System.Console.WriteLine(progressOptions.Message);
            
            using (var mailHelper = new MailHelper(mailBoxOptions))
            {
                var i = 0;
                var connected = false;
                for (; i < 3 && !connected; i++)
                {
                    try
                    {
                        await mailHelper.Connect(new Progress<ProgressOptions>(progressOptionsFunc));
                        connected = true;
                    }
                    catch (MailAuthenticationException e)
                    {
                        System.Console.WriteLine(e.Message);
                        FillUserCredentials(mailHelper.MailOptions);
                    }
                }
                if (!connected) throw new ApplicationException("Превышен лимит попыток подключений");
                
                var dbh = new DbHelper();
            
                await dbh.ClearPriceListsTable(new Progress<ProgressOptions>(progressOptionsFunc));

                List<Task<List<PriceItem>>> getMessagesTasks = new List<Task<List<PriceItem>>>();

                foreach (ProviderElement providersSectionProvider in providersSection.Providers)
                {
                    var progress = new Progress<ProgressOptions>(progressOptionsFunc);
                    ;
                    getMessagesTasks.Add(mailHelper.GetMessages(new ProviderOptions
                    {
                        Name = providersSectionProvider.Name,
                        ColumnDefinitions = providersSectionProvider.ToDictionary()
                    }, progress));
                }

                await Task.WhenAll(getMessagesTasks).ContinueWith( _ =>
                {
                    foreach (var messagesTask in getMessagesTasks)
                    {
                        dbh.WritePriceLists(messagesTask.Result, new Progress<ProgressOptions>(progressOptionsFunc));
                    }
                });
            }
        }

        private static void FillUserCredentials(MailBoxOptions options)
        {
            System.Console.WriteLine("Введите логин. По завершению нажмите клавишу Enter...");
            var login = System.Console.ReadLine();
            System.Console.WriteLine("Введите пароль. По завершению нажмите клавишу Enter...");
            string password = string.Empty;
            while (true)
            {
                var key = System.Console.ReadKey(true);
                if (key.Key == ConsoleKey.Enter) break;
                if (key.Key == ConsoleKey.Backspace) continue;
                System.Console.Write("*");
                password += key.KeyChar;
            }
            System.Console.WriteLine();
            options.UserName = login;
            options.Password = password;
        }
    }
}

