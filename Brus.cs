using System;
using System.Threading.Tasks;

namespace BrusLib2 {

    // this class serves as a data container for a session.
    public class Brus {
        private DataProvider m_dataProvider;

        private Subject[]? m_subjects;


        public Brus(DataProvider dataProvider) {
            m_dataProvider = dataProvider;
        }

        public async Task<bool> Login(string login, string password) {
            return await m_dataProvider.Login(login, password);
        }

        public async Task<Subject[]> GetSubjectsGrades(bool forceRefetch = false) {
            if (m_subjects == null || forceRefetch) {
                m_subjects = await m_dataProvider.FetchSubjectsGrades();
            }
            return m_subjects;
        }

        public bool IsSessionValid => m_dataProvider.IsSessionValid;



        //this class will be our way to store the data from the session, ex. the grades and timetable events.
    }

}