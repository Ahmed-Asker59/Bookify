using Bookify.Web.Data.Migrations;
using Humanizer;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Text.Encodings.Web;
using WhatsAppCloudApi;
using WhatsAppCloudApi.Services;

namespace Bookify.Web.Controllers
{
    [Authorize(Roles = AppRoles.Reception)]
    public class SubscribersController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IDataProtector _dataProtector;
        private readonly IMapper _mapper;
        private readonly IWhatsAppClient _whatsAppClient;
        private readonly IImageService _imageService;
        private readonly IEmailBodyBuilder _emailBodyBuilder;
        private readonly IEmailSender _emailSender;




        public SubscribersController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment, IDataProtectionProvider dataProtector, IMapper mapper, IWhatsAppClient whatsAppClient, IImageService imageService, IEmailBodyBuilder emailBodyBuilder, IEmailSender emailSender)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
            _dataProtector = dataProtector.CreateProtector("MySecureKey");
            _mapper = mapper;
            _whatsAppClient = whatsAppClient;
            _imageService = imageService;
            _emailBodyBuilder = emailBodyBuilder;
            _emailSender = emailSender;
        }

        public  IActionResult Index()
        { 
            return View();
        }

        [HttpGet]
		public IActionResult Create()
		{
			var viewModel = PopulateViewModel();
			
			return View("Form", viewModel);
		}

       [HttpPost]
       [ValidateAntiForgeryToken]
       public async Task<IActionResult> Create(SubscriberFormViewModel model)
        {
			if (!ModelState.IsValid)
				return BadRequest();

			var subscriber = _mapper.Map<Subscriber>(model);

            var imgExtension = Path.GetExtension(model.Image!.FileName);
            var imgName = $"{Guid.NewGuid()}{imgExtension}";
           var (isUploaded, errorMessage) = await _imageService.UploadAsync(model.Image, imgName, "/Images/Subscribers", hasThumbNail: true);
            if (!isUploaded)
            {
                ModelState.AddModelError("Image", errorMessage!);
                return View("Form", PopulateViewModel(model));

            }


            subscriber.ImagePath = $"/Images/Subscribers/{imgName}";
            subscriber.ImageThumbnailPath = $"/Images/Subscribers/thumb/{imgName}";
            subscriber.CreatedById = GetUserId();

            //add subscription to the subsriber

            Subscription subscription = new()
            {
                CreatedById = subscriber.CreatedById,
                CreatedOn = subscriber.CreatedOn,
                StartDate = DateTime.Today,
                EndDate = DateTime.Today.AddYears(1)
            };

            subscriber.Subscriptions.Add(subscription);
            _context.Subscribers.Add(subscriber);
            _context.SaveChanges();

            //send welcome email
            var placeholders = new Dictionary<string, string>()
                {
                    {"imageUrl", "https://res.cloudinary.com/askerhub/image/upload/v1704823333/reset_password_gp0irt.png" },
                    {"header", $"Welcome {model.FirstName}," },
                    {"body", "Thanks for joining Bookify 🤩" },    

                };
            
            var body = _emailBodyBuilder.GenerateEmailBody(EmailTemplates.Notification, placeholders);
            await _emailSender.SendEmailAsync(
              model.Email,
              "Welcome to Bookify",
              body
              );

            //send welcome  message using whatsapp
            if (model.HasWhatsApp)
            {
                var components = new List<WhatsAppComponent>()
            {
                new WhatsAppComponent
                {
                   Type = "body",
                   Parameters = new List<object>()
                   {
                      new WhatsAppTextParameter {Text= model.FirstName}
                   }
                }
                };

                var mobileNumber = _webHostEnvironment.IsDevelopment() ? "01027488227" : model.MobileNumber;

                await _whatsAppClient.
                SendMessage($"2{mobileNumber}", WhatsAppLanguageCode.English_US,
                WhatsAppTemplates.WelcomeMessage, components);
            }
            var subscriberId = _dataProtector.Protect(subscriber.Id.ToString());
            return RedirectToAction(nameof(Details), new { id = subscriberId });
        }

        [HttpGet]
        public IActionResult Edit(string id)
        {
            var subscriberId = int.Parse(_dataProtector.Unprotect(id));

            var subscriber = _context.Subscribers.Find(subscriberId);
            if (subscriber is null)
                return NotFound();

            var viewModel = _mapper.Map<SubscriberFormViewModel>(subscriber);
            //populate with all governates
            viewModel = PopulateViewModel(viewModel);
            viewModel.Key = id;

            return View("Form", viewModel);

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(SubscriberFormViewModel model)
        {
            
           if (!ModelState.IsValid)
               return View("Form",PopulateViewModel(model));

            var subscriberId = int.Parse(_dataProtector.Unprotect(model.Key!));
            var subscriber = _context.Subscribers.Find(subscriberId);

            if (subscriber is null)
                return NotFound();

            if(model.Image is not null)
            {
                //if there is already an image, then delete it
                if (!string.IsNullOrEmpty(subscriber.ImagePath))
                    _imageService.DeleteImage(subscriber.ImagePath, subscriber.ImageThumbnailPath);
                var imgExtension = Path.GetExtension(model.Image.FileName);
                var imgName = $"{Guid.NewGuid()}{imgExtension}";
                var imgPath = "/Images/Subscribers";
                var (isUploaded, errorMessage) = await _imageService.UploadAsync(model.Image, imgName, imgPath, hasThumbNail: true);

                if (!isUploaded)
                {
                    ModelState.AddModelError("Image", errorMessage!);
                    return View("Form", PopulateViewModel(model));
                }

                model.ImagePath = $"{imgPath}/{imgName}";
                model.ImageThumbnailPath = $"{imgPath}/thumb/{imgName}";
            }
            else if (!string.IsNullOrEmpty(subscriber.ImagePath)){
                model.ImagePath = subscriber.ImagePath;
                model.ImageThumbnailPath= subscriber.ImageThumbnailPath;
            }
           subscriber = _mapper.Map(model, subscriber);
           subscriber.LastUpdatedById = GetUserId();
           subscriber.LastUpdatedOn = DateTime.Now;

            _context.SaveChanges();

            return RedirectToAction(nameof(Details), new { id = model.Key});

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Search(SearchFormViewModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var queryValue = model.Value;
            var subscriber = _context.Subscribers.SingleOrDefault(
                s => s.Email == queryValue || s.MobileNumber == queryValue
                || s.NationalId == queryValue
                );

            var viewModel = _mapper.Map<SubscriberSearchResultViewModel>(subscriber);

            if(subscriber is not null)
                viewModel.Key = _dataProtector.Protect(subscriber.Id.ToString());
            return PartialView("_Result", viewModel);
        }

        public IActionResult Details(string id)
        {
            var subscriberId = int.Parse(_dataProtector.Unprotect(id));
            var subscriber = _context.Subscribers
                .Include(s => s.Governorate)
                .Include(s => s.Area)
                .Include(s => s.Subscriptions)
                .SingleOrDefault(s => s.Id == subscriberId);

            if (subscriber is null)
                return NotFound();

            var viewModel = _mapper.Map<SubscriberViewModel>(subscriber);
            viewModel.Key = id;

            return View(viewModel);
                
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RenewSubscription(string skey)
        {
            //decrept id
            var subscriberId = int.Parse(_dataProtector.Unprotect(skey)); 

            var subscriber = _context.Subscribers
                .Include(s => s.Subscriptions)
                .SingleOrDefault(s => s.Id == subscriberId);

            if(subscriber is null)
                return NotFound();

            if(subscriber.IsBlackListed)
                return BadRequest();

            Subscription lastSubscription = subscriber.Subscriptions.Last();
            var startDate = lastSubscription.EndDate < DateTime.Today ? DateTime.Today: lastSubscription.EndDate.AddDays(1);

			Subscription newSubscription = new()
            {
                CreatedById = GetUserId(),
                CreatedOn = DateTime.Now,
                StartDate = startDate,
                EndDate = startDate.AddYears(1)
            };

            
            subscriber.Subscriptions.Add(newSubscription);
            _context.SaveChanges();
            //send email and whatsapp message of renewal date
            var placeholders = new Dictionary<string, string>()
            {
			    {"imageUrl", "https://res.cloudinary.com/askerhub/image/upload/v1704823333/reset_password_gp0irt.png" },
			    {"header", $"Hello {subscriber.FirstName}," },
			    {"body", $"Your subscription has been successfully renewed" },
                {"body2" , $"Renewal Date: [{newSubscription.EndDate.ToString("d MMM yyyy")} 🎉🎉]"}
			};

            var body = _emailBodyBuilder.GenerateEmailBody(EmailTemplates.RenewSubscription, placeholders);

            await _emailSender.SendEmailAsync(
                subscriber.Email,
               "Bookify Subscription Renewal",
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
                           new WhatsAppTextParameter {Text = newSubscription.EndDate.ToString("d MMM yyyy")}
                       }
                   }
                };
				var mobileNumber = _webHostEnvironment.IsDevelopment() ? "01027488227" : subscriber.MobileNumber;

				await _whatsAppClient.SendMessage(
					$"2{mobileNumber}",
					WhatsAppLanguageCode.English_US,
					WhatsAppTemplates.SubscriptionRenew,
					components
					);
			}

            

			var viewModel = _mapper.Map<SubscriptionViewModel>(newSubscription); 

            return PartialView("_SubscriptionRow",viewModel);
        }


		[AjaxOnly]
		public IActionResult GetAreas(int governorateId)
		{
			var areas = _context.Areas.Where(a => a.GovernorateId == governorateId && !a.IsDeleted)
                .OrderBy(a => a.Name)
				.ToList();

			return Ok(_mapper.Map<IEnumerable<SelectListItem>>(areas));
		}

		public IActionResult AllowNationalId(SubscriberFormViewModel model)
		{
            var subscriberId = 0;
            if (!string.IsNullOrEmpty(model.Key))
                subscriberId = int.Parse(_dataProtector.Unprotect(model.Key));

            var subscriber = _context.Subscribers.SingleOrDefault(s => s.NationalId == model.NationalId);

			var isAllowed = subscriber is null || subscriber.Id.Equals(subscriberId);
			return Json(isAllowed);
		}

        public IActionResult AllowEmail(SubscriberFormViewModel model)
        {

            var subscriberId = 0;
            if (!string.IsNullOrEmpty(model.Key))
                subscriberId = int.Parse(_dataProtector.Unprotect(model.Key));
            var subscriber =  _context.Subscribers.SingleOrDefault(s => s.Email == model.Email);

            var isAllowed = subscriber is null || subscriber.Id.Equals(subscriberId);
            return Json(isAllowed);
        }

        public IActionResult AllowMobileNumber(SubscriberFormViewModel model)
        {
            var subscriberId = 0;
            if (!string.IsNullOrEmpty(model.Key))
                subscriberId = int.Parse(_dataProtector.Unprotect(model.Key));
            var subscriber = _context.Subscribers.SingleOrDefault(s => s.MobileNumber == model.MobileNumber);

            var isAllowed = subscriber is null || subscriber.Id.Equals(subscriberId);
            return Json(isAllowed);
        }

        private SubscriberFormViewModel PopulateViewModel(SubscriberFormViewModel? model = null)
        {
            SubscriberFormViewModel viewModel = model is null ? new SubscriberFormViewModel() : model;
            var governorates = _context.Governorates.Where(g => !g.IsDeleted)
                .OrderBy(g => g.Name).ToList();
            viewModel.Governorates = _mapper.Map<IEnumerable<SelectListItem>>(governorates);
            //if we are editing, we need to populate the areas too
            if (model?.GovernorateId > 0)
            {
                var areas = _context.Areas.Where(a => a.GovernorateId == model.GovernorateId).ToList();
                model.Areas = _mapper.Map<IEnumerable<SelectListItem>>(areas);
            }
            return viewModel;
        }

        private string GetUserId()
        {
            return User.FindFirst(ClaimTypes.NameIdentifier)!.Value;
        }
    }
}
