using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Options;
using SixLabors.ImageSharp.Processing;
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

        public IActionResult Details(int id)
        {
            var book = _context.Books.Include(b => b.Author)
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
                var thumbPath = Path.Combine($"{_webHostEnvironment.WebRootPath}/Images/Books/thumb", imgName);

                using var stream = System.IO.File.Create(path);
                await model.Image.CopyToAsync(stream);
                //dispose the stream to start another
                stream.Dispose();

                book.ImagePath = $"/Images/Books/{imgName}";
                book.ImageThumbnailPath = $"/Images/Books/thumb/{imgName}";

                //open the image sent by end user
                using var image = Image.Load(model.Image.OpenReadStream());
                var ratio = (float) image.Width / 200;
                var height =  image.Height / ratio;
                //reduce the size to create a thumbnail
                image.Mutate(i => i.Resize(width: 200, height: (int)height));
                //save the thumbnail
                image.Save(thumbPath);

            }
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

            var book = _context.Books.Include(b => b.Categories).SingleOrDefault(b => b.Id == model.Id);
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
                    //get the old image path in the application
                    var oldImagePath = $"{_webHostEnvironment.WebRootPath}{book.ImagePath}";
                    var oldThumbPath = $"{_webHostEnvironment.WebRootPath}{book.ImageThumbnailPath}";
                    //delete the old image
                    if(System.IO.File.Exists(oldImagePath))
                        System.IO.File.Delete(oldImagePath);
                    //delete the thumbnail
                    if (System.IO.File.Exists(oldThumbPath))
                        System.IO.File.Delete(oldThumbPath);



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
                var thumbPath = Path.Combine($"{_webHostEnvironment.WebRootPath}/Images/Books/thumb", imgName);

                using var stream = System.IO.File.Create(path);
                await model.Image.CopyToAsync(stream);
                //dispose the stream to start another
                stream.Dispose();

                model.ImagePath = $"/Images/Books/{imgName}";
                model.ImageThumbnailPath = $"/Images/Books/thumb/{imgName}";

                //open the image sent by end user
                using var image = Image.Load(model.Image.OpenReadStream());
                var ratio = (float) image.Width / 200;
                var height = image.Height / ratio;
                //reduce the size to create a thumbnail
                image.Mutate(i => i.Resize(width: 200, height: (int)height));
                //save the thumbnail
                image.Save(thumbPath);
            }
            else if(!string.IsNullOrEmpty(book.ImagePath))
            {
                model.ImagePath = book.ImagePath;
                model.ImageThumbnailPath = book.ImageThumbnailPath;
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

            return RedirectToAction(nameof(Details), new {id = book.Id});
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
