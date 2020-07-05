using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using MailKit;
using MailKit.Net.Imap;
using MailKit.Search;
using MailKit.Security;
using MimeKit;
using PricelistMailReader.DataBase;

namespace PricelistMailReader.Mail
{
    public class MailHelper : IDisposable
    {
        private MailBoxOptions mailOptions;

        private ImapClient _client;

        public MailBoxOptions MailOptions { get => mailOptions; set => mailOptions = value; }

        public MailHelper(MailBoxOptions options = null)
        {
            MailOptions = options;
        }

        public async Task Connect(IProgress<ProgressOptions> progress)
        {
            try
            {
                progress.Report(new ProgressOptions {Message = $"Попытка аутентификации для пользователя {MailOptions.UserName}..." });
                _client = new ImapClient();
                var uri = new Uri(
                    $"{MailOptions.ImapServerUrl}:{MailOptions.Port}");
                await _client.ConnectAsync(uri);
                await _client.AuthenticateAsync(MailOptions.UserName, MailOptions.Password);
                progress.Report(new ProgressOptions { Message = "Аутентификация прошла успешно." });
            }
            catch (Exception)
            {
                throw new MailAuthenticationException($"Ошибка аутентификации для пользователя {MailOptions.UserName}. Проверьте правильность ввода логина и пароля. ");
            }
        }

        public async Task<List<PriceItem>> GetMessages(ProviderOptions provider, IProgress<ProgressOptions> progress)
        {
            
            if (_client == null || !_client.IsConnected) throw new ArgumentNullException("Подключение не было выполнено!");

            List<PriceItem> pricelists = null;
            var inbox = _client.Inbox;
            await inbox.OpenAsync(FolderAccess.ReadOnly);
            var allMessages = await inbox.SearchAsync(SearchQuery.All);
            int count = 0;
            progress?.Report(new ProgressOptions {Message = "Чтение писем..."});
            foreach (var message in allMessages)
            {
                count++;
                if (count % 50 == 0 || count == allMessages.Count)
                {
                    progress?.Report(new ProgressOptions
                    {
                        Message = $"{count} писем зачитано",
                        Percents = allMessages.Count / 100 * count,
                        Count = count
                    });
                }

                var m = await inbox.GetMessageAsync(message);
                foreach (var mAttachment in m.Attachments)
                {
                    if (mAttachment.GetAttachmentName().Contains(".csv"))
                    {
                        progress?.Report(new ProgressOptions {Message = "Найдет прайс-лист. Обработка..."});
                        using (var stream = File.Create($"{message}_priceList.csv"))
                        {
                            var attach = (MimePart) mAttachment;
                            attach.Content.DecodeTo(stream);
                            var sr = new StreamReader(stream);
                            sr.DiscardBufferedData();
                            sr.BaseStream.Seek(0, SeekOrigin.Begin); // Возвращаем курсор потока в начало
                            pricelists = CsvReader.GetPriceItemsFromCsv(sr, provider.ColumnDefinitions, progress);
                        }

                        progress?.Report(new ProgressOptions {Message = "Прайс-лист обработан."});
                    }
                }
            }

            return pricelists;
            
		}

        public void Dispose()
        {
            _client?.Disconnect(true);
            _client?.Dispose();
        }
    }

    public class MailBoxOptions
    {
        public string ImapServerUrl { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public int Port { get; set; }
    }

    [Serializable]
    public class MailAuthenticationException : Exception
    {

        public MailAuthenticationException()
        {

        }

        public MailAuthenticationException(string message) : base(message)
        {
        }

        public MailAuthenticationException(string message, Exception inner) : base(message, inner)
        {
        }

        protected MailAuthenticationException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
    }
}
