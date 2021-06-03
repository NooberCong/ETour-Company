using Company.Models;
using Core.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Company.Controllers
{
    public class AccountController : Controller
    {
        private readonly ICustomerRepository _customerRepository;

        public IActionResult Index(bool showBanned = true)
        {
            return View(new AccountListModel
            {

            });
        }
    }
}
