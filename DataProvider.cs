using System;
using System.Threading.Tasks;

namespace BrusLib2 {
    public abstract class DataProvider {
        private DateTime m_sessionValidity = DateTime.MinValue;
        public virtual bool IsSessionValid => DateTime.Now < m_sessionValidity;
        public abstract bool Login(string username, string password);
    }
}