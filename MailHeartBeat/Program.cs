using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Limilabs.Client.IMAP;
using Limilabs.Client.SMTP;
using Limilabs.Mail;
using Limilabs.Mail.Fluent;
using Limilabs.Mail.Headers;
using Limilabs.Mail.MIME;
using System.Threading;
using log4net;
using log4net.Repository.Hierarchy;
using Limilabs.Client;


[assembly: log4net.Config.XmlConfigurator(Watch = true)]
namespace MailHeartBeat
{
    internal class HeartBeatListener
    {
        private static readonly ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
	
        private const string Server = "imap-mail.outlook.com"; //"imap.server.com";
        private const string SMTPServer = "smtp.live.com";
        private const string User = "h_n_davis@hotmail.com";
        private const string Password = "YaelEtta3";
        private List<string> mailMonitor = new List<string>() {"hndavis983@outlook.com", "heddydavis@hotmail.com"};
        private const string mailSubject = "fsdavis@bellsouth.net";
        private Dictionary<DateTime, long> _messageTimes = new Dictionary<DateTime, long>();
        private const int initialCheckWaitTime = 12;
        private int CheckWaitTime;

        private static ICriterion _searchExp = Expression.And(
            // Expression.From("Frederick Davis"),
            Expression.From(mailSubject),
            Expression.SentSince(DateTime.Now.AddDays(-2)));

        private static void Main(string[] args)
        {

            HeartBeatListener hbl = new HeartBeatListener(); 
            hbl.Run();
        }

        private void Run()
        {
            log.Info("Staring MailHeartBeat for" + mailSubject);
            CheckWaitTime = initialCheckWaitTime;
            while (true)
            {


                GetHeartBeatMessages();
                if (CheckHeartBeat())
                {
                    CheckWaitTime = initialCheckWaitTime;
                }
                else
                {
                    List<IMail> resp = new List<IMail>();
                    resp.Add(GenerateInterogativeMessage(mailSubject));
                    resp.Add(GenerateBadHeartBeatResponse());
                    sendStatusUpdate(resp);
                    CheckWaitTime = 1;
                }
                Thread.Sleep(60000 * 60 * CheckWaitTime);
            }
        }

        private IMail GenerateInterogativeMessage(string mailAddress)
        {
            MailBuilder builder = new MailBuilder();
            builder.To.Add(new MailBox(mailAddress));
            builder.From.Add(new MailBox("Do-not-Reply@MailHeartBeat.GOV"));
            builder.Subject = "Alert from Mail Heart Beat ( DACSYS inc)";
            builder.PriorityHigh();
            builder.Text = "You have not sent " + mailMonitor.First() + " in 24 hours. " +
                           "A mail message will be sent every hour until you respond.";

            return builder.Create();
        }
        private void GetHeartBeatMessages()
        {

            using (Imap imap = new Imap())
            {
                //imap.Connect(_server);                              // Use overloads or ConnectSSL if you need to specify different port or SSL.
                imap.ConnectSSL(Server);
                imap.Login(User, Password); // You can also use: LoginPLAIN, LoginCRAM, LoginDIGEST, LoginOAUTH methods,

                imap.SelectInbox();
                List<long> uids = imap.Search(_searchExp);
                foreach (long uid in uids)
                {
                    IMail email = new MailBuilder().CreateFromEml( // Download and parse each message.
                        imap.GetMessageByUID(uid));
                    if (email.Date != null)
                        _messageTimes[(DateTime) email.Date] = uid;

                }

            }

        }

        private bool CheckHeartBeat()
        {

            var lastMessage = _messageTimes.Where(mt => mt.Key > DateTime.Now - TimeSpan.FromHours(24));
            if (!lastMessage.Any())
            {
                IMail email = Mail
                    .Html(@"<img src=""cid:lemon@id"" align=""left"" /> This is simple 
                        <strong>Warning</strong> with an image and attachment")
                    .Subject("Missing email")
                    .From(new MailBox("h_n_davis@outlook.com", "DAC int"))
                    .To(new MailBox("hndavis983@outlook.com", "Howard Davis"))
                    .Create();
                return false;

            }
            return true;
            
        }

        private IMail GenerateBadHeartBeatResponse()
        {
            MailBuilder builder = new MailBuilder();
            foreach (var address in mailMonitor)
            {
                builder.To.Add(new MailBox(address));
            }

            builder.From.Add(new MailBox("Do-not-Reply@MailHeartBeat.GOV"));
            builder.Subject = "Alert from Mail Heart Beat";
            builder.PriorityHigh();
            builder.Text = "Mail has not been received from " + mailMonitor + " in 24 hours";
            return builder.Create();
        }
        void sendStatusUpdate(IEnumerable<IMail> mails)
        {

         using (Smtp smtp = new Smtp())              // Now connect to SMTP server and send it
            {
                try
                {

                    smtp.Connect(SMTPServer);                  // Use overloads or ConnectSSL if you need to specify different port or SSL.
                    //smtp.ConnectSSL(SMTPServer);
                    smtp.UseBestLogin(User, Password);    // You can also use: Login, LoginPLAIN, LoginCRAM, LoginDIGEST, LoginOAUTH methods,
                    // or use UseBestLogin method if you want Mail.dll to choose for you.

                    foreach (var  mail in mails)
                    {
                        log.Info("Sending mail to :" + mail.To);
                        var result = smtp.SendMessage(mail);
                        log.Info(result.Status);
                    }

                    smtp.Close();
                }
                catch(Exception mailException)
                {
                    log.Error(mailException);
                }
            }
            return;
        }
    }
}

