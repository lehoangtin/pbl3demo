using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudyShare.DTOs.Requests;
using StudyShare.Services.Interfaces;
using StudyShare.ViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StudyShare.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class CategoryController : Controller
    {
        private readonly ICategoryService _categoryService;
        private readonly IMapper _mapper;

        public CategoryController(ICategoryService categoryService, IMapper mapper)
        {
            _categoryService = categoryService;
            _mapper = mapper;
        }

        public async Task<IActionResult> Index()
        {
            var dtoList = await _categoryService.GetAllAsync();
            var viewModels = _mapper.Map<IEnumerable<CategoryViewModel>>(dtoList);
            return View(viewModels);
        }

        public IActionResult Create() => View();


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CategoryCreateViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                var request = _mapper.Map<CategoryCreateRequest>(viewModel);
                await _categoryService.CreateAsync(request);
                TempData["Success"] = "Thêm danh mục mới thành công!"; 
                return RedirectToAction(nameof(Index));
           }
            return View(viewModel);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var categoryDto = await _categoryService.GetForUpdateAsync(id);
            if (categoryDto == null) return NotFound();
            var viewModel = _mapper.Map<CategoryEditViewModel>(categoryDto);
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(CategoryEditViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                var request = _mapper.Map<CategoryUpdateRequest>(viewModel);
                await _categoryService.UpdateAsync(request);
                TempData["Success"] = "Cập nhật danh mục thành công!";
                return RedirectToAction(nameof(Index));
            }
            return View(viewModel);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var category = await _categoryService.GetByIdAsync(id);
            if (category == null) return NotFound();
            var viewModel = _mapper.Map<CategoryViewModel>(category);
            return View(viewModel);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var success = await _categoryService.DeleteAsync(id);
            if (success) TempData["Success"] = "Xóa danh mục thành công!";
            else TempData["Error"] = "Không thể xóa danh mục này.";
            
            return RedirectToAction(nameof(Index));
        }
    }
}
