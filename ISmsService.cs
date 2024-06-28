using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Vonage;
using Vonage.Request;
using Vonage.Messaging;

public interface ISmsService
{
    Task SendSmsAsync(string number, string message);
}

public class VonageSmsService : ISmsService
{
    private readonly IConfiguration _configuration;
    private readonly VonageClient _client;

    public VonageSmsService(IConfiguration configuration)
    {
        _configuration = configuration;
        var credentials = Credentials.FromApiKeyAndSecret(
            _configuration["Vonage:ApiKey"],
            _configuration["Vonage:ApiSecret"]
        );
        _client = new VonageClient(credentials);
    }

    public async Task SendSmsAsync(string number, string message)
    {
        var response = await _client.SmsClient.SendAnSmsAsync(new SendSmsRequest
        {
            To = number,
            From = "VonageAPI",
            Text = message
        });

        if (response.Messages[0].Status != "0")
        {
            throw new Exception($"Failed to send SMS: {response.Messages[0].ErrorText}");
        }
    }
}
