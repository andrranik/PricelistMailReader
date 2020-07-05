using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MimeKit;

namespace PricelistMailReader.Mail
{
    static class MailKitExtention
    {
        public static string GetAttachmentName(this MimeEntity attachment)
        {
            var subStr = attachment.Headers["Content-Type"];
            subStr = subStr.Split(';').Single(x => x.Contains("name=\""));
            return subStr.Substring(subStr.IndexOf("\"") + 1, subStr.LastIndexOf("\"") - subStr.IndexOf("\""));

        }
    }
}
