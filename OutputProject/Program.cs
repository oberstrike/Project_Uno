﻿
using SQLiteApplication.Tools;
using SQLiteApplication.UserData;
using SQLiteApplication.Web;
using System;
using System.Collections.Generic;
using System.Net.Mail;
using System.Threading.Tasks;

namespace OutputProject
{
    internal class Program
    {
        public static List<string> buildOrder = new List<string>() { "snob" };


        /*
        Neue Anforderungen:
            1. Exceptionhandling Outsourcen
            2. Firefox window am Ende nicht schließen, sondern nur ausloggen und auf google gehen.
            3. Neue Configurationsmöglichkeiten:
                - Farm/Ausbauen ein und ausstellen (CHECK)
                - Buildorder als String array darstellen (CHECK)
                - Wartezeiten zwischen den Aktionen
        */

        public static void Main(string[] args)
        {
            Configuration config = new ConfigurationManager(@"Config.json").Configuration;
            var randomizer = new Randomizer();
            int errorCount = 0;
            while (errorCount == 0)
            {
                Client.Print("Starte Build Routine 1.0");
                Client.Print(config.User);

                Client client = Factory.GetAdvancedClient(config);
                client.Connect();
                try
                {

                    client.Login();

                    client.Update();



                    TimeSpan? timeSpan = null;
                    client.Logout();


                    if (config.Build)
                    {
                        timeSpan = client.GetBestTimeToCanBuild();
                        if (!timeSpan.HasValue)
                        {
      
                            TimeSpan? timeForQueue = client.GetBestTimeForQueue();
                            if (!timeForQueue.HasValue)
                                timeSpan = new TimeSpan(new Random().Next(2, 3), new Random().Next(1, 20), new Random().Next(1, 20));
                        }
                        timeSpan = timeSpan.Value.Add(new TimeSpan(0, 1, 0));
                    }


                    timeSpan = new TimeSpan(0,randomizer.Next(config.MinimumTimeToWait, config.MaximumTimeToWait), randomizer.Next(0, 60));
                    Client.Print("Schlafe für " + timeSpan);
                    Client.Print("Schlafe bis " + DateTime.Now.Add(timeSpan.Value));
                    Task.Delay(timeSpan.Value).Wait();


                }
                catch (Exception e)
                {
                    ExceptionHandling(e, client);
     
                }

            }
        }

        public static void ExceptionHandling(Exception e, Client client)
        {
            if (e.Message.Contains("SecurityError"))
            {
                SendEmail(e.Message);
                return;
            }
            else if (e.Message.Contains("imeout") | e.Message.Contains("Tried to run"))
            {
                Client.Print("Upps there was a mistake.");
                Client.Print(e.Message);
                client.Close();
            }
            else
            {
                Client.Print(e.Message);
                client.Close();
            }
        }

        private static void SendEmail(string error)
        {
            MailMessage mail = new MailMessage();
            mail.From = new MailAddress("g.i.g.a.n.t@hotmail.de"); //Absender 
            mail.To.Add("tribal-wars.bot@gmx.de"); //Empfänger 
            mail.Subject = "BOT-SCHUTZ";
            mail.Body = $"{DateTime.Now} + \n {error}";

            SmtpClient client = new SmtpClient("smtp.live.com", 25); //SMTP Server von Hotmail und Outlook. 

            try
            {
                client.Credentials = new System.Net.NetworkCredential("g.i.g.a.n.t@hotmail.de", "Pianohits2.");//Anmeldedaten für den SMTP Server 

                client.EnableSsl = true; //Die meisten Anbieter verlangen eine SSL-Verschlüsselung 

                client.Send(mail); //Senden 

                Console.WriteLine("E-Mail wurde versendet");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Fehler beim Senden der E-Mail\n\n{0}", ex.Message);
            }
            Console.ReadKey();
        }
    }
}
