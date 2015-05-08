using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Linq.Expressions;
using Newtonsoft.Json.Linq;

namespace embedd_wpf_demo
{
    class PeopleData
    {
        public static List<Person> Get()
        {
            List<Person> people;
            using (StreamReader sr = new StreamReader("data.json"))
            {

                var dataStr = sr.ReadToEnd();
                people = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Person>>(dataStr);
            }

            return people;
        }
    }
}
