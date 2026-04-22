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
            CreateMap<AppUser, UserViewModel>()
                .ForMember(dest => dest.Points, opt => opt.MapFrom(src => src.Points));
            // Thêm đoạn này để sửa lỗi trang Bảng xếp hạng (Ranking)
            CreateMap<AppUser, UserResponse>()
                .ForMember(dest => dest.Points, opt => opt.MapFrom(src => src.Points));
            CreateMap<Document, DocumentViewModel>()
                .ForMember(dest => dest.AuthorName, opt => opt.MapFrom(src => src.User != null ? src.User.UserName : "Ẩn danh"))
                .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category != null ? src.Category.Name : "Chưa phân loại"));

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

            // --- QUESTION MAPPINGS ---
            CreateMap<QuestionCreateRequest, Question>();
            CreateMap<Question, QuestionResponse>()
                .ForMember(dest => dest.AuthorName, opt => opt.MapFrom(src => src.User != null ? src.User.UserName : "Ẩn danh"))
                .ForMember(dest => dest.AnswerCount, opt => opt.MapFrom(src => src.Answers != null ? src.Answers.Count : 0));
            CreateMap<QuestionResponse, QuestionViewModel>();

            // --- ANSWER MAPPINGS ---
            CreateMap<AnswerCreateRequest, Answer>();
            CreateMap<Answer, AnswerResponse>()
                .ForMember(dest => dest.AuthorName, opt => opt.MapFrom(src => src.User != null ? src.User.UserName : "Ẩn danh"));
            CreateMap<AnswerResponse, AnswerViewModel>();

            // --- CATEGORY MAPPINGS ---
            CreateMap<CategoryCreateRequest, Category>();
            CreateMap<Category, CategoryResponse>();
            CreateMap<CategoryResponse, CategoryViewModel>();

            //REPORT 
            CreateMap<ReportResponse, ReportViewModel>();
            CreateMap<Report, ReportResponse>()
                .ForMember(dest => dest.ReporterName, opt => opt.MapFrom(src => src.Reporter != null ? src.Reporter.FullName : "Ẩn danh"))
                .ForMember(dest => dest.TargetUserName, opt => opt.MapFrom(src => src.Target != null ? src.Target.FullName : "Ẩn danh"));
            CreateMap<UserResponse, UserViewModel>();   
            CreateMap<Report, ReportViewModel>()
                .ForMember(dest => dest.ReporterName, opt => opt.MapFrom(src => src.Reporter.UserName))
                // Cấu hình ánh xạ duy nhất cho tên người bị báo cáo
                .ForMember(dest => dest.TargetUserName, opt => opt.MapFrom(src => src.Target.UserName));
        }
    }
}
