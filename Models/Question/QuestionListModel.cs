using Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Company.Models
{
    public class QuestionListModel
    {
        public bool ShowClosed { get; set; }
        public IEnumerable<Question> Questions { get; set; }
    }
}
