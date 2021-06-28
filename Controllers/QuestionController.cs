using Company.Models;
using Core.Entities;
using Core.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Company.Controllers
{
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
                .Include(q => q.Author)
                .Where(q => showClosed || q.Status != Core.Entities.Question.QuestionStatus.Closed)
                .AsEnumerable();

            return View(new QuestionListModel { 
                ShowClosed = showClosed,
                Questions = questions
            });
        }

        public async Task<IActionResult> Answer(int id)
        {
            Question question = await _questionRepository.Queryable.Include(p => p.Author)
               .FirstOrDefaultAsync(p => p.ID == id);

            if (post == null)
            {
                return NotFound();
            }

            return View(question);
        }
    

        public async Task<IActionResult> Answer(string answer, string returnUrl)
        {
            returnUrl ??= Url.Action("Index");
            string empID = User.Claims.First(cl => cl.Type == ClaimTypes.NameIdentifier).Value;

            return Redirect(returnUrl);
        }
    }
}
