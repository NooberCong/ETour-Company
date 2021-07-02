﻿using Company.Models;
using Core.Entities;
using Core.Interfaces;
using Infrastructure.InterfaceImpls;
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
            Question question = await _questionRepository.Queryable.Include(p => p.Author)
               .FirstOrDefaultAsync(p => p.ID == id);

            

            

            return View(new QuestionDetailModel
            {
                Question=question
            });
        }
    
        [HttpPost]
        public async Task<IActionResult> Answer(string Answer, int id, string returnUrl)
        {
            
            returnUrl ??= Url.Action("Index");
            string empID = User.Claims.First(cl => cl.Type == ClaimTypes.NameIdentifier).Value;
            Employee Author = await _employeeRepository.FindAsync(empID);
            Question question1 = await _questionRepository.Queryable.Include(p => p.Author)
               .FirstOrDefaultAsync(p => p.ID == id);
            Answer answer = new Answer()
            {
                Author = Author.FullName,
                Content = Answer,
                QuestionID = id,
                AuthoredByCustomer = false,
                LastUpdated = DateTime.UtcNow
            };
           

            await _answerRepository.AddAsync(answer);
            await _unitOfWork.CommitAsync();


            return Redirect(returnUrl);
        }
    }
}
