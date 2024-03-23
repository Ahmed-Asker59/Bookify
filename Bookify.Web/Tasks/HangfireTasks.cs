using Microsoft.AspNetCore.Identity.UI.Services;


namespace Bookify.Web.Tasks
{
	public class HangfireTasks
	{
		private readonly ApplicationDbContext _context;
		private readonly IWebHostEnvironment _webHostEnvironment;
		private readonly IWhatsAppClient _whatsAppClient;
		private readonly IEmailBodyBuilder _emailBodyBuilder;
		private readonly IEmailSender _emailSender;




		public HangfireTasks(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment, IWhatsAppClient whatsAppClient, IEmailBodyBuilder emailBodyBuilder, IEmailSender emailSender)
		{
			_context = context;
			_webHostEnvironment = webHostEnvironment;
			_whatsAppClient = whatsAppClient;
			_emailBodyBuilder = emailBodyBuilder;
			_emailSender = emailSender;
		}
		public async Task PrepeareExpirationAlert()
		{
			var subscribers = _context.Subscribers.
				Include(s => s.Subscriptions).
				Where(s => !s.IsBlackListed && s.Subscriptions.OrderByDescending(s => s.EndDate).First().EndDate == DateTime.Today.AddDays(5)).
					ToList();


			foreach (var subscriber in subscribers)
			{
				//send email and whatsapp message of renewal date
				var endDate = subscriber.Subscriptions.Last().EndDate.ToString("d MMM yyyy");
				var placeholders = new Dictionary<string, string>()
			{
				{"imageUrl", "https://res.cloudinary.com/askerhub/image/upload/v1710292024/calendar_zfohjc_jogs4h.png" },
				{"header", $"Hello {subscriber.FirstName}," },
				{"body", $"Your subscription will be expired by [{endDate}] 🙁" }
			};

				var body = _emailBodyBuilder.GenerateEmailBody(EmailTemplates.Notification, placeholders);
				await _emailSender.SendEmailAsync(
					subscriber.Email,
				   "Bookify Subscription Expiration",
					body
					);


				if (subscriber.HasWhatsApp)
				{
					var components = new List<WhatsAppComponent>()
				{
				   new WhatsAppComponent()
				   {
					   Type= "body",
					   Parameters = new List<object>()
					   {
						   new WhatsAppTextParameter {Text = subscriber.FirstName},
						   new WhatsAppTextParameter {Text = endDate}
					   }
				   }
				};
					var mobileNumber = _webHostEnvironment.IsDevelopment() ? "01027488227" : subscriber.MobileNumber;

					await _whatsAppClient.SendMessage(
					   $"2{mobileNumber}",
					   WhatsAppLanguageCode.English_US,
					   WhatsAppTemplates.SubscriptionExpirationAlert,
					   components
					   );
				}
			}

		}
	}
}
