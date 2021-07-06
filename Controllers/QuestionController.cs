using Company.Models;
using Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace Company.Controllers
{
    [Authorize(Roles = "Admin,CustomerRelation")]
    public class QuestionController : Controller
    {
        private readonly IQuestionRepository _questionRepository;

        public QuestionController(IQuestionRepository questionRepository)
        {
            _questionRepository = questionRepository;
        }

        public IActionResult Index(bool showClosed)
        {
            var questions = _questionRepository.Queryable
                .OrderBy(q => q.LastUpdated)
                .Include(q => q.Owner)
                .Where(q => showClosed || q.Status != Core.Entities.Question.QuestionStatus.Closed)
                .AsEnumerable();

            return View(new QuestionListModel { 
                ShowClosed = showClosed,
                Questions = questions
            });
        }
    }
}
