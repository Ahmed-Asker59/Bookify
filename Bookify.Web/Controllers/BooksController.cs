using Microsoft.AspNetCore.Mvc.Rendering;
using System.Linq.Dynamic.Core;

namespace Bookify.Web.Controllers
{
    [Authorize(Roles = AppRoles.Archive)]
    public class BooksController : Controller
    {
        private IWebHostEnvironment _webHostEnvironment;
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
		private readonly IImageService _IImageService;

		public BooksController(ApplicationDbContext context, IMapper mapper, IWebHostEnvironment webHostEnvironment, IImageService iImageService)
		{
			_context = context;
			_mapper = mapper;
			_webHostEnvironment = webHostEnvironment;
			_IImageService = iImageService;
		}

		public IActionResult Index()
        {
            return View();
        }


        [HttpPost]
        public IActionResult GetBooks()
        {
            //data sent from datatables
            var skip = int.Parse(Request.Form["start"]);
            var pageSize = int.Parse(Request.Form["length"]);
            var searchValue = Request.Form["search[value]"];

            //column selected by user to order

            var columnIndex = Request.Form["order[0][column]"];
            var columnName = Request.Form[$"columns[{columnIndex}][name]"];
            var sortDirection = Request.Form["order[0][dir]"];

            IQueryable<Book> books = _context.Books.Include(b => b.Author)
                .Include(b => b.Categories)
                .ThenInclude(bc => bc.Category);

            if (!string.IsNullOrEmpty(searchValue))
            {
                books = books.Where(b => b.Title.Contains(searchValue) || b.Author!.Name.Contains(searchValue) );
            }
            books = books.OrderBy($"{columnName} {sortDirection}");
            var data = books.Skip(skip).Take(pageSize).ToList();
            var mappedData = _mapper.Map<IEnumerable<BookViewModel>>(data);
            var recordsTotal = books.Count();
            var jsonData = new { recordsFiltered = recordsTotal, recordsTotal, data = mappedData };
            return Ok(jsonData);
        }

        public IActionResult Details(int id)
        {
            var book = _context.Books.Include(b => b.Author)
                .Include(b => b.Copies)
                .Include(b => b.Categories)
                .ThenInclude(bc => bc.Category)
                .SingleOrDefault(b => b.Id == id);

            if(book is null)
                return NotFound();

            var viewModel = _mapper.Map<BookViewModel>(book);   

            return View(viewModel);
        }

        public IActionResult Create()
        {
            var viewModel = PopulateViewModel();
            
            return View("Form", viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(BookFormViewModel model)
        {
           
            if (!ModelState.IsValid)
            {
                model = PopulateViewModel(model);    
                return View("form", model);
            }

            var book = _mapper.Map<Book>(model);
            if (model.Image is not null)
            {
                var extension = Path.GetExtension(model.Image.FileName);
                var imgName = $"{Guid.NewGuid()}{extension}";
                var (isUploaded,errorMessage) = await _IImageService.UploadAsync(model.Image, imgName, "/Images/Books", hasThumbNail: true);
                if (!isUploaded)
                {
                    ModelState.AddModelError(nameof(Image), errorMessage!);
                    return View("Form", PopulateViewModel(model));
                }
				book.ImagePath = $"/Images/Books/{imgName}";
				book.ImageThumbnailPath = $"/Images/Books/thumb/{imgName}";
			}
            book.CreatedById = User.FindFirst(ClaimTypes.NameIdentifier)!.Value; 
            foreach (var category in model.SelectedCategories)
            {
                book.Categories.Add(new BookCategory { CategoryId = category });
            }

            
            _context.Add(book);
            _context.SaveChanges();

            return RedirectToAction(nameof(Details), new {id = book.Id});
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            var book = _context.Books.Include(b => b.Categories).SingleOrDefault(b => b.Id == id);
            if (book is null)
                return NotFound();
            var model = _mapper.Map<BookFormViewModel>(book);

            var viewModel = PopulateViewModel(model);

            viewModel.SelectedCategories = book.Categories.Select(c => c.CategoryId).ToList();
            
            return View("Form", viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(BookFormViewModel model)
        {

            if (!ModelState.IsValid)
            {
                model = PopulateViewModel(model);
                return View("form", model);
            }

            var book = _context.Books.Include(b => b.Categories)
                .Include(b => b.Copies).SingleOrDefault(b => b.Id == model.Id);
            if (book is null)
                return NotFound();

            //if the user uploaded a new img
            //we need to check if there is an already an image or not
            //if there is an image we need to delete it
            if (model.Image is not null)
            {
                //check if the database has  the name of the image
                if (!string.IsNullOrEmpty(book.ImagePath))
                {
                   
                    _IImageService.DeleteImage(book.ImagePath, book.ImageThumbnailPath);

                }
				var extension = Path.GetExtension(model.Image.FileName);
				var imgName = $"{Guid.NewGuid()}{extension}";
				var (isUploaded, errorMessage) = await _IImageService.UploadAsync(model.Image, imgName, "/Images/Books", hasThumbNail: true);
				if (!isUploaded)
				{
					ModelState.AddModelError(nameof(Image), errorMessage!);
					return View("Form", PopulateViewModel(model));
				}
				
				model.ImagePath = $"/Images/Books/{imgName}";
				model.ImageThumbnailPath = $"/Images/Books/thumb/{imgName}";
			}
            else if(!string.IsNullOrEmpty(book.ImagePath))
            {
                model.ImagePath = book.ImagePath;
                model.ImageThumbnailPath = book.ImageThumbnailPath;
            }


            //get the new data from the edited model
            book = _mapper.Map(model, book);
            book.LastUpdatedById = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;
            book.LastUpdatedOn = DateTime.Now;
            //add the selecte categories to the book
            foreach (var category in model.SelectedCategories)
            {
                book.Categories.Add(new BookCategory { CategoryId = category });
            }
            //if user changed rental status to false,
            //any copies should be false too
            if(!book.IsAvailableForRental)
            {
                foreach(var copy in book.Copies)
                {
                    copy.IsAvailableForRental = false;
                }
            }
            _context.SaveChanges();

            return RedirectToAction(nameof(Details), new {id = book.Id});
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ToggleStatus(int id)
        {
            var book = _context.Books.Find(id);
            if (book is null)
                return NotFound();

            book.IsDeleted = !book.IsDeleted;
            book.LastUpdatedById = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;
            book.LastUpdatedOn = DateTime.Now;
            _context.SaveChanges();

            return Ok();
        }
        public IActionResult AllowItem(BookFormViewModel model)
        {
            var book = _context.Books.SingleOrDefault(b => b.Title == model.Title && b.AuthorId == model.AuthorId);
            var isAllowed = book is null || book.Id.Equals(model.Id);
            return Json(isAllowed);

        }
        private BookFormViewModel PopulateViewModel(BookFormViewModel? model= null)
        {
            BookFormViewModel viewModel = model is null ? new BookFormViewModel() : model;
            var authors = _context.Authors.Where(a => !a.IsDeleted).OrderBy(a => a.Name).ToList();
            var categories = _context.Categories.Where(c => !c.IsDeleted).OrderBy(c => c.Name).ToList();



            viewModel.Authors = _mapper.Map<IEnumerable<SelectListItem>>(authors);
            viewModel.Categories = _mapper.Map<IEnumerable<SelectListItem>>(categories);
           

            return viewModel;

        }
    }
}
