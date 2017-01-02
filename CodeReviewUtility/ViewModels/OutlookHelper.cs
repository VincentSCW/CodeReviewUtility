using System;
using System.IO;

namespace CodeReviewUtility.Misc
{
    /// <summary>
    /// Helper class to compose a simple email message for outlook
    /// </summary>
    class OutlookHelper
    {
        public dynamic OutlookApp { get; private set; }

        public static OutlookHelper Create()
        {
            
            try
            {
                return new OutlookHelper();
            }
            catch
            {
            }

            return null;
        }
        
        private OutlookHelper()
        {
            // If this fails with error Retrieving the COM class factory for component with CLSID {0006F03A-0000-0000-C000-000000000046} failed..
            // This is cause by running outlook with different user rights. i.e Run as Admin  or not..
            // run this VS IDE in normal mode.. or close done out look run this session again.. then right click on the outlook icon in task bas and select open..
            this.OutlookApp = Activator.CreateInstance(Type.GetTypeFromProgID("Outlook.Application"));
        }

        public string CurrentUserName
        {
            get
            {
                string name = Environment.UserName;
                try
                {
                    name = this.OutlookApp.Session.Session.CurrentUser.Name;
                    int index = name.LastIndexOf(',');
                    if (index != -1)
                        name = name.Substring(index + 1);
                }
                catch
                {
                }
                return name.Trim();
            }
        }

        public void CreateMessage(string subject, string attactment, string body)
        {
            // Creating a new Outlook Message from the Outlook Application Instance
            // Check to see if there is one we can Reply to
            dynamic msg = this.OutlookApp.CreateItem(0); // Outlook.OlItemType.olMailItem

            msg.Subject = subject;
            msg.Importance = 1; // Outlook.OlImportance.olImportanceNormal;
            msg.BodyFormat = 2; // Outlook.OlBodyFormat.olFormatHTML;

            CreateMessage(msg, subject, attactment, body);
        }

        public void CreateMessage(dynamic mail, string subject, string attactment, string body)
        {
            if (mail == null)
            {
                // Creating a new Outlook Message from the Outlook Application Instance
                mail = this.OutlookApp.CreateItem(0); // Outlook.OlItemType.olMailItem

                mail.Subject = subject;
                mail.Importance = 1; //Outlook.OlImportance.olImportanceNormal;
                mail.BodyFormat = 2; // Outlook.OlBodyFormat.olFormatHTML;
                mail.HTMLBody = body;
            }
            else // existing message, we need to merge the current HTMLBody with this body
            {
                mail.HTMLBody = body + mail.HTMLBody;
            }

            if (!string.IsNullOrEmpty(attactment))
            {
                // Adds Attachment to the Mail Message. 
                // Note: You could add more than one attachment to the mail message. 
                // All you need to do is to declare this relative to the number of attachments you have
                mail.Attachments.Add(attactment, 1 /* Outlook.OlAttachmentType.olByValue */, 1, Path.GetFileName(attactment));
            }

            mail.Display();
        }

        public dynamic FindMessage(string task, string subject)
        {
            dynamic oNS = this.OutlookApp.GetNamespace("mapi");
            dynamic oRecip = oNS.CreateRecipient(this.OutlookApp.Session.CurrentUser.Address);
            oRecip.Resolve();
            if (oRecip.Resolved)
            {
                // See if there is a message i can respond to
                var inbox = oNS.GetSharedDefaultFolder(oRecip, 6 /*Outlook.OlDefaultFolders.olFolderInbox */);

                for (int i = 1; i < inbox.Items.Count; i++)
                {
                    try
                    {
                        dynamic m = inbox.Items[i];
                        if ((m.Subject != null && m.Subject.Contains(task) && m.Subject.EndsWith(subject)))
                            return m.Reply();
                    }
                    catch
                    {
                    }
                }
            }

            return null;
        }
    }
}
