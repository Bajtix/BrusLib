using System;
using System.Threading.Tasks;

namespace BrusLib2;
public abstract class DataProvider {
    protected DateTime m_sessionValidity = DateTime.MinValue;
    public virtual bool IsSessionValid => DateTime.Now < m_sessionValidity;
    public abstract Task<bool> Login(string username, string password);

    public abstract Task<Subject[]> FetchSubjectsGrades();
}
