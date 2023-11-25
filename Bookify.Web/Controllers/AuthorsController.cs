namespace Bookify.Web.Controllers
{
    public class AuthorsController:Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public AuthorsController(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        [HttpGet]
        public IActionResult Index()
        {
            var authors = _context.Authors.AsNoTracking().ToList();
            var viewModel = _mapper.Map<IEnumerable<AuthorViewModel>>(authors);
            return View(viewModel);
        }

        [HttpGet]
        [AjaxOnly]
        public IActionResult Create()
        {
            //return a partial view to render it in the modal
            return PartialView("_Form");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(AuthorFormViewModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            var author = _mapper.Map<Author>(model);

            _context.Add(author);
            _context.SaveChanges();

            var viewModel = _mapper.Map<AuthorViewModel>(author);

            return PartialView("_AuthorRow", viewModel);
        }

        [HttpGet]
        [AjaxOnly]
        public IActionResult Edit(int id)
        {
            var author = _context.Authors.Find(id);

            if (author is null)
                return NotFound();

            var viewModel = _mapper.Map<AuthorFormViewModel>(author);
            return PartialView("_Form", viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(AuthorFormViewModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            var authorToEdit = _context.Authors.Find(model.Id);

            if (authorToEdit is null)
                return NotFound();

            authorToEdit = _mapper.Map(model, authorToEdit);
            authorToEdit.LastUpdatedOn = DateTime.Now;

            _context.SaveChanges();

            var viewModel = _mapper.Map<AuthorViewModel>(authorToEdit);

            return PartialView("_AuthorRow", viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ToggleStatus(int id)
        {
            var Author = _context.Authors.Find(id);
            if (Author is null)
                return NotFound();

            Author.IsDeleted = !Author.IsDeleted;
            Author.LastUpdatedOn = DateTime.Now;
            _context.SaveChanges();

            return Ok(Author.LastUpdatedOn.ToString());
        }

        public IActionResult AllowItem(AuthorFormViewModel model)
        {
            var Exist = _context.Authors.Any(c => c.Name == model.Name);
            return Json(!Exist);

        }
    }
}
