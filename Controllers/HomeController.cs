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

public async Task<IActionResult> Index(int? categoryId, string? searchTerm)
{
    // 1. Lấy danh sách danh mục để hiển thị ở Sidebar
    var categories = await _categoryService.GetAllAsync();
    ViewBag.Categories = categories;
    ViewBag.CurrentCategory = categoryId;
    ViewBag.SearchTerm = searchTerm;

    // 2. Lấy tất cả tài liệu đã duyệt
    var documentsDto = await _documentService.GetAllApprovedAsync(); 
    
    // 3. Lọc theo Danh mục (CategoryId) nếu người dùng có bấm chọn
    if (categoryId.HasValue && categoryId.Value > 0)
    {
        documentsDto = documentsDto.Where(d => d.CategoryId == categoryId.Value).ToList();
    }

    // 4. Lọc theo Từ khóa tìm kiếm nếu người dùng có nhập
    if (!string.IsNullOrWhiteSpace(searchTerm))
    {
        var term = searchTerm.ToLower().Trim();
        documentsDto = documentsDto.Where(d => 
            d.Title.ToLower().Contains(term) || 
            (d.Description != null && d.Description.ToLower().Contains(term))
        ).ToList();
    }

    // 5. Map sang ViewModel và trả về giao diện
    var viewModels = _mapper.Map<IEnumerable<DocumentViewModel>>(documentsDto);

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