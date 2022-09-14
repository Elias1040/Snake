using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Snake.Model
{
    public class ViewModel
    {
        public ObservableCollection<SnakeScore> SnakeScores { get; set; }
        public ViewModel(List<SnakeScore> scores)
        {
            SnakeScores = new ObservableCollection<SnakeScore>(scores);
        }
    }
}
