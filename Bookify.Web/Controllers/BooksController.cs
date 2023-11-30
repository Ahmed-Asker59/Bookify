using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Options;
using System.Security.Principal;

namespace Bookify.Web.Controllers
{

    public class BooksController : Controller
    {
        private IWebHostEnvironment _webHostEnvironment;
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private List<string>  _allowedExtensions = new (){".jpg",".jpeg",".png"};
        private int _maxedAllowedSize = 2097152;

        public BooksController(ApplicationDbContext context, IMapper mapper, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _mapper = mapper;
            _webHostEnvironment = webHostEnvironment;
          
        }

        public IActionResult Index()
        {
            return View();
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
                if (!_allowedExtensions. Contains(extension))
                {
                    ModelState.AddModelError(nameof(model.Image),Errors.NotAllowedExtension);
                    return View("form", model);
                }

                if(model.Image.Length > _maxedAllowedSize)
                {
                    ModelState.AddModelError(nameof(model.Image), Errors.MaxSize);
                    return View("form", model);
                }
                var imgName = $"{Guid.NewGuid()}{extension}";
                var path = Path.Combine($"{_webHostEnvironment.WebRootPath}/Images/Books", imgName);

                using var stream = System.IO.File.Create(path);
                await model.Image.CopyToAsync(stream);
                book.ImageUrl = imgName;
            }
            foreach (var category in model.SelectedCategories)
            {
                book.Categories.Add(new BookCategory { CategoryId = category });
            }

            
            _context.Add(book);
            _context.SaveChanges();

            return RedirectToAction(nameof(Index));
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

            var book = _context.Books.Include(b => b.Categories).SingleOrDefault(b => b.Id == model.Id);
            if (book is null)
                return NotFound();

            //if the user uploaded a new img
            //we need to check if there is an already an image or not
            //if there is an image we need to delete it
            if (model.Image is not null)
            {
                //check if the database has  the name of the image
                if (!string.IsNullOrEmpty(book.ImageUrl))
                {
                    //get the old image path in the application
                    var oldImagePath = Path.Combine($"{_webHostEnvironment.WebRootPath}/Images/Books", book.ImageUrl);
                    //delete the old image
                    if(System.IO.File.Exists(oldImagePath))
                        System.IO.File.Delete(oldImagePath);

                    
                }
                var extension = Path.GetExtension(model.Image.FileName);
                if (!_allowedExtensions.Contains(extension))
                {
                    ModelState.AddModelError(nameof(model.Image), Errors.NotAllowedExtension);
                    return View("form", model);
                }

                if (model.Image.Length > _maxedAllowedSize)
                {
                    ModelState.AddModelError(nameof(model.Image), Errors.MaxSize);
                    return View("form", model);
                }
                var imgName = $"{Guid.NewGuid()}{extension}";
                var path = Path.Combine($"{_webHostEnvironment.WebRootPath}/Images/Books", imgName);

                using var stream = System.IO.File.Create(path);
                await model.Image.CopyToAsync(stream);
                model.ImageUrl = imgName;
            }
            else if(!string.IsNullOrEmpty(book.ImageUrl))
            {
                model.ImageUrl = book.ImageUrl;
            }

            

            //get the new data from the edited model
            book = _mapper.Map(model, book);
            book.LastUpdatedOn = DateTime.Now;
            //add the selecte categories to the book
            foreach (var category in model.SelectedCategories)
            {
                book.Categories.Add(new BookCategory { CategoryId = category });
            }
            _context.SaveChanges();

            return RedirectToAction(nameof(Index));
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
