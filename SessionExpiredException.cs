using System;
using System.Runtime.Serialization;

namespace BrusLib2;

[Serializable]
internal class SessionExpiredException : Exception {
    private DateTime? m_expirationDate;

    public SessionExpiredException(DateTime expirationDate) : base("Session expired at " + expirationDate.ToString("yyyy-MM-dd HH:mm:ss")) {
        this.m_expirationDate = expirationDate;
    }

}
