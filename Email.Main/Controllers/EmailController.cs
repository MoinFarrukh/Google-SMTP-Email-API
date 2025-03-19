using System;
using System.Collections.Generic;
using System.Configuration;
using Oracle.ManagedDataAccess.Client;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Mail;
using System.Web.Http;
using System.Web.Http.Results;
using Email.Main.LogFileAddress;
using System.IO;

namespace Email.Main.Controllers
{
    public class EmailController : ApiController
    {
        TextLogFileAddress address = new TextLogFileAddress();
        public string connectionString = "";   // Your DB Connection String
        List<string> sentEmails = new List<string>();
        List<string> failedEmails = new List<string>();

        [HttpPost]
        [Route("api/email/send")]
        public void working()
        {
            File.AppendAllText(address.FilePath, $"Email Sending Job Has Started: {DateTime.Now}\n");
            SendEmail();
            File.AppendAllText(address.FilePath, $"No. of Email Sent In Current Job =>  : {sentEmails.Count}\n");
            File.AppendAllText(address.FilePath, $"Email Sending Job Has Ended: {DateTime.Now}\n");
            File.AppendAllText(address.FilePath, "........................................\n");
        }



        public IHttpActionResult SendEmail()
        {
            try
            {
                var emailList = GetEmailDetails();

                if (emailList == null || emailList.Count == 0)
                    return BadRequest("No pending emails found.");

                string senderEmail = "";    // enter your email from where you want to send emails
                string senderPassword = "";    // enter Google App password

                                               //Go to Google App Passwords.
                                               //Select Mail as the app and Device as your choice.
                                               //Click Generate and copy the generated password.
                                               //Use this password in your SMTP configuration.

                sentEmails.Clear();  // Ensure lists are empty before processing
                failedEmails.Clear();
                foreach (var emailData in emailList)
                {
                    bool isSent = SendEmailToClient(senderEmail, senderPassword, emailData.Recipient, emailData.Subject, emailData.Body, emailData.Id);

                    if (isSent)
                    {
                        sentEmails.Add(emailData.Recipient);
                        File.AppendAllText(address.FilePath, $"Emails Sent Successfully: {string.Join(", ", "Email => " + emailData.Recipient + "ID => " + emailData.Id)}\n");
                    }
                    else
                    {
                        failedEmails.Add(emailData.Recipient);
                        File.AppendAllText(address.FilePath, $"Failed Emails: {string.Join(", ", "Email => " + emailData.Recipient + "ID => " + emailData.Id)}\n");
                    }
                }

                var response = new
                {
                    SentEmails = sentEmails,
                    FailedEmails = failedEmails
                };

                if (failedEmails.Count > 0)
                    return Content(HttpStatusCode.PartialContent, response);
                else
                    //return Ok(response);
                    return Content(HttpStatusCode.OK, new
                    {
                        success = true,
                        message = "Emails Send successfully to following's",
                        data = sentEmails
                    });
            }
            catch (Exception ex)
            {
                File.AppendAllText(address.FilePath, $"Error: {ex.Message}\n");
                return InternalServerError(ex);
            }
        }

        private List<EmailData> GetEmailDetails()
        {
            List<EmailData> emailList = new List<EmailData>();

            using (OracleConnection conn = new OracleConnection(connectionString))
            {
                string query = "SELECT * FROM email_sender WHERE Email_status = 0";
                OracleCommand cmd = new OracleCommand(query, conn);
                conn.Open();

                using (OracleDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())  // Loop through multiple records
                    {
                        EmailData emailData = new EmailData
                        {
                            Id = Convert.ToInt32(reader["SR_NO"]),
                            Recipient = reader["EMAIL"].ToString(),
                            Subject = reader["EMAIL_SUB"].ToString(),
                            Body = reader["EMAIL_BODY"].ToString()
                        };
                        emailList.Add(emailData);
                    }
                }
            }
            return emailList;
        }

        private bool SendEmailToClient(string senderEmail, string senderPassword, string recipient, string subject, string body, int emailId)
        {
            try
            {
                MailMessage mail = new MailMessage
                {
                    From = new MailAddress(senderEmail),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true
                };
                mail.To.Add(recipient);

                SmtpClient smtp = new SmtpClient
                {
                    Host = "smtp.gmail.com",
                    Port = 587,
                    EnableSsl = true,
                    Credentials = new NetworkCredential(senderEmail, senderPassword)
                };

                smtp.Send(mail);
                UpdateEmailStatus(emailId);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error sending email: " + ex.Message);
                return false;
            }
        }

        private void UpdateEmailStatus(int emailId)
        {
            using (OracleConnection conn = new OracleConnection(connectionString))
            {
                string updateQuery = "UPDATE email_sender SET Email_status = 1 WHERE SR_NO  = :emailId";

                using (OracleCommand cmd = new OracleCommand(updateQuery, conn))
                {
                    cmd.Parameters.Add(new OracleParameter("emailId", emailId));

                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
        }

        private class EmailData
        {
            public int Id { get; set; }
            public string Recipient { get; set; }
            public string Subject { get; set; }
            public string Body { get; set; }
        }
    }
}








//test code start
//EmailData emailData = null;

//emailData = new EmailData
//{
//    Recipient = "muhammadmoin604@gmail.com",
//    Subject = "Testing API",
//    Body = "Api tested successfully"
//};
// test code end