using System;
using System.Net;

namespace BrusLib {
    public struct LibrusConnection {
        public string username, password;
        public CookieContainer cookieSession;
        public readonly DateTime creationDate;
        public readonly bool successful;
        
        public LibrusConnection(string username, string password,  CookieContainer cookieSession, bool successful = true) {
            this.username = username;
            this.password = password;
            this.cookieSession = cookieSession;
            this.creationDate = DateTime.Now;
            this.successful = successful;
        }

        public bool IsAlive() {
            if (!successful) return false;
            return (creationDate - DateTime.Now).Minutes < 10;
        }
    }
}