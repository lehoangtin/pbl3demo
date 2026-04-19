using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization; 
using System.Threading.Tasks;
using StudyShare.Services.Interfaces;
using System.Security.Claims; // Bổ sung Claims
using AutoMapper;
using StudyShare.ViewModels; // Bổ sung ViewModels


namespace StudyShare.Controllers
{
    public class HomeController : Controller
    {
        private readonly IDocumentService _documentService;
        private readonly ICategoryService _categoryService;
        private readonly IUserService _userService;
        private readonly IMapper _mapper; // Bổ sung IMapper
        private readonly IAuthService _authService; // Thay SignInManager bằng IAuthService của bạn

        public HomeController(
            IDocumentService documentService,
            ICategoryService categoryService,
            IUserService userService,
            IMapper mapper, // Tiêm IMapper  
            IAuthService authService) // Tiêm IAuthService
        {
            _documentService = documentService;
            _categoryService = categoryService;
            _userService = userService;
            _authService = authService;
            _mapper = mapper;
        }

public async Task<IActionResult> Index()
{
    // 1. Gọi hàm đã khai báo ở Bước 1
    var documentsDto = await _documentService.GetAllApprovedAsync(); 

    // 2. Map từ List DTO sang List ViewModel
    var viewModels = _mapper.Map<IEnumerable<DocumentViewModel>>(documentsDto);

    // 3. Trả về View với đúng kiểu dữ liệu (DocumentViewModel)
    return View(viewModels); 
}


        [Authorize] 
        public async Task<IActionResult> ViewDocument(int id)
        {
            var document = await _documentService.GetDocumentDetailsAsync(id);
            if (document == null) return NotFound();

            // Tối ưu: Lấy User ID qua Claims
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            
            bool isSaved = false;
            if (!string.IsNullOrEmpty(userId))
            {
                isSaved = await _userService.IsDocumentSavedAsync(userId, id);
            }

            ViewBag.IsSaved = isSaved;

            return View(document);
        }
    }
}