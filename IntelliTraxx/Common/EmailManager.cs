using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Net;
using System.Net.Mail;
using System.Diagnostics;

namespace IntelliTraxx.Common
{
    public static class EmailManager
    {
        public static void SendEmail(List<EmailRecipient> toRecipients, string subject, string body, List<EmailRecipient> ccAddresses)
        {
            try
            {
                var emailAccount = Utilities.GetApplicationSettingValue("EmailAccount");
                var emailAccountName = Utilities.GetApplicationSettingValue("EmailAccountName");
                var emailAccountPassword = Utilities.GetApplicationSettingValue("EmailAccountPassword");
                var emailHost = Utilities.GetApplicationSettingValue("EmailHost");

                SmtpClient smtp = new SmtpClient();

#if(TOLGAPC)
                    emailHost = "smtp.gmail.com";
                    emailAccount = "latatrax@gmail.com";
                    emailAccountName = "LATA Trax Admin";
                    emailAccountPassword = "L@T@2013";

                    smtp.Port = 587;
                    smtp.EnableSsl = true;
#endif
                var fromAddress = new MailAddress(emailAccount, emailAccountName);
                var fromPassword = emailAccountPassword;

                smtp.Host = emailHost;
                smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
                smtp.UseDefaultCredentials = false;
                smtp.Credentials = new NetworkCredential(fromAddress.Address, fromPassword);

                using (var message = new MailMessage
                {
                    IsBodyHtml = true,
                    Subject = subject,
                    Body = body
                })
                {
                    message.From = fromAddress;

                    foreach (var toRecipient in toRecipients)
                    {
                        if (!string.IsNullOrEmpty(toRecipient.Email))
                        {
                            message.To.Add(new MailAddress(toRecipient.Email, toRecipient.Name));
                        }
                    }

                    if (ccAddresses != null)
                    {
                        foreach (var ccAddress in ccAddresses)
                        {
                            if (!string.IsNullOrEmpty(ccAddress.Email))
                            {
                                message.CC.Add(new MailAddress(ccAddress.Email, ccAddress.Name));
                            }
                        }
                    }

                    try
                    {
                        smtp.Send(message);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine("Error sending email " + ex.Message);
                    }
                }
            }

            catch
            {
            }
        }
    }

    public class EmailRecipient
    {
        public string Email { get; set; }
        public string Name { get; set; }
    }
}