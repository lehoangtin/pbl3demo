using AutoMapper;
using StudyShare.Models;
using StudyShare.DTOs.Requests;
using StudyShare.DTOs.Responses;
using System.Linq;
using System.Collections.Generic;
using StudyShare.ViewModels;

namespace StudyShare.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // ==========================================
            // THÊM MỚI: ENTITY -> VIEWMODEL (Sửa lỗi 500 cho MyQuestions, MyDocuments)
            // ==========================================
            // Thêm đoạn này để sửa lỗi trang Bảng xếp hạng (Ranking)
            CreateMap<AppUser, UserViewModel>();
            CreateMap<Document, DocumentViewModel>()
                .ForMember(dest => dest.AuthorName, opt => opt.MapFrom(src => src.User.FullName));
            CreateMap<AppUser, UserResponse>()
                .ForMember(dest => dest.Points, opt => opt.MapFrom(src => src.Points))
                .ForMember(dest => dest.IsBanned, opt => opt.MapFrom(src => src.IsBanned)); // THÊM DÒNG NÀY
            CreateMap<Document, DocumentViewModel>()
                .ForMember(dest => dest.AuthorName, opt => opt.MapFrom(src => src.User != null ? src.User.UserName : "Ẩn danh"))
                .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category != null ? src.Category.Name : "Chưa phân loại"));
            CreateMap<UserEditViewModel, ProfileUpdateRequest>().ReverseMap();
            CreateMap<AppUser, UserEditViewModel>();
            CreateMap<AppUser, UserViewModel>()
                .ForMember(dest => dest.DocumentCount, opt => opt.MapFrom(src => src.Documents != null ? src.Documents.Count : 0))
                .ForMember(dest => dest.QuestionCount, opt => opt.MapFrom(src => src.Questions != null ? src.Questions.Count : 0))
                .ForMember(dest => dest.Points, opt => opt.MapFrom(src => src.Points))
                .ForMember(dest => dest.IsBanned, opt => opt.MapFrom(src => src.IsBanned)); // THÊM DÒNG NÀY
            // Fix lỗi Question -> QuestionViewModel
            CreateMap<Question, QuestionViewModel>()
                .ForMember(dest => dest.AuthorName, opt => opt.MapFrom(src => src.User != null ? src.User.UserName : "Ẩn danh"))
                .ForMember(dest => dest.AnswerCount, opt => opt.MapFrom(src => src.Answers != null ? src.Answers.Count : 0));

            // Fix lỗi Answer -> AnswerViewModel (Lỗi bạn vừa gặp)
            CreateMap<Answer, AnswerViewModel>()
                .ForMember(dest => dest.AuthorName, opt => opt.MapFrom(src => src.User != null ? src.User.UserName : "Ẩn danh"));


            // ==========================================
            // PHẦN CODE CŨ CỦA BẠN (GIỮ NGUYÊN)
            // ==========================================
            
            // --- DOCUMENT MAPPINGS ---
            CreateMap<DocumentCreateRequest, Document>();
            CreateMap<Document, DocumentResponse>()
                .ForMember(dest => dest.AuthorName, opt => opt.MapFrom(src => src.User != null ? src.User.UserName : "Ẩn danh"))
                .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category != null ? src.Category.Name : "Chưa phân loại"));
            CreateMap<DocumentResponse, DocumentViewModel>();
            // --- THÊM VÀO MAPPING PROFILE CHO DOCUMENT ---
            CreateMap<DocumentCreateViewModel, DocumentCreateRequest>();

            // ReverseMap cho phép tự động map 2 chiều (từ ViewModel -> Request và Request -> ViewModel)
            CreateMap<DocumentEditViewModel, DocumentUpdateRequest>().ReverseMap();

            // --- QUESTION MAPPINGS ---
            CreateMap<QuestionCreateRequest, Question>();
            CreateMap<Question, QuestionResponse>()
                .ForMember(dest => dest.AuthorName, opt => opt.MapFrom(src => src.User != null ? src.User.UserName : "Ẩn danh"))
                .ForMember(dest => dest.AnswerCount, opt => opt.MapFrom(src => src.Answers != null ? src.Answers.Count : 0));
            CreateMap<QuestionResponse, QuestionViewModel>();
            CreateMap<QuestionCreateViewModel, QuestionCreateRequest>();
            CreateMap<QuestionEditViewModel, QuestionUpdateRequest>().ReverseMap();
            // --- ANSWER MAPPINGS ---
            CreateMap<AnswerCreateRequest, Answer>();
            CreateMap<Answer, AnswerResponse>()
                .ForMember(dest => dest.AuthorName, opt => opt.MapFrom(src => src.User != null ? src.User.UserName : "Ẩn danh"));
            CreateMap<AnswerResponse, AnswerViewModel>();
            CreateMap<AnswerCreateViewModel, AnswerCreateRequest>();
            // --- CATEGORY MAPPINGS ---
            CreateMap<CategoryCreateRequest, Category>();
            CreateMap<Category, CategoryResponse>();
            CreateMap<CategoryResponse, CategoryViewModel>();
            // --- THÊM VÀO MAPPING PROFILE CỦA CATEGORY ---
            CreateMap<CategoryCreateViewModel, CategoryCreateRequest>();
            CreateMap<CategoryEditViewModel, CategoryUpdateRequest>();
            CreateMap<CategoryUpdateRequest, CategoryEditViewModel>(); // Dùng để map lúc lấy dữ liệu lên form Edit
            CreateMap<Category, CategoryViewModel>(); // Dùng để map Entity ra ViewModel ở form Delete

            //REPORT 
            CreateMap<ReportResponse, ReportViewModel>();
            CreateMap<Report, ReportResponse>()
                .ForMember(dest => dest.ReporterName, opt => opt.MapFrom(src => src.Reporter != null ? src.Reporter.FullName : "Ẩn danh"))
                .ForMember(dest => dest.TargetUserName, opt => opt.MapFrom(src => src.Target != null ? src.Target.FullName : "Ẩn danh"));
            CreateMap<UserResponse, UserViewModel>()
                .ForMember(dest => dest.IsBanned, opt => opt.MapFrom(src => src.IsBanned));

            CreateMap<Report, ReportViewModel>()
                .ForMember(dest => dest.ReporterName, opt => opt.MapFrom(src => src.Reporter.UserName))
                // Cấu hình ánh xạ duy nhất cho tên người bị báo cáo
                .ForMember(dest => dest.TargetUserName, opt => opt.MapFrom(src => src.Target.UserName));
        }
    }
}
