using Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Company.Models
{
    public class QuestionDetailModel
    {
        public Question Question { get; set; }
        public Answer Answer { get; set; }
    }
}
