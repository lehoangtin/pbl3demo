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
            // --- DOCUMENT MAPPINGS ---
            CreateMap<DocumentCreateRequest, Document>();
            CreateMap<Document, DocumentResponse>()
                // Đổi thành AuthorName cho đúng với DTO của bạn
                .ForMember(dest => dest.AuthorName, opt => opt.MapFrom(src => src.User != null ? src.User.UserName : "Ẩn danh"))
                .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category != null ? src.Category.Name : "Chưa phân loại"));

            // --- QUESTION MAPPINGS ---
            CreateMap<QuestionCreateRequest, Question>();
            CreateMap<Question, QuestionResponse>()
                // Đổi thành AuthorName
                .ForMember(dest => dest.AuthorName, opt => opt.MapFrom(src => src.User != null ? src.User.UserName : "Ẩn danh"))
                // Map thêm số lượng câu trả lời (AnswerCount) cho QuestionResponse
                .ForMember(dest => dest.AnswerCount, opt => opt.MapFrom(src => src.Answers != null ? src.Answers.Count : 0));

            // --- ANSWER MAPPINGS ---
            CreateMap<AnswerCreateRequest, Answer>();
            CreateMap<Answer, AnswerResponse>()
                // Đổi thành AuthorName
                .ForMember(dest => dest.AuthorName, opt => opt.MapFrom(src => src.User != null ? src.User.UserName : "Ẩn danh"));
            
            // --- CATEGORY MAPPINGS ---
            CreateMap<CategoryCreateRequest, Category>();
            CreateMap<Category, CategoryResponse>();

            CreateMap<DocumentResponse, DocumentViewModel>();
            CreateMap<QuestionResponse, QuestionViewModel>();
            CreateMap<AnswerResponse, AnswerViewModel>();
        }
    }
}