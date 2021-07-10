using Company.Models;
using Core.Entities;
using Core.Interfaces;
using Infrastructure.InterfaceImpls;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Company.Controllers
{

    [Authorize(Roles = "Admin,CustomerRelation")]
    public class QuestionController : Controller
    {
        private readonly IQuestionRepository _questionRepository;
        private readonly IEmployeeRepository<Employee> _employeeRepository;
        private readonly IAnswerRepository _answerRepository;
        private readonly IUnitOfWork _unitOfWork;

        public QuestionController(IQuestionRepository questionRepository, IEmployeeRepository<Employee> employeeRepository, IAnswerRepository answerRepository, IUnitOfWork unitOfWork)
        {
            _questionRepository = questionRepository;
            _employeeRepository = employeeRepository;
            _answerRepository = answerRepository;
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index(bool showClosed)
        {
            var questions = _questionRepository.Queryable
                .OrderBy(q => q.LastUpdated)
                .Include(q => q.Owner)
                .Where(q => showClosed || q.Status != Core.Entities.Question.QuestionStatus.Closed)
                .AsEnumerable();

            return View(new QuestionListModel
            {
                ShowClosed = showClosed,
                Questions = questions
            });
        }

        public async Task<IActionResult> Answer(int id)
        {
            Question question = await _questionRepository.Queryable
                .Include(q => q.Owner)
                .Include(q => q.Answers)
                .FirstOrDefaultAsync(p => p.ID == id);

            return View(new QuestionDetailModel
            {
                Question = question
            });
        }

        [HttpPost]
        public async Task<IActionResult> Answer(Answer answer, string returnUrl)
        {
            var question = await _questionRepository.Queryable
                .Include(q => q.Owner)
                .Include(q => q.Answers)
                .FirstOrDefaultAsync(q => q.ID == answer.QuestionID);
            if (question == null)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return View(new QuestionDetailModel
                {
                    Question = question,
                    Answer = answer
                });
            }

            returnUrl ??= Url.Action("Answer", new { id = question.ID });

            string empID = User.Claims.First(cl => cl.Type == ClaimTypes.NameIdentifier).Value;
            Employee author = await _employeeRepository.FindAsync(empID);

            answer.Author = author.FullName;
            answer.AuthoredByCustomer = false;
            answer.LastUpdated = DateTime.Now;

            await _answerRepository.AddAsync(answer);
            await _unitOfWork.CommitAsync();
            return Redirect(returnUrl);
        }


        [HttpPost]
        public async Task<IActionResult> Edit(int id, Question.QuestionPriority priority, Question.QuestionStatus status, string returnUrl)
        {
            Question question = await _questionRepository.Queryable.FirstOrDefaultAsync(p => p.ID == id);

            if (question == null)
            {
                return NotFound();
            }

            returnUrl ??= Url.Action("Answer", new { id = question.ID });
            question.Status = status;
            question.Priority = priority;

            await _questionRepository.UpdateAsync(question);
            await _unitOfWork.CommitAsync();
            return LocalRedirect(returnUrl);
        }
    }
}
