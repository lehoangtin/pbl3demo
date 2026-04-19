using AutoMapper;
using StudyShare.Models;
using StudyShare.DTOs.Responses;
using StudyShare.DTOs.Requests;
using StudyShare.ViewModels;

namespace StudyShare.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // ==========================================
            // 1. CATEGORY MAPPINGS
            // ==========================================
            CreateMap<Category, CategoryResponse>()
                .ForMember(dest => dest.DocumentCount, 
                           opt => opt.MapFrom(src => src.Documents != null ? src.Documents.Count : 0));
            
            CreateMap<CategoryResponse, CategoryViewModel>();
            
            CreateMap<CategoryCreateRequest, Category>();
            CreateMap<Category, CategoryUpdateRequest>().ReverseMap();


            // ==========================================
            // 2. DOCUMENT MAPPINGS
            // ==========================================
            CreateMap<Document, DocumentResponse>()
                .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category != null ? src.Category.Name : "Chưa phân loại"))
                .ForMember(dest => dest.AuthorName, opt => opt.MapFrom(src => src.User != null ? src.User.FullName : "Ẩn danh"));

            // Map từ Response sang ViewModel để hiển thị ở View
            CreateMap<DocumentResponse, DocumentViewModel>();

            CreateMap<DocumentCreateRequest, Document>()
                .ForMember(dest => dest.FilePath, opt => opt.Ignore())
                .ForMember(dest => dest.FileName, opt => opt.Ignore())
                .ForMember(dest => dest.FileType, opt => opt.Ignore())
                .ForMember(dest => dest.FileSize, opt => opt.Ignore());


            // ==========================================
            // 3. QUESTION & ANSWER MAPPINGS
            // ==========================================
            CreateMap<Question, QuestionResponse>()
                .ForMember(dest => dest.AuthorName, opt => opt.MapFrom(src => src.User != null ? src.User.FullName : "Ẩn danh"))
                .ForMember(dest => dest.AnswerCount, opt => opt.MapFrom(src => src.Answers != null ? src.Answers.Count : 0));
            
            CreateMap<QuestionResponse, QuestionViewModel>();
            CreateMap<QuestionCreateRequest, Question>();
            CreateMap<QuestionUpdateRequest, Question>();

            CreateMap<Answer, AnswerResponse>()
                .ForMember(dest => dest.AuthorName, opt => opt.MapFrom(src => src.User != null ? src.User.FullName : "Ẩn danh"));
            
            CreateMap<AnswerResponse, AnswerViewModel>();
            CreateMap<AnswerCreateRequest, Answer>();


            // ==========================================
            // 4. USER & PROFILE MAPPINGS
            // ==========================================
            CreateMap<AppUser, UserResponse>();
            CreateMap<UserResponse, UserViewModel>();
            CreateMap<ProfileUpdateRequest, AppUser>();


            // ==========================================
            // 5. REPORT MAPPINGS
            // ==========================================
            CreateMap<Report, ReportResponse>()
                .ForMember(dest => dest.ReporterName, opt => opt.MapFrom(src => src.Reporter != null ? src.Reporter.FullName : "Ẩn danh"))
                // Lưu ý: Đảm bảo tên thuộc tính ở Report Entity là 'Target' hay 'TargetUser' cho khớp
                .ForMember(dest => dest.ReportedUserName, opt => opt.MapFrom(src => src.Target != null ? src.Target.FullName : "N/A"))
                .ForMember(dest => dest.DocumentTitle, opt => opt.MapFrom(src => src.Document != null ? src.Document.Title : "N/A"));

            CreateMap<ReportResponse, ReportViewModel>();
        }
    }
}
