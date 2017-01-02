using System;
using System.Collections.Generic;
using System.Net.Mail;
using System.ComponentModel;
using System.Net;

namespace Utility.Library.Misc
{
    public class EmailClient
    {
        public string SmtpHost { get; private set; }
        public int Port { get; private set; }

        public string From { get; private set; }
        public NetworkCredential Credentials { get; private set; }
        
        private EmailClient()
        {
        }

        public static EmailClient GetGMailClient(string userId, string password)
        {
            return new EmailClient()
            {
                Credentials = new NetworkCredential(userId, password),
                SmtpHost = "smtp.gmail.com",
                Port = 587,
                From = userId
            };
        }

        public static EmailClient GetClient(string from, NetworkCredential credentials, string host, int port)
        {
            return new EmailClient()
            {
                Credentials = credentials,
                SmtpHost = host,
                Port = port,
                From = from
            };
        }

        private SmtpClient GetSmtpClient()
        {
            return new SmtpClient(this.SmtpHost, this.Port)
            {
               // EnableSsl = true,
                //UseDefaultCredentials = false,
                Credentials = this.Credentials,
              //  DeliveryMethod = System.Net.Mail.SmtpDeliveryMethod.Network,
                Timeout = int.MaxValue
            };
        }

        public void Send(string message, bool isBodyHtml, string subject, string sendTo, IEnumerable<string> attachtments = null, bool sendAsync = false)
        {
            this.Send(message, isBodyHtml, subject, new string[] { sendTo }, attachtments, sendAsync);
        }

        public void Send(string message, bool isBodyHtml, string subject, IEnumerable<string> sendTo, IEnumerable<string> attachtments = null, bool sendAsync = false)
        {
            this.Send(
                new MailMessage()
                {
                    Body = message,
                    From = new MailAddress(this.From),
                    Priority = MailPriority.Normal,
                    Subject = subject,
                    IsBodyHtml = isBodyHtml,
                    BodyEncoding = isBodyHtml ? System.Text.Encoding.Default : System.Text.Encoding.UTF8
                },
                sendTo, attachtments, sendAsync);
        }

        private void Send(MailMessage mm, IEnumerable<string> sendTo, IEnumerable<string> attachtments = null, bool sendAsync = false)
        {
            try
            {
                foreach (string m in sendTo)
                    mm.To.Add(new MailAddress(m));

                if (attachtments != null)
                {
                    foreach (string fileName in attachtments)
                    {
                        if (System.IO.File.Exists(fileName))
                            mm.Attachments.Add(new Attachment(fileName));
                    }
                }

                SmtpClient client = this.GetSmtpClient();

                // Set the method that is called back when the send operation ends.
                client.SendCompleted += new SendCompletedEventHandler(SendCompletedCallback);

                if (sendAsync)
                    client.SendAsync(mm, sendTo);
                else
                    client.Send(mm);
            }
            catch (Exception e)
            {
                foreach (string m in sendTo)
                {
                    System.Diagnostics.Debug.WriteLine(string.Format("Sending Email Failed for {0} with errror {1}", m, e.Message));
                }
            }
        }

        private void SendCompletedCallback(object sender, AsyncCompletedEventArgs e)
        {
            // Get the unique identifier for this asynchronous operation.
            string[] token = (string[])e.UserState;

            if (e.Cancelled)
            {
                foreach (string s in token)
                {
                    System.Diagnostics.Debug.WriteLine(string.Format("[{0}] Send canceled.", s));
                }
            }
            if (e.Error != null)
            {
                foreach (string s in token)
                {
                    System.Diagnostics.Debug.WriteLine(string.Format("Message sent to {0} failed with {1}", s, e.Error.ToString()));
                }
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("Message sent.");
            }
        }

    }
}
