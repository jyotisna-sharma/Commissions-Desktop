using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Mail;

namespace MyAgencyVault.ViewModel
{
    static class SendMail
    {
        #region "Local Variable"
        /// <summary>
        /// Local Variable
        /// </summary>
        static string _email;
        static SmtpClient objSmtp;
        static MailMessage oMessage;

        #endregion

        #region "Property"
        /// <summary>
        /// to get mail id
        /// </summary>
        public static string Email
        {

            get
            {
                return _email;
            }
            set
            {
                _email = value;

            }
        }
        #endregion

        #region "Method"


        /// <summary>
        /// Process to send mail
        /// </summary>
        public static void SendEmail()
        {
            try
            {
                objSmtp = new SmtpClient("192.168.0.64");
                oMessage = new MailMessage();
                oMessage.To.Add(Email);
                oMessage.From = new MailAddress("gaurav.sharma@hanusoftware.com");
                oMessage.Body = "Hi how are you";
                objSmtp.Send(oMessage);
            }
            catch { }
        }
        #endregion

    }

}
