using System;
using System.Threading.Tasks;

namespace BrusLib2 {

    // this class serves as a data container for a session.
    public class Brus {
        private DataProvider m_dataProvider;


        public Brus(DataProvider dataProvider) {
            m_dataProvider = dataProvider;
        }

        public async Task<bool> Login(string login, string password) => await m_dataProvider.Login(login, password);

        public async Task<Subject[]> GetSubjectsGrades() => await m_dataProvider.FetchSubjectsGrades();



        //this class will be our way to store the data from the session, ex. the grades and timetable events.
    }

}