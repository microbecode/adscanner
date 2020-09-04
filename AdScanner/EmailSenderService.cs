﻿using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AdScanner
{
    public class EmailSenderService
    {
        private readonly SendGridClient _client;
        private readonly string _basicReceiver;
        public EmailSenderService(string apiKey, string basicReceiver)
        {
            if (apiKey == null || basicReceiver == null)
            {
                throw new ArgumentException("Email sender doesn't have arguments");
            }
            var client = new SendGridClient(apiKey);
            _client = client;
            _basicReceiver = basicReceiver;
        }

        public async Task Send(string text)
        {
            var split = _basicReceiver.Split(';');
            var msg = new SendGridMessage()
            {
                From = new EmailAddress(split[0], "Pintunena Service"),
                Subject = "New properties found!",
                //PlainTextContent = text,
                HtmlContent = text
            };
            foreach (var receiver in split)
            {
                msg.AddTo(new EmailAddress(receiver));
            }
            
            var response = await _client.SendEmailAsync(msg);
        }
    }
}
