using System;

namespace fakebook.Models
{
    public class Topic
    {
        public String Topic_id{ set;get; }
        public String Topic_name { set; get; }
        public String Topic_Content { get; set; }
        public long Topic_date { set; get; }
        public int Topic_view { set; get; }
        public String username { set; get; }
        public String showdate { set; get; }
        public string userimg { set; get; }
    }
}