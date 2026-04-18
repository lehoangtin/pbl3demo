using AutoMapper;
using StudyShare.Models;
using StudyShare.DTOs.Responses;
using StudyShare.DTOs.Requests;

namespace StudyShare.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Map từ DB Model -> DTO hiển thị (Kèm tính tổng số tài liệu)
            CreateMap<Category, CategoryResponse>()
                .ForMember(dest => dest.DocumentCount, 
                           opt => opt.MapFrom(src => src.Documents != null ? src.Documents.Count : 0));

            // Map từ Form Request -> DB Model
            CreateMap<CategoryCreateRequest, Category>();
            CreateMap<CategoryUpdateRequest, Category>();
            CreateMap<Question, QuestionResponse>()
                // Lấy FullName từ bảng User đính kèm vào AuthorName
                .ForMember(dest => dest.AuthorName, opt => opt.MapFrom(src => src.User != null ? src.User.FullName : "Ẩn danh"))
                // Đếm số lượng câu trả lời
                .ForMember(dest => dest.AnswerCount, opt => opt.MapFrom(src => src.Answers != null ? src.Answers.Count : 0));

            CreateMap<QuestionCreateRequest, Question>();
            CreateMap<QuestionUpdateRequest, Question>();
            // --- MAPPING CHO DOCUMENT ---
            CreateMap<Document, DocumentResponse>()
                .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category != null ? src.Category.Name : "Chưa phân loại"))
                .ForMember(dest => dest.AuthorName, opt => opt.MapFrom(src => src.User != null ? src.User.FullName : "Ẩn danh"));

            // Cực kỳ quan trọng: Bỏ qua thuộc tính File khi map từ Request sang Entity DB (vì DB không lưu IFormFile)
            CreateMap<DocumentCreateRequest, Document>()
                .ForMember(dest => dest.FilePath, opt => opt.Ignore())
                .ForMember(dest => dest.FileName, opt => opt.Ignore())
                .ForMember(dest => dest.FileType, opt => opt.Ignore())
                .ForMember(dest => dest.FileSize, opt => opt.Ignore());

            // --- MAPPING CHO USER & PROFILE ---
            CreateMap<AppUser, UserResponse>();
            CreateMap<ProfileUpdateRequest, AppUser>();

            // --- MAPPING CHO ANSWER ---
            CreateMap<Answer, AnswerResponse>()
                .ForMember(dest => dest.AuthorName, opt => opt.MapFrom(src => src.User != null ? src.User.FullName : "Ẩn danh"));
            CreateMap<AnswerCreateRequest, Answer>();

            // --- MAPPING CHO REPORT ---
            CreateMap<Report, ReportResponse>()
                .ForMember(dest => dest.ReporterName, opt => opt.MapFrom(src => src.Reporter != null ? src.Reporter.FullName : "Ẩn danh"))
                .ForMember(dest => dest.ReportedUserName, opt => opt.MapFrom(src => src.Target != null ? src.Target.FullName : null))
                .ForMember(dest => dest.DocumentTitle, opt => opt.MapFrom(src => src.Document != null ? src.Document.Title : null));
        }
    }
}