//using Microsoft.Extensions.Options;
//using Twilio;
//using Twilio.Rest.Api.V2010.Account;
//using Twilio.Types;

//namespace freelance_marketplace_backend.Services
//{
//    public class TwilioSettings
//    {
//        public string AccountSid { get; set; }
//        public string AuthToken { get; set; }
//        public string FromPhone { get; set; }
//    }

//    public interface ITwilioService
//    {
//        Task SendSmsAsync(string toPhone, string message);
//    }

//    public class TwilioService : ITwilioService
//    {
//        private readonly TwilioSettings _settings;

//        public TwilioService(IOptions<TwilioSettings> settings)
//        {
//            _settings = settings.Value;
//            TwilioClient.Init(_settings.AccountSid, _settings.AuthToken);
//        }

//        public async Task SendSmsAsync(string toPhone, string message)
//        {
//            await MessageResource.CreateAsync(
//                to: new PhoneNumber(toPhone),
//                from: new PhoneNumber(_settings.FromPhone),
//                body: message
//            );

//        }
//    }

//}
