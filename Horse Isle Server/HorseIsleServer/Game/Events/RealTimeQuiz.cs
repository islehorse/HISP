using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HISP.Game.Events
{
    class RealTimeQuiz
    {
        public class QuizQuestion
        {
            public QuizQuestion(QuizCategory category)
            {
                BaseCategory = category;
            }
            public QuizCategory BaseCategory;
            public string Question;
            public string[] Answers;
        }
        public class QuizCategory
        {
            public string Name;
            public QuizQuestion[] Questions;
        }

        public static QuizCategory[] Categories;

    }
}
