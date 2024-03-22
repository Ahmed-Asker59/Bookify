using Microsoft.AspNetCore.DataProtection;

namespace Bookify.Web.Controllers
{

	[Authorize(Roles = AppRoles.Reception)]
	public class RentalsController : Controller
	{
        private readonly ApplicationDbContext _context;
        private readonly IDataProtector _dataProtector;
        private readonly IMapper _mapper;

        public RentalsController(ApplicationDbContext context, IDataProtectionProvider dataProtector, IMapper mapper)
        {
            _context = context;
            _dataProtector = dataProtector.CreateProtector("MySecureKey");
            _mapper = mapper;
        }

        public IActionResult Details(int id)
        {
            var rental = _context.Rentals
                .Include(r => r.RentalCopies)
                .ThenInclude(rc => rc.BookCopy)
                .ThenInclude(bc => bc!.Book)
                .SingleOrDefault(r => r.Id == id);

            if (rental is null)
                return NotFound();

            var viewModel = _mapper.Map<RentalViewModel>(rental);

            return View(viewModel);
        }

        public IActionResult Create(string sKey)
		{
            int subscriberId = int.Parse(_dataProtector.Unprotect(sKey));

            var subscriber = _context.Subscribers
                .Include(s => s.Subscriptions)
                .Include(s => s.Rentals)
                .ThenInclude(r => r.RentalCopies)
                .SingleOrDefault(s => s.Id == subscriberId);

            if(subscriber == null) 
                return NotFound();

           var (errorMessage, maxAllowedCopies) =  ValidateSubscriber(subscriber);
            if (!string.IsNullOrEmpty(errorMessage))
                return View("NotAllowedRental", errorMessage);
            var viewModel = new RentalFormViewModel
			{
				SubscriberKey = sKey,
                MaxAllowedCopies = maxAllowedCopies
            };

			return View(viewModel);
		}



        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(RentalFormViewModel model)
        {
            if(!ModelState.IsValid)
                 return View(model);

            int subscriberId = int.Parse(_dataProtector.Unprotect(model.SubscriberKey));

            var subscriber = _context.Subscribers
                .Include(s => s.Subscriptions)
                .Include(s => s.Rentals)
                .ThenInclude(r => r.RentalCopies)
                .SingleOrDefault(s => s.Id == subscriberId);

            if (subscriber == null)
                return NotFound();

            var (errorMessage, maxAllowedCopies) = ValidateSubscriber(subscriber);
            if (!string.IsNullOrEmpty(errorMessage))
                return View("NotAllowedRental", errorMessage);
           
            var selectedCopies = _context.BookCopies
                .Include(c => c.Book)
                .Include(c => c.Rentals)
                .Where(c => model.SelectedCopies.Contains(c.SerialNumber))
                .ToList();

            //get Id's of books rented by this subscriber
            var currentSubscriberRentals = _context.Rentals
                .Include(r => r.RentalCopies)
                .ThenInclude(rc => rc.BookCopy)
                .Where(r => r.SubscriberId == subscriberId)
                .SelectMany(r => r.RentalCopies)
                .Where(rc => !rc.ReturnDate.HasValue)
                .Select(rc => rc.BookCopy!.BookId).ToList();

            List<RentalCopy> rentalCopies = new();

            foreach (var copy in selectedCopies)
            {
                if (!copy.IsAvailableForRental || !copy.Book!.IsAvailableForRental)
                    return View("NotAllowedRental", Errors.NotAvailableRental);

                if(copy.Rentals.Any(rc => !rc.ReturnDate.HasValue))
                    return View("NotAllowedRental", Errors.CopyInRental);

                //check if user rented the same book as a different copy
                if(currentSubscriberRentals.Any(bookId => bookId == copy.BookId))
                    return View("NotAllowedRental", $"This subscriber has already rented a copy of '{copy.Book.Title}' book");

                rentalCopies.Add(new RentalCopy { BookCopyId = copy.Id });

            }

            Rental rental = new()
            {
                RentalCopies = rentalCopies,
                CreatedById = User.FindFirst(ClaimTypes.NameIdentifier)!.Value
            };

            subscriber.Rentals.Add(rental);
            _context.SaveChanges();
            return RedirectToAction(nameof(Details), new {id = rental.Id});

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult GetCopyDetails(SearchFormViewModel model)
        {
            if (!ModelState.IsValid)
                 return BadRequest();

            var copy = _context.BookCopies
                .Include(c => c.Book)
                .SingleOrDefault(c => c.SerialNumber.ToString() == model.Value && !c.IsDeleted && !c.Book!.IsDeleted);

            if(copy is null)
                return NotFound(Errors.InvalidSerialNumber);

            if(!copy.IsAvailableForRental || !copy.Book!.IsAvailableForRental)
                return BadRequest(Errors.NotAvailableRental);


            //CHECK THAT COPY IS  IN another RENTAL
            var isCopyInRental = _context.RentalCopies.Any(rc => rc.BookCopyId == copy.Id && !rc.ReturnDate.HasValue);
            if (isCopyInRental)
                return BadRequest(Errors.CopyInRental);
            var viewModel = _mapper.Map<BookCopyViewModel>(copy);

            return PartialView("_CopyDetails", viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult MarkAsDeleted(int id)
        {
            var rental = _context.Rentals.Find(id);

            if(rental is null || rental.CreatedOn.Date != DateTime.Today) return NotFound();

            rental.IsDeleted = true;
            rental.LastUpdatedOn = DateTime.Today;
            rental.LastUpdatedById = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;

            _context.SaveChanges();

            var copiesCount = _context.RentalCopies.Count(r => r.RentalId == id);

            return Ok(copiesCount);
        }

		private (string errorMessage, int? maxAllowedCopies) ValidateSubscriber(Subscriber subscriber)
        {
            if (subscriber.IsBlackListed)
                return (errorMessage: Errors.BlackListedSubscriber, maxAllowedCopies: null);

            if (subscriber.Subscriptions.Last().EndDate < DateTime.Today.AddDays((int)RentalConfigurations.MaxAllowedCopies))
                return (errorMessage: Errors.InactiveSubscriber, maxAllowedCopies: null);

            var currentRentals = subscriber.Rentals.SelectMany(r => r.RentalCopies)
                .Count(rc => !rc.ReturnDate.HasValue);
            var availableCopiesCount = (int)RentalConfigurations.MaxAllowedCopies - currentRentals;

            if (availableCopiesCount.Equals(0))
                return (errorMessage: Errors.MaxCopiesReached, maxAllowedCopies: null);

            return (errorMessage: string.Empty, maxAllowedCopies:  availableCopiesCount);
        }


    }
}
