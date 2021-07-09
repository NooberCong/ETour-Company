using Company.Models;
using Core.Entities;
using Core.Interfaces;

using Infrastructure.InterfaceImpls;

using Microsoft.AspNetCore.Authorization;

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

            return View(new QuestionListModel { 
                ShowClosed = showClosed,
                Questions = questions
            });
        }

        public async Task<IActionResult> Answer(int id)
        {
            Question question = await _questionRepository.Queryable.Include(p => p.Owner)
               .FirstOrDefaultAsync(p => p.ID == id);
            List<Answer> answers = (from a in _answerRepository.Queryable where a.QuestionID == id select a).ToList();

            return View(new QuestionDetailModel
            {
                Question=question,
                Answers = answers,
                QuestionId = id
            });
        }
    
        [HttpPost]
        public async Task<IActionResult> Answer(string Answer, int QuestionId, string returnUrl, Question Question)
        {
            
            returnUrl ??= Url.Action("Index");
            string empID = User.Claims.First(cl => cl.Type == ClaimTypes.NameIdentifier).Value;
            Employee Author = await _employeeRepository.FindAsync(empID);
            Question question1 = await _questionRepository.Queryable.Include(p => p.Owner)
               .FirstOrDefaultAsync(p => p.ID == QuestionId);

            question1.Status = Question.Status;
            question1.Priority = Question.Priority;

            Answer answer = new Answer()
            {
                Author = Author.FullName,
                Content = Answer,
                QuestionID = QuestionId,
                AuthoredByCustomer = false,
                LastUpdated = DateTime.UtcNow
            };

            await _questionRepository.UpdateAsync(question1);
            if(Answer != null)await _answerRepository.AddAsync(answer);
            await _unitOfWork.CommitAsync();


            return Redirect(returnUrl);
        }


        [HttpPost]
        public async Task<IActionResult> Edit(int QuestionId, string returnUrl, Question Question)
        {

            returnUrl ??= Url.Action("Index");
            Question question1 = await _questionRepository.Queryable.Include(p => p.Owner)
               .FirstOrDefaultAsync(p => p.ID == QuestionId);
            question1.Status = Question.Status;
            question1.Priority = Question.Priority;
            await _questionRepository.UpdateAsync(question1);
            await _unitOfWork.CommitAsync();
            return Redirect(returnUrl);
        }


    }
}
