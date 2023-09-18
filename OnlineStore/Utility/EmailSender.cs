using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using System.Configuration;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.UI.Services;

public class EmailSender:IEmailSender
{
    public Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
        return Execute(email, subject, htmlMessage);
    }

    public async Task<Response> Execute(string email, string subject, string body)
    {
        
        // Извлечение API-ключа из переменных окружения
        var apiKey = Environment.GetEnvironmentVariable("SENDGRID_API_KEY");
    
        // Проверка на null
        if (apiKey == null) 
        {
            throw new ArgumentNullException("API Key not found in environment variables.");
        }

        // Инициализация клиента SendGrid
        var client = new SendGridClient(apiKey);

        // Создание нового сообщения
        var from = new EmailAddress("margatuk@proton.me", "OnlineStore");
        var to = new EmailAddress(email, "Example User");
        var plainTextContent = "This is your plain text content"; // или генерируйте из body
        var htmlContent = body;
        var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);

        // Отправка сообщения и возврат ответа
        return await client.SendEmailAsync(msg);
    }
}